using System.Numerics;

namespace VEMS.MathCore
{
    public class Pwct2DCplxData
    {
        #region nested types

        public enum Shape2D
        {
            Rectangle,
            Circle,
            Ellipse,
            Diamond
        }

        public class Region2D
        {
            public Shape2D Shape { get; set; }
            public Complex Value { get; set; }

            public double CenterX { get; set; }
            public double CenterY { get; set; }

            public double A { get; set; }
            public double B { get; set; }

            public double Angle { get; set; } = 0.0;

            public Region2D(
                Shape2D shape,
                Complex value,
                double centerX,
                double centerY,
                double a,
                double b = 0.0,
                double angle = 0.0)
            {
                Shape = shape;
                Value = value;
                CenterX = centerX;
                CenterY = centerY;
                A = a;
                B = b;
                Angle = angle;
            }

            public bool Contains(double x, double y)
            {
                double dx = x - CenterX;
                double dy = y - CenterY;

                if (Angle != 0.0)
                {
                    double c = Math.Cos(-Angle);
                    double s = Math.Sin(-Angle);
                    double xr = c * dx - s * dy;
                    double yr = s * dx + c * dy;
                    dx = xr;
                    dy = yr;
                }

                switch (Shape)
                {
                    case Shape2D.Rectangle:
                        return Math.Abs(dx) <= A && Math.Abs(dy) <= B;

                    case Shape2D.Circle:
                        return dx * dx + dy * dy <= A * A;

                    case Shape2D.Ellipse:
                        return (dx * dx) / (A * A) + (dy * dy) / (B * B) <= 1.0;

                    case Shape2D.Diamond:
                        return Math.Abs(dx) / A + Math.Abs(dy) / B <= 1.0;

                    default:
                        return false;
                }
            }
        }

        #endregion

        #region properties

        public double PeriodX { get; set; }
        public double PeriodY { get; set; }

        public Complex BackgroundValue { get; set; }

        public List<Region2D> Regions { get; set; }

        #endregion

        #region constructor

        public Pwct2DCplxData(
            double periodX,
            double periodY,
            Complex backgroundValue,
            IEnumerable<Region2D>? regions = null)
        {
            if (periodX <= 0.0)
                throw new ArgumentException("periodX must be positive.", nameof(periodX));
            if (periodY <= 0.0)
                throw new ArgumentException("periodY must be positive.", nameof(periodY));

            PeriodX = periodX;
            PeriodY = periodY;
            BackgroundValue = backgroundValue;
            Regions = regions?.ToList() ?? new List<Region2D>();
        }

        #endregion

        #region methods

        public Complex Evaluate(double x, double y)
        {
            double xx = x % PeriodX;
            double yy = y % PeriodY;

            if (xx < 0) xx += PeriodX;
            if (yy < 0) yy += PeriodY;

            Complex value = BackgroundValue;

            foreach (var region in Regions)
            {
                if (region.Contains(xx, yy))
                    value = region.Value;
            }

            return value;
        }

        /// <summary>
        /// re-samples the data on a target uniform 2D grid
        /// </summary>
        /// <param name="grid"> target uniform 2D grid </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled values on the target grid </returns>
        public MatrixZ SampleOnGrid(GridInfo2D grid,
            LoopMode loopMode = Defaults.LoopOption)
        {
            MatrixZ vs = new(rows: grid.Rows, cols: grid.Cols);

            switch (loopMode)
            {
                case LoopMode.Sequential:
                    for (long i = 0; i < grid.Rows; i++)
                    {
                        double y = grid.GetCoordinateY(i);
                        for (long j = 0; j < grid.Cols; j++)
                        {
                            double x = grid.GetCoordinateX(j);
                            vs[i, j, false] = Evaluate(x, y);
                        }
                    }
                    break;

                case LoopMode.Parallel:
                    Parallel.For(0, (int)grid.Rows, i =>
                    {
                        double y = grid.GetCoordinateY(i);
                        for (long j = 0; j < grid.Cols; j++)
                        {
                            double x = grid.GetCoordinateX(j);
                            vs[i, j, false] = Evaluate(x, y);
                        }
                    });
                    break;

                default:
                    goto case LoopMode.Sequential;
            }

            return vs;
        }

        /// <summary>
        /// re-samples the data on uniform 2D grid covering
        /// full range with specific row/column sample counts
        /// </summary>
        /// <param name="rows"> number of samples along y </param>
        /// <param name="cols"> number of samples along x </param>
        /// <param name="loopMode"> loop-computational mode options </param>
        /// <returns> sampled values on the uniform grid and the grid itself </returns>
        public (MatrixZ, GridInfo2D) SampleOnGrid(long rows, long cols,
            LoopMode loopMode = Defaults.LoopOption)
        {
            GridInfo2D g = new(
                rows: rows,
                cols: cols,
                spacingY: PeriodY / rows,
                spacingX: PeriodX / cols,
                refPointY: 0.5 * PeriodY / rows,
                refPointX: 0.5 * PeriodX / cols,
                refTypeY: GridRefType.Start,
                refTypeX: GridRefType.Start
            );

            MatrixZ v = SampleOnGrid(grid: g, loopMode: loopMode);
            return (v, g);
        }

        #endregion
    }
}
