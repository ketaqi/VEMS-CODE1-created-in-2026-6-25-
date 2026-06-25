using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VEMS.MathCore;

namespace VEMS.MathCore
{
/// <summary>
/// Represents a coordinate system in 3D space, including position and orientation.
/// Provides functionality to manage global and relative locations, rotation matrices.
/// </summary>
   public class CoordinateSystem
    {
        #region properties
        /// <summary>
        /// This property stores the absolute position of the coordinate system in 3D space.  
        /// </summary> 
        private VecD3? location;

        /// <summary> 
        /// This property stores the position of the coordinate system relative to another coordinate system.  
        /// </summary>  
        private VecD3? relativeLocation;

        /// <summary>
        /// Gets or sets the global location of the coordinate system in 3D space.
        /// When set, it updates the relative location based on the relative coordinate system if available.
        /// Otherwise, the relative location is set to the same value as the global location.
        /// </summary>
        public VecD3 Location
        {
            get { return location; }
            set
            {
                location = value;
                if (RelativeCoordinate != null)
                {
                    Location2RelativeLocation();
                }
                else
                {
                    relativeLocation = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the relative location of the coordinate system based on the relative coordinate system.
        /// When set, it updates the global location based on the relative coordinate system if available.
        /// Otherwise, the global location is set to the same value as the relative location.
        /// </summary>
        public VecD3 RelativeLocation
        {
            get { return relativeLocation; }
            set
            {
                relativeLocation = value;
                if (RelativeCoordinate != null)
                {
                    RelativeLocation2Location();
                }
                else
                {
                    location = value;
                }
            }
        }

        /// <summary>
        /// This matrix represents the orientation of the coordinate system in 3D space.
        /// </summary>
        public MatrixD RotationMatrix { get; set; }

        /// <summary> 
        /// This matrix represents the orientation of the coordinate system relative to another coordinate system.  
        /// </summary>  
        public MatrixD RelativeRotationMatrix { get; set; }

        /// <summary>
        /// This property defines the rotation convention applied to the coordinate system,  
        /// which determines the sequence and axes of rotations in 3D space.  
        /// </summary>  
        public EulerAnglesCovention RotatingConvention { get; set; }

        /// <summary>
        /// This property defines the coordinate system relative to which the current coordinate system is positioned and oriented.  
        /// If set, the relative location and rotation matrices are calculated based on this relative coordinate system.  
        /// </summary>  
        public CoordinateSystem? RelativeCoordinate { get; set; }

        #endregion

        #region static properties
        /// <summary>
        /// This instance represents the default coordinate system at the origin of 3D space,  
        /// with zero position and zero rotation.  
        /// </summary>  
        public static readonly CoordinateSystem Origin = new CoordinateSystem(new VecD3(VecD3.Zeros), new VecD3(VecD3.Zeros));
        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSystem"/> class based on a relative coordinate system.
        /// </summary>
        /// <param name="relativeCoordinate">The coordinate system relative to which this coordinate system is defined.</param>
        /// <param name="relativeLocation">The position of this coordinate system relative to the <paramref name="relativeCoordinate"/>.</param>
        /// <param name="relativeRotation">The rotation of this coordinate system relative to the <paramref name="relativeCoordinate"/>.</param>
        /// <param name="eulerAnglesCovention">The convention for Euler angles used for rotation calculations. Defaults to <see cref="Defaults.EulerAnglesConvention"/>.</param>
        public CoordinateSystem(CoordinateSystem relativeCoordinate, 
            VecD3 relativeLocation, VecD3 relativeRotation, 
            EulerAnglesCovention eulerAnglesCovention = Defaults.EulerAnglesConvention)
        {
            RotatingConvention = eulerAnglesCovention;
            RelativeCoordinate = relativeCoordinate;
            RelativeLocation = relativeLocation;
            RelativeRotationMatrix = CoordinateSystemTransform.RotationMatrix(relativeRotation.X, relativeRotation.Y, relativeRotation.Z, eulerAnglesCovention);
            RotationMatrix = LinAlg.Dot(RelativeRotationMatrix, RelativeCoordinate.RotationMatrix);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSystem"/> class based on the global location and rotation.
        /// </summary>
        /// <param name="location">The global location of the coordinate system in 3D space.</param>
        /// <param name="rotation">The global rotation of the coordinate system in 3D space.</param>
        /// <param name="relativeCoordinate">The coordinate system relative to which this coordinate system is defined.
        /// When set, the class will calculate relative location and rotation matrix automatically. Defaults to <c>null</c>.</param>
        /// <param name="eulerAnglesCovention">The convention for Euler angles used for rotation calculations. Defaults to <see cref="Defaults.EulerAnglesConvention"/>.</param>
        public CoordinateSystem(VecD3 location, VecD3 rotation,
            CoordinateSystem? relativeCoordinate = null, 
            EulerAnglesCovention eulerAnglesCovention = Defaults.EulerAnglesConvention)
        {
            RotatingConvention = eulerAnglesCovention;
            RelativeCoordinate = relativeCoordinate;
            Location = location;
            RotationMatrix = CoordinateSystemTransform.RotationMatrix(rotation.X, rotation.Y, rotation.Z, eulerAnglesCovention);
            RelativeRotationMatrix = relativeCoordinate != null
                ? LinAlg.Dot(RotationMatrix, LinAlg.Transpose(relativeCoordinate.RotationMatrix))
                : RotationMatrix;
        }

        /// <summary>  
        /// Initializes a new instance of the <see cref="CoordinateSystem"/> class by copying the properties of another coordinate system.  
        /// </summary>  
        /// <param name="other">The coordinate system to copy from.</param>  
        public CoordinateSystem(CoordinateSystem other)
        {
            RotatingConvention = other.RotatingConvention;
            RelativeCoordinate = other.RelativeCoordinate;
            RelativeLocation = new VecD3(other.RelativeLocation);
            RotationMatrix = new MatrixD(other.RotationMatrix);
            RelativeRotationMatrix = new MatrixD(other.RelativeRotationMatrix);
        }
        #endregion

        #region private methods
        /// <summary>
        /// This method calculates the global position of the coordinate system in 3D space  
        /// based on its relative position and the rotation matrix of the relative coordinate system.  
        /// </summary>  
        /// <remarks>  
        /// The calculation involves transforming the relative location using the transpose  
        /// of the rotation matrix of the relative coordinate system and adding the global  
        /// location of the relative coordinate system to get the global location of the 
        /// current coordinate system.  
        /// </remarks>
        private void RelativeLocation2Location()
        {
            VectorD Relative = new(3)
            {
                [0] = relativeLocation.X,
                [1] = relativeLocation.Y,
                [2] = relativeLocation.Z
            };
            Relative = LinAlg.Dot(LinAlg.Transpose(RelativeCoordinate.RotationMatrix), Relative);

            location = new VecD3(Relative[0] + RelativeCoordinate.Location.X,
                Relative[1] + RelativeCoordinate.Location.Y,
                Relative[2] + RelativeCoordinate.Location.Z);
        }

        /// <summary> 
        /// This method calculates the relative position of the coordinate system in 3D space  
        /// by subtracting the global location of the relative coordinate system from the global location  
        /// of the current coordinate system and transforming the result using the rotation matrix  
        /// of the relative coordinate system.  
        /// </summary>  
        /// <remarks>  
        /// The calculation involves subtracting the global position of the relative coordinate system  
        /// from the global position of the current coordinate system, followed by a transformation  
        /// using the rotation matrix of the relative coordinate system to obtain the relative position.  
        /// </remarks>  
        private void Location2RelativeLocation()
        {
            VectorD Relative = new(3)
            {
                [0] = location.X - RelativeCoordinate.Location.X,
                [1] = location.Y - RelativeCoordinate.Location.Y,
                [2] = location.Z - RelativeCoordinate.Location.Z
            };
            Relative = LinAlg.Dot(RelativeCoordinate.RotationMatrix, Relative);
            relativeLocation = new VecD3(Relative[0], Relative[1], Relative[2]);
        }
        #endregion
    }
}