using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpticalChainSimulator
{
    /// <summary>
    /// 光学场景核心类：管理所有光学元件、光源和光线追踪路径
    /// </summary>
    public sealed class Scene
    {
        /// <summary>
        /// 场景中的平面层集合
        /// </summary>
        public List<Layer> Layers { get; } = new();

        /// <summary>
        /// 场景中的镜面集合
        /// </summary>
        public List<Mirror> Mirrors { get; } = new();

        /// <summary>
        /// 场景中的薄透镜集合
        /// </summary>
        public List<ThinLens> Lenses { get; } = new();

        /// <summary>
        /// 场景中的探测器集合
        /// </summary>
        public List<Detector> Detectors { get; } = new();

        /// <summary>
        /// 场景中的普通方盒光学元件集合（支持反射/折射/吸收）
        /// </summary>
        public List<BoxOptic> Boxes { get; } = new();

        /// <summary>
        /// 场景中的端口盒集合（带入/出射口+光线处理算法）
        /// </summary>
        public List<PortBox> PortBoxes { get; } = new();

        /// <summary>
        /// 光源定义（起点、方向、最大反射次数）
        /// </summary>
        public RayDefinition Source { get; set; } = new RayDefinition();

        /// <summary>
        /// 光线追踪后的路径数据
        /// </summary>
        public RayPath RayPath { get; } = new RayPath();

        /// <summary>
        /// 创建默认测试场景（包含镜面、透镜、探测器等示例元件）
        /// </summary>
        /// <returns>初始化后的场景实例</returns>
        public static Scene CreateDefault()
        {
            var scene = new Scene();

            // 添加两块镜面
            scene.Mirrors.Add(Mirror.MakeRect(
                center: new Vector3(-2.0f, 0.01f, -1.5f),
                normal: Vector3.Normalize(new Vector3(0.2f, 0.9f, 0.3f)),
                width: 3.6f, height: 2.6f));
            scene.Mirrors.Add(Mirror.MakeRect(
                center: new Vector3(3.2f, 2.31f, 1.2f),
                normal: Vector3.Normalize(new Vector3(-0.1f, 0.95f, -0.2f)),
                width: 3.6f, height: 2.6f));

            // 添加薄透镜
            scene.Lenses.Add(ThinLens.MakeRect(
                center: new Vector3(4f, 3.9f, 2f),
                normal: Vector3.UnitY,
                width: 2.8f, height: 2.2f,
                focalLength: 3.5f));

            // 添加探测器
            scene.Detectors.Add(Detector.MakeRect(
                center: new Vector3(5f, 5f, 2f),
                normal: Vector3.UnitY,
                width: 8f, height: 2.0f));

            // 初始化光源参数
            scene.Source = new RayDefinition
            {
                Origin = new Vector3(0f, 0f, 0f),          // 光源起点
                Direction = Vector3.Normalize(new Vector3(1f, 0f, 0f)), // 初始方向
                MaxBounces = 6                             // 最大反射/折射次数
            };

            // 初始追踪光线路径
            scene.UpdateRayPath();
            return scene;
        }

        /// <summary>
        /// 核心方法：执行光线追踪，更新光线路径数据
        /// 逻辑说明：从光源出发，逐段检测与光学元件的交点，根据元件类型更新光线方向，直到达到最大步数或无交点
        /// </summary>
        public void UpdateRayPath()
        {
            // 清空原有路径
            RayPath.Segments.Clear();

            // 防御性检查：避免空引用或无效参数导致崩溃
            if (Source == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdateRayPath: 光源为空，跳过光线追踪");
                return;
            }
            if (Source.MaxBounces <= 0)
            {
                System.Diagnostics.Debug.WriteLine("UpdateRayPath: 最大反射次数≤0，跳过光线追踪");
                return;
            }
            if (Source.Direction.LengthSquared() < 1e-12f)
            {
                System.Diagnostics.Debug.WriteLine("UpdateRayPath: 光源方向为零向量，跳过光线追踪");
                return;
            }

            // 初始化光线起点和方向（确保方向向量归一化）
            Vector3 rayOrigin = Source.Origin;
            Vector3 rayDir = Vector3.Normalize(Source.Direction);
            // 最大追踪步数（防止无限循环，取最大反射次数的5倍）
            int maxSteps = Math.Max(1, Source.MaxBounces * 5);

            // 逐步追踪光线
            for (int step = 0; step < maxSteps; step++)
            {
                // 初始化交点检测结果：记录最近的有效交点
                float bestT = float.PositiveInfinity;       // 光线到交点的距离（t参数）
                HitType bestHitType = HitType.None;         // 命中的元件类型
                int bestIndex = -1;                         // 命中元件在集合中的索引
                Vector3 bestHitPoint = default;             // 交点坐标
                Vector3 bestNormal = default;               // 交点处的法向量
                bool fromInside = false;                    // 是否从元件内部命中（仅用于方盒）

                // 1. 检测与镜面的交点
                for (int i = 0; i < Mirrors.Count; i++)
                {
                    var mirror = Mirrors[i];
                    // 检测光线与镜面平面的交点
                    if (!RayPlaneIntersection(rayOrigin, rayDir, mirror.Point, mirror.Normal, out float t) || t <= 1e-4f)
                        continue;
                    // 计算交点坐标并验证是否在镜面范围内
                    Vector3 hitPoint = rayOrigin + rayDir * t;
                    if (!mirror.ContainsPoint(hitPoint))
                        continue;
                    // 更新最近交点
                    if (t < bestT)
                    {
                        bestT = t;
                        bestHitType = HitType.Mirror;
                        bestIndex = i;
                        bestHitPoint = hitPoint;
                        bestNormal = mirror.Normal;
                    }
                }

                // 2. 检测与薄透镜的交点
                for (int i = 0; i < Lenses.Count; i++)
                {
                    var lens = Lenses[i];
                    if (!RayPlaneIntersection(rayOrigin, rayDir, lens.Point, lens.Normal, out float t) || t <= 1e-4f)
                        continue;
                    Vector3 hitPoint = rayOrigin + rayDir * t;
                    if (!lens.ContainsPoint(hitPoint))
                        continue;
                    if (t < bestT)
                    {
                        bestT = t;
                        bestHitType = HitType.Lens;
                        bestIndex = i;
                        bestHitPoint = hitPoint;
                        bestNormal = lens.Normal;
                    }
                }

                // 3. 检测与探测器的交点
                for (int i = 0; i < Detectors.Count; i++)
                {
                    var detector = Detectors[i];
                    if (!RayPlaneIntersection(rayOrigin, rayDir, detector.Point, detector.Normal, out float t) || t <= 1e-4f)
                        continue;
                    Vector3 hitPoint = rayOrigin + rayDir * t;
                    if (!detector.ContainsPoint(hitPoint))
                        continue;
                    if (t < bestT)
                    {
                        bestT = t;
                        bestHitType = HitType.Detector;
                        bestIndex = i;
                        bestHitPoint = hitPoint;
                        bestNormal = detector.Normal;
                    }
                }

                // 4. 检测与普通方盒的交点
                for (int i = 0; i < Boxes.Count; i++)
                {
                    var box = Boxes[i];
                    if (!box.RayIntersect(rayOrigin, rayDir, out float t, out Vector3 normal, out _, out bool inside) || t <= 1e-4f)
                        continue;
                    if (t < bestT)
                    {
                        bestT = t;
                        bestHitType = HitType.Box;
                        bestIndex = i;
                        bestHitPoint = rayOrigin + rayDir * t;
                        bestNormal = normal;
                        fromInside = inside;
                    }
                }

                // 5. 检测与端口盒入射口的交点
                for (int i = 0; i < PortBoxes.Count; i++)
                {
                    var portBox = PortBoxes[i];
                    if (!portBox.IntersectInput(rayOrigin, rayDir, out float t, out Vector3 hitPoint, out Vector3 normal) || t <= 1e-4f)
                        continue;
                    if (t < bestT)
                    {
                        bestT = t;
                        bestHitType = HitType.PortBox;
                        bestIndex = i;
                        bestHitPoint = hitPoint;
                        bestNormal = normal;
                    }
                }

                // 无有效交点：光线延伸至60单位处并结束追踪
                if (bestHitType == HitType.None)
                {
                    Vector3 endPoint = rayOrigin + rayDir * 60f;
                    RayPath.Segments.Add(new RaySegment
                    {
                        Start = rayOrigin,
                        End = endPoint,
                        LayerColor = GuessLayerColorForY(endPoint.Y)
                    });
                    break;
                }

                // 记录当前光线段（从起点到交点）
                RayPath.Segments.Add(new RaySegment
                {
                    Start = rayOrigin,
                    End = bestHitPoint,
                    LayerColor = GuessLayerColorForY(bestHitPoint.Y)
                });

                // 根据命中的元件类型更新光线状态
                switch (bestHitType)
                {
                    // 镜面：反射光线
                    case HitType.Mirror:
                        rayDir = Vector3.Reflect(rayDir, bestNormal);
                        rayOrigin = bestHitPoint + rayDir * 0.002f; // 偏移避免重复命中同一平面
                        break;

                    // 薄透镜：折射光线（应用透镜焦距计算新方向）
                    case HitType.Lens:
                        rayDir = Lenses[bestIndex].Transmit(rayDir, bestHitPoint);
                        rayOrigin = bestHitPoint + rayDir * 0.002f;
                        break;

                    // 探测器：不终止追踪（光线穿过探测器继续传播）
                    case HitType.Detector:
                        rayOrigin = bestHitPoint + rayDir * 0.002f;
                        break;

                    // 普通方盒：根据类型执行反射/折射/吸收
                    case HitType.Box:
                        var boxOptic = Boxes[bestIndex];
                        // 吸收型：终止追踪
                        if (boxOptic.Kind == BoxKind.Absorber)
                            return;
                        // 反射型：执行反射
                        else if (boxOptic.Kind == BoxKind.Reflector)
                        {
                            rayDir = Vector3.Reflect(rayDir, bestNormal);
                            rayOrigin = bestHitPoint + rayDir * 0.002f;
                        }
                        // 折射型：计算折射（全反射时退化为反射）
                        else
                        {
                            float n1 = fromInside ? boxOptic.RefrIndex : 1.0f; // 入射介质折射率
                            float n2 = fromInside ? 1.0f : boxOptic.RefrIndex; // 出射介质折射率
                            if (!Refract(rayDir, bestNormal, n1, n2, out Vector3 refractedDir))
                                rayDir = Vector3.Reflect(rayDir, bestNormal); // 全反射
                            else
                                rayDir = refractedDir;
                            rayOrigin = bestHitPoint + rayDir * 0.002f;
                        }
                        break;

                    // 端口盒：执行自定义处理算法（如偏转）并更新光线起点/方向
                    case HitType.PortBox:
                        var portBoxOptic = PortBoxes[bestIndex];
                        portBoxOptic.Process(bestHitPoint, rayDir, out Vector3 newOrigin, out Vector3 newDir);
                        rayOrigin = newOrigin;
                        rayDir = newDir;
                        break;
                }
            }
        }

        #region 工具方法

        /// <summary>
        /// 计算光线折射方向（斯内尔定律）
        /// </summary>
        /// <param name="inDir">入射光线方向（需归一化）</param>
        /// <param name="normal">交点法向量（需归一化，指向入射介质）</param>
        /// <param name="n1">入射介质折射率</param>
        /// <param name="n2">出射介质折射率</param>
        /// <param name="outDir">输出：折射光线方向（归一化）</param>
        /// <returns>是否成功折射（false表示全反射）</returns>
        private static bool Refract(Vector3 inDir, Vector3 normal, float n1, float n2, out Vector3 outDir)
        {
            inDir = Vector3.Normalize(inDir);
            normal = Vector3.Normalize(normal);

            float eta = n1 / n2;                     // 折射率比
            float cosIncident = -Vector3.Dot(normal, inDir); // 入射角余弦值
            float k = 1 - eta * eta * (1 - cosIncident * cosIncident);

            // k<0 表示全反射，返回false
            if (k < 0)
            {
                outDir = default;
                return false;
            }

            // 计算折射方向并归一化
            outDir = Vector3.Normalize(eta * inDir + (eta * cosIncident - MathF.Sqrt(k)) * normal);
            return true;
        }

        /// <summary>
        /// 检测光线与平面的交点
        /// </summary>
        /// <param name="rayOrigin">光线起点</param>
        /// <param name="rayDir">光线方向（需归一化）</param>
        /// <param name="planePoint">平面上任意一点</param>
        /// <param name="planeNormal">平面法向量（需归一化）</param>
        /// <param name="t">输出：光线参数t（交点=rayOrigin + rayDir*t）</param>
        /// <returns>是否存在有效交点（t≥0）</returns>
        private static bool RayPlaneIntersection(Vector3 rayOrigin, Vector3 rayDir, Vector3 planePoint, Vector3 planeNormal, out float t)
        {
            t = 0;
            // 计算光线方向与平面法向量的点积（分母）
            float denominator = Vector3.Dot(rayDir, planeNormal);
            // 光线与平面平行（无交点或共面）
            if (MathF.Abs(denominator) < 1e-6f)
                return false;

            // 计算t参数（光线到平面的距离）
            t = Vector3.Dot(planePoint - rayOrigin, planeNormal) / denominator;
            // 仅返回t≥0的有效交点（光线前进方向）
            return t >= 0;
        }

        /// <summary>
        /// 根据Y坐标猜测分层颜色（用于可视化）
        /// </summary>
        /// <param name="y">Y轴坐标</param>
        /// <returns>对应的RGB颜色向量（0~1范围）</returns>
        private static Vector3 GuessLayerColorForY(float y)
        {
            if (y < 1.15f) return new Vector3(0.9f, 0.2f, 0.2f);  // 红色层
            if (y < 3.45f) return new Vector3(0.2f, 0.7f, 0.2f);  // 绿色层
            return new Vector3(0.2f, 0.5f, 0.9f);                 // 蓝色层
        }

        #endregion
    }

    #region 枚举定义

    /// <summary>
    /// 光线命中的元件类型枚举
    /// </summary>
    public enum HitType
    {
        None,       // 无命中
        Mirror,     // 镜面
        Lens,       // 薄透镜
        Detector,   // 探测器
        Box,        // 普通方盒
        PortBox     // 端口盒
    }

    /// <summary>
    /// 普通方盒的光学行为类型
    /// </summary>
    public enum BoxKind
    {
        Reflector,  // 反射
        Refractor,  // 折射
        Absorber    // 吸收
    }

    /// <summary>
    /// 端口盒的面枚举（用于指定入/出射口）
    /// </summary>
    public enum Face
    {
        XPos,   // X轴正方向面
        XNeg,   // X轴负方向面
        YPos,   // Y轴正方向面
        YNeg,   // Y轴负方向面
        ZPos,   // Z轴正方向面
        ZNeg    // Z轴负方向面
    }

    /// <summary>
    /// 端口盒的光线处理算法类型
    /// </summary>
    public enum ProcessorKind
    {
        PassThrough = 0,  // 直通（方向不变）
        Deflect = 1       // 偏转（按偏航/俯仰角调整方向）
    }

    #endregion

    #region 光学元件类

    /// <summary>
    /// 平面层类（用于可视化分层）
    /// </summary>
    public sealed class Layer
    {
        /// <summary>
        /// 绑定到逻辑元件的唯一标识（为空表示未关联）
        /// </summary>
        public Guid ElementId { get; set; } = Guid.Empty;

        /// <summary>
        /// 层的原点
        /// </summary>
        public Vector3 Origin;

        /// <summary>
        /// 层的法向量
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// 层的四边形边界（用于碰撞检测）
        /// </summary>
        public Quad Quad;

        /// <summary>
        /// 层的显示颜色
        /// </summary>
        public Vector3 Color;

        /// <summary>
        /// 创建平面层实例
        /// </summary>
        /// <param name="origin">原点</param>
        /// <param name="normal">法向量</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="color">颜色</param>
        /// <returns>初始化后的层实例</returns>
        public static Layer Make(Vector3 origin, Vector3 normal, float width, float height, Vector3 color)
        {
            var (u, v) = BuildBasis(normal);
            u *= width * 0.5f;
            v *= height * 0.5f;

            return new Layer
            {
                Origin = origin,
                Normal = Vector3.Normalize(normal),
                Quad = new Quad
                {
                    A = origin - u - v,
                    B = origin + u - v,
                    C = origin + u + v,
                    D = origin - u + v
                },
                Color = color
            };
        }

        /// <summary>
        /// 基于法向量构建平面内的正交基向量（u和v）
        /// </summary>
        /// <param name="normal">平面法向量</param>
        /// <returns>平面内的两个正交基向量（u, v）</returns>
        public static (Vector3 u, Vector3 v) BuildBasis(Vector3 normal)
        {
            // 选择与法向量垂直的初始向量（优先Y轴，避免共线时选X轴）
            Vector3 u = Vector3.Normalize(Vector3.Cross(MathF.Abs(normal.Y) < 0.99f ? Vector3.UnitY : Vector3.UnitX, normal));
            // 计算第二个正交基向量
            Vector3 v = Vector3.Normalize(Vector3.Cross(normal, u));
            return (u, v);
        }
    }

    /// <summary>
    /// 镜面类（矩形镜面，支持碰撞检测和反射）
    /// </summary>
    public sealed class Mirror
    {
        /// <summary>
        /// 绑定到逻辑元件的唯一标识（为空表示未关联）
        /// </summary>
        public Guid ElementId { get; set; } = Guid.Empty;

        /// <summary>
        /// 镜面中心点
        /// </summary>
        public Vector3 Point;

        /// <summary>
        /// 镜面法向量
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// 镜面的四边形边界
        /// </summary>
        public Quad Quad;

        /// <summary>
        /// 镜面宽度
        /// </summary>
        public float Width;

        /// <summary>
        /// 镜面高度
        /// </summary>
        public float Height;

        /// <summary>
        /// 创建矩形镜面实例
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="normal">法向量</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>初始化后的镜面实例</returns>
        public static Mirror MakeRect(Vector3 center, Vector3 normal, float width, float height)
        {
            var mirror = new Mirror
            {
                Point = center,
                Normal = Vector3.Normalize(normal),
                Width = width,
                Height = height
            };
            mirror.Rebuild();
            return mirror;
        }

        /// <summary>
        /// 重建镜面的四边形边界（修改尺寸/法向量后调用）
        /// </summary>
        public void Rebuild()
        {
            var (u, v) = Layer.BuildBasis(Normal);
            u *= Width * 0.5f;
            v *= Height * 0.5f;

            Quad = new Quad
            {
                A = Point - u - v,
                B = Point + u - v,
                C = Point + u + v,
                D = Point - u + v
            };
        }

        /// <summary>
        /// 检测点是否在镜面范围内（带容差）
        /// </summary>
        /// <param name="point">待检测点</param>
        /// <param name="eps">容差（默认5e-2）</param>
        /// <returns>是否在范围内</returns>
        public bool ContainsPoint(Vector3 point, float eps = 5e-2f)
        {
            // 将点投影到镜面平面
            float distance = Vector3.Dot(point - Point, Normal);
            Vector3 projectedPoint = point - Normal * distance;

            // 计算平面内的基向量和长度
            Vector3 u = (Quad.B - Quad.A) * 0.5f;
            Vector3 v = (Quad.D - Quad.A) * 0.5f;
            float uLength = u.Length();
            float vLength = v.Length();

            // 无效尺寸直接返回false
            if (uLength < 1e-6f || vLength < 1e-6f)
                return false;

            // 归一化基向量
            Vector3 uDir = u / uLength;
            Vector3 vDir = v / vLength;
            // 计算点在基向量上的投影
            Vector3 relativePoint = projectedPoint - Point;
            float du = Vector3.Dot(relativePoint, uDir);
            float dv = Vector3.Dot(relativePoint, vDir);

            // 检测投影是否在镜面范围内（带容差）
            return MathF.Abs(du) <= uLength + eps && MathF.Abs(dv) <= vLength + eps;
        }
    }

    /// <summary>
    /// 薄透镜类（矩形透镜，支持折射计算）
    /// </summary>
    public sealed class ThinLens
    {
        /// <summary>
        /// 绑定到逻辑元件的唯一标识（为空表示未关联）
        /// </summary>
        public Guid ElementId { get; set; } = Guid.Empty;

        /// <summary>
        /// 透镜中心点
        /// </summary>
        public Vector3 Point;

        /// <summary>
        /// 透镜法向量
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// 透镜的四边形边界
        /// </summary>
        public Quad Quad;

        /// <summary>
        /// 透镜宽度
        /// </summary>
        public float Width;

        /// <summary>
        /// 透镜高度
        /// </summary>
        public float Height;

        /// <summary>
        /// 透镜焦距
        /// </summary>
        public float FocalLength;

        /// <summary>
        /// 创建矩形薄透镜实例
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="normal">法向量</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="focalLength">焦距</param>
        /// <returns>初始化后的透镜实例</returns>
        public static ThinLens MakeRect(Vector3 center, Vector3 normal, float width, float height, float focalLength)
        {
            var lens = new ThinLens
            {
                Point = center,
                Normal = Vector3.Normalize(normal),
                Width = width,
                Height = height,
                FocalLength = focalLength
            };
            lens.Rebuild();
            return lens;
        }

        /// <summary>
        /// 重建透镜的四边形边界
        /// </summary>
        public void Rebuild()
        {
            var (u, v) = Layer.BuildBasis(Normal);
            u *= Width * 0.5f;
            v *= Height * 0.5f;

            Quad = new Quad
            {
                A = Point - u - v,
                B = Point + u - v,
                C = Point + u + v,
                D = Point - u + v
            };
        }

        /// <summary>
        /// 检测点是否在透镜范围内
        /// </summary>
        /// <param name="point">待检测点</param>
        /// <param name="eps">容差</param>
        /// <returns>是否在范围内</returns>
        public bool ContainsPoint(Vector3 point, float eps = 5e-2f)
        {
            // 同镜面的点检测逻辑
            float distance = Vector3.Dot(point - Point, Normal);
            Vector3 projectedPoint = point - Normal * distance;

            Vector3 u = (Quad.B - Quad.A) * 0.5f;
            Vector3 v = (Quad.D - Quad.A) * 0.5f;
            float uLength = u.Length();
            float vLength = v.Length();

            if (uLength < 1e-6f || vLength < 1e-6f)
                return false;

            Vector3 uDir = u / uLength;
            Vector3 vDir = v / vLength;
            Vector3 relativePoint = projectedPoint - Point;
            float du = Vector3.Dot(relativePoint, uDir);
            float dv = Vector3.Dot(relativePoint, vDir);

            return MathF.Abs(du) <= uLength + eps && MathF.Abs(dv) <= vLength + eps;
        }

        /// <summary>
        /// 计算光线穿过透镜后的折射方向
        /// </summary>
        /// <param name="inDir">入射光线方向</param>
        /// <param name="hitPoint">交点坐标</param>
        /// <returns>折射后的光线方向（归一化）</returns>
        public Vector3 Transmit(Vector3 inDir, Vector3 hitPoint)
        {
            // 构建透镜平面内的正交基
            var (uDir, vDir) = Layer.BuildBasis(Normal);
            // 计算交点相对于透镜中心的偏移
            Vector3 relativePoint = hitPoint - Point;
            float du = Vector3.Dot(relativePoint, uDir);
            float dv = Vector3.Dot(relativePoint, vDir);

            // 根据焦距计算方向偏移（薄透镜近似）
            Vector3 delta = (-du / FocalLength) * uDir + (-dv / FocalLength) * vDir;
            // 计算折射方向并归一化
            Vector3 outDir = Vector3.Normalize(inDir + delta);

            // 确保方向与法向量夹角为锐角
            if (Vector3.Dot(outDir, Normal) < 0)
                outDir = -outDir;

            return outDir;
        }
    }

    /// <summary>
    /// 探测器类（用于检测光线到达）
    /// </summary>
    public sealed class Detector
    {
        /// <summary>
        /// 绑定到逻辑元件的唯一标识（为空表示未关联）
        /// </summary>
        public Guid ElementId { get; set; } = Guid.Empty;

        /// <summary>
        /// 探测器中心点
        /// </summary>
        public Vector3 Point;

        /// <summary>
        /// 探测器法向量
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// 探测器的四边形边界
        /// </summary>
        public Quad Quad;

        /// <summary>
        /// 探测器宽度
        /// </summary>
        public float Width;

        /// <summary>
        /// 探测器高度
        /// </summary>
        public float Height;

        /// <summary>
        /// 创建矩形探测器实例
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="normal">法向量</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>初始化后的探测器实例</returns>
        public static Detector MakeRect(Vector3 center, Vector3 normal, float width, float height)
        {
            var detector = new Detector
            {
                Point = center,
                Normal = Vector3.Normalize(normal),
                Width = width,
                Height = height
            };
            detector.Rebuild();
            return detector;
        }

        /// <summary>
        /// 重建探测器的四边形边界
        /// </summary>
        public void Rebuild()
        {
            var (u, v) = Layer.BuildBasis(Normal);
            u *= Width * 0.5f;
            v *= Height * 0.5f;

            Quad = new Quad
            {
                A = Point - u - v,
                B = Point + u - v,
                C = Point + u + v,
                D = Point - u + v
            };
        }

        /// <summary>
        /// 检测点是否在探测器范围内
        /// </summary>
        /// <param name="point">待检测点</param>
        /// <param name="eps">容差</param>
        /// <returns>是否在范围内</returns>
        public bool ContainsPoint(Vector3 point, float eps = 5e-2f)
        {
            // 同镜面的点检测逻辑
            float distance = Vector3.Dot(point - Point, Normal);
            Vector3 projectedPoint = point - Normal * distance;

            Vector3 u = (Quad.B - Quad.A) * 0.5f;
            Vector3 v = (Quad.D - Quad.A) * 0.5f;
            float uLength = u.Length();
            float vLength = v.Length();

            if (uLength < 1e-6f || vLength < 1e-6f)
                return false;

            Vector3 uDir = u / uLength;
            Vector3 vDir = v / vLength;
            Vector3 relativePoint = projectedPoint - Point;
            float du = Vector3.Dot(relativePoint, uDir);
            float dv = Vector3.Dot(relativePoint, vDir);

            return MathF.Abs(du) <= uLength + eps && MathF.Abs(dv) <= vLength + eps;
        }
    }

    /// <summary>
    /// 普通方盒光学元件类（OBB包围盒，支持反射/折射/吸收）
    /// </summary>
    public sealed class BoxOptic
    {
        /// <summary>
        /// 绑定到逻辑元件的唯一标识（为空表示未关联）
        /// </summary>
        public Guid ElementId { get; set; } = Guid.Empty;

        /// <summary>
        /// 方盒中心点
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// 方盒X轴方向向量（归一化）
        /// </summary>
        public Vector3 AxisX;

        /// <summary>
        /// 方盒Y轴方向向量（归一化）
        /// </summary>
        public Vector3 AxisY;

        /// <summary>
        /// 方盒Z轴方向向量（归一化）
        /// </summary>
        public Vector3 AxisZ;

        /// <summary>
        /// 方盒半长（各轴方向的长度/2）
        /// </summary>
        public Vector3 Half;

        /// <summary>
        /// 方盒的光学行为类型
        /// </summary>
        public BoxKind Kind;

        /// <summary>
        /// 方盒的折射率（仅折射型有效）
        /// </summary>
        public float RefrIndex;

        /// <summary>
        /// 创建普通方盒实例
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="yawDeg">偏航角（度）</param>
        /// <param name="pitchDeg">俯仰角（度）</param>
        /// <param name="rollDeg">翻滚角（度）</param>
        /// <param name="size">方盒尺寸（长、宽、高）</param>
        /// <param name="kind">光学行为类型</param>
        /// <param name="refrIndex">折射率</param>
        /// <returns>初始化后的方盒实例</returns>
        public static BoxOptic Make(Vector3 center, float yawDeg, float pitchDeg, float rollDeg,
            Vector3 size, BoxKind kind, float refrIndex)
        {
            var box = new BoxOptic
            {
                Center = center,
                Kind = kind,
                RefrIndex = MathF.Max(refrIndex, 1e-3f) // 确保折射率有效（≥1e-3）
            };
            // 根据欧拉角计算方盒的轴向
            (box.AxisX, box.AxisY, box.AxisZ) = PortBox.YawPitchRollToBasis(yawDeg, pitchDeg, rollDeg);
            // 计算半长
            box.Half = size * 0.5f;
            return box;
        }

        /// <summary>
        /// 检测光线与方盒的交点（OBB射线相交算法）
        /// </summary>
        /// <param name="rayOrigin">光线起点</param>
        /// <param name="rayDir">光线方向</param>
        /// <param name="tHit">输出：交点的t参数</param>
        /// <param name="normalWorld">输出：交点处的世界坐标系法向量</param>
        /// <param name="axis">输出：命中的轴（0=X,1=Y,2=Z）</param>
        /// <param name="fromInside">输出：是否从方盒内部命中</param>
        /// <returns>是否存在有效交点</returns>
        public bool RayIntersect(Vector3 rayOrigin, Vector3 rayDir, out float tHit, out Vector3 normalWorld, out int axis, out bool fromInside)
        {
            // 初始化输出参数
            tHit = 0;
            normalWorld = default;
            axis = -1;
            fromInside = false;

            // 将光线转换到方盒局部坐标系
            Vector3 localOrigin = rayOrigin - Center;
            Vector3 localOriginProjected = new Vector3(
                Vector3.Dot(localOrigin, AxisX),
                Vector3.Dot(localOrigin, AxisY),
                Vector3.Dot(localOrigin, AxisZ));
            Vector3 localDirProjected = new Vector3(
                Vector3.Dot(rayDir, AxisX),
                Vector3.Dot(rayDir, AxisY),
                Vector3.Dot(rayDir, AxisZ));

            // 初始化相交区间
            float tMin = -float.MaxValue;
            float tMax = float.MaxValue;
            int minAxis = -1, minSign = 0;
            int maxAxis = -1, maxSign = 0;

            // 检测单个轴的相交
            bool IntersectSingleAxis(float p, float d, float halfSize, int axisIdx,
                ref float tmin, ref float tmax, ref int minAx, ref int minS, ref int maxAx, ref int maxS)
            {
                // 光线与轴平行
                if (MathF.Abs(d) < 1e-6f)
                {
                    // 光线在盒外则无交点
                    if (p < -halfSize || p > halfSize)
                        return false;
                    return true;
                }

                // 计算t参数
                float invD = 1f / d;
                float t1 = (-halfSize - p) * invD;
                float t2 = (halfSize - p) * invD;
                int s1 = invD >= 0 ? -1 : +1;
                int s2 = -s1;

                // 确保t1 < t2
                if (t1 > t2)
                {
                    (t1, t2) = (t2, t1);
                    (s1, s2) = (s2, s1);
                }

                // 更新相交区间
                if (t1 > tmin)
                {
                    tmin = t1;
                    minAx = axisIdx;
                    minS = s1;
                }
                if (t2 < tmax)
                {
                    tmax = t2;
                    maxAx = axisIdx;
                    maxS = s2;
                }

                // 区间无效（无交点）
                if (tmin > tmax || tmax < 0)
                    return false;

                return true;
            }

            // 检测三个轴的相交
            if (!IntersectSingleAxis(localOriginProjected.X, localDirProjected.X, Half.X, 0,
                ref tMin, ref tMax, ref minAxis, ref minSign, ref maxAxis, ref maxSign))
                return false;
            if (!IntersectSingleAxis(localOriginProjected.Y, localDirProjected.Y, Half.Y, 1,
                ref tMin, ref tMax, ref minAxis, ref minSign, ref maxAxis, ref maxSign))
                return false;
            if (!IntersectSingleAxis(localOriginProjected.Z, localDirProjected.Z, Half.Z, 2,
                ref tMin, ref tMax, ref minAxis, ref minSign, ref maxAxis, ref maxSign))
                return false;

            // 确定有效交点
            if (tMin > 1e-6f)
            {
                // 从外部命中
                tHit = tMin;
                axis = minAxis;
                fromInside = false;
                normalWorld = axis switch
                {
                    0 => AxisX * minSign,
                    1 => AxisY * minSign,
                    _ => AxisZ * minSign
                };
                return true;
            }
            else if (tMax > 1e-6f)
            {
                // 从内部命中
                tHit = tMax;
                axis = maxAxis;
                fromInside = true;
                normalWorld = axis switch
                {
                    0 => AxisX * maxSign,
                    1 => AxisY * maxSign,
                    _ => AxisZ * maxSign
                };
                return true;
            }

            // 无有效交点
            return false;
        }
    }

    /// <summary>
    /// 端口盒类（带入/出射口+自定义光线处理算法）
    /// </summary>
    public sealed class PortBox
    {
        /// <summary>
        /// 绑定到逻辑元件的唯一标识（为空表示未关联）
        /// </summary>
        public Guid ElementId { get; set; } = Guid.Empty;

        /// <summary>
        /// 端口盒中心点
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// 端口盒X轴方向向量（归一化）
        /// </summary>
        public Vector3 AxisX;

        /// <summary>
        /// 端口盒Y轴方向向量（归一化）
        /// </summary>
        public Vector3 AxisY;

        /// <summary>
        /// 端口盒Z轴方向向量（归一化）
        /// </summary>
        public Vector3 AxisZ;

        /// <summary>
        /// 端口盒半长
        /// </summary>
        public Vector3 Half;

        /// <summary>
        /// 入射口所在面
        /// </summary>
        public Face InFace;

        /// <summary>
        /// 出射口所在面
        /// </summary>
        public Face OutFace;

        /// <summary>
        /// 入/出射口的宽度（口径）
        /// </summary>
        public float ApertureW = 1.0f;

        /// <summary>
        /// 入/出射口的高度（口径）
        /// </summary>
        public float ApertureH = 1.0f;

        /// <summary>
        /// 光线处理算法类型
        /// </summary>
        public ProcessorKind Processor = ProcessorKind.PassThrough;

        /// <summary>
        /// 偏转算法的偏航角（度）
        /// </summary>
        public float DeflectYawDeg = 0;

        /// <summary>
        /// 偏转算法的俯仰角（度）
        /// </summary>
        public float DeflectPitchDeg = 0;

        /// <summary>
        /// 创建端口盒实例
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="yawDeg">偏航角</param>
        /// <param name="pitchDeg">俯仰角</param>
        /// <param name="rollDeg">翻滚角</param>
        /// <param name="size">尺寸</param>
        /// <param name="inFace">入射口面</param>
        /// <param name="outFace">出射口面</param>
        /// <param name="apertureW">口径宽度</param>
        /// <param name="apertureH">口径高度</param>
        /// <param name="proc">处理算法类型</param>
        /// <param name="defYawDeg">偏转偏航角</param>
        /// <param name="defPitchDeg">偏转俯仰角</param>
        /// <returns>初始化后的端口盒实例</returns>
        public static PortBox Make(Vector3 center, float yawDeg, float pitchDeg, float rollDeg,
            Vector3 size, Face inFace, Face outFace, float apertureW, float apertureH,
            ProcessorKind proc, float defYawDeg, float defPitchDeg)
        {
            var portBox = new PortBox
            {
                Center = center,
                Half = size * 0.5f,
                InFace = inFace,
                OutFace = outFace,
                ApertureW = apertureW,
                ApertureH = apertureH,
                Processor = proc,
                DeflectYawDeg = defYawDeg,
                DeflectPitchDeg = defPitchDeg
            };
            // 根据欧拉角计算轴向
            (portBox.AxisX, portBox.AxisY, portBox.AxisZ) = YawPitchRollToBasis(yawDeg, pitchDeg, rollDeg);
            return portBox;
        }

        /// <summary>
        /// 更新端口盒的姿态（欧拉角）
        /// </summary>
        /// <param name="yawDeg">偏航角</param>
        /// <param name="pitchDeg">俯仰角</param>
        /// <param name="rollDeg">翻滚角</param>
        public void SetPose(float yawDeg, float pitchDeg, float rollDeg)
        {
            (AxisX, AxisY, AxisZ) = YawPitchRollToBasis(yawDeg, pitchDeg, rollDeg);
        }

        /// <summary>
        /// 将欧拉角（偏航/俯仰/翻滚）转换为正交基向量
        /// </summary>
        /// <param name="yawDeg">偏航角（度）</param>
        /// <param name="pitchDeg">俯仰角（度）</param>
        /// <param name="rollDeg">翻滚角（度）</param>
        /// <returns>正交基向量（X,Y,Z）</returns>
        public static (Vector3 X, Vector3 Y, Vector3 Z) YawPitchRollToBasis(float yawDeg, float pitchDeg, float rollDeg)
        {
            // 转换为弧度
            float yaw = yawDeg * MathF.PI / 180f;
            float pitch = pitchDeg * MathF.PI / 180f;
            float roll = rollDeg * MathF.PI / 180f;

            // 构建旋转矩阵
            Matrix4x4 rotYaw = new Matrix4x4(
                MathF.Cos(yaw), 0, MathF.Sin(yaw), 0,
                0, 1, 0, 0,
                -MathF.Sin(yaw), 0, MathF.Cos(yaw), 0,
                0, 0, 0, 1);
            Matrix4x4 rotPitch = new Matrix4x4(
                1, 0, 0, 0,
                0, MathF.Cos(pitch), -MathF.Sin(pitch), 0,
                0, MathF.Sin(pitch), MathF.Cos(pitch), 0,
                0, 0, 0, 1);
            Matrix4x4 rotRoll = new Matrix4x4(
                MathF.Cos(roll), -MathF.Sin(roll), 0, 0,
                MathF.Sin(roll), MathF.Cos(roll), 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);

            // 组合旋转矩阵（Yaw -> Pitch -> Roll）
            Matrix4x4 rotation = rotYaw;
            rotation = Matrix4x4.Multiply(rotPitch, rotation);
            rotation = Matrix4x4.Multiply(rotRoll, rotation);

            // 提取基向量并归一化
            Vector3 x = Vector3.Normalize(new Vector3(rotation.M11, rotation.M12, rotation.M13));
            Vector3 y = Vector3.Normalize(new Vector3(rotation.M21, rotation.M22, rotation.M23));
            Vector3 z = Vector3.Normalize(new Vector3(rotation.M31, rotation.M32, rotation.M33));

            return (x, y, z);
        }

        /// <summary>
        /// 检测光线与入射口的交点
        /// </summary>
        /// <param name="rayOrigin">光线起点</param>
        /// <param name="rayDir">光线方向</param>
        /// <param name="t">输出：t参数</param>
        /// <param name="hitPoint">输出：交点坐标</param>
        /// <param name="normal">输出：入射口法向量</param>
        /// <returns>是否命中入射口</returns>
        public bool IntersectInput(Vector3 rayOrigin, Vector3 rayDir, out float t, out Vector3 hitPoint, out Vector3 normal)
        {
            t = 0;
            hitPoint = default;
            normal = default;

            // 获取入射口的几何信息
            GetFace(InFace, out normal, out Vector3 faceCenter, out Vector3 u, out Vector3 v, out float uLen, out float vLen);
            // 检测光线与入射口平面的交点
            if (!RayPlaneIntersection(rayOrigin, rayDir, faceCenter, normal, out t) || t < 0)
                return false;

            // 计算交点并检测是否在口径范围内
            hitPoint = rayOrigin + rayDir * t;
            Vector3 relativePoint = hitPoint - faceCenter;
            float du = Vector3.Dot(relativePoint, u);
            float dv = Vector3.Dot(relativePoint, v);
            float halfW = MathF.Min(ApertureW * 0.5f, uLen);
            float halfH = MathF.Min(ApertureH * 0.5f, vLen);

            return MathF.Abs(du) <= halfW && MathF.Abs(dv) <= halfH;
        }

        /// <summary>
        /// 处理光线：从入射口映射到出射口并应用算法
        /// </summary>
        /// <param name="inHit">入射口交点</param>
        /// <param name="inDir">入射光线方向</param>
        /// <param name="newOrigin">输出：出射光线起点</param>
        /// <param name="newDir">输出：出射光线方向</param>
        public void Process(Vector3 inHit, Vector3 inDir, out Vector3 newOrigin, out Vector3 newDir)
        {
            // 计算入射点在入射口内的偏移
            GetFace(InFace, out _, out Vector3 inCenter, out Vector3 inU, out Vector3 inV, out _, out _);
            Vector3 inRelative = inHit - inCenter;
            float du = Vector3.Dot(inRelative, inU);
            float dv = Vector3.Dot(inRelative, inV);

            // 获取出射口的几何信息
            GetFace(OutFace, out Vector3 outNormal, out Vector3 outCenter, out Vector3 outU, out Vector3 outV, out _, out _);

            // 映射偏移到出射口（添加小偏移避免重复命中）
            newOrigin = outCenter + outU * du + outV * dv + outNormal * 0.003f;

            // 应用处理算法
            switch (Processor)
            {
                // 直通：方向为出射口法向量
                case ProcessorKind.PassThrough:
                    newDir = outNormal;
                    break;
                // 偏转：按偏航/俯仰角旋转方向
                case ProcessorKind.Deflect:
                    newDir = RotateFromYawPitch(outNormal, DeflectYawDeg, DeflectPitchDeg);
                    break;
                default:
                    newDir = outNormal;
                    break;
            }

            // 归一化方向向量
            newDir = Vector3.Normalize(newDir);
        }

        /// <summary>
        /// 获取指定面的几何信息（用于渲染和碰撞检测）
        /// </summary>
        /// <param name="face">面类型</param>
        /// <param name="normal">输出：面法向量</param>
        /// <param name="center">输出：面中心点</param>
        /// <param name="u">输出：面内U方向基向量</param>
        /// <param name="v">输出：面内V方向基向量</param>
        /// <param name="uLen">输出：U方向半长</param>
        /// <param name="vLen">输出：V方向半长</param>
        public void GetFace(Face face, out Vector3 normal, out Vector3 center, out Vector3 u, out Vector3 v, out float uLen, out float vLen)
        {
            switch (face)
            {
                case Face.XPos:
                    normal = AxisX;
                    center = Center + AxisX * Half.X;
                    u = AxisY; v = AxisZ;
                    uLen = Half.Y; vLen = Half.Z;
                    break;
                case Face.XNeg:
                    normal = -AxisX;
                    center = Center - AxisX * Half.X;
                    u = AxisY; v = AxisZ;
                    uLen = Half.Y; vLen = Half.Z;
                    break;
                case Face.YPos:
                    normal = AxisY;
                    center = Center + AxisY * Half.Y;
                    u = AxisX; v = AxisZ;
                    uLen = Half.X; vLen = Half.Z;
                    break;
                case Face.YNeg:
                    normal = -AxisY;
                    center = Center - AxisY * Half.Y;
                    u = AxisX; v = AxisZ;
                    uLen = Half.X; vLen = Half.Z;
                    break;
                case Face.ZPos:
                    normal = AxisZ;
                    center = Center + AxisZ * Half.Z;
                    u = AxisX; v = AxisY;
                    uLen = Half.X; vLen = Half.Y;
                    break;
                default: // ZNeg
                    normal = -AxisZ;
                    center = Center - AxisZ * Half.Z;
                    u = AxisX; v = AxisY;
                    uLen = Half.X; vLen = Half.Y;
                    break;
            }

            // 归一化面内基向量
            u = Vector3.Normalize(u);
            v = Vector3.Normalize(v);
        }

        #region 内部工具方法

        /// <summary>
        /// 光线与平面交点检测（内部复用）
        /// </summary>
        private static bool RayPlaneIntersection(Vector3 rayOrigin, Vector3 rayDir, Vector3 planePoint, Vector3 planeNormal, out float t)
        {
            t = 0;
            float denominator = Vector3.Dot(rayDir, planeNormal);
            if (MathF.Abs(denominator) < 1e-6f)
                return false;

            t = Vector3.Dot(planePoint - rayOrigin, planeNormal) / denominator;
            return true;
        }

        /// <summary>
        /// 按偏航/俯仰角旋转方向向量
        /// </summary>
        /// <param name="baseDir">基础方向</param>
        /// <param name="yawDeg">偏航角</param>
        /// <param name="pitchDeg">俯仰角</param>
        /// <returns>旋转后的方向向量</returns>
        private static Vector3 RotateFromYawPitch(Vector3 baseDir, float yawDeg, float pitchDeg)
        {
            float yaw = yawDeg * MathF.PI / 180f;
            float pitch = pitchDeg * MathF.PI / 180f;

            // 构建旋转矩阵
            Matrix4x4 rotYaw = new Matrix4x4(
                MathF.Cos(yaw), 0, MathF.Sin(yaw), 0,
                0, 1, 0, 0,
                -MathF.Sin(yaw), 0, MathF.Cos(yaw), 0,
                0, 0, 0, 1);
            Matrix4x4 rotPitch = new Matrix4x4(
                1, 0, 0, 0,
                0, MathF.Cos(pitch), -MathF.Sin(pitch), 0,
                0, MathF.Sin(pitch), MathF.Cos(pitch), 0,
                0, 0, 0, 1);

            // 应用旋转
            Vector4 dir4 = Vector4.Transform(new Vector4(baseDir, 1), rotYaw);
            dir4 = Vector4.Transform(dir4, rotPitch);

            // 归一化并返回
            return Vector3.Normalize(new Vector3(dir4.X, dir4.Y, dir4.Z));
        }

        #endregion
    }

    #endregion

    #region 辅助结构

    /// <summary>
    /// 四边形结构（由四个顶点定义）
    /// </summary>
    public struct Quad
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
        public Vector3 D;
    }

    /// <summary>
    /// 光线定义结构（光源参数）
    /// </summary>
    public sealed class RayDefinition
    {
        /// <summary>
        /// 光线起点
        /// </summary>
        public Vector3 Origin;

        /// <summary>
        /// 光线初始方向
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// 最大反射/折射次数
        /// </summary>
        public int MaxBounces = 6;

        /// <summary>
        /// 绑定到逻辑元件的唯一标识（为空表示未关联）
        /// </summary>
        public Guid ElementId { get; set; } = Guid.Empty;
    }

    /// <summary>
    /// 光线路径结构（包含所有光线段）
    /// </summary>
    public sealed class RayPath
    {
        /// <summary>
        /// 光线段集合
        /// </summary>
        public List<RaySegment> Segments { get; } = new();
    }

    /// <summary>
    /// 光线段结构（单段光线的起点、终点和颜色）
    /// </summary>
    public sealed class RaySegment
    {
        /// <summary>
        /// 段起点
        /// </summary>
        public Vector3 Start;

        /// <summary>
        /// 段终点
        /// </summary>
        public Vector3 End;

        /// <summary>
        /// 段的显示颜色
        /// </summary>
        public Vector3 LayerColor;
    }

    #endregion
}