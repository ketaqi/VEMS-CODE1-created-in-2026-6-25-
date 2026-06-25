using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace OpticalChainSimulator
{
    /// <summary>
    /// 基于Skia渲染的光学链模拟器视口控件
    /// 负责3D场景渲染、鼠标交互（旋转/平移/缩放）、物体拾取（单击/双击）、高亮显示等核心功能
    /// </summary>
    public sealed class SkiaViewport : Control
    {
        #region 字段定义
        /// <summary>
        /// 调试用高亮矩形（用于样式实验）
        /// </summary>
        public Quad? DebugHighlightQuad { get; private set; } = null;

        /// <summary>
        /// 当前需要高亮显示的元素ID（Guid.Empty表示清除高亮）
        /// </summary>
        public Guid HighlightedElementId { get; set; } = Guid.Empty;

        /// <summary>
        /// 3D场景实例
        /// </summary>
        private readonly Scene _scene = Scene.CreateDefault();

        /// <summary>
        /// 相机实例，负责视角控制
        /// </summary>
        private Camera _cam = Camera.Default();

        /// <summary>
        /// 上一帧鼠标位置（用于计算鼠标移动增量）
        /// </summary>
        private Point _last;

        /// <summary>
        /// 是否正在旋转视角（左键按下）
        /// </summary>
        private bool _rotating;

        /// <summary>
        /// 是否正在平移视角（右键按下）
        /// </summary>
        private bool _panning;

        /// <summary>
        /// 左键是否按下
        /// </summary>
        private bool _pressedLeft;

        /// <summary>
        /// 右键是否按下
        /// </summary>
        private bool _pressedRight;

        /// <summary>
        /// 鼠标按下期间是否产生了移动（用于区分点击和拖动）
        /// </summary>
        private bool _movedDuringPress;

        #region 双击判定相关字段
        /// <summary>
        /// 上一次点击命中的元素类型
        /// </summary>
        private HitType _lastClickHitType = HitType.None;

        /// <summary>
        /// 上一次点击命中的元素索引
        /// </summary>
        private int _lastClickIndex = -1;

        /// <summary>
        /// 上一次点击的时间戳
        /// </summary>
        private DateTime _lastClickTime;

        /// <summary>
        /// 双击超时阈值（毫秒）
        /// </summary>
        private const double DoubleClickTimeMs = 500;

        /// <summary>
        /// 是否处于等待第二次点击的状态（判定双击）
        /// </summary>
        private bool _isWaitingForDoubleClick = false;

        /// <summary>
        /// 用于取消第一次单击延迟执行的令牌源
        /// </summary>
        private CancellationTokenSource? _doubleClickCts;

        /// <summary>
        /// 当前拾取结果的元素类型
        /// </summary>
        private HitType _currentHitType = HitType.None;

        /// <summary>
        /// 当前拾取结果的元素索引
        /// </summary>
        private int _currentIndex = -1;
        #endregion

        #endregion

        #region 公共属性
        /// <summary>
        /// 获取场景实例（只读）
        /// </summary>
        public Scene Scene => _scene;
        #endregion

        #region 事件定义
        /// <summary>
        /// 元素单击事件
        /// </summary>
        /// <param name="HitType">命中的元素类型（Mirror/Lens/Detector/Box/PortBox）</param>
        /// <param name="int">元素在对应集合中的索引</param>
        public event Action<HitType, int>? ObjectClicked;

        /// <summary>
        /// 元素双击事件
        /// </summary>
        /// <param name="HitType">命中的元素类型（Mirror/Lens/Detector/Box/PortBox）</param>
        /// <param name="int">元素在对应集合中的索引</param>
        public event Action<HitType, int>? ObjectDoubleClicked;
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化视口控件
        /// </summary>
        public SkiaViewport()
        {
            // 设置控件可获得焦点（用于键盘事件）
            Focusable = true;

            // 注册鼠标/键盘事件
            PointerPressed += OnPressed;
            PointerReleased += OnReleased;
            PointerMoved += OnMoved;
            PointerWheelChanged += OnWheel;
            KeyDown += OnKeyDown;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 强制高亮指定位置的矩形（用于调试/样式实验）
        /// </summary>
        /// <param name="center">矩形中心坐标</param>
        /// <param name="normal">矩形法向量</param>
        /// <param name="width">矩形宽度</param>
        /// <param name="height">矩形高度</param>
        public void ForceHighlightAt(Vector3 center, Vector3 normal, float width, float height)
        {
            // 根据法向量构建面内基向量u/v
            var (u, v) = BuildBasisForNormal(normal);
            u *= width * 0.5f;
            v *= height * 0.5f;

            // 构造四边形顶点（中心±半宽/半高）
            var q = new Quad
            {
                A = center - u - v,
                B = center + u - v,
                C = center + u + v,
                D = center - u + v
            };

            DebugHighlightQuad = q;
            // 触发重绘
            InvalidateVisual();
        }

        /// <summary>
        /// 清除调试用高亮矩形
        /// </summary>
        public void ClearDebugHighlight()
        {
            DebugHighlightQuad = null;
            InvalidateVisual();
        }

        /// <summary>
        /// 强制重绘视口
        /// </summary>
        public void Redraw() => InvalidateVisual();
        #endregion

        #region 私有辅助方法
        /// <summary>
        /// 根据法向量构建面内正交基向量（u/v）
        /// </summary>
        /// <param name="n">法向量</param>
        /// <returns>面内正交基向量(u, v)</returns>
        private static (Vector3 u, Vector3 v) BuildBasisForNormal(Vector3 n)
        {
            var nn = Vector3.Normalize(n);
            // 选择参考向量（避免与法向量平行）
            Vector3 refVec = MathF.Abs(nn.Y) < 0.99f ? Vector3.UnitY : Vector3.UnitX;
            // 计算u向量（参考向量×法向量）
            var u = Vector3.Normalize(Vector3.Cross(refVec, nn));
            // 计算v向量（法向量×u向量）
            var v = Vector3.Normalize(Vector3.Cross(nn, u));
            return (u, v);
        }

        /// <summary>
        /// 射线与平面相交检测
        /// </summary>
        /// <param name="ro">射线起点</param>
        /// <param name="rd">射线方向（归一化）</param>
        /// <param name="p0">平面上一点</param>
        /// <param name="n">平面法向量（归一化）</param>
        /// <param name="t">相交点距离射线起点的参数</param>
        /// <returns>是否相交</returns>
        private static bool RayPlaneIntersection(Vector3 ro, Vector3 rd, Vector3 p0, Vector3 n, out float t)
        {
            float denom = Vector3.Dot(rd, n);
            // 射线与平面平行（无交点）
            if (MathF.Abs(denom) < 1e-6f)
            {
                t = 0;
                return false;
            }
            // 计算相交参数t
            t = Vector3.Dot(p0 - ro, n) / denom;
            // 只返回正方向的交点
            return t >= 0;
        }

        /// <summary>
        /// 将世界坐标点投影到屏幕坐标
        /// </summary>
        /// <param name="v">世界坐标点</param>
        /// <param name="vp">视图投影矩阵</param>
        /// <param name="b">视口矩形边界</param>
        /// <returns>屏幕坐标点（超出视口返回null）</returns>
        private static Point? Project(Vector3 v, Matrix4x4 vp, Rect b)
        {
            // 转换到裁剪空间
            Vector4 clip = Vector4.Transform(new Vector4(v, 1), vp);
            // 点在视锥体外（不可见）
            if (clip.W <= 0.01f) return null;

            // 转换到NDC空间（归一化设备坐标）
            Vector3 ndc = new Vector3(clip.X, clip.Y, clip.Z) / clip.W;

            // 转换到屏幕坐标
            double x = (ndc.X * 0.5 + 0.5) * b.Width;
            double y = ((-ndc.Y) * 0.5 + 0.5) * b.Height;
            return new Point(x, y);
        }

        /// <summary>
        /// 计算四边形的平均深度（用于深度排序）
        /// </summary>
        /// <param name="q">四边形</param>
        /// <param name="view">视图矩阵</param>
        /// <returns>平均深度值</returns>
        private static float AvgDepth(Quad q, Matrix4x4 view)
        {
            var va = Vector3.Transform(q.A, view);
            var vb = Vector3.Transform(q.B, view);
            var vc = Vector3.Transform(q.C, view);
            var vd = Vector3.Transform(q.D, view);
            return (va.Length() + vb.Length() + vc.Length() + vd.Length()) * 0.25f;
        }

        /// <summary>
        /// 计算线段中点的深度（用于深度排序）
        /// </summary>
        /// <param name="a">线段起点</param>
        /// <param name="b">线段终点</param>
        /// <param name="view">视图矩阵</param>
        /// <returns>中点深度值</returns>
        private static float MidDepth(Vector3 a, Vector3 b, Matrix4x4 view)
        {
            var mid = (a + b) * 0.5f;
            return Vector3.Transform(mid, view).Length();
        }

        /// <summary>
        /// 将Vector3颜色转换为Avalonia Color
        /// </summary>
        /// <param name="c">Vector3颜色（RGB范围0-1）</param>
        /// <returns>Avalonia Color</returns>
        private static Color ToColor(Vector3 c)
        {
            return Color.FromRgb((byte)(c.X * 255), (byte)(c.Y * 255), (byte)(c.Z * 255));
        }
        #endregion

        #region 交互事件处理
        /// <summary>
        /// 鼠标按下事件处理
        /// </summary>
        /// <param name="s">事件源</param>
        /// <param name="e">鼠标按下事件参数</param>
        private void OnPressed(object? s, PointerPressedEventArgs e)
        {
            // 记录当前鼠标位置
            _last = e.GetCurrentPoint(this).Position;
            var prop = e.GetCurrentPoint(this).Properties;

            // 设置视角操作状态
            _rotating = prop.IsLeftButtonPressed;
            _panning = prop.IsRightButtonPressed;

            // 记录按下的鼠标键
            _pressedLeft = prop.IsLeftButtonPressed;
            _pressedRight = prop.IsRightButtonPressed;

            // 初始化移动标记
            _movedDuringPress = false;

            // 获取焦点（确保键盘事件生效）
            Focus();
        }

        /// <summary>
        /// 鼠标释放事件处理（核心：单击/双击判定）
        /// </summary>
        /// <param name="s">事件源</param>
        /// <param name="e">鼠标释放事件参数</param>
        private void OnReleased(object? s, PointerReleasedEventArgs e)
        {
            var pt = e.GetCurrentPoint(this).Position;

            // 左键按下且未移动 → 判定为点击操作
            if (_pressedLeft && !_movedDuringPress)
            {
                // 执行物体拾取
                TryPickAtPoint(pt);

                // 双击判定核心逻辑
                var now = DateTime.Now;
                var timeDiff = now - _lastClickTime;

                // 情况1：500ms内点击同一元素 → 触发双击
                if (timeDiff.TotalMilliseconds <= DoubleClickTimeMs
                    && _lastClickHitType != HitType.None
                    && _lastClickHitType == _currentHitType
                    && _lastClickIndex == _currentIndex)
                {
                    // 触发双击事件
                    ObjectDoubleClicked?.Invoke(_currentHitType, _currentIndex);

                    // 取消第一次单击的延迟执行
                    _doubleClickCts?.Cancel();
                    _doubleClickCts?.Dispose();

                    // 重置双击状态
                    _lastClickHitType = HitType.None;
                    _lastClickIndex = -1;
                    _isWaitingForDoubleClick = false;
                }
                // 情况2：第一次单击 → 记录信息，延迟判定单击
                else
                {
                    // 记录本次单击信息
                    _lastClickHitType = _currentHitType;
                    _lastClickIndex = _currentIndex;
                    _lastClickTime = now;

                    // 有效元素点击 → 延迟500ms确认单击
                    if (_currentHitType != HitType.None && !_isWaitingForDoubleClick)
                    {
                        _isWaitingForDoubleClick = true;
                        _doubleClickCts = new CancellationTokenSource();
                        var cts = _doubleClickCts;

                        // 延迟执行：未触发双击则重置状态
                        Task.Delay((int)DoubleClickTimeMs, cts.Token)
                            .ContinueWith(t =>
                            {
                                if (!t.IsCanceled)
                                {
                                    _lastClickHitType = HitType.None;
                                    _lastClickIndex = -1;
                                    _isWaitingForDoubleClick = false;
                                }
                            }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }
            }

            // 重置鼠标/视角状态
            _rotating = _panning = false;
            _pressedLeft = _pressedRight = false;
            _movedDuringPress = false;
        }

        /// <summary>
        /// 鼠标移动事件处理（视角旋转/平移）
        /// </summary>
        /// <param name="s">事件源</param>
        /// <param name="e">鼠标移动事件参数</param>
        private void OnMoved(object? s, PointerEventArgs e)
        {
            var p = e.GetCurrentPoint(this).Position;
            float dx = (float)(p.X - _last.X);
            float dy = (float)(p.Y - _last.Y);

            // 检测是否产生有效移动（阈值1像素）
            if (Math.Abs(dx) > 1.0 || Math.Abs(dy) > 1.0)
            {
                _movedDuringPress = _movedDuringPress || (_pressedLeft || _pressedRight);
            }

            // 更新上一帧鼠标位置
            _last = p;

            // 视角旋转（左键拖动）
            if (_rotating)
            {
                _cam.Yaw += dx * 0.3f;
                _cam.Pitch += dy * 0.3f;
                // 限制俯仰角（避免翻转）
                _cam.Pitch = Math.Clamp(_cam.Pitch, -89f, 89f);
                InvalidateVisual();
            }

            // 视角平移（右键拖动）
            if (_panning)
            {
                var yaw = MathF.PI * _cam.Yaw / 180f;
                // 计算相机右方向
                var right = new Vector3(MathF.Cos(yaw), 0, -MathF.Sin(yaw));
                var up = Vector3.UnitY;
                // 计算平移量（与距离成正比）
                var pan = new Vector3(-dx, dy, 0) * (_cam.Distance * 0.0015f);
                _cam.Target += right * pan.X + up * pan.Y;
                InvalidateVisual();
            }
        }

        /// <summary>
        /// 鼠标滚轮事件处理（视角缩放）
        /// </summary>
        /// <param name="s">事件源</param>
        /// <param name="e">鼠标滚轮事件参数</param>
        private void OnWheel(object? s, PointerWheelEventArgs e)
        {
            // 缩放相机距离（滚轮Y轴增量）
            _cam.Distance *= (float)Math.Pow(0.9, e.Delta.Y);
            // 限制缩放范围
            _cam.Distance = Math.Clamp(_cam.Distance, 1.2f, 200f);
            InvalidateVisual();
        }

        /// <summary>
        /// 键盘按下事件处理
        /// </summary>
        /// <param name="s">事件源</param>
        /// <param name="e">键盘事件参数</param>
        private void OnKeyDown(object? s, KeyEventArgs e)
        {
            // R键重置相机视角
            if (e.Key == Key.R)
            {
                _cam = Camera.Default();
                InvalidateVisual();
            }
        }

        /// <summary>
        /// 屏幕坐标点拾取场景物体（核心拾取逻辑）
        /// </summary>
        /// <param name="pt">屏幕坐标点</param>
        private void TryPickAtPoint(Point pt)
        {
            var b = Bounds;
            // 视口无效（宽度/高度为0）
            if (b.Width <= 0 || b.Height <= 0) return;

            // 计算视图投影矩阵
            var view = _cam.ViewMatrix();
            var proj = Matrix4x4.CreatePerspectiveFieldOfView(60f * MathF.PI / 180f, (float)(b.Width / b.Height), 0.1f, 1000f);
            var vp = view * proj;

            // 矩阵求逆（屏幕→世界射线转换）
            if (!Matrix4x4.Invert(vp, out var invVP)) return;

            // 转换为NDC坐标（归一化设备坐标）
            float ndcX = (float)((pt.X / b.Width) * 2.0 - 1.0);
            float ndcY = (float)(((pt.Y / b.Height) * 2.0 - 1.0) * -1.0);

            // 近裁剪面/远裁剪面的NDC点
            var nearClip = new Vector4(ndcX, ndcY, 0f, 1f);
            var farClip = new Vector4(ndcX, ndcY, 1f, 1f);

            // 转换为世界坐标
            var worldNearV4 = Vector4.Transform(nearClip, invVP);
            var worldFarV4 = Vector4.Transform(farClip, invVP);
            if (Math.Abs(worldNearV4.W) < 1e-6f || Math.Abs(worldFarV4.W) < 1e-6f) return;

            var worldNear = new Vector3(worldNearV4.X / worldNearV4.W, worldNearV4.Y / worldNearV4.W, worldNearV4.Z / worldNearV4.W);
            var worldFar = new Vector3(worldFarV4.X / worldFarV4.W, worldFarV4.Y / worldFarV4.W, worldFarV4.Z / worldFarV4.W);

            // 构建世界空间射线（起点+方向）
            var ro = worldNear;
            var rd = Vector3.Normalize(worldFar - worldNear);

            // 初始化最佳拾取结果（最近的交点）
            float bestT = float.PositiveInfinity;
            HitType bestType = HitType.None;
            int bestIndex = -1;

            // 1. 检测镜子（Mirror）
            for (int i = 0; i < _scene.Mirrors.Count; i++)
            {
                var m = _scene.Mirrors[i];
                if (!RayPlaneIntersection(ro, rd, m.Point, m.Normal, out float t) || t <= 1e-4f) continue;
                var hit = ro + rd * t;
                if (!m.ContainsPoint(hit)) continue;
                if (t < bestT) { bestT = t; bestType = HitType.Mirror; bestIndex = i; }
            }

            // 2. 检测透镜（Lens）
            for (int i = 0; i < _scene.Lenses.Count; i++)
            {
                var l = _scene.Lenses[i];
                if (!RayPlaneIntersection(ro, rd, l.Point, l.Normal, out float t) || t <= 1e-4f) continue;
                var hit = ro + rd * t;
                if (!l.ContainsPoint(hit)) continue;
                if (t < bestT) { bestT = t; bestType = HitType.Lens; bestIndex = i; }
            }

            // 3. 检测探测器（Detector）
            for (int i = 0; i < _scene.Detectors.Count; i++)
            {
                var d = _scene.Detectors[i];
                if (!RayPlaneIntersection(ro, rd, d.Point, d.Normal, out float t) || t <= 1e-4f) continue;
                var hit = ro + rd * t;
                if (!d.ContainsPoint(hit)) continue;
                if (t < bestT) { bestT = t; bestType = HitType.Detector; bestIndex = i; }
            }

            // 4. 检测普通盒子（Box）
            for (int i = 0; i < _scene.Boxes.Count; i++)
            {
                var bxo = _scene.Boxes[i];
                if (!bxo.RayIntersect(ro, rd, out float t, out Vector3 n, out _, out bool inside)) continue;
                if (t <= 1e-4f) continue;
                if (t < bestT) { bestT = t; bestType = HitType.Box; bestIndex = i; }
            }

            // 5. 检测端口盒（PortBox）输入面
            for (int i = 0; i < _scene.PortBoxes.Count; i++)
            {
                var pb = _scene.PortBoxes[i];
                if (!pb.IntersectInput(ro, rd, out float t, out Vector3 hit, out Vector3 n)) continue;
                if (t <= 1e-4f) continue;
                if (t < bestT) { bestT = t; bestType = HitType.PortBox; bestIndex = i; }
            }

            // 触发拾取结果
            if (bestType != HitType.None)
            {
                // 触发单击事件
                ObjectClicked?.Invoke(bestType, bestIndex);
                // 记录拾取结果（供双击判定）
                _currentHitType = bestType;
                _currentIndex = bestIndex;
            }
            else
            {
                // 空点击 → 重置拾取结果
                _currentHitType = HitType.None;
                _currentIndex = -1;
            }
        }
        #endregion

        #region 渲染相关方法
        /// <summary>
        /// 控件渲染入口（核心渲染逻辑）
        /// </summary>
        /// <param name="ctx">绘图上下文</param>
        public override void Render(DrawingContext ctx)
        {
            base.Render(ctx);
            var b = Bounds;

            // 1. 绘制背景
            ctx.FillRectangle(new SolidColorBrush(Color.FromRgb(245, 250, 255)), b);

            // 计算视图投影矩阵
            var view = _cam.ViewMatrix();
            var proj = Matrix4x4.CreatePerspectiveFieldOfView(60f * MathF.PI / 180f, (float)(b.Width / b.Height), 0.1f, 1000f);
            var vp = view * proj;

            // 2. 绘制网格
            DrawGrid(ctx, vp, b, 20, 1f, Color.FromArgb(16, 0, 0, 0));

            // 3. 收集所有需要渲染的多边形（按深度排序）
            var polys = new List<Poly>();

            // 渲染图层（Layer）
            foreach (var layer in _scene.Layers)
            {
                polys.Add(Poly.FromQuad(layer.Quad, AvgDepth(layer.Quad, view),
                    Color.FromArgb(155, (byte)(layer.Color.X * 255), (byte)(layer.Color.Y * 255), (byte)(layer.Color.Z * 255)),
                    Color.FromArgb(200, (byte)(layer.Color.X * 255), (byte)(layer.Color.Y * 255), (byte)(layer.Color.Z * 255))));
            }

            // 渲染镜子（挤出成棱柱，增加视觉厚度）
            foreach (var m in _scene.Mirrors)
            {
                polys.AddRange(Poly.FromPrism(m.Quad, AvgDepth(m.Quad, view), 0.08f,
                    Color.FromArgb(120, 105, 105, 115), Color.FromArgb(200, 25, 25, 45)));
            }

            // 渲染透镜（挤出成棱柱）
            foreach (var l in _scene.Lenses)
            {
                polys.AddRange(Poly.FromPrism(l.Quad, AvgDepth(l.Quad, view), 0.22f,
                    Color.FromArgb(110, 130, 80, 200), Color.FromArgb(200, 80, 40, 140)));
            }

            // 渲染探测器（挤出成棱柱）
            foreach (var d in _scene.Detectors)
            {
                polys.AddRange(Poly.FromPrism(d.Quad, AvgDepth(d.Quad, view), 0.16f,
                    Color.FromArgb(110, 229, 192, 123), Color.FromArgb(200, 179, 139, 73)));
            }

            // 渲染普通盒子（6个面）
            foreach (var bx in _scene.Boxes)
            {
                var hx = bx.Half.X; var hy = bx.Half.Y; var hz = bx.Half.Z;
                var cx = bx.Center + bx.AxisX * hx; var cxn = bx.Center - bx.AxisX * hx;
                var cy = bx.Center + bx.AxisY * hy; var cyn = bx.Center - bx.AxisY * hy;
                var cz = bx.Center + bx.AxisZ * hz; var czn = bx.Center - bx.AxisZ * hz;

                // 根据盒子类型设置颜色
                Color fill; Color wire;
                switch (bx.Kind)
                {
                    case BoxKind.Reflector:
                        fill = Color.FromArgb(120, 90, 90, 100);
                        wire = Color.FromArgb(220, 35, 35, 55);
                        break;
                    case BoxKind.Refractor:
                        fill = Color.FromArgb(100, 80, 150, 220);
                        wire = Color.FromArgb(220, 50, 90, 150);
                        break;
                    default:
                        fill = Color.FromArgb(160, 60, 60, 60);
                        wire = Color.FromArgb(220, 20, 20, 20);
                        break;
                }

                // 绘制单个面
                void AddFace(Vector3 c0, Vector3 u, float uLen, Vector3 v, float vLen)
                {
                    var A = c0 - u * uLen - v * vLen;
                    var B = c0 + u * uLen - v * vLen;
                    var C = c0 + u * uLen + v * vLen;
                    var D = c0 - u * uLen + v * vLen;
                    var q = new Quad { A = A, B = B, C = C, D = D };
                    polys.Add(Poly.FromQuad(q, AvgDepth(q, view), fill, wire));
                }

                AddFace(cx, bx.AxisY, hy, bx.AxisZ, hz);
                AddFace(cxn, bx.AxisY, hy, bx.AxisZ, hz);
                AddFace(cy, bx.AxisX, hx, bx.AxisZ, hz);
                AddFace(cyn, bx.AxisX, hx, bx.AxisZ, hz);
                AddFace(cz, bx.AxisX, hx, bx.AxisY, hy);
                AddFace(czn, bx.AxisX, hx, bx.AxisY, hy);
            }

            // 渲染端口盒（6个面 + 入/出射口高亮）
            foreach (var pb in _scene.PortBoxes)
            {
                var hx = pb.Half.X; var hy = pb.Half.Y; var hz = pb.Half.Z;
                var cx = pb.Center + pb.AxisX * hx; var cxn = pb.Center - pb.AxisX * hx;
                var cy = pb.Center + pb.AxisY * hy; var cyn = pb.Center - pb.AxisY * hy;
                var cz = pb.Center + pb.AxisZ * hz; var czn = pb.Center - pb.AxisZ * hz;

                Color fill = Color.FromArgb(110, 50, 200, 210);
                Color wire = Color.FromArgb(220, 40, 130, 150);

                // 绘制单个面
                void AddFace(Vector3 c0, Vector3 u, float uLen, Vector3 v, float vLen)
                {
                    var A = c0 - u * uLen - v * vLen;
                    var B = c0 + u * uLen - v * vLen;
                    var C = c0 + u * uLen + v * vLen;
                    var D = c0 - u * uLen + v * vLen;
                    var q = new Quad { A = A, B = B, C = C, D = D };
                    polys.Add(Poly.FromQuad(q, AvgDepth(q, view), fill, wire));
                }

                AddFace(cx, pb.AxisY, hy, pb.AxisZ, hz);
                AddFace(cxn, pb.AxisY, hy, pb.AxisZ, hz);
                AddFace(cy, pb.AxisX, hx, pb.AxisZ, hz);
                AddFace(cyn, pb.AxisX, hx, pb.AxisZ, hz);
                AddFace(cz, pb.AxisX, hx, pb.AxisY, hy);
                AddFace(czn, pb.AxisX, hx, pb.AxisY, hy);

                // 绘制入/出射口矩形
                DrawPortRect(pb, pb.InFace, vp, b, ctx, Color.FromArgb(180, 255, 180, 90));
                DrawPortRect(pb, pb.OutFace, vp, b, ctx, Color.FromArgb(180, 90, 180, 255));
            }

            // 4. 深度排序（从后到前）并绘制多边形
            polys.Sort((a, bp) => bp.AvgDepth.CompareTo(a.AvgDepth));
            foreach (var p in polys)
            {
                DrawQuad(ctx, p, vp, b);
                DrawQuadWire(ctx, p, vp, b);
            }

            // 5. 绘制光线
            _scene.UpdateRayPath();
            var segs = _scene.RayPath.Segments
                .Select(s => new Seg
                {
                    A = s.Start,
                    B = s.End,
                    Color = ToColor(s.LayerColor),
                    Depth = MidDepth(s.Start, s.End, view),
                    Thick = 3.0
                })
                .OrderByDescending(s => s.Depth).ToList();

            foreach (var s in segs)
            {
                DrawWorldLine(ctx, vp, b, s.A, s.B, s.Color, s.Thick);
            }

            // 6. 绘制光线命中点（十字标记）
            for (int i = 0; i < _scene.RayPath.Segments.Count - 1; i++)
            {
                DrawCross(ctx, vp, b, _scene.RayPath.Segments[i].End, Color.FromRgb(30, 30, 30));
            }

            // 7. 绘制端口盒轴向箭头
            foreach (var bx in _scene.PortBoxes)
            {
                var c = bx.Center;
                DrawArrow(ctx, vp, b, c, c + bx.AxisX * 1.0f, Color.FromRgb(230, 90, 90), 2);
                DrawArrow(ctx, vp, b, c, c + bx.AxisY * 1.0f, Color.FromRgb(90, 230, 90), 2);
                DrawArrow(ctx, vp, b, c, c + bx.AxisZ * 1.0f, Color.FromRgb(90, 90, 230), 2);
            }

            // 8. 绘制世界坐标系轴向
            DrawWorldLine(ctx, vp, b, Vector3.Zero, new Vector3(2, 0, 0), Color.FromRgb(255, 0, 0), 2);
            DrawWorldLine(ctx, vp, b, Vector3.Zero, new Vector3(0, 2, 0), Color.FromRgb(0, 165, 0), 2);
            DrawWorldLine(ctx, vp, b, Vector3.Zero, new Vector3(0, 0, 2), Color.FromRgb(0, 115, 255), 2);

            // 9. 绘制元素高亮（在所有元素之上）
            if (HighlightedElementId != Guid.Empty)
            {
                DrawHighlightForElementId(HighlightedElementId, ctx, vp, b);
            }

            // 10. 绘制调试用高亮矩形
            if (DebugHighlightQuad.HasValue)
            {
                var dq = DebugHighlightQuad.Value;
                var hlColor = Color.FromRgb(255, 100, 0);
                DrawQuadOutline(dq, ctx, vp, b, hlColor, 4.0);
            }
        }

        /// <summary>
        /// 绘制网格背景
        /// </summary>
        /// <param name="ctx">绘图上下文</param>
        /// <param name="vp">视图投影矩阵</param>
        /// <param name="b">视口边界</param>
        /// <param name="half">网格半长（-half ~ half）</param>
        /// <param name="step">网格步长</param>
        /// <param name="c">网格颜色</param>
        private void DrawGrid(DrawingContext ctx, Matrix4x4 vp, Rect b, int half, float step, Color c)
        {
            for (int i = -half; i <= half; i++)
            {
                float x = i * step;
                // 绘制X轴方向网格线
                DrawWorldLine(ctx, vp, b, new Vector3(-half * step, 0, x), new Vector3(half * step, 0, x), c, 1);
                // 绘制Z轴方向网格线
                DrawWorldLine(ctx, vp, b, new Vector3(x, 0, -half * step), new Vector3(x, 0, half * step), c, 1);
            }
        }

        /// <summary>
        /// 绘制带箭头的线段
        /// </summary>
        /// <param name="ctx">绘图上下文</param>
        /// <param name="vp">视图投影矩阵</param>
        /// <param name="b">视口边界</param>
        /// <param name="a">线段起点</param>
        /// <param name="to">线段终点</param>
        /// <param name="c">线段颜色</param>
        /// <param name="thick">线段宽度</param>
        private void DrawArrow(DrawingContext ctx, Matrix4x4 vp, Rect b, Vector3 a, Vector3 to, Color c, double thick)
        {
            // 绘制主线段
            DrawWorldLine(ctx, vp, b, a, to, c, thick);

            // 计算箭头方向和辅助向量
            var dir = Vector3.Normalize(to - a);
            var right = Vector3.Normalize(Vector3.Cross(dir, Vector3.UnitY));
            if (right.LengthSquared() < 1e-6) right = Vector3.Normalize(Vector3.Cross(dir, Vector3.UnitX));
            var up = Vector3.Normalize(Vector3.Cross(right, dir));

            // 绘制箭头头部
            float s = 0.1f;
            var tip = to - dir * 0.25f;
            DrawWorldLine(ctx, vp, b, to, tip + right * s, c, thick);
            DrawWorldLine(ctx, vp, b, to, tip - right * s, c, thick);
            DrawWorldLine(ctx, vp, b, to, tip + up * s, c, thick);
            DrawWorldLine(ctx, vp, b, to, tip - up * s, c, thick);
        }

        /// <summary>
        /// 绘制四边形填充
        /// </summary>
        /// <param name="ctx">绘图上下文</param>
        /// <param name="poly">多边形数据</param>
        /// <param name="vp">视图投影矩阵</param>
        /// <param name="b">视口边界</param>
        private void DrawQuad(DrawingContext ctx, Poly poly, Matrix4x4 vp, Rect b)
        {
            var pA = Project(poly.A, vp, b);
            var pB = Project(poly.B, vp, b);
            var pC = Project(poly.C, vp, b);
            var pD = Project(poly.D, vp, b);

            // 任意顶点超出视口则不绘制
            if (pA == null || pB == null || pC == null || pD == null) return;

            // 构建几何路径
            var geo = new StreamGeometry();
            using (var g = geo.Open())
            {
                g.BeginFigure(pA.Value, true);
                g.LineTo(pB.Value);
                g.LineTo(pC.Value);
                g.LineTo(pD.Value);
                g.EndFigure(true);
            }

            // 绘制填充
            ctx.DrawGeometry(new SolidColorBrush(poly.Fill), null, geo);
        }

        /// <summary>
        /// 绘制四边形轮廓
        /// </summary>
        /// <param name="ctx">绘图上下文</param>
        /// <param name="poly">多边形数据</param>
        /// <param name="vp">视图投影矩阵</param>
        /// <param name="b">视口边界</param>
        private void DrawQuadWire(DrawingContext ctx, Poly poly, Matrix4x4 vp, Rect b)
        {
            var pA = Project(poly.A, vp, b);
            var pB = Project(poly.B, vp, b);
            var pC = Project(poly.C, vp, b);
            var pD = Project(poly.D, vp, b);

            if (pA == null || pB == null || pC == null || pD == null) return;

            // 创建画笔
            var pen = new Pen(new SolidColorBrush(poly.Wire), 1.0);

            // 绘制四条边
            ctx.DrawLine(pen, pA.Value, pB.Value);
            ctx.DrawLine(pen, pB.Value, pC.Value);
            ctx.DrawLine(pen, pC.Value, pD.Value);
            ctx.DrawLine(pen, pD.Value, pA.Value);
        }

        /// <summary>
        /// 绘制世界空间线段
        /// </summary>
        /// <param name="ctx">绘图上下文</param>
        /// <param name="vp">视图投影矩阵</param>
        /// <param name="b">视口边界</param>
        /// <param name="a">线段起点（世界坐标）</param>
        /// <param name="d">线段终点（世界坐标）</param>
        /// <param name="c">线段颜色</param>
        /// <param name="thick">线段宽度</param>
        private void DrawWorldLine(DrawingContext ctx, Matrix4x4 vp, Rect b, Vector3 a, Vector3 d, Color c, double thick)
        {
            var pa = Project(a, vp, b);
            var pb = Project(d, vp, b);

            if (pa == null || pb == null) return;

            // 绘制线段
            ctx.DrawLine(new Pen(new SolidColorBrush(c), thick), pa.Value, pb.Value);
        }

        /// <summary>
        /// 绘制十字标记（用于命中点）
        /// </summary>
        /// <param name="ctx">绘图上下文</param>
        /// <param name="vp">视图投影矩阵</param>
        /// <param name="b">视口边界</param>
        /// <param name="p">十字中心（世界坐标）</param>
        /// <param name="c">十字颜色</param>
        private void DrawCross(DrawingContext ctx, Matrix4x4 vp, Rect b, Vector3 p, Color c)
        {
            double s = 0.08;
            // 水平十字线
            DrawWorldLine(ctx, vp, b, p + new Vector3(-(float)s, 0, 0), p + new Vector3((float)s, 0, 0), c, 1.5);
            // 垂直十字线
            DrawWorldLine(ctx, vp, b, p + new Vector3(0, -(float)s, 0), p + new Vector3(0, (float)s, 0), c, 1.5);
        }

        /// <summary>
        /// 绘制端口盒的入/出射口矩形
        /// </summary>
        /// <param name="pb">端口盒实例</param>
        /// <param name="f">面类型（入/出射面）</param>
        /// <param name="vp">视图投影矩阵</param>
        /// <param name="b">视口边界</param>
        /// <param name="ctx">绘图上下文</param>
        /// <param name="c">矩形颜色</param>
        private void DrawPortRect(PortBox pb, Face f, Matrix4x4 vp, Rect b, DrawingContext ctx, Color c)
        {
            pb.GetFace(f, out var n, out var center, out var u, out var v, out var uLen, out var vLen);

            // 计算端口矩形半宽/半高（不超过面大小）
            float halfW = MathF.Min(pb.ApertureW * 0.5f, uLen);
            float halfH = MathF.Min(pb.ApertureH * 0.5f, vLen);

            // 构建矩形顶点
            var A = center - u * halfW - v * halfH;
            var B = center + u * halfW - v * halfH;
            var C = center + u * halfW + v * halfH;
            var D = center - u * halfW + v * halfH;

            // 投影到屏幕坐标
            var pA = Project(A, vp, b);
            var pB = Project(B, vp, b);
            var pC = Project(C, vp, b);
            var pD = Project(D, vp, b);

            if (pA == null || pB == null || pC == null || pD == null) return;

            // 绘制填充矩形
            var geo = new StreamGeometry();
            using (var g = geo.Open())
            {
                g.BeginFigure(pA.Value, true);
                g.LineTo(pB.Value);
                g.LineTo(pC.Value);
                g.LineTo(pD.Value);
                g.EndFigure(true);
            }

            ctx.DrawGeometry(new SolidColorBrush(c), new Pen(new SolidColorBrush(Color.FromRgb(30, 30, 30)), 1), geo);
        }

        /// <summary>
        /// 根据元素ID绘制高亮轮廓
        /// </summary>
        /// <param name="id">元素ID</param>
        /// <param name="ctx">绘图上下文</param>
        /// <param name="vp">视图投影矩阵</param>
        /// <param name="b">视口边界</param>
        private void DrawHighlightForElementId(Guid id, DrawingContext ctx, Matrix4x4 vp, Rect b)
        {
            // 高亮颜色（金色）和宽度
            var hlColor = Color.FromRgb(255, 215, 0);
            float thick = 4.0f;

            // 1. 检测镜子
            var m = _scene.Mirrors.FirstOrDefault(x => x.ElementId == id);
            if (m != null)
            {
                DrawQuadOutline(m.Quad, ctx, vp, b, hlColor, thick);
                return;
            }

            // 2. 检测透镜
            var l = _scene.Lenses.FirstOrDefault(x => x.ElementId == id);
            if (l != null)
            {
                DrawQuadOutline(l.Quad, ctx, vp, b, hlColor, thick);
                return;
            }

            // 3. 检测探测器
            var d = _scene.Detectors.FirstOrDefault(x => x.ElementId == id);
            if (d != null)
            {
                DrawQuadOutline(d.Quad, ctx, vp, b, hlColor, thick);
                return;
            }

            // 4. 检测普通盒子
            var bx = _scene.Boxes.FirstOrDefault(x => x.ElementId == id);
            if (bx != null)
            {
                DrawBoxOutline(bx, ctx, vp, b, hlColor, thick);
                return;
            }

            // 5. 检测端口盒
            var pb = _scene.PortBoxes.FirstOrDefault(x => x.ElementId == id);
            if (pb != null)
            {
                DrawBoxOutline(pb, ctx, vp, b, hlColor, thick);
                return;
            }

            // 6. 检测光源
            if (_scene.Source != null && _scene.Source.ElementId == id)
            {
                DrawCross(ctx, vp, b, _scene.Source.Origin, hlColor);
                return;
            }
        }

        /// <summary>
        /// 绘制盒子的高亮轮廓（支持BoxOptic和PortBox）
        /// </summary>
        /// <param name="bx">盒子实例（动态类型）</param>
        /// <param name="ctx">绘图上下文</param>
        /// <param name="vp">视图投影矩阵</param>
        /// <param name="b">视口边界</param>
        /// <param name="c">轮廓颜色</param>
        /// <param name="thickness">轮廓宽度</param>
        private void DrawBoxOutline(dynamic bx, DrawingContext ctx, Matrix4x4 vp, Rect b, Color c, double thickness)
        {
            var hx = bx.Half.X; var hy = bx.Half.Y; var hz = bx.Half.Z;
            var cx = bx.Center + bx.AxisX * hx; var cxn = bx.Center - bx.AxisX * hx;
            var cy = bx.Center + bx.AxisY * hy; var cyn = bx.Center - bx.AxisY * hy;
            var cz = bx.Center + bx.AxisZ * hz; var czn = bx.Center - bx.AxisZ * hz;

            // 绘制单个面的轮廓
            void DrawFace(Vector3 c0, Vector3 u, float uLen, Vector3 v, float vLen)
            {
                var A = c0 - u * uLen - v * vLen;
                var B = c0 + u * uLen - v * vLen;
                var C = c0 + u * uLen + v * vLen;
                var D = c0 - u * uLen + v * vLen;
                DrawQuadOutline(new Quad { A = A, B = B, C = C, D = D }, ctx, vp, b, c, thickness);
            }

            // 绘制6个面的轮廓
            DrawFace(cx, bx.AxisY, hy, bx.AxisZ, hz);
            DrawFace(cxn, bx.AxisY, hy, bx.AxisZ, hz);
            DrawFace(cy, bx.AxisX, hx, bx.AxisZ, hz);
            DrawFace(cyn, bx.AxisX, hx, bx.AxisZ, hz);
            DrawFace(cz, bx.AxisX, hx, bx.AxisY, hy);
            DrawFace(czn, bx.AxisX, hx, bx.AxisY, hy);
        }

        /// <summary>
        /// 绘制四边形轮廓（用于高亮/调试）
        /// </summary>
        /// <param name="q">四边形</param>
        /// <param name="ctx">绘图上下文</param>
        /// <param name="vp">视图投影矩阵</param>
        /// <param name="b">视口边界</param>
        /// <param name="c">轮廓颜色</param>
        /// <param name="thickness">轮廓宽度</param>
        private void DrawQuadOutline(Quad q, DrawingContext ctx, Matrix4x4 vp, Rect b, Color c, double thickness)
        {
            var pA = Project(q.A, vp, b);
            var pB = Project(q.B, vp, b);
            var pC = Project(q.C, vp, b);
            var pD = Project(q.D, vp, b);

            if (pA == null || pB == null || pC == null || pD == null) return;

            // 创建圆角画笔
            var pen = new Pen(new SolidColorBrush(c), thickness)
            {
                LineJoin = PenLineJoin.Round,
                LineCap = PenLineCap.Round
            };

            // 绘制四条边
            ctx.DrawLine(pen, pA.Value, pB.Value);
            ctx.DrawLine(pen, pB.Value, pC.Value);
            ctx.DrawLine(pen, pC.Value, pD.Value);
            ctx.DrawLine(pen, pD.Value, pA.Value);
        }
        #endregion

        #region 内部辅助类
        /// <summary>
        /// 多边形渲染数据（包含深度、颜色、顶点）
        /// </summary>
        private sealed class Poly
        {
            public Vector3 A, B, C, D;
            public float AvgDepth;
            public Color Fill, Wire;

            /// <summary>
            /// 从四边形创建多边形渲染数据
            /// </summary>
            /// <param name="q">四边形</param>
            /// <param name="avgDepth">平均深度</param>
            /// <param name="fill">填充颜色</param>
            /// <param name="wire">轮廓颜色</param>
            /// <returns>多边形渲染数据</returns>
            public static Poly FromQuad(Quad q, float avgDepth, Color fill, Color wire)
            {
                return new Poly
                {
                    A = q.A,
                    B = q.B,
                    C = q.C,
                    D = q.D,
                    AvgDepth = avgDepth,
                    Fill = fill,
                    Wire = wire
                };
            }

            /// <summary>
            /// 将四边形沿法向量挤出成棱柱（增加视觉厚度）
            /// </summary>
            /// <param name="q">原始四边形</param>
            /// <param name="avgDepth">平均深度</param>
            /// <param name="thickness">挤出厚度</param>
            /// <param name="fill">填充颜色</param>
            /// <param name="wire">轮廓颜色</param>
            /// <returns>棱柱的6个面多边形</returns>
            public static IEnumerable<Poly> FromPrism(Quad q, float avgDepth, float thickness, Color fill, Color wire)
            {
                thickness = MathF.Max(0.01f, thickness);

                // 计算四边形法向量
                var n = Vector3.Normalize(Vector3.Cross(q.B - q.A, q.D - q.A));
                if (n.LengthSquared() < 1e-8f) n = Vector3.UnitY;

                // 计算挤出偏移量
                var off = n * thickness;

                // 挤出后的四边形（背面）
                var q2 = new Quad { A = q.A + off, B = q.B + off, C = q.C + off, D = q.D + off };

                // 返回6个面：正面、背面、4个侧面
                yield return FromQuad(q, avgDepth, fill, wire);
                yield return FromQuad(q2, avgDepth, fill, wire);
                yield return FromQuad(new Quad { A = q.A, B = q.B, C = q2.B, D = q2.A }, avgDepth, fill, wire);
                yield return FromQuad(new Quad { A = q.B, B = q.C, C = q2.C, D = q2.B }, avgDepth, fill, wire);
                yield return FromQuad(new Quad { A = q.C, B = q.D, C = q2.D, D = q2.C }, avgDepth, fill, wire);
                yield return FromQuad(new Quad { A = q.D, B = q.A, C = q2.A, D = q2.D }, avgDepth, fill, wire);
            }
        }

        /// <summary>
        /// 线段渲染数据（用于光线绘制）
        /// </summary>
        private sealed class Seg
        {
            public Vector3 A, B;
            public Color Color;
            public double Thick;
            public float Depth;
        }
        #endregion
    }

    /// <summary>
    /// 3D相机类（负责视角计算）
    /// </summary>
    public struct Camera
    {
        /// <summary>
        /// 相机目标点（注视点）
        /// </summary>
        public Vector3 Target;

        /// <summary>
        /// 相机到目标点的距离
        /// </summary>
        public float Distance;

        /// <summary>
        /// 相机偏航角（水平旋转，单位：度）
        /// </summary>
        public float Yaw;

        /// <summary>
        /// 相机俯仰角（垂直旋转，单位：度）
        /// </summary>
        public float Pitch;

        /// <summary>
        /// 创建默认相机（初始视角）
        /// </summary>
        /// <returns>默认相机实例</returns>
        public static Camera Default()
        {
            return new Camera
            {
                Target = new Vector3(0, 2.2f, 0),
                Distance = 18f,
                Yaw = 35f,
                Pitch = -20f
            };
        }

        /// <summary>
        /// 计算相机视图矩阵
        /// </summary>
        /// <returns>视图矩阵</returns>
        public Matrix4x4 ViewMatrix()
        {
            // 转换为弧度
            float yaw = Yaw * MathF.PI / 180f;
            float pitch = Pitch * MathF.PI / 180f;

            // 计算相机朝向
            float cy = MathF.Cos(yaw), sy = MathF.Sin(yaw);
            float cp = MathF.Cos(pitch), sp = MathF.Sin(pitch);
            Vector3 dir = Vector3.Normalize(new Vector3(cp * cy, sp, cp * sy));

            // 计算相机位置
            Vector3 eye = Target - dir * Distance;

            // 创建LookAt矩阵
            return LookAt(eye, Target, Vector3.UnitY);
        }

        /// <summary>
        /// 创建LookAt视图矩阵
        /// </summary>
        /// <param name="eye">相机位置</param>
        /// <param name="center">注视点</param>
        /// <param name="up">上方向</param>
        /// <returns>视图矩阵</returns>
        public static Matrix4x4 LookAt(Vector3 eye, Vector3 center, Vector3 up)
        {
            // 计算前/右/上方向
            Vector3 f = Vector3.Normalize(center - eye);
            Vector3 s = Vector3.Normalize(Vector3.Cross(f, up));
            Vector3 u = Vector3.Cross(s, f);

            // 构建旋转矩阵
            var rot = new Matrix4x4(
                s.X, u.X, -f.X, 0,
                s.Y, u.Y, -f.Y, 0,
                s.Z, u.Z, -f.Z, 0,
                0, 0, 0, 1);

            // 构建平移矩阵
            var trans = Matrix4x4.CreateTranslation(-eye);

            // 视图矩阵 = 平移 × 旋转
            return trans * rot;
        }
    }

}