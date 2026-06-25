using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEMS.MathCore
{
    /// <summary>
    /// Provides methods for coordinate system transformations, including rotation and translation.
    /// </summary>
    /// <remarks>
    /// This class includes functionality to compute rotation matrices based on Euler angles,
    /// translation matrices for shifting coordinates.
    /// </remarks>
    public class CoordinateSystemTransform
    {
        /// <summary>
        /// A rotation matrix that rotates counterclockwise about the x-axis.
        /// </summary>
        /// <remarks>  
        /// This matrix is used to perform rotations around the x-axis in a 3D coordinate system.  
        /// The rotation is counterclockwise when viewed from the positive x-axis.  
        /// </remarks>  
        /// <param name="Alpha">The rotation angle in radians.</param>  
        /// <returns>A 3x3 matrix representing the rotation.</returns>  
        private static readonly Func<double, MatrixD> Rx = (Alpha) => new MatrixD(3, 3)
        {
            [0, 0] = 1.0,
            [0, 1] = 0.0,
            [0, 2] = 0.0,
            [1, 0] = 0.0,
            [1, 1] = Math.Cos(Alpha),
            [1, 2] = Math.Sin(Alpha),
            [2, 0] = 0.0,
            [2, 1] = -Math.Sin(Alpha),
            [2, 2] = Math.Cos(Alpha),
        };
        /// <summary>
        /// A rotation matrix that rotates counterclockwise about the y-axis.
        /// </summary>
        /// <remarks>
        /// This matrix is used to perform rotations around the y-axis in a 3D coordinate system.
        /// The rotation is counterclockwise when viewed from the positive y-axis.
        /// </remarks>
        /// <param name="Beta">The rotation angle in radians.</param>
        /// <returns>A 3x3 matrix representing the rotation.</returns>
        private static readonly Func<double, MatrixD> Ry = (Beta) => new MatrixD(3, 3)
        {
            [0, 0] = Math.Cos(Beta),
            [0, 1] = 0.0,
            [0, 2] = -Math.Sin(Beta),
            [1, 0] = 0.0,
            [1, 1] = 1.0,
            [1, 2] = 0.0,
            [2, 0] = Math.Sin(Beta),
            [2, 1] = 0.0,
            [2, 2] = Math.Cos(Beta),
        };
        /// <summary>
        /// A rotation matrix that rotates counterclockwise about the z-axis.
        /// </summary>
        /// <remarks>  
        /// This matrix is used to perform rotations around the z-axis in a 3D coordinate system.  
        /// The rotation is counterclockwise when viewed from the positive z-axis.  
        /// </remarks>  
        /// <param name="Gamma">The rotation angle in radians.</param>  
        /// <returns>A 3x3 matrix representing the rotation.</returns>  
        private static readonly Func<double, MatrixD> Rz = (Gamma) => new MatrixD(3, 3)
        {
            [0, 0] = Math.Cos(Gamma),
            [0, 1] = Math.Sin(Gamma),
            [0, 2] = 0.0,
            [1, 0] = -Math.Sin(Gamma),
            [1, 1] = Math.Cos(Gamma),
            [1, 2] = 0.0,
            [2, 0] = 0.0,
            [2, 1] = 0.0,
            [2, 2] = 1.0,
        };
        /// <summary>
        /// Computes the rotation matrix from given Euler angles.
        /// </summary>
        /// <param name="phi">Rotation angle about the z-axis in degrees.</param>
        /// <param name="theta">Rotation angle about the x-axis in degrees.</param>
        /// <param name="psi">Rotation angle about the z-axis in degrees.</param>
        /// <param name="convention">The convention of Euler angles to use.</param>
        /// <returns>
        /// A 3x3 rotation matrix that represents the transformation of the coordinate system
        /// based on the specified Euler angles and convention.
        /// </returns>
        /// <remarks>
        /// This method supports different conventions for Euler angles, such as XConvention, YConvention, and XYZConvention.
        /// The angles are provided in degrees and internally converted to radians for computation.
        /// The resulting matrix can be used to transform the coordinate system, not for points or vectors directly.
        /// </remarks>
        public static MatrixD RotationMatrix(double phi, double theta, double psi, EulerAnglesCovention convention = Defaults.EulerAnglesConvention)
        {
            // Convert degrees to radians
            double Phi = phi * Math.PI / 180.0;
            double Theta = theta * Math.PI / 180.0;
            double Psi = psi * Math.PI / 180.0;
            // Create the rotation matrices based on the convention
            MatrixD D = new(3, 3);
            MatrixD C = new(3, 3);
            MatrixD B = new(3, 3);
            switch (convention)
            {
                case EulerAnglesCovention.XConvention:
                    // the first rotation by angle phi is about the z-axis using D
                    // the second rotation by angle theta is about the new x`-axis using C
                    // the third rotation by angle psi is about the new z`-axis using B
                    D = Rz(Phi);
                    C = Rx(Theta);
                    B = Rz(Psi);
                    break;
                case EulerAnglesCovention.YConvention:
                    // Not implemented yet
                    throw new NotImplementedException("YConvention is not implemented yet.");
                case EulerAnglesCovention.XYZConvention:
                    // Not implemented yet
                    throw new NotImplementedException("XYZConvention is not implemented yet.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(convention), convention, null);
            }

            return LinAlg.Dot(B, LinAlg.Dot(C, D));
        }

        /// <summary>
        /// Computes the translation matrix for shifting the coordinate system.
        /// </summary>
        /// <remarks>
        /// This method generates a 4x4 matrix that represents a translation transformation in 3D space.
        /// The translation is applied along the x, y, and z axes, shifting the coordinate system by the specified amounts.
        /// Negative values are used internally to reverse the direction of the translation.
        /// So you should be very careful when using this method to translate points and vectors, instead of coordinate systems.
        /// </remarks>
        /// <param name="dx">Translation along the x-axis (negative internally).</param>
        /// <param name="dy">Translation along the y-axis (negative internally).</param>
        /// <param name="dz">Translation along the z-axis (negative internally).</param>
        /// <returns>
        /// A 4x4 matrix where the last column contains the translation values, and the rest of the matrix represents the identity transformation.
        /// </returns>
        public static MatrixD MoveMatrix(double dx, double dy, double dz)
        {
            dx = -dx;
            dy = -dy;
            dz = -dz;
            MatrixD MoveMatrix = new(4, 4)
            {
                [0, 0] = 1,
                [0, 1] = 0.0,
                [0, 2] = 0.0,
                [0, 3] = dx,
                [1, 0] = 0.0,
                [1, 1] = 1.0,
                [1, 2] = 0.0,
                [1, 3] = dy,
                [2, 0] = 0.0,
                [2, 1] = 0.0,
                [2, 2] = 1.0,
                [2, 3] = dz,
                [3, 0] = 0.0,
                [3, 1] = 0.0,
                [3, 2] = 0.0,
                [3, 3] = 1.0,
            };
            return MoveMatrix;
        }
    }

}



