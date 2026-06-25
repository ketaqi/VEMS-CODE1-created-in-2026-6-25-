namespace VEMS.AMathCore
{
    /// <summary>
    /// conventions of Euler angles
    /// </summary>
    public enum EulerAnglesCovention
    {
        /// <summary>
        /// x convention
        /// the first rotation by angle phi is about the z-axis
        /// the second rotation by angle theta is about the new x`-axis
        /// the third rotation by angle psi is about the new z`-axis
        /// </summary>
        XConvention,

        /// <summary>
        /// Not implemented yet
        /// </summary>
        YConvention,

        /// <summary>
        /// Not implemented yet
        /// </summary>
        XYZConvention,
    }
}
