# RCWA2D 数学推导 ↔ 代码对照文档 (VEMS)

> 本文件用于让 AI（或人）快速建立 VEMS 中二维 RCWA（`RCWA2D`）的**数学推导**与**源代码实现**之间的映射关系。
> 每一节先给出该步的数学含义，再指向**具体文件 / 类 / 方法 / 代码段**，并说明代码所用的变量名与数学符号的对应。
>
> 说明：本仓库为**删减版**（"VEMS删减读代码版本"）。计算主链（采样→傅里叶卷积矩阵→本征模式→S 矩阵→系数）完整；最末端的"衍射效率（坡印亭功率比）"在删减版中止于把输出系数转回 `PlaneWave` 振幅一步，效率值由 `PlaneWave` 的 Sz/坡印亭在外部计算。凡是代码与推导约定不同之处，本文用 **⚠ 约定差异** 明确标注，不强行对齐。

---

## 0. 全局视图：调用链与文件职责

### 0.1 顶层调用链（一次正向半 S 矩阵求解）

```
RCWA2D (构造: wavelength, 三层, period, thickness, nKxs/nKys, kx0/ky0)
   │
   └─ RCWA2D.ComputeHalfSMatrix(...)                     // RCWA/RCWA2D.cs
        │  (采样数自动决定: RCWAHelper.DetermineSampling)
        └─ SMatrix.HalfSMatrix(wavelength, front, middle, behind, ...)   // SMatrix/SMatrix.cs
             ├─ layerBehind.ComputeModes(...)   // UniformLayer  → (gamma, W1, W2) 解析
             ├─ layerMiddle.ComputeModes(...)   // Periodic2DLayer → (gamma, W1, W2) 数值本征分解
             │     ├─ SampleMedium(...)         // 把 ε(λ,x,y) 采样到网格 → MatrixZ epsilon
             │     ├─ FG(kx,ky,epsilon,mu)      // 构造 [F],[G] 分块矩阵
             │     │     └─ EigenHelper.DirXDirY / InvXDirY / InvYDirX  // Li 规则 Toeplitz 卷积矩阵
             │     ├─ E = F·G;  LinAlg.EigenSystem(E)     // 求本征值/向量
             │     ├─ gamma = i·k0·sqrt(eigenvalues); EigenHelper.CheckGamma   // 选分支
             │     └─ w1 = eigvec; w2 = -i/k0 · F⁻¹ · (w1·diag(gamma))
             ├─ layerFront.ComputeModes(...)    // UniformLayer  → (gamma, W1, W2) 解析
             └─ HalfSMatrixBoundary × 2         // behind→middle, middle→front
                   └─ HalfSMatrixKernel         // W⇒t⇒S（Redheffer）或 W⇒S 变体
        → 输出 S11, S21  (存入 RCWA2D.S11 / S21)

后续（按平面波输入求衍射系数）:
   RCWA2D.ComputeTCoefficients(PlaneWave) / ComputeRCoefficients(PlaneWave)  // RCWA/RCWA2D.cs
        ├─ RCWAHelper.PlaneWaveToCoefficients(pw,...)   // 入射 → 系数向量 cIn
        ├─ cOut = LinAlg.Dot(S11 或 S21, cIn)
        └─ RCWAHelper.Convert1DFieldTo2X2D(cOut,...)    // → (Ex, Ey) 二维谱
   (→ RCWAHelper.CoefficientToPlaneWave → PlaneWave.Sz/Poynting → 衍射效率，删减版在外部完成)
```

### 0.2 文件职责一览表

| 文件（相对 `VEMS/VEMS.EMSolver/`） | 类 | 在 RCWA2D 中的职责 |
|---|---|---|
| `RCWA/RCWA2D.cs` | `RCWA2D` | **顶层编排**：保存三层与采样参数；`ComputeHalfSMatrix` 触发求解；平面波系数包装 `ComputeT/RCoefficients` |
| `RCWA/RCWAHelper.cs` | `RCWAHelper` | **采样数决策**、`(Ex,Ey)↔1D向量`互转、`平面波↔模式系数`互转 |
| `EigenMode/Periodic2DLayer.cs` | `Periodic2DLayer` | **FMM 核心**：介质采样 `SampleMedium`、构造 `[F]/[G]`、本征分解得 `(gamma, W1, W2)` |
| `EigenMode/EigenHelper.cs` | `EigenHelper` | **傅里叶/Toeplitz 卷积矩阵**（Li 规则 `DirXDirY`/`InvXDirY`/`InvYDirX`）、`CheckGamma`、`GenerateKs` |
| `EigenMode/UniformLayer.cs` | `UniformLayer` | **均匀区解析模式**：`nz=kz/k0`、解析 `gamma`、`W1`、`W2`（参数 `wB,wC,wD`） |
| `EigenMode/Layer.cs` | `EigenLayer`(抽象基类) | 层公共属性 `Thickness, Gamma, W1, W2, LoopOption` |
| `SMatrix/SMatrix.cs` | `SMatrix` | **S 矩阵**：`HalfSMatrix` 编排、`HalfSMatrixBoundary` 界面、`HalfSMatrixKernel` 核（Redheffer/W⇒S） |
| `Media/Layer2DMedium.cs` | `Layer2DMedium` | 介质分布 `ε(λ,x,y)`，`Sample(...)` 采样到二维网格 |
| `Fields/PlaneWave.cs` | `PlaneWave` | 入射/出射平面波：`Kx,Ky,Kz`、本征向量参数 `Wb,Wc,Wd`、`Ex,Ey`、坡印亭 |
| `MathCore`：`MatrixZ/VectorZ/LinAlg/VMath/Transform/GridInfo2D` | — | 复矩阵/向量、本征分解 `EigenSystem`、求逆/线性解、`FFS1D`（傅里叶级数）、网格 |

⚠ **约定差异（务必注意）**：历史推导文档用的是 `∂ζΨ = i·A·Ψ`（4N×4N 的一阶系统）；**代码实际采用 F/G 双矩阵形式**，把问题压成 2N×2N 的二阶本征问题 `E = [F][G]`，本征值是 `(kz/k0)²`。两者数学等价，但矩阵分块、符号定义不同。下文以**代码的 F/G 形式为准**，并在每步注明它对应推导中的哪一物理环节。

---

## 1. 几何结构与入射条件

**数学**：真空波数 `k0 = 2π/λ`；横向中心波数 `kx0, ky0`（由入射角决定，外部传入）；双周期 `Λx, Λy` 给出倒格矢 `dKx=2π/Λx, dKy=2π/Λy`。

**代码**：
- 参数入口：`RCWA2D` 构造函数（`RCWA/RCWA2D.cs`，`Wavelength, Kx0, Ky0, LayerMiddle.PeriodX/Y`）。
- `k0` 计算：在 `Periodic2DLayer.ComputeModes` 与 `UniformLayer.ComputeModes` 中均为 `double k0 = 2.0*Math.PI/wavelength;`
- 倒格矢：`double dKx = 2.0*Math.PI/PeriodX; double dKy = 2.0*Math.PI/PeriodY;`（`Periodic2DLayer.ComputeModes` 开头、`SMatrix.HalfSMatrix` 内）
- ⚠ 入射角 `(θ,φ)` 并不在 RCWA 内部出现，而是由调用方折算成 `kx0,ky0`（以及 `PlaneWave.Kx,Ky`）后传入。

---

## 2. Floquet–Fourier 展开（横向波数与谱基）

**数学**：`kxm = kx0 + m·Kx`，`kyn = ky0 + n·Ky`；保留有限级次，归一化 `nx=kxm/k0, ny=kyn/k0`。

**代码**：`EigenHelper.GenerateKs(n, dk, kc)`（`EigenMode/EigenHelper.cs`）
```csharp
VectorD vk = new(count: n, initVal: -(n-1)/2*dk + kc, increment: dk);  // = kc + (i-(n-1)/2)·dk
```
- 在 `Periodic2DLayer.ComputeModes`：
  ```csharp
  VectorD kx = EigenHelper.GenerateKs(n: fieldsSamplingX, dk: dKx, kc: kx0);
  VectorD ky = EigenHelper.GenerateKs(n: fieldsSamplingY, dk: dKy, kc: ky0);
  VMath.ScaleOn(ref kx, 1.0/k0);   // kx ← nx = kxm/k0
  VMath.ScaleOn(ref ky, 1.0/k0);   // ky ← ny = kyn/k0
  ```
- **2D 谱展平为 1D**（行优先 `iy*nKx+ix`）得到长度 `n = nKxs·nKys` 的 `nx, ny` 向量：
  ```csharp
  for (iy...) for (ix...) { nx[iy*kx.Count+ix] = kx[ix]; ny[iy*kx.Count+ix] = ky[iy]; }
  ```
  对应数学里把 `(m,n)` 双指标排成单一向量；`diag(nx), diag(ny)` 即推导中的归一化 `K̃x, K̃y`。
- 级次总数决策：`RCWAHelper.DetermineSampling(...)`（`RCWA/RCWAHelper.cs`），按 k 域窗口或 x 域分辨率给出**奇数**采样数（保证含中心级次 0）。

---

## 3. 介电函数的傅里叶卷积矩阵（Li 规则）

**数学**：`ε(x,y)` 与 `1/ε` 各自做二维傅里叶展开，构成**分块 Toeplitz**卷积矩阵 `[ε]`、`[η]=[1/ε]`。Li(1997) 法则：连续分量用 `[ε]`（"direct rule"），跨界面突变分量用逆规则（"inverse rule"）。

**代码**：`EigenMode/EigenHelper.cs` —— 这是 Li 规则的具体落地，三个方法对应三种乘法卷积矩阵：

| 方法 | 含义（沿 x / 沿 y 的规则） | 对应数学对象 |
|---|---|---|
| `DirXDirY(f, nKxs, nKys)` | x、y 都用**直接规则** | `[[f]]`（如 `[ε]`） |
| `InvXDirY(f, nKxs, nKys)` | x 用**逆规则**、y 用直接规则 | `⟦1/ε⟧ₓ` 型混合卷积 |
| `InvYDirX(f, nKxs, nKys)` | y 用**逆规则**、x 用直接规则 | `⟦1/ε⟧_y` 型混合卷积 |

底层构件：
- `MakeToeplitzMatrix(VectorZ)`：由一维傅里叶系数生成单块 Toeplitz 矩阵。
- `ToeplitzValue(input, nKxs, type)`：按 `2·nKxs-1` 截断/补零（`Nonperiodic`）或周期延拓（`Periodic`）。
- `Transform.FFS1D(ref v, isForward:true)`（`MathCore/Transform/Transform.cs`）：把空域采样的 `ε`/`1/ε` 切片做**傅里叶级数变换**得到系数，再排成 Toeplitz。
- 逆规则的关键在于：`InvXDirY/InvYDirX` 中先对 `1/f` 做 FFS、生成 Toeplitz、再 `LinAlg.Inverse` 求逆——即"先取倒数的卷积再求逆"，这正是 Li 逆规则。

⚠ **本仓库默认 `ToeplitzMatrixType.Nonperiodic`**（见 `ComputeModes` 内 `FG(..., ToeplitzMatrixType.Nonperiodic)`）。

---

## 4. F / G 矩阵构造（Maxwell 横向方程的傅里叶化）

**数学（代码采用的 F/G 形式）**：消去 `Ez,Hz` 后，横向电磁场满足耦合方程，可写成
`[E_t]'' 型本征问题 → E = [F][G]`，本征值为 `(kz/k0)²`。`[F]` 来自磁场驱动电场（含 `[ε]⁻¹` 即逆介电卷积），`[G]` 来自电场驱动磁场（含 `[ε]` 卷积）。这与历史推导中的 `P`(含 `[η]`) / `Q`(含 `[ε]`) 子块**物理同源**。

**代码**：`Periodic2DLayer.FG(VectorD kx, VectorD ky, MatrixZ epsilon, MatrixZ? mu, ...)`（`EigenMode/Periodic2DLayer.cs`）

先求逆介电卷积：
```csharp
MatrixZ epsilonDxDy = EigenHelper.DirXDirY(epsilon, kx.Count, ky.Count, Nonperiodic);
LinAlg.Inverse(ref epsilonDxDy);     // [ε]⁻¹  （供 [F] 使用）
```

**[F] 分块**（`n=nKxs·nKys`，`nx,ny` 为对角化的归一化波数；`LinAlg.DiagonalMatrixHelper.Dot` 表示"对角阵×矩阵"或"矩阵×对角阵"）：
```csharp
Omega13 = diag(nx) · [ε]⁻¹ · diag(ny)
Omega14 = -diag(nx) · [ε]⁻¹ · diag(nx) + I
Omega23 =  diag(ny) · [ε]⁻¹ · diag(ny) - I
Omega24 = -diag(ny) · [ε]⁻¹ · diag(nx)
F = [[Omega13, Omega14],
     [Omega23, Omega24]]
```
对应历史推导的 `P` 子块：`±(K̃·[η]·K̃ − I)`，符号/转置随 F/G 约定。

**[G] 分块**（含 `[ε]` 直接卷积与逆规则混合卷积 `InvYDirX/InvXDirY`）：
```csharp
Omega31 = -diag(nx·ny)
Omega32 = -InvYDirX(epsilon,...) + diag(nx²)        // ≈ K̃x² - [ε]_eff
Omega41 =  InvXDirY(epsilon,...) - diag(ny²)        // ≈ -(K̃y² - [ε]_eff)
Omega42 =  diag(ny·nx)
G = [[Omega31, Omega32],
     [Omega41, Omega42]]
```
对应历史推导的 `Q` 子块：`K̃² − [ε]` 与 `−K̃·K̃` 交叉项。

**物理对应**：`[F]` ←→ "磁场如何生成电场"（出现 `1/ε`，因为 `Ez ∝ (1/ε)(∇×H)_z`，即历史推导中 `e�z = -(1/ωε₀)[η](…)` 的来源）；`[G]` ←→ "电场如何生成磁场"（直接含 `ε`）。

---

## 5. 本征值问题与层内模式 (gamma, W1, W2)

**数学**：`E·Ψ₀ = (kz/k0)²·Ψ₀`；传播常数 `γ = i·k0·(kz/k0) = i·kz`；前/后向由 `γ` 实虚部分支选取（避免指数增长）。`W1` 给电场模式分布，`W2` 给对应磁场模式。

**代码**：`Periodic2DLayer.ComputeModes(...)`（`EigenMode/Periodic2DLayer.cs`，FMM 段）
```csharp
(MatrixZ F, MatrixZ G) = FG(kx, ky, epsilon, mu, Nonperiodic);
MatrixZ E = LinAlg.Dot(F, G);                                  // 本征矩阵 E=[F][G]
LinAlg.EigenSystem(ref E, out VectorZ eigenValues, out MatrixZ eigenVectors);
VectorZ gamma = Complex.ImaginaryOne * k0 * VMath.Sqrt(eigenValues);   // γ = i·k0·√λ
EigenHelper.CheckGamma(ref gamma);                             // 选择物理分支
MatrixZ w1 = eigenVectors;                                     // W1 = 电场模式
MatrixZ w2 = -Complex.ImaginaryOne / k0 * LinAlg.DiagonalMatrixHelper.Dot(w1, gamma);
LinAlg.LinearSolve(ref F, ref w2);                             // w2 ← F⁻¹ · (-i/k0 · W1·diag(γ))
```
- `eigenValues` = `(kz/k0)²`；`VMath.Sqrt` 取 `kz/k0`；`gamma = i·k0·(kz/k0) = i·kz`。
- **分支选择** `EigenHelper.CheckGamma`：若 `Re(γ)>0` 取反（强制衰减/能流方向一致）；`|Re(γ)|≈0` 且 `Im(γ)<0` 也取反。即历史推导中"前向 Λ⁺ / 后向 Λ⁻"的代码实现。
- `W2 = F⁻¹ (-i/k0) W1 diag(γ)`：由电场模式 `W1` 推出磁场模式 `W2`，对应推导中"由 e 推 h"。

**均匀区对照**：`UniformLayer.ComputeModes(...)`（`EigenMode/UniformLayer.cs`）**无需数值分解**，解析给出：
```csharp
nz = sqrt(εμ - nx² - ny²);   gamma = i·k0·nz;     // 每个级次独立
// W1 = I (对角)；W2 由 wB,wC,wD 组成 2×2 子块：
wB = nx·ny/nz;  wC = nz + nx²/nz;  wD = nz + ny²/nz;
w2[i,i]=-wB; w2[i,n+i]=-wD; w2[n+i,i]=wC; w2[n+i,n+i]=wB;
```
这对应历史推导第 6 节"均匀区各级次独立传播"，`nz=kz/k0` 即纵向归一化波数。

---

## 6. 入射 / 透射区与平面波 ↔ 系数

**数学**：入射波是单一中心级次 `(0,0)`；反射/透射为各级次平面波叠加；振幅按周期归一化。

**代码**：`RCWA/RCWAHelper.cs`
- `PlaneWaveToCoefficients(PlaneWave pw, periodX, periodY, nx, ny)`：把入射平面波放到谱中心 `((ny-1)/2,(nx-1)/2)`，振幅乘 `scal = periodX·periodY/(2π)`，并返回 k 域网格 `GridInfo2D`。
- `Convert2X2DFieldTo1D(Ex,Ey)` / `Convert1DFieldTo2X2D(v,nRow,nCol)`：在 `[Ex;Ey]` 拼接的 `2n` 长向量与两张二维谱之间互转（行优先），与 F/G 中 `up/down` 分块一致。
- `CoefficientToPlaneWave(cEx,cEy,...)`：把输出系数还原为带 `Kx,Ky,Kz` 与振幅的 `PlaneWave`（振幅乘 `scal = 2π/(periodX·periodY)`，与上面互逆）。
- `Fields/PlaneWave.cs`：`Kz`、本征参数 `Wb,Wc,Wd`、`Ex,Ey`、坡印亭——是**衍射效率**的最终载体。

---

## 7. 边界条件与 S 矩阵递推（Redheffer）

**数学**：界面处切向场连续；用 S 矩阵级联（Redheffer 星积）保证大厚度数值稳定（后向波参考点放在层末端，避免 `exp` 溢出）。

**代码**：`SMatrix/SMatrix.cs`
- **编排**：`SMatrix.HalfSMatrix(wavelength, UniformLayer front, Periodic2DLayer middle, UniformLayer behind, kx0,ky0, fieldsSampling*, mediumSampling*, ...)`
  ```csharp
  s11 = I(2n);  s21 = 0;
  (gammaBehind,w1Behind,w2Behind) = layerBehind.ComputeModes(...);
  (gammaMiddle,w1Middle,w2Middle) = layerMiddle.ComputeModes(...);
  (_,w1Front,w2Front)            = layerFront.ComputeModes(...);
  HalfSMatrixBoundary(... behind→middle, layerBehind.Thickness ...);
  HalfSMatrixBoundary(... middle→front,  layerMiddle.Thickness ...);
  return (s11, s21);
  ```
- **界面**：`HalfSMatrixBoundary(ref s11, ref s21, gammaLast, w1Last, w2Last, w1, w2, tLast, useWSvariation)`
  - 传播相位（稳定形式，仅用衰减指数）：`p11 = exp(gammaLast · tLast)`；
  - 辅助量 `Q1 = (W1)⁻¹`，`Q2 = (W2)⁻¹·W2Last`；
  - 调用核 `HalfSMatrixKernel`。
- **核**：`HalfSMatrixKernel(ref s11, ref s21, q1, q2, p11, useWSvariation)` 两种等价变体：
  - `useWSvariation=false`（默认，**W⇒t⇒S**）：
    ```
    t11=(q1+q2)/2;  t12=(q1-q2)/2;
    t1 = (t11 + t12·s21)⁻¹;
    s21 ← (t12 + t11·s21)·t1;
    s11 ← (diag(p11)·s11)·t1;
    ```
    这是 Redheffer 星积的传输矩阵实现。
  - `useWSvariation=true`（**W⇒S** 直接变体）：用 `τ=(q1·(I+s21)+q2·(I−s21))⁻¹` 更新，数学等价、数值路径不同。
- ⚠ 删减版只保留**半 S 矩阵**（`S11,S21`，即正向输入→正向/反向输出）。`FullSMatrix`/`FullSMatrixKernel`（含 `S12,S22`）也在文件中，但 `RCWA2D` 顶层只调 `HalfSMatrix`。

---

## 8. 衍射效率（坡印亭功率比）

**数学**：`η_mn = |系数|² · Re(kz,mn)/kz,00`，反射用入射区 `kz^I`、透射用透射区 `kz^II`，分母统一 `kz,00^I`；消逝波 `Re(kz)=0 ⇒ η=0`；无损耗时 `Σηref+Σηtr=1`。

**代码（删减版现状）**：
- 输出系数在 `RCWA2D.ComputeTCoefficients(PlaneWave)` / `ComputeRCoefficients(PlaneWave)` 得到 `(cEx,cEy,grid)`：
  ```csharp
  (cEx,cEy,g) = RCWAHelper.PlaneWaveToCoefficients(pw, ...);
  cIn  = RCWAHelper.Convert2X2DFieldTo1D(cEx,cEy);
  cOut = LinAlg.Dot(S11 或 S21, cIn);              // S11→透射T, S21→反射R
  (cEx,cEy) = RCWAHelper.Convert1DFieldTo2X2D(cOut, NKys, NKxs);
  ```
- 由系数到带 `Kz` 的平面波：`RCWAHelper.CoefficientToPlaneWave(...)` → `PlaneWave`（`Fields/PlaneWave.cs` 提供 `Kz` 与坡印亭）。
- ⚠ **`Re(kz)/kz,00` 的功率归一与求和守恒检验**在本删减版的 `EMSolver` 内**未见独立方法**（`grep` 未命中 `efficiency/Poynting/Sz` 的专用计算器）；最末一步效率值预期在 `PlaneWave` 的坡印亭分量上、由调用方（或被删减的后处理）完成。**这是删减版与完整推导之间唯一"断点"，后续工作如需效率应在此补齐。**

---

## 9. 数学符号 → 代码标识 速查表

| 数学符号 | 代码标识 | 位置 |
|---|---|---|
| `k0 = 2π/λ` | `k0 = 2.0*Math.PI/wavelength` | `*Layer.ComputeModes` |
| `Kx=2π/Λx, Ky=2π/Λy` | `dKx, dKy` | `Periodic2DLayer`, `SMatrix.HalfSMatrix` |
| `kxm, kyn` | `kx, ky`（`GenerateKs`） | `EigenHelper.GenerateKs` |
| `nx=kxm/k0, ny=kyn/k0` | `kx,ky`（`ScaleOn(1/k0)` 后）→展平 `nx,ny` | `Periodic2DLayer.ComputeModes` |
| `[ε]` 卷积矩阵 | `DirXDirY(epsilon,...)` | `EigenHelper.DirXDirY` |
| `[η]=[1/ε]` / 逆规则 | `epsilonDxDy⁻¹`, `InvXDirY`, `InvYDirX` | `EigenHelper`, `Periodic2DLayer.FG` |
| `P/Q` 子块（推导）≈ `F/G`（代码） | `Omega13..24`(F), `Omega31..42`(G) | `Periodic2DLayer.FG` |
| 本征值 `(kz/k0)²` | `eigenValues` | `Periodic2DLayer.ComputeModes` |
| `γ = i·kz` | `gamma = i*k0*sqrt(eigenValues)` | 同上 + `CheckGamma` |
| `W⁺/W⁻`（模式向量） | `w1`(电), `w2`(磁) | `*Layer.ComputeModes` |
| `nz=kz/k0`（均匀区） | `nz` | `UniformLayer.ComputeNz/ComputeModes` |
| 均匀区模式参数 | `wB,wC,wD` | `UniformLayer.ComputeModes` |
| 传播相位 `exp(i k0 Λ z)` | `p11 = exp(gammaLast*tLast)` | `SMatrix.HalfSMatrixBoundary` |
| Redheffer 星积 | `HalfSMatrixKernel`（t11,t12,t1） | `SMatrix.HalfSMatrixKernel` |
| 反射/透射系数 `R/T` | `S21·cIn` / `S11·cIn` | `RCWA2D.ComputeR/TCoefficients` |

---

## 10. 给后续 AI 工作的提示

1. **改物理 / 加公式** → 多半落在 `Periodic2DLayer.FG`（F/G 分块）与 `EigenHelper`（卷积规则）。
2. **改数值稳定性 / S 矩阵** → `SMatrix.HalfSMatrixKernel`（`useWSvariation` 切换两条路径）。
3. **改收敛性（Li 规则选择）** → `EigenHelper.DirXDirY/InvXDirY/InvYDirX` 的调用搭配，与 `ToeplitzMatrixType`。
4. **补衍射效率** → 在 `RCWA2D.ComputeR/TCoefficients` 之后，用 `CoefficientToPlaneWave` + `PlaneWave.Kz` 实现 `|·|²·Re(kz)/kz00` 与能量守恒检验（删减版缺口）。
5. **有两套 `ComputeModes`**：新版（`fieldsSampling*/mediumSampling*` + `FG`）与 `[Obsolete]` 旧版（内联 F/G）。**以新版为准**；旧版仅供对照，勿在其上扩展。
6. 线性代数全部走 `MathCore` 的 `LinAlg`/`VMath`（`Dot, Inverse, LinearSolve, EigenSystem`）与 `Transform.FFS1D`；新功能应复用这些，保持 `MatrixZ/VectorZ` 类型一致。
