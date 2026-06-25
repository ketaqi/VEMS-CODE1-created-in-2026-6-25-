using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace OpticalChainSimulator.Services;



/// <summary>
/// 简化的一维光学链路仿真器核心类
/// 核心功能：
/// 1. 基于ABCD矩阵的光学链路传输计算；
/// 2. 高斯光束q参数的传输仿真；
/// 3. 计算链路中任意位置的光束半径、波前曲率、功率等参数；
/// 4. 生成ASCII格式的光路描述文本。
/// </summary>
public class OpticalSimulator
{
    /// <summary>
    /// 仿真全局设置类
    /// 存储仿真过程中的通用配置参数
    /// </summary>
    public class SimulationSettings
    {
        /// <summary>
        /// 光学元件之间的默认间距（单位：mm）
        /// 仅用于几何z轴插值计算，若链路中包含FreeSpace元件，优先使用其自身Length参数
        /// </summary>
        public double DefaultElementSpacing { get; set; } = 100.0;

        /// <summary>
        /// 是否将反射镜等效为薄透镜参与ABCD矩阵计算（等效焦距F=R/2，R为反射镜曲率半径）
        /// </summary>
        public bool TreatMirrorAsThinLens { get; set; } = true;
    }

    /// <summary>
    /// 光束状态类
    /// 存储链路中某一位置的光束完整参数
    /// </summary>
    public class BeamState
    {
        /// <summary>当前位置（单位：mm）</summary>
        public double Z { get; set; }

        /// <summary>当前位置的光束功率（单位：W）</summary>
        public double Power { get; set; }

        /// <summary>光束波长（单位：nm）</summary>
        public double Wavelength { get; set; }

        /// <summary>当前位置的光束半径w(z)（单位：mm）</summary>
        public double BeamRadiusW { get; set; }

        /// <summary>当前位置的波前曲率半径R(z)（单位：mm）</summary>
        public double WavefrontRadiusR { get; set; }

        /// <summary>累积损耗因子（0~1，1表示无损耗）</summary>
        public double LossFactor { get; set; }

        /// <summary>当前状态的文本描述信息</summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 仿真结果封装类
    /// 包含完整的仿真输出数据
    /// </summary>
    public class SimulationResult
    {
        /// <summary>链路中各位置的光束状态列表</summary>
        public List<BeamState> States { get; } = new();

        /// <summary>ASCII格式的光路描述文本</summary>
        public string AsciiDescription { get; set; } = string.Empty;

        /// <summary>详细的仿真报告文本</summary>
        public string DetailedReport { get; set; } = string.Empty;

        /// <summary>探测器位置的光束状态（若链路包含探测器）</summary>
        public BeamState? DetectorState { get; set; }
    }

    /// <summary>
    /// 执行完整的光学链路仿真
    /// </summary>
    /// <param name="elements">光学元件列表（必须包含至少一个Source类型元件）</param>
    /// <param name="settings">仿真配置（为null时使用默认配置）</param>
    /// <returns>封装了所有仿真结果的SimulationResult对象</returns>
    public SimulationResult Run(IReadOnlyList<OpticalElement> elements, SimulationSettings? settings = null)
    {
        // 使用默认配置或传入的配置
        var s = settings ?? new SimulationSettings();
        var result = new SimulationResult();

        // 校验：元件列表为空时直接返回结果
        if (elements.Count == 0)
        {
            result.DetailedReport = "链路为空，无法执行仿真。";
            return result;
        }

        // 从第一个Source元件获取初始光束参数
        var firstSource = elements.FirstOrDefault(e => e.Type == OpticalElementType.Source);
        if (firstSource == null)
        {
            result.DetailedReport = "链路中未找到光源（Source），无法执行高斯光束仿真。";
            return result;
        }

        // 提取光源参数（无参数时使用默认值）
        firstSource.Parameters.TryGetValue("Power", out var power);
        firstSource.Parameters.TryGetValue("Wavelength", out var lambdaNm);
        firstSource.Parameters.TryGetValue("BeamWaist", out var w0);
        firstSource.Parameters.TryGetValue("BeamQualityM2", out var m2);
        if (m2 <= 0) m2 = 1.0; // M²默认值为1（理想高斯光束）

        if (w0 <= 0)
            w0 = 0.5; // 束腰半径默认值：0.5 mm
        if (lambdaNm <= 0)
            lambdaNm = 632.8; // 波长默认值：632.8 nm（He-Ne激光器）

        // 单位转换：nm -> mm
        var lambdaMm = lambdaNm * 1e-6;

        // 初始q参数计算（束腰处波前曲率半径R=∞，1/q = -i * λ / (π w0²)）
        var q = new Complex(0, -Math.PI * w0 * w0 / (lambdaMm * m2));

        // 初始化仿真变量
        double z = 0.0;               // 当前z轴位置
        double cumulativeLoss = 1.0;  // 累积损耗因子
        double currentPower = power;  // 当前位置功率

        // 构建详细报告
        var sbReport = new StringBuilder();
        sbReport.AppendLine("光学链路仿真报告");
        sbReport.AppendLine("----------------");
        sbReport.AppendLine($"初始光源: Power={power} W, λ={lambdaNm} nm, w0={w0} mm, M^2={m2}");
        sbReport.AppendLine();

        // 构建ASCII光路描述
        var ascii = new StringBuilder();
        ascii.Append("Source");

        // 探测器位置的光束状态（初始化）
        BeamState? detectorState = null;

        // 遍历所有光学元件，逐段计算光束传输
        for (int i = 0; i < elements.Count; i++)
        {
            var el = elements[i];

            // 计算当前元件与上一个元件之间的自由空间/默认间距的ABCD矩阵
            if (i > 0)
            {
                double segLength = s.DefaultElementSpacing;
                // 若上一个元件是FreeSpace，使用其自身长度参数
                if (elements[i - 1].Type == OpticalElementType.FreeSpace &&
                    elements[i - 1].Parameters.TryGetValue("Length", out var len) &&
                    len > 0)
                {
                    segLength = len;
                }

                // 自由空间ABCD矩阵：[[1, L], [0, 1]]
                var abcdFree = CreateFreeSpaceMatrix(segLength);
                // 传输q参数
                q = PropagateABCD(q, abcdFree);
                // 更新z轴位置
                z += segLength;
                // 追加ASCII描述
                ascii.Append($" --[L={segLength}]-> ");
            }

            // 追加当前元件类型到ASCII描述
            ascii.Append($"{el.Type}");

            // 处理元件自身的ABCD矩阵对q参数的影响
            switch (el.Type)
            {
                case OpticalElementType.Lens:
                    // 薄透镜ABCD矩阵计算
                    if (el.Parameters.TryGetValue("FocalLength", out var f) && Math.Abs(f) > 1e-9)
                    {
                        var abcdLens = CreateThinLensMatrix(f);
                        q = PropagateABCD(q, abcdLens);
                    }
                    break;
                case OpticalElementType.Mirror when s.TreatMirrorAsThinLens:
                    // 反射镜等效为薄透镜（F=R/2）
                    if (el.Parameters.TryGetValue("RadiusOfCurvature", out var roc) && Math.Abs(roc) > 1e-9)
                    {
                        var m = roc / 2.0;
                        var abcdLens = CreateThinLensMatrix(m);
                        q = PropagateABCD(q, abcdLens);
                    }
                    break;
                case OpticalElementType.BeamExpander:
                    // 扩束器：按放大倍数调整q参数
                    if (el.Parameters.TryGetValue("ExpanderMagnification", out var mag) && mag > 0)
                    {
                        q *= mag;
                    }
                    break;
            }

            // 计算当前位置的光束半径w(z)和波前曲率R(z)
            var (wz, rz) = ComputeBeamParameters(q, lambdaMm, m2);

            // 计算当前元件的损耗
            double localLoss = 1.0;
            // 通用损耗参数
            if (el.Parameters.TryGetValue("Loss", out var lossValue) && lossValue >= 0 && lossValue <= 1)
                localLoss *= lossValue;
            // 光阑透过率损耗
            if (el.Type == OpticalElementType.Aperture &&
                el.Parameters.TryGetValue("ApertureTransmission", out var at) && at >= 0 && at <= 1)
            {
                localLoss *= at;
            }
            // 滤光片透过率损耗
            if (el.Type == OpticalElementType.Filter &&
                el.Parameters.TryGetValue("FilterTransmission", out var ft) && ft >= 0 && ft <= 1)
            {
                localLoss *= ft;
            }

            // 更新累积损耗和当前功率
            cumulativeLoss *= localLoss;
            currentPower = power * cumulativeLoss;

            // 构建当前位置的光束状态
            var state = new BeamState
            {
                Z = z,
                Power = currentPower,
                Wavelength = lambdaNm,
                BeamRadiusW = wz,
                WavefrontRadiusR = rz,
                LossFactor = cumulativeLoss,
                Description = BuildElementDescription(el, z, wz, rz, currentPower, cumulativeLoss)
            };

            // 添加到结果列表
            result.States.Add(state);
            sbReport.AppendLine(state.Description);
            sbReport.AppendLine();

            // 记录探测器位置的状态
            if (el.Type == OpticalElementType.Detector)
            {
                detectorState = state;
            }
        }

        // 填充仿真结果
        result.AsciiDescription = ascii.ToString();
        result.DetailedReport = sbReport.ToString();
        result.DetectorState = detectorState;

        return result;
    }

    /// <summary>
    /// 根据q参数计算光束半径w(z)和波前曲率半径R(z)
    /// 公式：1/q = 1/R - iλ/(πw²M²)
    /// </summary>
    /// <param name="q">高斯光束复参数q</param>
    /// <param name="lambdaMm">波长（单位：mm）</param>
    /// <param name="m2">光束质量因子M²</param>
    /// <returns>元组(光束半径w, 波前曲率半径R)</returns>
    private static (double w, double R) ComputeBeamParameters(Complex q, double lambdaMm, double m2)
    {
        var invQ = 1.0 / q;
        var invQReal = invQ.Real;   // 1/R
        var invQImag = invQ.Imaginary; // -λ/(πw²M²)

        double w = double.NaN;
        double R = double.NaN;

        // 计算光束半径w
        if (Math.Abs(invQImag) > 1e-12)
        {
            w = Math.Sqrt(-lambdaMm / (Math.PI * m2 * invQImag));
        }

        // 计算波前曲率半径R
        if (Math.Abs(invQReal) > 1e-12)
        {
            R = 1.0 / invQReal;
        }

        return (w, R);
    }

    /// <summary>
    /// 创建自由空间的ABCD传输矩阵
    /// 矩阵形式：[[1, L], [0, 1]]
    /// </summary>
    /// <param name="lengthMm">自由空间长度（单位：mm）</param>
    /// <returns>2x2复数矩阵</returns>
    private static ComplexMatrix2x2 CreateFreeSpaceMatrix(double lengthMm)
    {
        return new ComplexMatrix2x2(
            1.0, lengthMm,
            0.0, 1.0);
    }

    /// <summary>
    /// 创建薄透镜的ABCD传输矩阵
    /// 矩阵形式：[[1, 0], [-1/F, 1]]
    /// </summary>
    /// <param name="focalLengthMm">透镜焦距（单位：mm）</param>
    /// <returns>2x2复数矩阵</returns>
    private static ComplexMatrix2x2 CreateThinLensMatrix(double focalLengthMm)
    {
        return new ComplexMatrix2x2(
            1.0, 0.0,
            -1.0 / focalLengthMm, 1.0);
    }

    /// <summary>
    /// 基于ABCD矩阵传输高斯光束q参数
    /// 传输公式：q' = (Aq + B) / (Cq + D)
    /// </summary>
    /// <param name="q">入射光束的q参数</param>
    /// <param name="m">ABCD传输矩阵</param>
    /// <returns>出射光束的q参数</returns>
    private static Complex PropagateABCD(Complex q, ComplexMatrix2x2 m)
    {
        var numerator = m.A * q + m.B;
        var denominator = m.C * q + m.D;

        return numerator / denominator;
    }

    /// <summary>
    /// 构建光学元件的文本描述信息
    /// </summary>
    /// <param name="el">光学元件</param>
    /// <param name="z">元件位置（单位：mm）</param>
    /// <param name="w">光束半径（单位：mm）</param>
    /// <param name="R">波前曲率半径（单位：mm）</param>
    /// <param name="power">当前功率（单位：W）</param>
    /// <param name="cumulativeLoss">累积损耗因子</param>
    /// <returns>格式化的描述文本</returns>
    private static string BuildElementDescription(
        OpticalElement el,
        double z,
        double w,
        double R,
        double power,
        double cumulativeLoss)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"位置 z = {z:F2} mm: {el.Type} ({el.Name})");

        // 根据元件类型补充特有参数
        switch (el.Type)
        {
            case OpticalElementType.Source:
                el.Parameters.TryGetValue("Power", out var p);
                el.Parameters.TryGetValue("Wavelength", out var lambda);
                el.Parameters.TryGetValue("BeamWaist", out var w0);
                sb.AppendLine($"  光源参数: 功率 = {p:F4} W, 波长 = {lambda:F1} nm, 束腰半径 = {w0:F3} mm");
                break;
            case OpticalElementType.Mirror:
                el.Parameters.TryGetValue("Angle", out var a);
                el.Parameters.TryGetValue("RadiusOfCurvature", out var roc);
                el.Parameters.TryGetValue("RadiusOfCurvature1", out var roc1);
                sb.AppendLine($"  反射镜参数: 角度 = {a:F1} °, 曲率半径 R = {roc:F1} mm, R1 = {roc1:F1} mm");
                break;
            case OpticalElementType.Lens:
                el.Parameters.TryGetValue("FocalLength", out var f);
                sb.AppendLine($"  透镜参数: 焦距 = {f:F1} mm");
                break;
            case OpticalElementType.FreeSpace:
                el.Parameters.TryGetValue("Length", out var l);
                el.Parameters.TryGetValue("RefractiveIndex", out var n);
                sb.AppendLine($"  自由空间参数: 长度 = {l:F1} mm, 折射率 n = {n:F3}");
                break;
            case OpticalElementType.Aperture:
                el.Parameters.TryGetValue("ApertureDiameter", out var ad);
                el.Parameters.TryGetValue("ApertureShapeCode", out var asc);
                el.Parameters.TryGetValue("ApertureTransmission", out var at);
                sb.AppendLine($"  光阑参数: 直径 = {ad:F1} mm, 形状码 = {asc}, 透过率 = {at:F3}");
                break;
            case OpticalElementType.Filter:
                el.Parameters.TryGetValue("FilterCenterWavelength", out var fcw);
                el.Parameters.TryGetValue("FilterBandwidth", out var fbw);
                el.Parameters.TryGetValue("FilterTransmission", out var ft);
                sb.AppendLine($"  滤光片参数: 中心波长 = {fcw:F1} nm, 带宽 = {fbw:F1} nm, 透过率 = {ft:F3}");
                break;
            case OpticalElementType.Detector:
                el.Parameters.TryGetValue("DetectorWidth", out var dw);
                el.Parameters.TryGetValue("DetectorHeight", out var dh);
                sb.AppendLine($"  探测器参数: 尺寸 = {dw:F1}×{dh:F1} mm");
                break;
            case OpticalElementType.BeamExpander:
                el.Parameters.TryGetValue("ExpanderMagnification", out var em);
                el.Parameters.TryGetValue("ExpanderInputDiameter", out var eid);
                el.Parameters.TryGetValue("ExpanderOutputDiameter", out var eod);
                sb.AppendLine($"  扩束器参数: 放大倍数 M = {em:F1}, 输入直径 = {eid:F1} mm, 输出直径 = {eod:F1} mm");
                break;
        }

        // 通用光束参数
        sb.AppendLine($"  光束半径 w(z) = {w:F4} mm");
        sb.AppendLine($"  波前曲率半径 R(z) = {R:F1} mm");
        sb.AppendLine($"  功率 = {power:F4} W");
        sb.AppendLine($"  累积损耗因子 = {cumulativeLoss:F4}");

        return sb.ToString();
    }
}

/// <summary>
/// 2x2复数矩阵结构体
/// 专用于光学ABCD矩阵运算，包含矩阵乘法重载
/// </summary>
public readonly struct ComplexMatrix2x2
{
    /// <summary>矩阵第一行第一列元素</summary>
    public Complex A { get; }

    /// <summary>矩阵第一行第二列元素</summary>
    public Complex B { get; }

    /// <summary>矩阵第二行第一列元素</summary>
    public Complex C { get; }

    /// <summary>矩阵第二行第二列元素</summary>
    public Complex D { get; }

    /// <summary>构造2x2复数矩阵</summary>
    /// <param name="a">A元素（1,1）</param>
    /// <param name="b">B元素（1,2）</param>
    /// <param name="c">C元素（2,1）</param>
    /// <param name="d">D元素（2,2）</param>
    public ComplexMatrix2x2(Complex a, Complex b, Complex c, Complex d)
    {
        A = a;
        B = b;
        C = c;
        D = d;
    }

    /// <summary>
    /// 重载矩阵乘法运算符
    /// 实现两个2x2复数矩阵的乘法
    /// </summary>
    /// <param name="m1">左矩阵</param>
    /// <param name="m2">右矩阵</param>
    /// <returns>乘法结果矩阵</returns>
    public static ComplexMatrix2x2 operator *(ComplexMatrix2x2 m1, ComplexMatrix2x2 m2)
    {
        var a = m1.A * m2.A + m1.B * m2.C;
        var b = m1.A * m2.B + m1.B * m2.D;
        var c = m1.C * m2.A + m1.D * m2.C;
        var d = m1.C * m2.B + m1.D * m2.D;
        return new ComplexMatrix2x2(a, b, c, d);
    }
}