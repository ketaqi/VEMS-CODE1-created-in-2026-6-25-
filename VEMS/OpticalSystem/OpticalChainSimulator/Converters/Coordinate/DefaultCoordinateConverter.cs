using System;
using System.Numerics;

namespace OpticalChainSimulator.Converters.Coordinate
{
    /// <summary>
    /// 默认坐标转换器
    /// 实现ICoordinateConverter接口，提供绝对坐标与相对坐标的双向转换逻辑
    /// 核心转换规则：相对坐标 = 绝对坐标 + 1（各轴独立计算）；绝对坐标 = 相对坐标 - 1（各轴独立计算）
    /// </summary>
    public class DefaultCoordinateConverter : ICoordinateConverter
    {
        /// <summary>
        /// 将绝对三维坐标转换为相对三维坐标
        /// </summary>
        /// <param name="absoluteCoord">待转换的绝对三维坐标（Vector3表示X/Y/Z轴坐标）</param>
        /// <returns>转换后的相对三维坐标，各轴数值为绝对坐标对应轴数值+1</returns>
        public Vector3 AbsoluteToRelative(Vector3 absoluteCoord)
        {
            // 相对坐标转换规则：X/Y/Z轴分别在绝对坐标基础上加1
            return new Vector3(
                absoluteCoord.X + 1f,
                absoluteCoord.Y + 1f,
                absoluteCoord.Z + 1f
            );
        }

        /// <summary>
        /// 将相对三维坐标转换为绝对三维坐标
        /// </summary>
        /// <param name="relativeCoord">待转换的相对三维坐标（Vector3表示X/Y/Z轴坐标）</param>
        /// <returns>转换后的绝对三维坐标，各轴数值为相对坐标对应轴数值-1</returns>
        public Vector3 RelativeToAbsolute(Vector3 relativeCoord)
        {
            // 绝对坐标转换规则：X/Y/Z轴分别在相对坐标基础上减1
            return new Vector3(
                relativeCoord.X - 1f,
                relativeCoord.Y - 1f,
                relativeCoord.Z - 1f
            );
        }
    }
}