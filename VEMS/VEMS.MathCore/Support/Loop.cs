
namespace VEMS.MathCore
{

    /// <summary>
    /// Provides static methods for executing sequential and parallel loops in one and two dimensions.
    /// </summary>
    internal class Loops
    {

        #region ---- 1D Loops ----

        /// <summary>
        /// Executes a sequential loop for the 1D case.
        /// </summary>
        /// <param name="op">The operation to be performed on each iteration.</param>
        /// <param name="start">Starting index of the loop.</param>
        /// <param name="end">Ending index of the loop (exclusive).</param>
        /// <param name="step">Step size for each iteration; default is 1.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="op"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="step"/> is zero.</exception>
        public static void Seq1D(Action<long> op,
            long start, long end, long step = 1)
        {
            if (op == null) { throw new ArgumentNullException(nameof(op)); }
            if (step == 0) { throw new ArgumentOutOfRangeException(nameof(step), "Step cannot be zero."); }

            // Optimize for step == 1 (most common case)
            if (step == 1)
            {
                for (long i = start; i < end; ++i) { op(i); }
            }
            else if (step > 0)
            {
                for (long i = start; i < end; i += step) { op(i); }
            }
            else // step < 0
            {
                for (long i = start; i > end; i += step) { op(i); }
            }
        }

        /// <summary>
        /// Executes a parallel loop for the 1D case.
        /// </summary>
        /// <param name="op">The operation to be performed on each iteration.</param>
        /// <param name="start">Starting index of the loop.</param>
        /// <param name="end">Ending index of the loop (exclusive).</param>
        /// <param name="step">Step size for each iteration; default is 1.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="op"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="step"/> is zero.</exception>
        public static void Par1D(Action<long> op,
            long start, long end, long step = 1)
        {
            if (op == null) { throw new ArgumentNullException(nameof(op)); }
            if (step == 0) { throw new ArgumentOutOfRangeException(nameof(step), "Step cannot be zero."); }

            // Optimize for step == 1 (most common case)
            if (step == 1)
            {
                Parallel.For(start, end, i => op(i));
            }
            else if (step > 0)
            {
                long range = end - start;
                long count = (range + step - 1) / step;
                Parallel.For(0L, count, idx =>
                {
                    long i = start + idx * step;
                    if (i < end) { op(i); }
                });
            }
            else // step < 0
            {
                long range = start - end;
                long count = (range - step - 1) / (-step);
                Parallel.For(0L, count, idx =>
                {
                    long i = start + idx * step;
                    if (i > end) { op(i); }
                });
            }
        }

        #endregion
        #region ---- 2D Loops ----

        /// <summary>
        /// Executes a sequential loop for the 2D case.
        /// </summary>
        /// <param name="op">The operation to be performed on each iteration.</param>
        /// <param name="rowStart">Starting row index of the loop.</param>
        /// <param name="rowEnd">Ending row index of the loop (exclusive).</param>
        /// <param name="colStart">Starting column index of the loop.</param>
        /// <param name="colEnd">Ending column index of the loop (exclusive).</param>
        /// <param name="rowStep">Row step size for each iteration; default is 1.</param>
        /// <param name="colStep">Column step size for each iteration; default is 1.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="op"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="rowStep"/> or <paramref name="colStep"/> is zero.</exception>
        public static void Seq2D(Action<long, long> op,
            long rowStart, long rowEnd,
            long colStart, long colEnd,
            long rowStep = 1, long colStep = 1)
        {
            if (op == null) { throw new ArgumentNullException(nameof(op)); }
            if (rowStep == 0) { throw new ArgumentOutOfRangeException(nameof(rowStep), "Row step cannot be zero."); }
            if (colStep == 0) { throw new ArgumentOutOfRangeException(nameof(colStep), "Column step cannot be zero."); }

            // Optimize for step == 1 (most common case)
            if (rowStep == 1 && colStep == 1)
            {
                for (long iRow = rowStart; iRow < rowEnd; ++iRow)
                    for (long iCol = colStart; iCol < colEnd; ++iCol)
                        op(iRow, iCol);
            }
            else
            {
                for (long iRow = rowStart; rowStep > 0 ? iRow < rowEnd : iRow > rowEnd; iRow += rowStep)
                    for (long iCol = colStart; colStep > 0 ? iCol < colEnd : iCol > colEnd; iCol += colStep)
                        op(iRow, iCol);
            }
        }

        /// <summary>
        /// Executes a parallel loop for the 2D case.
        /// </summary>
        /// <param name="op">The operation to be performed on each iteration.</param>
        /// <param name="rowStart">Starting row index of the loop.</param>
        /// <param name="rowEnd">Ending row index of the loop (exclusive).</param>
        /// <param name="colStart">Starting column index of the loop.</param>
        /// <param name="colEnd">Ending column index of the loop (exclusive).</param>
        /// <param name="rowStep">Row step size for each iteration; default is 1.</param>
        /// <param name="colStep">Column step size for each iteration; default is 1.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="op"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="rowStep"/> or <paramref name="colStep"/> is zero.</exception>
        public static void Par2D(Action<long, long> op,
            long rowStart, long rowEnd,
            long colStart, long colEnd,
            long rowStep = 1, long colStep = 1)
        {
            if (op == null) { throw new ArgumentNullException(nameof(op)); }
            if (rowStep == 0) { throw new ArgumentOutOfRangeException(nameof(rowStep), "Row step cannot be zero."); }
            if (colStep == 0) { throw new ArgumentOutOfRangeException(nameof(colStep), "Column step cannot be zero."); }

            // Optimize for step == 1 (most common case)
            if (rowStep == 1 && colStep == 1)
            {
                Parallel.For(rowStart, rowEnd, iRow =>
                {
                    for (long iCol = colStart; iCol < colEnd; ++iCol)
                        op(iRow, iCol);
                });
            }
            else
            {
                long rows = (rowEnd - rowStart + (rowStep > 0 ? rowStep - 1 : rowStep + 1)) / rowStep;
                long cols = (colEnd - colStart + (colStep > 0 ? colStep - 1 : colStep + 1)) / colStep;
                long total = rows * cols;

                Parallel.For(0L, total, i =>
                {
                    long iRow = (i / cols) * rowStep + rowStart;
                    long iCol = (i % cols) * colStep + colStart;
                    // Check bounds for negative steps
                    if ((rowStep > 0 ? iRow < rowEnd : iRow > rowEnd) &&
                        (colStep > 0 ? iCol < colEnd : iCol > colEnd))
                    {
                        op(iRow, iCol);
                    }
                });
            }
        }

        #endregion

    }


    /// <summary>
    /// Represents a one-dimensional (1D) loop with customizable operation, range, and step size.
    /// </summary>
    public class Loop1D
    {
        #region properties

        /// <summary>
        /// Gets or sets the operation to be performed on each iteration of the loop.
        /// </summary>
        /// <remarks>
        /// The operation is a delegate that takes the current iteration index as a parameter.
        /// </remarks>
        public Action<long>? Operation { get; set; }

        /// <summary>
        /// Gets or sets the starting index of the loop.
        /// </summary>
        public long Start { get; set; }

        /// <summary>
        /// Gets or sets the ending index of the loop (exclusive).
        /// </summary>
        public long End { get; set; }

        /// <summary>
        /// Gets or sets the step size for each iteration.
        /// </summary>
        public long Step { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Loop1D"/> class.
        /// </summary>
        internal Loop1D() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Loop1D"/> class with the specified operation, range, and step size.
        /// </summary>
        /// <param name="operation">The action to be performed on each iteration.</param>
        /// <param name="start">The starting index of the loop.</param>
        /// <param name="end">The ending index of the loop (exclusive).</param>
        /// <param name="step">The step size for each iteration; default is 1.</param>
        public Loop1D(Action<long> operation,
            long start, long end, long step = 1)
        {
            Operation = operation;
            Start = start;
            End = end;
            Step = step;
        }

        #endregion
        #region methods

        /// <summary>
        /// Evaluates the loop based on the specified computational mode.
        /// </summary>
        /// <param name="mode">The loop-computational mode option; default is <see cref="LoopMode.Sequential"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="Operation"/> is null.</exception>
        /// <exception cref="NotImplementedException">Thrown if <paramref name="mode"/> is <see cref="LoopMode.Vectorized"/>.</exception>
        public void Evaluate(LoopMode mode = Defaults.LoopOption)
        {
            if (Operation == null) { throw new ArgumentNullException(nameof(Operation)); }

            switch (mode)
            {
                case LoopMode.Sequential:
                    Loops.Seq1D(op: Operation, start: Start, end: End, step: Step);
                    break;
                case LoopMode.Parallel:
                    Loops.Par1D(op: Operation, start: Start, end: End, step: Step);
                    break;
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }
        }

        #endregion
    }


    /// <summary>
    /// Represents a two-dimensional (2D) loop with customizable operation, range, and step size for both rows and columns.
    /// </summary>
    public class Loop2D
    {
        #region properties

        /// <summary>
        /// Gets or sets the operation to be performed on each iteration.
        /// <para>The first parameter is the iteration index along the row.</para>
        /// <para>The second parameter is the iteration index along the column.</para>
        /// </summary>
        public Action<long, long>? Operation { get; set; }

        /// <summary>
        /// Gets or sets the starting row index of the loop.
        /// </summary>
        public long RowStart { get; set; }

        /// <summary>
        /// Gets or sets the ending row index of the loop (exclusive).
        /// </summary>
        public long RowEnd { get; set; }

        /// <summary>
        /// Gets or sets the row step size for each iteration.
        /// </summary>
        public long RowStep { get; set; }

        /// <summary>
        /// Gets or sets the starting column index of the loop.
        /// </summary>
        public long ColStart { get; set; }

        /// <summary>
        /// Gets or sets the ending column index of the loop (exclusive).
        /// </summary>
        public long ColEnd { get; set; }

        /// <summary>
        /// Gets or sets the column step size for each iteration.
        /// </summary>
        public long ColStep { get; set; }

        #endregion
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Loop2D"/> class.
        /// </summary>
        internal Loop2D() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Loop2D"/> class with the specified operation, range, and step sizes.
        /// </summary>
        /// <param name="operation">The action to be performed on each iteration.</param>
        /// <param name="rowStart">The starting row index of the loop.</param>
        /// <param name="rowEnd">The ending row index of the loop (exclusive).</param>
        /// <param name="colStart">The starting column index of the loop.</param>
        /// <param name="colEnd">The ending column index of the loop (exclusive).</param>
        /// <param name="rowStep">The row step size for each iteration; default is 1.</param>
        /// <param name="colStep">The column step size for each iteration; default is 1.</param>
        public Loop2D(Action<long, long> operation,
            long rowStart, long rowEnd,
            long colStart, long colEnd,
            long rowStep = 1, long colStep = 1)
        {
            Operation = operation;
            RowStart = rowStart;
            RowEnd = rowEnd;
            RowStep = rowStep;
            ColStart = colStart;
            ColEnd = colEnd;
            ColStep = colStep;
        }

        #endregion
        #region methods

        /// <summary>
        /// Evaluates the loop based on the specified computational mode.
        /// </summary>
        /// <param name="mode">The loop-computational mode option; default is <see cref="LoopMode.Sequential"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="Operation"/> is null.</exception>
        /// <exception cref="NotImplementedException">Thrown if <paramref name="mode"/> is <see cref="LoopMode.Vectorized"/>.</exception>
        public void Evaluate(LoopMode mode = Defaults.LoopOption)
        {
            if (Operation == null) { throw new ArgumentNullException(nameof(Operation)); }

            switch (mode)
            {
                case LoopMode.Sequential:
                    Loops.Seq2D(op: Operation,
                        rowStart: RowStart, rowEnd: RowEnd,
                        colStart: ColStart, colEnd: ColEnd,
                        rowStep: RowStep, colStep: ColStep);
                    break;
                case LoopMode.Parallel:
                    Loops.Par2D(op: Operation,
                        rowStart: RowStart, rowEnd: RowEnd,
                        colStart: ColStart, colEnd: ColEnd,
                        rowStep: RowStep, colStep: ColStep);
                    break;
                case LoopMode.Vectorized:
                    throw new NotImplementedException();
                default: goto case LoopMode.Sequential;
            }
        }

        #endregion
    }


    /// <summary>
    /// loop tests ...
    /// </summary>
    public static class LoopTest
    {

        /// <summary>
        /// 2D loop with least square operation in the kernel
        /// </summary>
        /// <param name="loopRows"></param>
        /// <param name="loopCols"></param>
        /// <param name="aRows"></param>
        /// <param name="aCols"></param>
        /// <returns></returns>
        public static void LeastSquare(long loopRows, long loopCols,
            long aRows, long aCols)
        {

            // matrix A on the left and vector B on the right
            var a = VStat.RngUniform(rows: aRows, cols: aCols);
            var b = new VectorD(count: aRows, initVal: 1.0);

            //// naive loop
            //for(long iRow = 0; iRow < loopRows; iRow ++)
            //{
            //    for(long iCol = 0; iCol < loopCols; iCol ++)
            //    {
            //        var x = LinAlg.LeastSquare(a, b);
            //    }
            //}

            // defines single operation
            Action<long, long> op = (iRow, iCol) =>
            {
                var x = LinAlg.LeastSquare(a, b);
            };
            Loop2D loop = new(operation: op,
                rowStart: 0, rowEnd: loopRows,
                colStart: 0, colEnd: loopCols);
            loop.Evaluate(mode: LoopMode.Parallel);
        }

    }
}


