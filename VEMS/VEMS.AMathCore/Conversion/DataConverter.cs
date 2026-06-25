using System.Numerics;
using System.Text;

namespace VEMS.AMathCore
{
    /// <summary>
    /// converter for data to string in different forms
    /// </summary>
    public class DataConverter
    {
        #region properties

        /// <summary>
        /// column separator
        /// </summary>
        public char ColumnSeparator { get; set; }

        /// <summary>
        /// number converter
        /// </summary>
        public NumberConverter NumConverter { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a DataConverter
        /// </summary>
        /// <param name="columnSeparator"> column separator option </param>
        /// <param name="numFormat"> numeric format used for conversion </param>
        /// <param name="digits"> number of digits </param>
        /// <param name="culture"> culture selected for conversion </param>
        public DataConverter(CharSeparator? columnSeparator = null,
            NumericFormat? numFormat = null,
            int? digits = null,
            CultureName? culture = null)
        {
            // default values
            columnSeparator ??= MathSettings.ColumnSeparator;
            numFormat ??= MathSettings.NumberFormat;
            digits ??= MathSettings.NumberOfDigits;
            culture ??= MathSettings.Culture;
            // sets parameters
            ColumnSeparator = Converter.SeparatorToChar(columnSeparator);
            NumConverter = new NumberConverter(numFormat: numFormat,
                digits: digits,
                culture: culture);
        }

        #endregion
        #region methods: data to string [StringBuilder]

        #region ---- Array<T> ----

        /// <summary>
        /// Converts an array to a string representation
        /// </summary>
        /// <param name="vs">The array to convert.</param>
        /// <param name="cplxFormat">Optional complex number format. If null, uses default from <see cref="MathSettings.ComplexNumberFormat"/>.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the string representation of the vector.</returns>
        public unsafe StringBuilder ToString<T>(DenseArray<T> vs,
            ComplexFormat? cplxFormat = null)
            where T : INumber<T>
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            for (long i = 0; i < vs.Count; i++)
            {
                T val = *((T*)vs.VPtr + i);
                string vi = NumConverter.ToString(x: val, cplxFormat: cplxFormat);
                content.Append(vi + ColumnSeparator);
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        #endregion
        #region ---- Vect<T> + Grid1D ----

        /// <summary>
        /// Converts a vector to a string representation,
        /// with optional grid information.
        /// </summary>
        /// <param name="vs">The vector to convert.</param>
        /// <param name="cplxFormat">Optional complex number format. If null, uses default from <see cref="MathSettings.ComplexNumberFormat"/>.</param>
        /// <param name="grid">Optional grid information.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the string representation of the vector.</returns>
        public StringBuilder ToString<T>(Vect<T> vs,
            ComplexFormat? cplxFormat = null,
            GridInfo1D? grid = null)
            where T : INumber<T>
        {
            // initializes content StringBuilder
            StringBuilder content = new();
            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }


            // version #1: generic T type
            for (long i = 0; i < vs.Count; i++)
            {
                string vi = NumConverter.ToString(x: vs[i, checkBound: false],
                    cplxFormat: cplxFormat);
                content.Append(vi + ColumnSeparator);
            }
            content.Remove(startIndex: content.Length - 1, length: 1);


            //// version #2: specific type conversion ...
            //if (typeof(T) == typeof(Real))
            //{
            //    Vect<Real> v = (Vect<Real>)(object)vs;
            //    for (long i = 0; i < vs.Count; i++)
            //    {
            //        string vi = NumConverter.ToString(x: v[i, checkBound: false]);
            //        content.Append(vi + ColumnSeparator);
            //    }
            //}
            //else if (typeof(T) == typeof(Int))
            //{
            //    Vect<Int> v = (Vect<Int>)(object)vs;
            //    for (long i = 0; i < vs.Count; i++)
            //    {
            //        string vi = NumConverter.ToString(x: v[i, checkBound: false]);
            //        content.Append(vi + ColumnSeparator);
            //    }
            //}
            //else if (typeof(T) == typeof(Cplx))
            //{
            //    Vect<Cplx> v = (Vect<Cplx>)(object)vs;
            //    for (long i = 0; i < vs.Count; i++)
            //    {
            //        string vi = NumConverter.ToString(x: v[i, checkBound: false],
            //            cplxFormat: cplxFormat);
            //        content.Append(vi + ColumnSeparator);
            //    }
            //}
            //else throw new NotSupportedException($"DataConverter.ToString: type {typeof(T).Name} not supported.");
            //content.Remove(startIndex: content.Length - 1, length: 1);


            // return
            return content;
        }

        #endregion
        #region ---- Matx<T> + Grid2D ----

        /// <summary>
        /// Converts a matrix with a generic numeric element type to a string representation.
        /// Each matrix row is converted to a line where elements are separated by the configured
        /// <see cref="ColumnSeparator"/>. Lines are separated by newline characters.
        /// </summary>
        /// <typeparam name="T">
        /// The element type of the matrix. Must implement <see cref="INumber{T}"/>.
        /// </typeparam>
        /// <param name="vs">The matrix to convert.</param>
        /// <param name="cplxFormat">
        /// Optional complex number format. If <c>null</c>, no special complex formatting is applied.
        /// When elements are complex, this value is forwarded to <see cref="NumberConverter.ToString"/>.
        /// </param>
        /// <param name="grid">
        /// Optional grid information. Grid support is not implemented for this overload.
        /// If a non-<c>null</c> value is provided, a <see cref="NotImplementedException"/> is thrown.
        /// </param>
        /// <param name="revRows">
        /// If <c>true</c>, rows are exported in reverse order (last row first).
        /// </param>
        /// <returns>
        /// A <see cref="StringBuilder"/> containing the string representation of the matrix.
        /// The last line does not contain a trailing newline.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// Thrown when <paramref name="grid"/> is not <c>null</c>.
        /// </exception>
        public StringBuilder ToString<T>(Matx<T> vs,
            ComplexFormat? cplxFormat = null,
            GridInfo2D? grid = null,
            bool revRows = false)
            where T : INumber<T>
        {
            // initializes content StringBuilder
            StringBuilder content = new();
            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }

            for (long iRow = 0; iRow < vs.Rows; iRow++)
            {
                // controls the order of rows
                long dataRow = revRows ? vs.Rows - iRow - 1 : iRow;

                StringBuilder line = new ();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false],
                        cplxFormat: cplxFormat);
                    line.Append(vij + ColumnSeparator);
                }
                line.Remove(startIndex: line.Length - 1, length: 1);
                line.Append('\n');
                content.Append(line);
            }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }


        /// <summary>
        /// Converts a real-valued matrix to a string representation,
        /// with optional grid information.
        /// </summary>
        /// <param name="vs">The real-valued matrix to convert.</param>
        /// <param name="grid">Optional grid information. If provided, not implemented and will throw.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <param name="revRows">Controls the order of rows when exporting the matrix. If true, rows are reversed.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the string representation of the matrix.</returns>
        [Obsolete]
        public StringBuilder ToString(Matx<Real> vs,
            GridInfo2D? grid = null,
            LoopMode loopMode = Defaults.LoopOption,
            bool revRows = false)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values in lines
            StringBuilder[] lines = new StringBuilder[vs.Rows];
            void op(long iRow)
            {
                //Control the order of rows when exporting the matrix
                long dataRow = revRows ? vs.Rows - iRow - 1 : iRow;

                lines[iRow] = new StringBuilder();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false]);
                    lines[iRow].Append(vij + ColumnSeparator);
                }
                lines[iRow].Remove(startIndex: lines[iRow].Length - 1, length: 1);
                lines[iRow].Append('\n');
            }
            Loop1D loop = new(operation: op, start: 0, end: vs.Rows, step: 1);
            loop.Evaluate(mode: loopMode);
            // connects lines
            for (long iRow = 0; iRow < vs.Rows; iRow++)
            { content.Append(lines[iRow]); }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// Converts a complex-valued matrix to a string representation,
        /// with optional grid information.
        /// </summary>
        /// <param name="vs">The complex-valued matrix to convert.</param>
        /// <param name="cplxFormat">Optional complex number format. If null, uses default from <see cref="MathSettings.ComplexNumberFormat"/>.</param>
        /// <param name="grid">Optional grid information. If provided, not implemented and will throw.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <param name="revRows">Controls the order of rows when exporting the matrix. If true, rows are reversed.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the string representation of the matrix.</returns>
        [Obsolete]
        public StringBuilder ToString(Matx<Cplx> vs,
            ComplexFormat? cplxFormat = null,
            GridInfo2D? grid = null,
            LoopMode loopMode = Defaults.LoopOption,
            bool revRows = false)
        {
            // default value
            cplxFormat ??= MathSettings.ComplexNumberFormat;
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values in lines
            StringBuilder[] lines = new StringBuilder[vs.Rows];
            void op(long iRow)
            {
                //Control the order of rows when exporting the matrix
                long dataRow = revRows ? vs.Rows - iRow - 1 : iRow;

                lines[iRow] = new StringBuilder();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false],
                        cplxFormat: cplxFormat);
                    lines[iRow].Append(vij + ColumnSeparator);
                }
                lines[iRow].Remove(startIndex: lines[iRow].Length - 1, length: 1);
                lines[iRow].Append('\n');
            }
            Loop1D loop = new(operation: op, start: 0, end: vs.Rows, step: 1);
            loop.Evaluate(mode: loopMode);
            // connects lines
            for (long iRow = 0; iRow < vs.Rows; iRow++)
            { content.Append(lines[iRow]); }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        /// <summary>
        /// Converts an integer-valued matrix to a string representation,
        /// with optional grid information.
        /// </summary>
        /// <param name="vs">The integer-valued matrix to convert.</param>
        /// <param name="grid">Optional grid information. If provided, not implemented and will throw.</param>
        /// <param name="loopMode">Loop-computational mode options.</param>
        /// <param name="revRows">Controls the order of rows when exporting the matrix. If true, rows are reversed.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the string representation of the matrix.</returns>
        [Obsolete]
        public StringBuilder ToString(Matx<Int> vs,
            GridInfo2D? grid = null,
            LoopMode loopMode = Defaults.LoopOption,
            bool revRows = false)
        {
            // initializes content StringBuilder
            StringBuilder content = new();

            // grid info
            if (grid != null)
            { throw new NotImplementedException(); }
            // values in lines
            StringBuilder[] lines = new StringBuilder[vs.Rows];
            void op(long iRow)
            {
                //Control the order of rows when exporting the matrix
                long dataRow = revRows ? vs.Rows - iRow - 1 : iRow;

                lines[iRow] = new StringBuilder();
                for (long iCol = 0; iCol < vs.Cols; iCol++)
                {
                    string vij = NumConverter.ToString(x: vs[dataRow, iCol, checkBound: false]);
                    lines[iRow].Append(vij + ColumnSeparator);
                }
                lines[iRow].Remove(startIndex: lines[iRow].Length - 1, length: 1);
                lines[iRow].Append('\n');
            }
            Loop1D loop = new(operation: op, start: 0, end: vs.Rows, step: 1);
            loop.Evaluate(mode: loopMode);
            // connects lines
            for (long iRow = 0; iRow < vs.Rows; iRow++)
            { content.Append(lines[iRow]); }
            content.Remove(startIndex: content.Length - 1, length: 1);

            // return
            return content;
        }

        #endregion

        #endregion
    }
}
