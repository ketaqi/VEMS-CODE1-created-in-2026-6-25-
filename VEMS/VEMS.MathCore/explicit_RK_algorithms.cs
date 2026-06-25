using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEMS.MathCore
{
    /// <summary>
    /// variable kinds of functions to calculate simple explicit RK algorithms
    /// </summary>
    public class Explicit_RK_algorithms
    {
        #region Euler
        /// <summary>
        ///  the Explicit Euler algorithms for scalar differential equation
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1"> an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete numerical solutions corresponding the x-coordinate of input_1</returns>
        public static VectorD Solve_Euler(Func<double, double, double> f, double initialValue, VectorD input_1)
        {
            var N = input_1.Count;
            var a = new VectorD(N) { [0] = initialValue };
            var h = 0.0;
            for (int i = 0; i < N-1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                a[i + 1] = a[i] + h * f(input_1[i], a[i]);
            }
            return a;
        }
        /// <summary>
        /// the Explicit Euler algorithms for vectorial differential equation
        /// </summary>
        /// <param name="f">a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialvalue">the boundary condition of a differential equation</param>
        /// <param name="input_1"> an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete vectorial numerical solutions corresponding the x-coordinate of input_1</returns>
        public static List<VectorD> Solve_Euler(Func<double,VectorD,VectorD> f, VectorD initialvalue, VectorD input_1)
        {
            var n = input_1.Count;
            var a = new List<VectorD>();
            a.Add(initialvalue);
            var h = 0.0;
            for (int i = 0; i < n - 1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                a.Add(a[i] + h * f(input_1[i], a[i]));
            }
            return a;
        }
        #endregion
        #region Modified Euler
        /// <summary>
        ///  the Modified Euler algorithms promoted by prediction and correction for scalar differential equatoin
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1"> an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete scalar numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static VectorD Solve_ModifiedEuler(Func<double, double, double> f, double initialValue, VectorD input_1) 
        {
            var N = input_1.Count;
            var a = new VectorD(N) { [0] = initialValue };
            var h = 0.0;
            for (int i = 0; i < N - 1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i + 1], a[i] + h * k1);
                a[i + 1] = a[i] + h / 2.0 * (k1 + k2);
            }
            return a;
        }
        /// <summary>
        /// the Modified Euler algorithms promoted by prediction and correction for vectorial differential equation
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1">an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete vectorial numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static List<VectorD> Solve_ModifiedEuler(Func<double, VectorD, VectorD> f, VectorD initialValue, VectorD input_1)
        { 
            var N = input_1.Count;
            var a = new List<VectorD>();
            var h = 0.0;
            a.Add(initialValue);
            for (int i = 0; i < N-1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i + 1], a[i] + h * k1);
                a.Add(a[i] + h * 0.5 * (k1 + k2));
            }
            return a;
        }
        #endregion
        #region Mid-Point2RK
        /// <summary>
        /// the Mid-point algorithms that is a kind of 2-order Runge Kutta method for scalar differential equatoin
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1"> an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete scalar numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static VectorD Mid_Point2RK(Func<double, double, double> f, double initialValue, VectorD input_1)
        {
            var N = input_1.Count;
            var a = new VectorD(N) { [0] = initialValue };
            var h = 0.0;
            for (int i = 0; i < N - 1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i] + h / 2.0, a[i] + h / 2.0 * k1);
                a[i + 1] = a[i] + h * k2;
            }
            return a;
        }
        /// <summary>
        /// the Mid-point algorithms that is a kind of 2-order Runge Kutta method for vectorial differential equatoin
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1">an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete vectorial numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static List<VectorD> Mid_Point2RK(Func<double, VectorD, VectorD> f, VectorD initialValue, VectorD input_1)
        { 
            var N = input_1.Count;
            var a = new List<VectorD>();
            var h = 0.0;
            a.Add(initialValue);
            for (int i = 0; i < N-1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i] + 0.5 * h, a[i] + 0.5 * h * k1);
                a.Add(a[i] + h * k2);
            }
            return a;
        }
        #endregion
        #region Heun 3-order RK
        /// <summary>
        /// the Heun 3-order Runge Kutta algorithms for scalar differential equatoin
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1"> an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete scalar numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static VectorD Heun3RK(Func<double, double, double> f, double initialValue, VectorD input_1)
        {
            var N = input_1.Count;
            var a = new VectorD(N) { [0] = initialValue };
            var h = 0.0;
            for (int i = 0; i < N - 1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i] + h / 3.0, a[i] + h / 3.0 * k1);
                var k3 = f(input_1[i] + 2.0 * h / 3.0, a[i] + 2.0 * h / 3.0 * k2);
                a[i + 1] = a[i] + h / 4.0 * (k1 + 3 * k3);
            }
            return a;
        }
        /// <summary>
        /// the Heun 3-order Runge Kutta algorithms for vectorial differential equatoin
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1">an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete vectorial numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static List<VectorD> Heun3RK(Func<double, VectorD, VectorD> f, VectorD initialValue, VectorD input_1)
        { 
            var N = input_1.Count;
            var a = new List<VectorD>();
            var h = 0.0;
            a.Add(initialValue);
            for (int i = 0; i < N-1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i] + h / 3.0, a[i] + h / 3.0 * k1);
                var k3 = f(input_1[i] + 2.0 * h / 3.0, a[i] + 2.0 * h / 3.0 * k2);
                a.Add(a[i] + h / 4.0 * (k1 + 3 + k3));
            }
            return a;
        }
        #endregion
        #region Kutta 3-order RK
        /// <summary>
        /// the Kutta 3-order Runge Kutta algorithms for scalar differential equatoin
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1"> an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete scalar numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static VectorD Kutta3RK(Func<double, double, double> f, double initialValue, VectorD input_1)
        {
            var N = input_1.Count;
            var a = new VectorD(N) { [0] = initialValue };
            var h = 0.0;
            for (int i = 0; i < N - 1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i] + h / 2.0, a[i] + h / 2.0 * k1);
                var k3 = f(input_1[i] + h, a[i] - h * k1 + 2 * h * k2);
                a[i + 1] = a[i] + h / 6.0 * (k1 + 4 * k2 + k3);
            }
            return a;
        }
        /// <summary>
        /// the Kutta 3-order Runge Kutta algorithms for vectorial differential equatoin
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1">an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete vectorial numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static List<VectorD> Kutta3RK(Func<double, VectorD, VectorD> f, VectorD initialValue, VectorD input_1)
        { 
            var N = input_1.Count;
            var a = new List<VectorD>();
            var h = 0.0;
            a.Add(initialValue);
            for (int i = 0; i < N-1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i] + 0.5 * h, a[i] + 0.5 * h * k1);
                var k3 = f(input_1[i] + h, a[i] - h * k1 + 2 * h * k2);
                a.Add(a[i] + h * (k1 + 4 + k2 + k3) / 6.0);
            }
            return a;
        }
        #endregion
        #region Classical 4-order RK
        /// <summary>
        /// the Classical 4-order Runge Kutta algorithms for scalar differential equatoin
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1"> an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete scalar numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static VectorD Classical4RK(Func<double, double, double> f, double initialValue, VectorD input_1)
        {
            var N = input_1.Count;
            var a = new VectorD(N) { [0] = initialValue };
            var h = 0.0;
            for (int i = 0; i < N - 1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i] + h / 2.0, a[i] + h / 2.0 * k1);
                var k3 = f(input_1[i] + h / 2.0, a[i] + h / 2.0 * k2);
                var k4 = f(input_1[i] + h, a[i] + h * k3);
                a[i + 1] = a[i] + h / 6.0 * (k1 + 2 * k2 + 2 * k3 + k4);
            }
            return a;
        }
        /// <summary>
        /// the Classical 4-order Runge Kutta algorithms for vectorial differential equatoin
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1">an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete vectorial numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static List<VectorD> Classical4RK(Func<double, VectorD, VectorD> f, VectorD initialValue, VectorD input_1)
        { 
            var N = input_1.Count;
            var a = new List<VectorD>();
            var h = 0.0;
            a.Add(initialValue);
            for (int i = 0; i < N-1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i] + h / 2.0, a[i] + h / 2.0 * k1);
                var k3 = f(input_1[i] + h / 2.0, a[i] + h / 2.0 + k2);
                var k4 = f(input_1[i] + h, a[i] + h * k3);
                a.Add((k1 + 2 * k2 + 2 * k3 + k4) / 6);
            }
            return a;
        }
        #endregion
        #region Another 4-order RK
        /// <summary>
        /// another one common 4-order Runge Kutta algorithms for scalar differential equatoin
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1"> an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete scalar numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static VectorD Additional4RK(Func<double, double, double> f, double initialValue, VectorD input_1)
        {
            var N = input_1.Count;
            var a = new VectorD(N) { [0] = initialValue };
            var h = 0.0;
            for (int i = 0; i < N - 1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i] + h / 3.0, a[i] + h / 3.0 * k1);
                var k3 = f(input_1[i] + 2.0 * h / 3.0, a[i] - h / 3.0 * k1 + h * k2);
                var k4 = f(input_1[i] + h, a[i] + h * k3);
                a[i + 1] = a[i] + h / 8.0 * (k1 + 3 * k2 + 3 * k3 + k4);
            }
            return a;
        }
        /// <summary>
        /// another one common 4-order Runge Kutta algorithms for vectorial differential equatoin
        /// </summary>
        /// <param name="f"> a function in the right side of a standard differential equation"dy/dx=f(x,y)"</param>
        /// <param name="initialValue"> the boundary condition of a differential equation</param>
        /// <param name="input_1">an x-coordinate Vector that the corresponding numerical result should be calculated</param>
        /// <returns>series of discrete vectorial numerical solutions cosseponding the x-coordinate of input_1</returns>
        public static List<VectorD> Additional4RK(Func<double, VectorD, VectorD> f, VectorD initialValue, VectorD input_1)
        { 
            var N = input_1.Count;
            var a = new List<VectorD>();
            var h = 0.0;
            a.Add(initialValue);
            for (int i = 0; i < N-1; i++)
            {
                h = input_1[i + 1] - input_1[i];
                var k1 = f(input_1[i], a[i]);
                var k2 = f(input_1[i] + h / 3.0, a[i] + h / 3.0 * k1);
                var k3 = f(input_1[i] + 2.0 * h / 3.0, a[i] - h / 3.0 * k1 + h * k2);
                var k4 = f(input_1[i] + h, a[i] + h + k3);
                a.Add(a[i] + h * (k1 + 3 * k2 + 3 * k3 + k4) / 8);
            }
            return a;
        }
        #endregion
    }
}
