using System.Collections.Generic;
using RoslynPad.IconsFront.Models;

namespace RoslynPad.IconsFront
{
    /// <summary>
    /// 定义图标读取器的接口。
    /// </summary>
    /// <remarks>
    /// 此接口提供根据图标标识获取图标模型的功能。
    /// </remarks>
    public interface IIconReader
    {
        /// <summary>
        /// 获取指定图标的模型数据。
        /// </summary>
        /// <param name="value">图标标识字符串（如 "fa-solid fa-user"）。</param>
        /// <returns>包含视口和路径信息的 <see cref="IconModel"/>。</returns>
        /// <exception cref="KeyNotFoundException">
        /// 当找不到与 <paramref name="value"/> 关联的图标时抛出。
        /// </exception>
        IconModel GetIcon(string value);
    }
}
