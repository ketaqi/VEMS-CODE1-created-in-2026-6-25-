namespace VEMS.AMathCore
{
    /// <summary>
    /// options for 2D finite difference
    /// </summary>
    public enum FiDi2DOption
    {
        /// <summary>
        /// first-order derivative along x
        /// </summary>
        Dx,
        /// <summary>
        /// first-order derivative along y
        /// </summary>
        Dy,
        /// <summary>
        /// second-order derivative along x
        /// </summary>
        Dxx,
        /// <summary>
        /// second-order derivative along y
        /// </summary>
        Dyy,
        /// <summary>
        /// cross derivative first along x and then along y
        /// </summary>
        Dxy
    }

}
