using System.Numerics;

namespace VEMS.MathCore
{
    /// <summary>
    /// Runge-Kutta methods collection
    /// </summary>
    public class RungeKutta
    {
        /// <summary>
        /// explicit Runge-Kutta methods
        /// </summary>
        public class Explicit
        {
            #region properties


            #endregion
            #region constructor

            /// <summary>
            /// initialize an empty RungeKutta Explicit class
            /// </summary>
            internal Explicit() { }

            #endregion
            #region methods

            /// <summary>
            /// Runge-Kutta solver for ordinary differential equations 
            /// in the form: df(t)/dt = g(t, f)
            /// </summary>
            /// <param name="df"> first-order derivative df(t)/dt = g(t, f) </param>
            /// <param name="f"> value of f, initialized as f(t0) </param>
            /// <param name="tSpan"> each t value during the calculation - can be non-equidistant </param>
            /// <param name="showProgress"> whether to show the calculation process </param>
            /// <param name="order"> order of the RK method, default is RK4 </param>
            public static void Solve(Func<double, Complex, Complex> df,
                ref Complex f, VectorD tSpan,
                RKOrder order = RKOrder.Fourth, 
                bool showProgress = false)
            {
                // loop over all variables in tSpan
                for (long i = 1; i < tSpan.Count; i++)
                {
                    // current step size
                    double dt = tSpan[i] - tSpan[i - 1];
                    // cases for different order
                    switch (order)
                    {
                        case RKOrder.First:
                            {
                                Complex k1 = df(tSpan[i - 1], f);
                                f += dt * k1;
                                break;
                            }
                        case RKOrder.Second:
                            {
                                Complex k1 = df(tSpan[i - 1], f);
                                Complex k2 = df(tSpan[i], f + dt * k1);
                                f += 0.5 * dt * (k1 + k2);
                                break;
                            }
                        case RKOrder.Fourth:
                            {
                                Complex k1 = df(tSpan[i - 1], f);
                                Complex k2 = df(tSpan[i - 1] + 0.5 * dt, f + 0.5 * dt * k1);
                                Complex k3 = df(tSpan[i - 1] + 0.5 * dt, f + 0.5 * dt * k2);
                                Complex k4 = df(tSpan[i], f + dt * k3);
                                f += 1.0 / 6.0 * dt * (k1 + 2 * k2 + 2 * k3 * k4);
                                break;
                            }
                        case RKOrder.Fifth:
                            {
                                Complex k1 = df(tSpan[i - 1], f);
                                Complex k2 = df(tSpan[i - 1] + 0.25 * dt, f + 0.25 * k1);
                                Complex k3 = df(tSpan[i - 1] + 0.25 * dt, f + 0.125 * k1 + 0.125 * k2);
                                Complex k4 = df(tSpan[i - 1] + 0.5 * dt, f + 0.5 * k3);
                                Complex k5 = df(tSpan[i - 1] + 0.75 * dt, f + 3.0 / 16 * k1 - 3.0 / 8.0 * k2 + 3.0 / 8.0 * k3 + 9.0 / 16.0 * k4);
                                Complex k6 = df(tSpan[i], f - 3.0 / 7.0 * k1 + 8.0 / 7.0 * k2 + 6.0 / 7.0 * k3 - 12.0 / 7.0 * k4 + 8.0 / 7.0 * k5);
                                f += 1.0 / 90 * dt * (7 * k1 + 32 * k3 + 12 * k4 + 32 * k5 + 7 * k6);
                                break;
                            }
                        default: goto case RKOrder.Fourth;
                    }
                    if (showProgress)
                    {
                        double ii = i;
                        double p = ii / tSpan.Count;
                        Printer.Write($"... current progress of the RK solver: {p * 100}%;");
                    }
                }
            }


            /// <summary>
            /// Runge-Kutta solver for ordinary differential equations
            /// in the form: df(x, t)/dt = g(f, t)
            /// </summary>
            /// <param name="df"> first-order derivative df(t)/dt = g(t, f) </param>
            /// <param name="f"> value of f, initialized as f(t0) </param>
            /// <param name="tSpan"> each t value during the calculation - can be non-equidistant </param>
            /// <param name="showProgress"> whether to show the calculation process </param>
            /// <param name="order"> order of the RK method, default is RK4 </param>
            public static void Solve(Func<double, VectorZ, VectorZ> df,
                ref VectorZ f, VectorD tSpan,
                RKOrder order = RKOrder.Fourth, 
                bool showProgress = false)
            {
                // loop over all variables in tSpan
                for (long i = 1; i < tSpan.Count; i++) // starting from 1, since 0 is the initial value
                {
                    // current step size
                    double dt = tSpan[i] - tSpan[i - 1];
                    // cases for different order
                    switch (order)
                    {
                        case RKOrder.First:
                            {
                                VectorZ k1 = df(tSpan[i - 1], f);
                                f += dt * k1;
                                break;
                            }
                        case RKOrder.Second:
                            {
                                VectorZ k1 = df(tSpan[i - 1], f);
                                VectorZ k2 = df(tSpan[i], f + dt * k1);
                                f += 0.5 * dt * (k1 + k2);
                                break;
                            }
                        case RKOrder.Fourth:
                            {
                                VectorZ k1 = df(tSpan[i - 1], f);
                                VectorZ k2 = df(tSpan[i - 1] + 0.5 * dt, f + 0.5 * dt * k1);
                                VectorZ k3 = df(tSpan[i - 1] + 0.5 * dt, f + 0.5 * dt * k2);
                                VectorZ k4 = df(tSpan[i], f + dt * k3);
                                f += 1.0 / 6.0 * dt * (k1 + 2 * k2 + 2 * k3 + k4);
                                break;
                            }
                        case RKOrder.Fifth:
                            {
                                VectorZ k1 = df(tSpan[i - 1], f);
                                VectorZ k2 = df(tSpan[i - 1] + 0.25 * dt, f + 0.25 * k1);
                                VectorZ k3 = df(tSpan[i - 1] + 0.25 * dt, f + 0.125 * k1 + 0.125 * k2);
                                VectorZ k4 = df(tSpan[i - 1] + 0.5 * dt, f + 0.5 * k3);
                                VectorZ k5 = df(tSpan[i - 1] + 0.75 * dt, f + 3.0 / 16 * k1 - 3.0 / 8.0 * k2 + 3.0 / 8.0 * k3 + 9.0 / 16.0 * k4);
                                VectorZ k6 = df(tSpan[i], f - 3.0 / 7.0 * k1 + 8.0 / 7.0 * k2 + 6.0 / 7.0 * k3 - 12.0 / 7.0 * k4 + 8.0 / 7.0 * k5);
                                f += 1.0 / 90 * dt * (7 * k1 + 32 * k3 + 12 * k4 + 32 * k5 + 7 * k6);
                                break;
                            }
                        default: goto case RKOrder.Fourth;
                    }
                    if (showProgress)
                    {
                        double ii = i;
                        double p = ii / tSpan.Count;
                        Printer.Write($"... current progress of the RK solver: {p * 100}%;");
                    }
                }
            }


            /// <summary>
            /// Runge-Kutta solver for ordinary differential equations
            /// in the form: df(x, t)/dt = g(f, t)
            /// </summary>
            /// <param name="df"> first-order derivative df(t)/dt = g(t, f) </param>
            /// <param name="f"> value of f, initialized as f(t0) </param>
            /// <param name="tSpan"> each t value during the calculation - can be non-equidistant </param>
            /// <param name="showProgress"> whether to show the calculation process </param>
            /// <param name="order"> order of the RK method, default is RK4 </param>
            public static void Solve(Func<double, MatrixZ, MatrixZ> df,
                ref MatrixZ f, VectorD tSpan,
                RKOrder order = RKOrder.Fourth,
                bool showProgress = false)
            {
                // loop over all variables in tSpan
                for (long i = 1; i < tSpan.Count; i++) // starting from 1, since 0 is the initial value
                {
                    // current step size
                    double dt = tSpan[i] - tSpan[i - 1];
                    // cases for different order
                    switch (order)
                    {
                        case RKOrder.First:
                            {
                                MatrixZ k1 = df(tSpan[i - 1], f);
                                f += dt * k1;
                                break;
                            }
                        case RKOrder.Second:
                            {
                                MatrixZ k1 = df(tSpan[i - 1], f);
                                MatrixZ k2 = df(tSpan[i], f + dt * k1);
                                f += 0.5 * dt * (k1 + k2);
                                break;
                            }
                        case RKOrder.Fourth:
                            {
                                MatrixZ k1 = df(tSpan[i - 1], f);
                                MatrixZ k2 = df(tSpan[i - 1] + 0.5 * dt, f + 0.5 * dt * k1);
                                MatrixZ k3 = df(tSpan[i - 1] + 0.5 * dt, f + 0.5 * dt * k2);
                                MatrixZ k4 = df(tSpan[i], f + dt * k3);
                                f += 1.0 / 6.0 * dt * (k1 + 2 * k2 + 2 * k3 + k4);
                                break;
                            }
                        case RKOrder.Fifth:
                            {
                                MatrixZ k1 = df(tSpan[i - 1], f);
                                MatrixZ k2 = df(tSpan[i - 1] + 0.25 * dt, f + 0.25 * k1);
                                MatrixZ k3 = df(tSpan[i - 1] + 0.25 * dt, f + 0.125 * k1 + 0.125 * k2);
                                MatrixZ k4 = df(tSpan[i - 1] + 0.5 * dt, f + 0.5 * k3);
                                MatrixZ k5 = df(tSpan[i - 1] + 0.75 * dt, f + 3.0 / 16 * k1 - 3.0 / 8.0 * k2 + 3.0 / 8.0 * k3 + 9.0 / 16.0 * k4);
                                MatrixZ k6 = df(tSpan[i], f - 3.0 / 7.0 * k1 + 8.0 / 7.0 * k2 + 6.0 / 7.0 * k3 - 12.0 / 7.0 * k4 + 8.0 / 7.0 * k5);
                                f += 1.0 / 90 * dt * (7 * k1 + 32 * k3 + 12 * k4 + 32 * k5 + 7 * k6);
                                break;
                            }
                        default: goto case RKOrder.Fourth;
                    }
                    if (showProgress)
                    {
                        double ii = i;
                        double p = ii / tSpan.Count;
                        Printer.Write($"... current progress of the RK solver: {p * 100}%;");
                    }
                }
            }


            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="df"></param>
            /// <param name="f"></param>
            /// <param name="tSpan"></param>
            /// <param name="order"></param>
            internal static void Solve<T>(Func<double, Vector<T>, Vector<T>> df,
                ref Vector<T> f, Vector<double> tSpan,
                RKOrder order = RKOrder.Fourth)
                where T : struct
            {
                // ...
                for (long i = 1; i < tSpan.Count; i++) // starting from 1, since 0 is the initial value
                {
                    // current step size
                    double dt = tSpan[i] - tSpan[i - 1];

                    Vector<T> k1 = df(tSpan[i - 1], f);
                    //f += dt * k1;
                }
            }

            #endregion
        }

        /// <summary>
        /// implicit Runge-Kutta methods
        /// </summary>
        internal class Implicit
        {
            #region properties


            #endregion
            #region constructor

            internal Implicit() { }

            #endregion
            #region methods

            // ...

            #endregion
        }

    }


    /// <summary>
    /// order of explicit Runge-Kutta algorithm
    /// </summary>
    public enum RKOrder
    {
        /// <summary>
        /// 1st order Runge-Kutta method
        /// </summary>
        First,

        /// <summary>
        /// 2nd order Runge-Kutta method
        /// </summary>
        Second,

        /// <summary>
        /// 4th order Runge-Kutta method
        /// </summary>
        Fourth,

        /// <summary>
        /// 5th order Runge-Kutta method
        /// </summary>
        Fifth,
    }


}
