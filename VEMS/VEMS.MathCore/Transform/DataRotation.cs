using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VEMS.MathCore
{
    /// <summary>
    /// Provides functionality to rotate a 2D complex matrix in the plane by a specified angle using a three-shear
    /// decomposition technique.
    /// </summary>
    /// <remarks>The rotation is performed in a fixed coordinate system and uses the three-shear decomposition
    /// method, which applies a sequence of shear transformations along the X and Y axes. This class is designed for
    /// operations on 2D complex matrices, such as those used in computational mathematics or signal
    /// processing.</remarks>
    public class RotateInPlane
    {
        #region static methods
        /// <summary>
        /// Rotates a 2D complex matrix in the plane by a specified angle using three-shear decomposition. The Rotation is based on a fixed coordinate system. 
        /// The sampling grid remains unchanged before and after the rotation. The opposite sign should be used when rotating the coordinate system.
        /// </summary>
        /// <param name="values">Input complex matrix to rotate.</param>
        /// <param name="grid">Grid information for the matrix. The sampling grid remains unchanged before and after the rotation.</param>
        /// <param name="theta">Rotation angle in degrees. Counterclockwise direction is considered as positive direction</param>
        /// <param name="cx">Analytical linear phase term along x(if have).</param>
        /// <param name="cy">Analytical linear phase term along y(if have).</param>
        /// <param name="loopMode">Loop computation mode.</param>
        /// <returns>Rotated complex matrix information.</returns>
        public static void Rotate(ref MatrixZ values, ref GridInfo2D grid,
            ref double cx, ref double cy, double theta,
            LoopMode loopMode = Defaults.LoopOption)
        {
            if (values == null || grid == null)
            {
                throw new ArgumentNullException("Values or grid cannot be null.");
            }
            MatrixZ field = new(values);
            GridInfo2D g = new(grid.Rows, grid.Cols, grid.SpacingY, grid.SpacingX, 0.0, 0.0, GridRefType.Center, GridRefType.Center);
            double Rotation = theta / 180.0 * Math.PI; // Convert angle to radians
            double a = Math.Tan(Rotation / 2);        // Shear parameter for X
            double b = -Math.Sin(Rotation);             // Shear parameter for Y
            // Apply three-shear decomposition: X-Y-X
            field = Shear(field, g, a, ShearAxis.X, loopMode);
            field = Shear(field, g, b, ShearAxis.Y, loopMode);
            field = Shear(field, g, a, ShearAxis.X, loopMode);
            values = field;

            // Update analytical shifts & linear phase terms
            double temp = Math.Cos(Rotation) * grid.CenterX - Math.Sin(Rotation) * grid.CenterY;
            grid.CenterY = Math.Sin(Rotation) * grid.CenterX + Math.Cos(Rotation) * grid.CenterY;
            grid.CenterX = temp;
            temp = Math.Cos(Rotation) * cx - Math.Sin(Rotation) * cy;
            cy = Math.Sin(Rotation) * cx + Math.Cos(Rotation) * cy;
            cx = temp;
        }
        #endregion

        #region --- helpers ---
        /// <summary>
        /// Applies a shear transformation to a 2D complex matrix along the specified axis.
        /// </summary>
        /// <param name="values">Input complex matrix.</param>
        /// <param name="grid">Grid information for the matrix.</param>
        /// <param name="scal">Shear scale factor.</param>
        /// <param name="shearAxis">Axis along which to apply the shear (X or Y).</param>
        /// <param name="loopMode">Loop computation mode.</param>
        /// <returns>Sheared complex matrix.</returns>
        private static MatrixZ Shear(MatrixZ values, GridInfo2D grid, double scal, ShearAxis shearAxis = ShearAxis.X, LoopMode loopMode = Defaults.LoopOption)
        {
            if (values == null || grid == null)
            {
                throw new ArgumentNullException("Values or grid cannot be null.");
            }
            MatrixZ field = new(values);
            Action<long> op = null;
            switch (shearAxis)
            {
                case ShearAxis.X:
                    // Shear along X-axis: process each row
                    GridInfo1D gridX = new(grid.Cols, grid.SpacingX);
                    op = i =>
                    {
                        VectorZ temp = new(field[i, new LongRange(0, field.Cols)]);
                        // Forward FFT
                        Transform.FFT1D(ref temp, ref gridX, FFTOptions.Direction.Forward);
                        // Apply phase shift for shear
                        temp *= VMath.Exp(Complex.ImaginaryOne * scal * grid.GetCoordinateY(i) * gridX.GetCoordinates());
                        // Inverse FFT
                        Transform.FFT1D(ref temp, ref gridX, FFTOptions.Direction.Backward);
                        field[i, new LongRange(0, field.Cols)] = temp;
                    };
                    break;
                case ShearAxis.Y:
                    // Shear along Y-axis: process each column
                    GridInfo1D gridY = new(grid.Rows, grid.SpacingY);
                    op = i =>
                    {
                        VectorZ temp = new(field[new LongRange(0, field.Rows), i]);
                        // Forward FFT
                        Transform.FFT1D(ref temp, ref gridY, FFTOptions.Direction.Forward);
                        // Apply phase shift for shear
                        temp *= VMath.Exp(Complex.ImaginaryOne * scal * grid.GetCoordinateX(i) * gridY.GetCoordinates());
                        // Inverse FFT
                        Transform.FFT1D(ref temp, ref gridY, FFTOptions.Direction.Backward);
                        field[new LongRange(0, field.Rows), i] = temp;
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shearAxis), shearAxis, null);
            }
            // Select loop range based on axis
            Loop1D loop = shearAxis == ShearAxis.X ? new(operation: op, start: 0, end: grid.Rows, step: 1) :
                new(operation: op, start: 0, end: grid.Cols, step: 1);
            loop.Evaluate(loopMode);
            return field;
        }

        /// <summary>
        /// Enum for specifying the shear axis.
        /// </summary>
        private enum ShearAxis
        {
            X,
            Y
        }
        #endregion
    }
}
