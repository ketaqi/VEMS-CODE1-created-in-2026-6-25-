using System;
using System.Globalization;
using System.Numerics;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using HarfBuzzSharp;
using OpticalChainSimulator.yuanjianku;

namespace OpticalChainSimulator
{
    /// <summary>
    /// 光轴反射演示主视图逻辑类
    /// 核心职责：
    /// 1. 管理3D视口（SkiaViewport）的交互逻辑（点击/双击元素）
    /// 2. 光学元件（镜面、透镜、探测器等）的创建/删除/属性修改
    /// 3. 场景更新与渲染刷新
    /// 4. 与ViewModel的事件通信（元素创建、选中、属性变更）
    /// </summary>
    public partial class MainView : UserControl
    {
        /// <summary>
        /// 光学元件类型枚举
        /// </summary>
        public enum ElemType
        {
            /// <summary>反射镜</summary>
            Mirror,
            /// <summary>薄透镜</summary>
            Lens,
            /// <summary>探测器</summary>
            Detector,
            /// <summary>光学盒子</summary>
            Box,
            /// <summary>端口盒</summary>
            PortBox
        }

        /// <summary>
        /// 选中元素的标识结构体（类型+索引）
        /// </summary>
        private struct Sel
        {
            /// <summary>元件类型</summary>
            public ElemType Type;
            /// <summary>元件在对应集合中的索引</summary>
            public int Index;
            /// <summary>重写ToString，便于调试显示</summary>
            public override string ToString() => $"{Type} {Index + 1}";
        }

        /// <summary>场景中所有可选元件的列表</summary>
        private readonly System.Collections.Generic.List<Sel> _items = new();
        /// <summary>当前选中元素的索引（单一数据源）</summary>
        private int _currentSelectedIndex = -1;

        #region 事件声明
        /// <summary>
        /// 元素创建事件：视图创建场景对象时触发，外部订阅以同步到ViewModel
        /// </summary>
        public event Action<OpticalElement>? ElementCreated;

        /// <summary>
        /// 场景对象单击事件：通知宿主选中的元件类型、索引、唯一ID
        /// </summary>
        public event Action<OpticalChainSimulator.Models.OpticalElementType, int, Guid>? SceneObjectClicked;

        /// <summary>
        /// 场景对象双击事件：通知宿主选中的元件类型、索引、唯一ID
        /// </summary>
        public event Action<OpticalChainSimulator.Models.OpticalElementType, int, Guid>? SceneObjectDoubleClicked;
        #endregion

        #region 构造函数
        /// <summary>
        /// 主视图构造函数
        /// </summary>
        public MainView()
        {
            InitializeComponent();

            // 确保场景至少包含2个反射镜以演示光轴反射
            EnsureTwoBounceDemo(Viewport.Scene);

            // 绑定视口单击事件
            if (Viewport != null)
            {
                SceneClickHelper.BindViewportClick(
                    attachClick: handler => Viewport.ObjectClicked += handler,
                    getScene: () => Viewport.Scene,
                    callback: (mappedType, index, elementId) => SceneObjectClicked?.Invoke(mappedType, index, elementId)
                );

                // 绑定视口双击事件（0108测试新增）
                SceneClickHelper.BindViewportDoubleClick(
                    attachDoubleClick: h => Viewport.ObjectDoubleClicked += h,
                    getScene: () => Viewport.Scene,
                    callback: (elementType, index, elementId) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"双击了 {elementType}，索引：{index}，ID：{elementId}");
                        SceneObjectDoubleClicked?.Invoke(elementType, index, elementId);
                    }
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Viewport控件初始化失败");
            }
        }
        #endregion

        #region 公共基础方法
        /// <summary>
        /// 供外部调用的列表刷新方法（兼容原有逻辑，内部未实现具体刷新）
        /// </summary>
        /// <param name="selectLast">是否选中最后一个元素</param>
        public void RefreshListPublic(bool selectLast)
        {
            // 预留扩展：可在此实现元件列表刷新逻辑
        }

        /// <summary>
        /// 获取当前选中的元素信息
        /// </summary>
        /// <returns>选中元素结构体 + 是否有效标识</returns>
        private (Sel sel, bool ok) GetCurrentSel()
        {
            int i = _currentSelectedIndex;
            if (i < 0 || i >= _items.Count) return (default, false);
            return (_items[i], true);
        }
        #endregion

        #region 场景初始化辅助方法
        /// <summary>
        /// 确保场景包含至少2个反射镜，并自动计算光轴反射路径
        /// </summary>
        /// <param name="sc">目标场景</param>
        private static void EnsureTwoBounceDemo(Scene sc)
        {
            if (sc.Mirrors.Count < 2) return;

            // 获取光源原点、两个反射镜的基础数据
            var ro = sc.Source.Origin;
            var m1 = sc.Mirrors[0];
            var m2 = sc.Mirrors[1];

            // 计算光源到第一个镜面的方向向量（归一化）
            var d_to_m1 = Vector3.Normalize(m1.Point - ro);
            // 计算第一个镜面到第二个镜面的反射方向（归一化）
            var r1 = Vector3.Normalize(m2.Point - m1.Point);

            // 本地方法：根据入射方向和反射方向求解镜面法向量
            static Vector3 SolveN(Vector3 d, Vector3 r)
            {
                var n = Vector3.Normalize(d - r);
                if (n.Y < 0) n = -n;
                return n;
            }

            // 设置两个镜面的法向量并重建几何体
            m1.Normal = SolveN(d_to_m1, r1);
            m1.Rebuild();
            var r2 = Vector3.Normalize(new Vector3(0.6f, 0.85f, 0.25f));
            m2.Normal = SolveN(r1, r2);
            m2.Rebuild();

            // 更新光源方向和光线路径
            sc.Source.Direction = d_to_m1;
            sc.UpdateRayPath();
        }

        /// <summary>
        /// 重置场景为默认状态
        /// </summary>
        private void ResetDefaults()
        {
            var fresh = Scene.CreateDefault();
            var sc = Viewport.Scene;

            // 清空现有场景并替换为默认场景的元素
            sc.Layers.Clear(); sc.Layers.AddRange(fresh.Layers);
            sc.Mirrors.Clear(); sc.Mirrors.AddRange(fresh.Mirrors);
            sc.Lenses.Clear(); sc.Lenses.AddRange(fresh.Lenses);
            sc.Detectors.Clear(); sc.Detectors.AddRange(fresh.Detectors);
            sc.Boxes.Clear(); sc.Boxes.AddRange(fresh.Boxes);
            sc.PortBoxes.Clear(); sc.PortBoxes.AddRange(fresh.PortBoxes);
            sc.Source = fresh.Source;
        }
        #endregion

        #region 数值解析/向量转换辅助方法
        /// <summary>
        /// 安全解析浮点数字符串，解析失败返回默认值
        /// </summary>
        /// <param name="s">待解析字符串</param>
        /// <param name="fallback">解析失败时的默认值</param>
        /// <returns>解析结果或默认值</returns>
        private static float Parse(string? s, float fallback)
            => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : fallback;

        /// <summary>
        /// 将方向向量转换为偏航角（Yaw）和俯仰角（Pitch）（单位：度）
        /// </summary>
        /// <param name="d">归一化的方向向量</param>
        /// <returns>偏航角、俯仰角（度）</returns>
        private static (double yaw, double pitch) YawPitchFromDir(Vector3 d)
        {
            var nd = Vector3.Normalize(d);
            double yaw = Math.Atan2(nd.Z, nd.X) * 180.0 / Math.PI;
            double pitch = Math.Asin(nd.Y) * 180.0 / Math.PI;
            return (yaw, pitch);
        }

        /// <summary>
        /// 将偏航角（Yaw）和俯仰角（Pitch）转换为方向向量（归一化）
        /// </summary>
        /// <param name="yawDeg">偏航角（度）</param>
        /// <param name="pitchDeg">俯仰角（度）</param>
        /// <returns>归一化的方向向量</returns>
        private static Vector3 DirFromYawPitch(double yawDeg, double pitchDeg)
        {
            float yaw = (float)(yawDeg * Math.PI / 180.0);
            float pit = (float)(pitchDeg * Math.PI / 180.0);
            float cp = MathF.Cos(pit), sp = MathF.Sin(pit), cy = MathF.Cos(yaw), sy = MathF.Sin(yaw);
            var v = new Vector3(cp * cy, sp, cp * sy);
            if (v.LengthSquared() < 1e-8f) v = new Vector3(1, 0, 0);
            return Vector3.Normalize(v);
        }

        /// <summary>
        /// 将Face枚举转换为文本标识（如X+、Z-）
        /// </summary>
        /// <param name="f">Face枚举值</param>
        /// <returns>文本标识</returns>
        private static string FaceToText(Face f) => f switch
        {
            Face.XPos => "X+",
            Face.XNeg => "X-",
            Face.YPos => "Y+",
            Face.YNeg => "Y-",
            Face.ZPos => "Z+",
            _ => "Z-"
        };

        /// <summary>
        /// 将文本标识解析为Face枚举，解析失败返回默认值
        /// </summary>
        /// <param name="s">文本标识（如X+）</param>
        /// <param name="fallback">解析失败的默认值</param>
        /// <returns>Face枚举值</returns>
        private static Face ParseFace(string? s, Face fallback)
        {
            s = (s ?? "").Trim().ToUpperInvariant();
            return s switch { "X+" => Face.XPos, "X-" => Face.XNeg, "Y+" => Face.YPos, "Y-" => Face.YNeg, "Z+" => Face.ZPos, "Z-" => Face.ZNeg, _ => fallback };
        }
        #endregion

        #region 光学元件添加方法
        /// <summary>
        /// 添加薄透镜到场景
        /// </summary>
        public void AddLens()
        {
            var sc = Viewport.Scene;
            sc.Lenses.Add(ThinLens.MakeRect(new Vector3(0.0f, 2.3f, 0.0f), Vector3.UnitY, 2.4f, 2.0f, 3.0f));
            sc.UpdateRayPath();
            Viewport.Redraw();
        }

        /// <summary>
        /// 添加探测器到场景
        /// </summary>
        public void AddDet()
        {
            var sc = Viewport.Scene;
            sc.Detectors.Add(Detector.MakeRect(new Vector3(0.0f, 4.6f, 0.8f), Vector3.UnitY, 6.0f, 2.0f));
            sc.UpdateRayPath();
            Viewport.Redraw();
        }

        /// <summary>
        /// 添加光学盒子到场景
        /// </summary>
        /// <param name="kind">盒子类型（反射/折射/吸收）</param>
        public void AddBox(BoxKind kind)
        {
            var sc = Viewport.Scene;
            // 折射盒折射率1.5，其他类型1.0
            float n = kind == BoxKind.Refractor ? 1.5f : 1.0f;
            sc.Boxes.Add(BoxOptic.Make(new Vector3(0.0f, 1.2f, 0.0f), 20, 0, 0, new Vector3(1.6f, 1.0f, 1.0f), kind, n));
            sc.UpdateRayPath();
            Viewport.Redraw();
        }

        /// <summary>
        /// 添加端口盒到场景
        /// </summary>
        public void AddPortBox()
        {
            var sc = Viewport.Scene;
            sc.PortBoxes.Add(PortBox.Make(
                center: new Vector3(0.5f, 2.0f, 0.0f),
                yawDeg: 0, pitchDeg: 0, rollDeg: 0,
                size: new Vector3(1.6f, 1.0f, 1.0f),
                inFace: Face.ZPos, outFace: Face.ZNeg,
                apertureW: 1.0f, apertureH: 0.8f,
                proc: ProcessorKind.PassThrough, defYawDeg: 0, defPitchDeg: 0));
            sc.UpdateRayPath();
            Viewport.Redraw();
        }

        /// <summary>
        /// 添加反射镜到场景（支持模板参数）
        /// </summary>
        /// <param name="template">元件模板（可为null）</param>
        public void AddMirror(OpticalChainSimulator.Models.OpticalElement? template, bool notifyVm = true)
        {
            var sc = Viewport.Scene;

            // 创建反射镜实例（默认位置/法向/尺寸）
            var m = Mirror.MakeRect(
                new Vector3(-1.0f, 0.6f, -1.0f),
                Vector3.Normalize(new Vector3(0.2f, 0.95f, 0.1f)),
                3.0f, 2.0f);

            // 分配唯一ID（模板有ID则复用，否则新建）
            var id = Guid.NewGuid();
            m.ElementId = id;

            // 添加到场景集合
            sc.Mirrors.Add(m);

            // 复制模板属性（如有）
            Dictionary<string, System.Text.Json.JsonElement>? props = null;
            if (template?.Properties != null && template.Properties.Count > 0)
                props = new Dictionary<string, System.Text.Json.JsonElement>(template.Properties);

            // 构造元件名称和参数（供ViewModel同步）
            int idx = sc.Mirrors.Count;
            var name = $"反射镜 {idx}";
            var parameters = new Dictionary<string, double>
            {
                ["Width"] = m.Width,
                ["Height"] = m.Height,
                ["X"] = m.Point.X,
                ["Y"] = m.Point.Y,
                ["Z"] = m.Point.Z,
                ["NormalX"] = m.Normal.X,
                ["NormalY"] = m.Normal.Y,
                ["NormalZ"] = m.Normal.Z
            };

            System.Diagnostics.Debug.WriteLine($"AddMirror: templateId={(template?.Id.ToString() ?? "<null>")}, created scene index={sc.Mirrors.Count - 1}, elementId={m.ElementId}");

            // 通知ViewModel创建对应元素
            NotifyElementCreated(OpticalChainSimulator.Models.OpticalElementType.Mirror, name, parameters, props, id);

            // 更新光线路径并刷新渲染
            sc.UpdateRayPath();
            Viewport.Redraw();
        }

        /// <summary>
        /// 重载：通过yuanjianku.OpticalElement1模板添加反射镜
        /// </summary>
        /// <param name="template">自定义元件模板</param>
        /// <param name="notifyVm">是否通知ViewModel</param>
        public void AddMirror1(OpticalChainSimulator.yuanjianku.OpticalElement1 template, bool notifyVm = true)
        {
            var sc = Viewport.Scene;

            // 根据模板位置创建反射镜
            var m = Mirror.MakeRect(
                 template.Point1,
                Vector3.Normalize(new Vector3(0.2f, 0.95f, 0.1f)),
                3.0f, 2.0f);

            // 分配唯一ID
            var id = (template != null && template.Id != Guid.Empty) ? template.Id : Guid.NewGuid();
            m.ElementId = id;

            // 添加到场景
            sc.Mirrors.Add(m);

            System.Diagnostics.Debug.WriteLine($"AddMirror: templateId={(template?.Id.ToString() ?? "<null>")}, created scene index={sc.Mirrors.Count - 1}, elementId={m.ElementId}");

            // 通知ViewModel（如需）
            if (notifyVm)
            {
                NotifyElementCreatedForRectangular(OpticalChainSimulator.Models.OpticalElementType.Mirror, "反射镜", m.Width, m.Height, m.Point, m.Point, id);
            }

            // 更新渲染
            sc.UpdateRayPath();
            Viewport.Redraw();
        }

        /// <summary>
        /// 添加薄透镜到场景（支持模板参数）
        /// </summary>
        /// <param name="template">元件模板（可为null）</param>
        public void AddLens(OpticalChainSimulator.Models.OpticalElement? template, bool notifyVm = true)
        {
            var sc = Viewport.Scene;

            // 创建透镜实例
            var l = ThinLens.MakeRect(new Vector3(0.0f, 2.3f, 0.0f), Vector3.UnitY, 2.4f, 2.0f, 3.0f);

            // 分配唯一ID
            var id = (template != null && template.Id != Guid.Empty) ? template.Id : Guid.NewGuid();
            l.ElementId = id;

            // 添加到场景
            sc.Lenses.Add(l);

            // 复制模板属性
            Dictionary<string, System.Text.Json.JsonElement>? props = null;
            if (template?.Properties != null && template.Properties.Count > 0)
                props = new Dictionary<string, System.Text.Json.JsonElement>(template.Properties);

            System.Diagnostics.Debug.WriteLine($"AddLens: templateId={(template?.Id.ToString() ?? "<null>")}, created scene index={sc.Lenses.Count - 1}, elementId={l.ElementId}");

            // 构造参数供ViewModel同步
            var parameters = new Dictionary<string, double>
            {
                ["Width"] = l.Width,
                ["Height"] = l.Height,
                ["X"] = l.Point.X,
                ["Y"] = l.Point.Y,
                ["Z"] = l.Point.Z,
                ["NormalX"] = l.Normal.X,
                ["NormalY"] = l.Normal.Y,
                ["NormalZ"] = l.Normal.Z
            };
            var name = $"透镜 {sc.Lenses.Count}";

            // 通知ViewModel
            NotifyElementCreated(OpticalChainSimulator.Models.OpticalElementType.Lens, name, parameters, props, id);

            // 更新渲染
            sc.UpdateRayPath();
            Viewport.Redraw();
        }

        /// <summary>
        /// 添加探测器到场景（支持模板参数）
        /// </summary>
        /// <param name="template">元件模板（可为null）</param>
        public void AddDet(OpticalChainSimulator.Models.OpticalElement? template, bool notifyVm = true)
        {
            var sc = Viewport.Scene;

            // 创建探测器实例
            var d = Detector.MakeRect(new Vector3(0.0f, 4.6f, 0.8f), Vector3.UnitY, 6.0f, 2.0f);

            // 分配唯一ID
            // 分配唯一ID
            var id = (template != null && template.Id != Guid.Empty) ? template.Id : Guid.NewGuid();
            d.ElementId = id;

            // 添加到场景
            sc.Detectors.Add(d);

            // 复制模板属性
            Dictionary<string, System.Text.Json.JsonElement>? props = null;
            if (template?.Properties != null && template.Properties.Count > 0)
                props = new Dictionary<string, System.Text.Json.JsonElement>(template.Properties);

            System.Diagnostics.Debug.WriteLine($"AddDet: templateId={(template?.Id.ToString() ?? "<null>")}, created scene index={sc.Detectors.Count - 1}, elementId={d.ElementId}");

            // 构造参数供ViewModel同步
            var parameters = new Dictionary<string, double>
            {
                ["Width"] = d.Width,
                ["Height"] = d.Height,
                ["X"] = d.Point.X,
                ["Y"] = d.Point.Y,
                ["Z"] = d.Point.Z,
                ["NormalX"] = d.Normal.X,
                ["NormalY"] = d.Normal.Y,
                ["NormalZ"] = d.Normal.Z
            };
            var name = $"探测器 {sc.Detectors.Count}";

            // 通知ViewModel
            NotifyElementCreated(OpticalChainSimulator.Models.OpticalElementType.Detector, name, parameters, props, id);

            // 更新渲染
            sc.UpdateRayPath();
            Viewport.Redraw();
        }

        /// <summary>
        /// 添加光源到场景（支持模板参数）
        /// </summary>
        /// <param name="template">元件模板（可为null）</param>
        public void AddSource(OpticalChainSimulator.Models.OpticalElement? template, bool notifyVm = true)
        {
            var sc = Viewport.Scene;

            // 默认光源参数
            var defaultOrigin = new Vector3(-9.0f, 0.9f, -6.0f);
            var defaultDirection = Vector3.Normalize(new Vector3(1.2f, 0.35f, 0.8f));
            int defaultMaxBounces = 6;

            // 从模板读取参数（优先使用模板值）
            Vector3 origin = defaultOrigin;
            Vector3 direction = defaultDirection;
            int maxBounces = defaultMaxBounces;

            if (template != null)
            {
                // 解析位置参数
                if (template.Parameters.TryGetValue("X", out var tx) || template.Parameters.TryGetValue("OriginX", out tx))
                    origin.X = (float)tx;
                if (template.Parameters.TryGetValue("Y", out var ty) || template.Parameters.TryGetValue("OriginY", out ty))
                    origin.Y = (float)ty;
                if (template.Parameters.TryGetValue("Z", out var tz) || template.Parameters.TryGetValue("OriginZ", out tz))
                    origin.Z = (float)tz;

                // 解析方向参数（直接向量 / 角度转换）
                if (template.Parameters.TryGetValue("DirectionX", out var dx) &&
                    template.Parameters.TryGetValue("DirectionY", out var dy) &&
                    template.Parameters.TryGetValue("DirectionZ", out var dz))
                    direction = Vector3.Normalize(new Vector3((float)dx, (float)dy, (float)dz));
                else if (template.Parameters.TryGetValue("Yaw", out var yawVal) && template.Parameters.TryGetValue("Pitch", out var pitchVal))
                {
                    double yaw = yawVal;
                    double pitch = pitchVal;
                    float yawR = (float)(yaw * Math.PI / 180.0);
                    float pitR = (float)(pitch * Math.PI / 180.0);
                    float cp = MathF.Cos(pitR), sp = MathF.Sin(pitR), cy = MathF.Cos(yawR), sy = MathF.Sin(yawR);
                    direction = Vector3.Normalize(new Vector3(cp * cy, sp, cp * sy));
                }

                // 解析最大反射次数
                if (template.Parameters.TryGetValue("MaxBounces", out var mb))
                    maxBounces = (int)Math.Max(1, Math.Round(mb));
            }

            // 分配唯一ID
            var id = (template != null && template.Id != Guid.Empty) ? template.Id : Guid.NewGuid();

            // 创建并设置光源
            sc.Source = new RayDefinition
            {
                Origin = origin,
                Direction = direction,
                MaxBounces = maxBounces,
                ElementId = id
            };

            // 构造参数供ViewModel同步
            var parameters = new Dictionary<string, double>
            {
                ["X"] = origin.X,
                ["Y"] = origin.Y,
                ["Z"] = origin.Z,
                ["DirX"] = direction.X,
                ["DirY"] = direction.Y,
                ["DirZ"] = direction.Z,
                ["MaxBounces"] = maxBounces
            };

            // 复制模板属性
            Dictionary<string, System.Text.Json.JsonElement>? props = null;
            if (template?.Properties != null && template.Properties.Count > 0)
                props = new Dictionary<string, System.Text.Json.JsonElement>(template.Properties);

            // 通知ViewModel
            var name = $"光源 {1}";
            NotifyElementCreated(OpticalChainSimulator.Models.OpticalElementType.Source, name, parameters, props, id);

            // 更新渲染
            sc.UpdateRayPath();
            Viewport.Redraw();
        }
        #endregion

        #region 元素属性修改方法（按ID操作）
        /// <summary>
        /// 按唯一ID选中场景中的元素
        /// </summary>
        /// <param name="elementId">元素唯一ID</param>
        /// <param name="syncEditors">是否同步到编辑器（预留）</param>
        /// <returns>是否找到并选中元素</returns>
        public bool SelectByElementId(Guid elementId, bool syncEditors = true)
        {
            var sc = Viewport.Scene;
            int overall = 0;

            // 检查反射镜
            for (int i = 0; i < sc.Mirrors.Count; i++)
            {
                if (sc.Mirrors[i].ElementId == elementId)
                {
                    _currentSelectedIndex = overall;
                    return true;
                }
                overall++;
            }

            // 检查透镜
            for (int i = 0; i < sc.Lenses.Count; i++)
            {
                if (sc.Lenses[i].ElementId == elementId)
                {
                    _currentSelectedIndex = overall;
                    return true;
                }
                overall++;
            }

            // 检查探测器
            for (int i = 0; i < sc.Detectors.Count; i++)
            {
                if (sc.Detectors[i].ElementId == elementId)
                {
                    _currentSelectedIndex = overall;
                    return true;
                }
                overall++;
            }

            // 检查光学盒子
            for (int i = 0; i < sc.Boxes.Count; i++)
            {
                if (sc.Boxes[i].ElementId == elementId)
                {
                    _currentSelectedIndex = overall;
                    return true;
                }
                overall++;
            }

            // 检查端口盒
            for (int i = 0; i < sc.PortBoxes.Count; i++)
            {
                if (sc.PortBoxes[i].ElementId == elementId)
                {
                    _currentSelectedIndex = overall;
                    return true;
                }
                overall++;
            }

            return false;
        }

        /// <summary>
        /// 按唯一ID设置元素中心坐标
        /// </summary>
        /// <param name="elementId">元素唯一ID</param>
        /// <param name="pos">新的中心坐标</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        /// <returns>是否成功设置</returns>
        public bool SetSelectedCenterByElementId(Guid elementId, Vector3 pos, bool updateScene = true)
        {
            var sc = Viewport.Scene;
            System.Diagnostics.Debug.WriteLine($"[DBG] SetSelectedCenterByElementId called id={elementId}");
            System.Diagnostics.Debug.WriteLine($"[DBG] Scene mirror ids = [{string.Join(", ", Viewport.Scene.Mirrors.Select(m => m.ElementId))}]");

            // 处理光源
            if (sc.Source != null && sc.Source.ElementId == elementId)
            {
                sc.Source.Origin = pos;
                if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                return true;
            }

            // 处理反射镜
            for (int i = 0; i < sc.Mirrors.Count; i++)
            {
                if (sc.Mirrors[i].ElementId == elementId)
                {
                    sc.Mirrors[i].Point = pos;
                    sc.Mirrors[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理透镜
            for (int i = 0; i < sc.Lenses.Count; i++)
            {
                if (sc.Lenses[i].ElementId == elementId)
                {
                    sc.Lenses[i].Point = pos;
                    sc.Lenses[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理探测器
            for (int i = 0; i < sc.Detectors.Count; i++)
            {
                if (sc.Detectors[i].ElementId == elementId)
                {
                    sc.Detectors[i].Point = pos;
                    sc.Detectors[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理光学盒子
            for (int i = 0; i < sc.Boxes.Count; i++)
            {
                if (sc.Boxes[i].ElementId == elementId)
                {
                    sc.Boxes[i].Center = pos;
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理端口盒
            for (int i = 0; i < sc.PortBoxes.Count; i++)
            {
                if (sc.PortBoxes[i].ElementId == elementId)
                {
                    sc.PortBoxes[i].Center = pos;
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 按唯一ID设置元素朝向（偏航角/俯仰角）
        /// </summary>
        /// <param name="elementId">元素唯一ID</param>
        /// <param name="yawDeg">偏航角（度）</param>
        /// <param name="pitchDeg">俯仰角（度）</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        /// <returns>是否成功设置</returns>
        public bool SetSelectedYawPitchByElementId(Guid elementId, double yawDeg, double pitchDeg, bool updateScene = true)
        {
            var sc = Viewport.Scene;
            var dir = DirFromYawPitch(yawDeg, pitchDeg);

            // 处理光源方向
            if (sc.Source != null && sc.Source.ElementId == elementId)
            {
                sc.Source.Direction = dir;
                if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                return true;
            }

            // 处理反射镜法向
            for (int i = 0; i < sc.Mirrors.Count; i++)
            {
                if (sc.Mirrors[i].ElementId == elementId)
                {
                    sc.Mirrors[i].Normal = dir;
                    sc.Mirrors[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理透镜法向
            for (int i = 0; i < sc.Lenses.Count; i++)
            {
                if (sc.Lenses[i].ElementId == elementId)
                {
                    sc.Lenses[i].Normal = dir;
                    sc.Lenses[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理探测器法向
            for (int i = 0; i < sc.Detectors.Count; i++)
            {
                if (sc.Detectors[i].ElementId == elementId)
                {
                    sc.Detectors[i].Normal = dir;
                    sc.Detectors[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理光学盒子朝向（AxisZ）
            for (int i = 0; i < sc.Boxes.Count; i++)
            {
                if (sc.Boxes[i].ElementId == elementId)
                {
                    (sc.Boxes[i].AxisX, sc.Boxes[i].AxisY, sc.Boxes[i].AxisZ) = PortBox.YawPitchRollToBasis((float)yawDeg, (float)pitchDeg, 0f);
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 按唯一ID设置元素尺寸
        /// </summary>
        /// <param name="elementId">元素唯一ID</param>
        /// <param name="width">宽度（最小0.05）</param>
        /// <param name="height">高度（最小0.05）</param>
        /// <param name="depth">深度（仅盒子有效，最小0.05）</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        /// <returns>是否成功设置</returns>
        public bool SetSelectedSizeByElementId(Guid elementId, float width, float height, float depth = 1f, bool updateScene = true)
        {
            var sc = Viewport.Scene;
            float w = Math.Max(0.05f, width);
            float h = Math.Max(0.05f, height);
            float dep = Math.Max(0.05f, depth);

            // 处理反射镜
            for (int i = 0; i < sc.Mirrors.Count; i++)
            {
                if (sc.Mirrors[i].ElementId == elementId)
                {
                    sc.Mirrors[i].Width = w;
                    sc.Mirrors[i].Height = h;
                    sc.Mirrors[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理透镜
            for (int i = 0; i < sc.Lenses.Count; i++)
            {
                if (sc.Lenses[i].ElementId == elementId)
                {
                    sc.Lenses[i].Width = w;
                    sc.Lenses[i].Height = h;
                    sc.Lenses[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理探测器
            for (int i = 0; i < sc.Detectors.Count; i++)
            {
                if (sc.Detectors[i].ElementId == elementId)
                {
                    sc.Detectors[i].Width = w;
                    sc.Detectors[i].Height = h;
                    sc.Detectors[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理光学盒子（Half为尺寸的一半）
            for (int i = 0; i < sc.Boxes.Count; i++)
            {
                if (sc.Boxes[i].ElementId == elementId)
                {
                    sc.Boxes[i].Half = new Vector3(w * 0.5f, h * 0.5f, dep * 0.5f);
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 按唯一ID设置元素法向量（适用于反射镜/透镜/探测器）
        /// </summary>
        /// <param name="elementId">元素唯一ID</param>
        /// <param name="normal">归一化的法向量</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        /// <returns>是否成功设置</returns>
        public bool SetSelectedNormalByElementId(Guid elementId, Vector3 normal, bool updateScene = true)
        {
            var sc = Viewport.Scene;
            if (normal.LengthSquared() < 1e-9f) return false;
            var n = Vector3.Normalize(normal);

            // 处理反射镜
            for (int i = 0; i < sc.Mirrors.Count; i++)
            {
                if (sc.Mirrors[i].ElementId == elementId)
                {
                    sc.Mirrors[i].Normal = n;
                    sc.Mirrors[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理透镜
            for (int i = 0; i < sc.Lenses.Count; i++)
            {
                if (sc.Lenses[i].ElementId == elementId)
                {
                    sc.Lenses[i].Normal = n;
                    sc.Lenses[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            // 处理探测器
            for (int i = 0; i < sc.Detectors.Count; i++)
            {
                if (sc.Detectors[i].ElementId == elementId)
                {
                    sc.Detectors[i].Normal = n;
                    sc.Detectors[i].Rebuild();
                    if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 按唯一ID设置光源方向
        /// </summary>
        /// <param name="elementId">光源唯一ID</param>
        /// <param name="direction">归一化的方向向量</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        /// <returns>是否成功设置</returns>
        public bool SetSourceDirectionByElementId(Guid elementId, Vector3 direction, bool updateScene = true)
        {
            var sc = Viewport.Scene;
            if (direction.LengthSquared() < 1e-9f) return false;

            // 归一化方向向量
            var dir = Vector3.Normalize(direction);

            if (sc.Source != null && sc.Source.ElementId == elementId)
            {
                sc.Source.Direction = dir;
                if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                return true;
            }

            return false;
        }

        /// <summary>
        /// 按唯一ID设置光源最大反射次数
        /// </summary>
        /// <param name="elementId">光源唯一ID</param>
        /// <param name="maxBounces">最大反射次数（最小1）</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        /// <returns>是否成功设置</returns>
        public bool SetSourceMaxBouncesByElementId(Guid elementId, int maxBounces, bool updateScene = true)
        {
            var sc = Viewport.Scene;
            if (sc.Source != null && sc.Source.ElementId == elementId)
            {
                sc.Source.MaxBounces = Math.Max(1, maxBounces);
                if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 按唯一ID获取元素中心坐标
        /// </summary>
        /// <param name="elementId">元素唯一ID</param>
        /// <param name="center">输出：元素中心坐标（获取失败则为默认值）</param>
        /// <returns>是否成功获取</returns>
        public bool TryGetElementCenterByElementId(Guid elementId, out System.Numerics.Vector3 center)
        {
            center = default;
            var sc = Viewport.Scene;

            // 处理光源
            if (sc.Source != null && sc.Source.ElementId == elementId)
            {
                center = sc.Source.Origin;
                return true;
            }

            // 处理反射镜
            for (int i = 0; i < sc.Mirrors.Count; i++)
            {
                if (sc.Mirrors[i].ElementId == elementId)
                {
                    center = sc.Mirrors[i].Point;
                    return true;
                }
            }

            // 处理透镜
            for (int i = 0; i < sc.Lenses.Count; i++)
            {
                if (sc.Lenses[i].ElementId == elementId)
                {
                    center = sc.Lenses[i].Point;
                    return true;
                }
            }

            // 处理探测器
            for (int i = 0; i < sc.Detectors.Count; i++)
            {
                if (sc.Detectors[i].ElementId == elementId)
                {
                    center = sc.Detectors[i].Point;
                    return true;
                }
            }

            // 处理光学盒子
            for (int i = 0; i < sc.Boxes.Count; i++)
            {
                if (sc.Boxes[i].ElementId == elementId)
                {
                    center = sc.Boxes[i].Center;
                    return true;
                }
            }

            // 处理端口盒
            for (int i = 0; i < sc.PortBoxes.Count; i++)
            {
                if (sc.PortBoxes[i].ElementId == elementId)
                {
                    center = sc.PortBoxes[i].Center;
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region 元素删除方法
        /// <summary>
        /// 移除场景中所有元素
        /// </summary>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        public void RemoveAllElements(bool updateScene = true)
        {
            var sc = Viewport.Scene;

            // 清空所有元件集合
            sc.Mirrors.Clear();
            sc.Lenses.Clear();
            sc.Detectors.Clear();
            sc.Boxes.Clear();
            sc.PortBoxes.Clear();

            // 重置光源
            if (sc.Source != null)
            {
                sc.Source.ElementId = Guid.Empty;
                sc.Source.MaxBounces = 0;
            }

            if (updateScene)
            {
                sc.UpdateRayPath();
                Viewport.Redraw();
            }

            System.Diagnostics.Debug.WriteLine("RemoveAllElements: scene cleared");
        }

        /// <summary>
        /// 按唯一ID移除场景中的元素
        /// </summary>
        /// <param name="elementId">元素唯一ID</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveElementByElementId(Guid elementId, bool updateScene = true)
        {
            var sc = Viewport.Scene;
            bool removed = false;

            // 处理光源
            if (sc.Source != null && sc.Source.ElementId == elementId)
            {
                sc.Source = null;
                removed = true;
                System.Diagnostics.Debug.WriteLine($"RemoveElementByElementId: removed Source elementId={elementId}");
            }

            // 反向遍历移除反射镜（避免索引错乱）
            for (int i = sc.Mirrors.Count - 1; i >= 0; i--)
            {
                if (sc.Mirrors[i].ElementId == elementId)
                {
                    sc.Mirrors.RemoveAt(i);
                    removed = true;
                    System.Diagnostics.Debug.WriteLine($"Removed Mirror index={i} id={elementId}");
                }
            }

            // 移除透镜
            for (int i = sc.Lenses.Count - 1; i >= 0; i--)
            {
                if (sc.Lenses[i].ElementId == elementId)
                {
                    sc.Lenses.RemoveAt(i);
                    removed = true;
                }
            }

            // 移除探测器
            for (int i = sc.Detectors.Count - 1; i >= 0; i--)
            {
                if (sc.Detectors[i].ElementId == elementId)
                {
                    sc.Detectors.RemoveAt(i);
                    removed = true;
                }
            }

            // 移除光学盒子
            for (int i = sc.Boxes.Count - 1; i >= 0; i--)
            {
                if (sc.Boxes[i].ElementId == elementId)
                {
                    sc.Boxes.RemoveAt(i);
                    removed = true;
                }
            }

            // 移除端口盒
            for (int i = sc.PortBoxes.Count - 1; i >= 0; i--)
            {
                if (sc.PortBoxes[i].ElementId == elementId)
                {
                    sc.PortBoxes.RemoveAt(i);
                    removed = true;
                }
            }

            // 更新渲染
            if (removed && updateScene)
            {
                sc.UpdateRayPath();
                Viewport.Redraw();
            }

            System.Diagnostics.Debug.WriteLine($"RemoveElementByElementId: id={elementId} removed={removed}");
            return removed;
        }
        #endregion

        #region 元素更新/高亮方法
        /// <summary>
        /// 更新场景中指定元素的属性
        /// </summary>
        /// <param name="template">包含新属性的元件模板</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        public void UpdateElement(OpticalElement1 template, bool updateScene = true)
        {
            // 校验参数有效性
            if (template == null || template.Id == Guid.Empty)
            {
                return;
            }

            // 确保在UI线程执行（避免跨线程访问控件）
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Invoke(() => UpdateElement(template, updateScene));
                return;
            }

            bool updated = false;
            var s = Viewport.Scene;

            // 更新反射镜属性
            var m = s.Mirrors.FirstOrDefault(x => x.ElementId == template.Id);
            if (m != null)
            {
                // 更新位置
                m.Point = new Vector3(
                    (float)(template.Point1.X),
                    (float)(template.Point1.Y),
                    (float)(template.Point1.Z)
                );

                // 更新法向量
                var nx = (float)GetParam(template, "DirX", m.Normal.X);
                var ny = (float)GetParam(template, "DirY", m.Normal.Y);
                var nz = (float)GetParam(template, "DirZ", m.Normal.Z);
                m.Normal = Vector3.Normalize(new Vector3(nx, ny, nz));

                // 更新尺寸
                m.Width = (float)GetParam(template, "Width", m.Width);
                m.Height = (float)GetParam(template, "Height", m.Height);

                // 重建几何体
                m.Rebuild();
                updated = true;
            }

            // 更新透镜属性
            var l = s.Lenses.FirstOrDefault(x => x.ElementId == template.Id);
            if (l != null)
            {
                // 更新位置
                l.Point = new Vector3(
                    (float)(template.Point1.X),
                    (float)(template.Point1.Y),
                    (float)(template.Point1.Z)
                );

                // 更新法向量
                var nx = (float)GetParam(template, "NormalX", l.Normal.X);
                var ny = (float)GetParam(template, "NormalY", l.Normal.Y);
                var nz = (float)GetParam(template, "NormalZ", l.Normal.Z);
                l.Normal = Vector3.Normalize(new Vector3(nx, ny, nz));

                // 更新尺寸和焦距
                l.Width = (float)GetParam(template, "Width", l.Width);
                l.Height = (float)GetParam(template, "Height", l.Height);
                l.FocalLength = (float)GetParam(template, "FocalLength", l.FocalLength);

                // 重建几何体
                l.Rebuild();
                updated = true;
            }

            // 更新探测器属性
            var d = s.Detectors.FirstOrDefault(x => x.ElementId == template.Id);
            if (d != null)
            {
                // 更新位置
                d.Point = new Vector3(
                    (float)(template.Point1.X),
                    (float)(template.Point1.Y),
                    (float)(template.Point1.Z)
                );

                // 更新法向量
                var nx = (float)GetParam(template, "NormalX", d.Normal.X);
                var ny = (float)GetParam(template, "NormalY", d.Normal.Y);
                var nz = (float)GetParam(template, "NormalZ", d.Normal.Z);
                d.Normal = Vector3.Normalize(new Vector3(nx, ny, nz));

                // 更新尺寸
                d.Width = (float)GetParam(template, "Width", d.Width);
                d.Height = (float)GetParam(template, "Height", d.Height);

                // 重建几何体
                d.Rebuild();
                updated = true;
            }

            // 更新渲染
            if (updated && updateScene)
            {
                Viewport.Redraw();
            }
            s.UpdateRayPath();
            Viewport.Redraw();
        }

        /// <summary>
        /// 安全读取元件参数（参数不存在则返回默认值）
        /// </summary>
        /// <param name="model">元件模板</param>
        /// <param name="key">参数键名</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns>参数值或默认值</returns>
        private static double GetParam(OpticalElement1 model, string key, double defaultVal)
            => model.Parameters1 != null && model.Parameters1.TryGetValue(key, out var v) ? v : defaultVal;

        /// <summary>
        /// 获取ViewModel实例（数据上下文转换）
        /// </summary>
        private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;

        /// <summary>
        /// 高亮指定ID的元素
        /// </summary>
        /// <param name="elementId">元素唯一ID（Guid.Empty取消高亮）</param>
        public void HighlightElement(Guid elementId)
        {
            // 设置视口高亮ID并刷新渲染
            Viewport.HighlightedElementId = elementId;
            Viewport.Redraw();
            // 同步到ViewModel
            Vm.Poin = elementId;
        }

        /// <summary>
        /// 辅助高亮方法（同步到ViewModel的Poin1属性）
        /// </summary>
        /// <param name="elementId">元素唯一ID</param>
        public void HighlightElement1(Guid elementId)
        {
            Vm.Poin1 = elementId;
        }
        #endregion

        #region 元素创建通知辅助方法
        /// <summary>
        /// 通知外部创建了光学元件（核心通知方法）
        /// </summary>
        /// <param name="type">元件类型</param>
        /// <param name="name">元件名称</param>
        /// <param name="numericParameters">数值参数</param>
        /// <param name="properties">扩展属性</param>
        /// <param name="id">元件唯一ID</param>
        private void NotifyElementCreated(OpticalChainSimulator.Models.OpticalElementType type, string name, Dictionary<string, double>? numericParameters = null, Dictionary<string, System.Text.Json.JsonElement>? properties = null, Guid? id = null)
        {
            try
            {
                var model = new OpticalElement
                {
                    Id = id ?? Guid.NewGuid(),
                    Type = type,
                    Name = name ?? string.Empty,
                    Parameters = numericParameters != null ? new Dictionary<string, double>(numericParameters) : new Dictionary<string, double>(),
                    Properties = properties != null ? new Dictionary<string, System.Text.Json.JsonElement>(properties) : new Dictionary<string, System.Text.Json.JsonElement>()
                };

                ElementCreated?.Invoke(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NotifyElementCreated error: {ex}");
            }
        }

        /// <summary>
        /// 通知外部创建了矩形光学元件（反射镜/透镜/探测器）
        /// </summary>
        /// <param name="type">元件类型</param>
        /// <param name="baseName">基础名称</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="center">中心坐标</param>
        /// <param name="center1">扩展坐标（测试用）</param>
        /// <param name="elementId">元件唯一ID</param>
        /// <param name="properties">扩展属性</param>
        private void NotifyElementCreatedForRectangular(OpticalChainSimulator.Models.OpticalElementType type, string baseName, float width, float height, Vector3 center, Vector3 center1, Guid? elementId = null, Dictionary<string, System.Text.Json.JsonElement>? properties = null)
        {
            try
            {
                var sc = Viewport.Scene;
                // 获取元件在对应集合中的索引
                int idx = type switch
                {
                    OpticalChainSimulator.Models.OpticalElementType.Mirror => sc.Mirrors.Count,
                    OpticalChainSimulator.Models.OpticalElementType.Lens => sc.Lenses.Count,
                    OpticalChainSimulator.Models.OpticalElementType.Detector => sc.Detectors.Count,
                    OpticalChainSimulator.Models.OpticalElementType.Box => sc.Boxes.Count,
                    OpticalChainSimulator.Models.OpticalElementType.PortBox => sc.PortBoxes.Count,
                    _ => 0
                };

                // 构造元件名称
                var name = $"{baseName} {idx}";

                // 构造数值参数
                var parameters = new Dictionary<string, double>
                {
                    ["Width"] = width,
                    ["Height"] = height,
                    ["X"] = center.X,
                    ["Y"] = center.Y,
                    ["Z"] = center.Z,
                    ["Q（测试）"] = center1.X,
                    ["W（测试）"] = center1.Y,
                    ["E（测试）"] = center1.Z
                };

                // 触发创建通知
                NotifyElementCreated(type, name, parameters, properties, elementId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NotifyElementCreatedForRectangular error: {ex}");
            }
        }
        #endregion

        #region 元素属性设置（按索引）
        /// <summary>
        /// 设置当前选中元素的中心坐标
        /// </summary>
        /// <param name="pos">新的中心坐标</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        /// <returns>是否成功设置</returns>
        public bool SetSelectedCenter(Vector3 pos, bool updateScene = true)
        {
            var sc = Viewport.Scene;
            var (s, ok) = GetCurrentSel();

            switch (s.Type)
            {
                case ElemType.Mirror:
                    {
                        var m = sc.Mirrors[s.Index];
                        m.Point = pos;
                        m.Rebuild();
                        break;
                    }
                case ElemType.Lens:
                    {
                        var l = sc.Lenses[s.Index];
                        l.Point = pos;
                        l.Rebuild();
                        break;
                    }
                case ElemType.Detector:
                    {
                        var d = sc.Detectors[s.Index];
                        d.Point = pos;
                        d.Rebuild();
                        break;
                    }
                case ElemType.Box:
                    {
                        var b = sc.Boxes[s.Index];
                        b.Center = pos;
                        break;
                    }
                default: return false;
            }

            if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
            return true;
        }

        /// <summary>
        /// 重载：按X/Y/Z值设置当前选中元素的中心坐标
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="z">Z坐标</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        /// <returns>是否成功设置</returns>
        public bool SetSelectedCenter(float x, float y, float z, bool updateScene = true)
            => SetSelectedCenter(new Vector3(x, y, z), updateScene);

        /// <summary>
        /// 设置当前选中元素的朝向（偏航角/俯仰角）
        /// </summary>
        /// <param name="yawDeg">偏航角（度）</param>
        /// <param name="pitchDeg">俯仰角（度）</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        /// <returns>是否成功设置</returns>
        public bool SetSelectedYawPitch(double yawDeg, double pitchDeg, bool updateScene = true)
        {
            var sc = Viewport.Scene;
            var (s, ok) = GetCurrentSel();
            if (!ok) return false;
            var dir = DirFromYawPitch(yawDeg, pitchDeg);

            switch (s.Type)
            {
                case ElemType.Mirror:
                    {
                        var m = sc.Mirrors[s.Index];
                        m.Normal = dir;
                        m.Rebuild();
                        break;
                    }
                case ElemType.Lens:
                    {
                        var l = sc.Lenses[s.Index];
                        l.Normal = dir;
                        l.Rebuild();
                        break;
                    }
                case ElemType.Detector:
                    {
                        var d = sc.Detectors[s.Index];
                        d.Normal = dir;
                        d.Rebuild();
                        break;
                    }
                case ElemType.Box:
                    {
                        var b = sc.Boxes[s.Index];
                        (b.AxisX, b.AxisY, b.AxisZ) = PortBox.YawPitchRollToBasis((float)yawDeg, (float)pitchDeg, 0f);
                        break;
                    }
                default: return false;
            }

            if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
            return true;
        }

        /// <summary>
        /// 设置当前选中元素的尺寸
        /// </summary>
        /// <param name="width">宽度（最小0.05）</param>
        /// <param name="height">高度（最小0.05）</param>
        /// <param name="depth">深度（仅盒子有效，最小0.05）</param>
        /// <param name="updateScene">是否立即更新场景和渲染</param>
        /// <returns>是否成功设置</returns>
        public bool SetSelectedSize(float width, float height, float depth = 1f, bool updateScene = true)
        {
            var sc = Viewport.Scene;
            var (s, ok) = GetCurrentSel();
            if (!ok) return false;
            float w = Math.Max(0.05f, width);
            float h = Math.Max(0.05f, height);
            float dep = Math.Max(0.05f, depth);

            switch (s.Type)
            {
                case ElemType.Mirror:
                    {
                        var m = sc.Mirrors[s.Index];
                        m.Width = w;
                        m.Height = h;
                        m.Rebuild();
                        break;
                    }
                case ElemType.Lens:
                    {
                        var l = sc.Lenses[s.Index];
                        l.Width = w;
                        l.Height = h;
                        l.Rebuild();
                        break;
                    }
                case ElemType.Detector:
                    {
                        var d = sc.Detectors[s.Index];
                        d.Width = w;
                        d.Height = h;
                        d.Rebuild();
                        break;
                    }
                case ElemType.Box:
                    {
                        var b = sc.Boxes[s.Index];
                        b.Half = new Vector3(w * 0.5f, h * 0.5f, dep * 0.5f);
                        break;
                    }
                default: return false;
            }

            if (updateScene) { sc.UpdateRayPath(); Viewport.Redraw(); }
            return true;
        }

        /// <summary>
        /// 按类型+索引选中场景中的元素
        /// </summary>
        /// <param name="type">元件类型</param>
        /// <param name="index">元件索引</param>
        /// <param name="syncEditors">是否同步到编辑器（预留）</param>
        /// <returns>是否成功选中</returns>
        public bool SelectObject(ElemType type, int index, bool syncEditors = true)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Type == type && _items[i].Index == index)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}