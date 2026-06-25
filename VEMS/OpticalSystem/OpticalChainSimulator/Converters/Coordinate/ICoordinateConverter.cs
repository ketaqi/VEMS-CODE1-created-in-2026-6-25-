using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpticalChainSimulator.Converters.Coordinate
{
    /// <summary>
    /// 坐标转换策略接口（定义绝对/相对坐标转换规则）
    /// </summary>
    public interface ICoordinateConverter
    {
        /// <summary>
        /// 绝对坐标转相对坐标
        /// </summary>
        Vector3 AbsoluteToRelative(Vector3 absoluteCoord);

        /// <summary>
        /// 相对坐标转绝对坐标
        /// </summary>
        Vector3 RelativeToAbsolute(Vector3 relativeCoord);
    }
}
