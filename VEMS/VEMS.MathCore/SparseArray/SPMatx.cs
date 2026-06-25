using System.Numerics;

namespace VEMS.MathCore
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SPMatx<T> : IMatx<T> where T : INumber<T>
    {
        #region properties


        public long Rows { get; set; }


        public long Cols { get; set; }

        public long NzCount { get; private set; }


        public IntPtr Handle { get; private set; }

        public SPARSE_Status Status { get; private set; }

        #endregion

        public T this[long iRow, long iCol, bool checkBound = true] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public T this[int iRow, int iCol, bool checkBound = true] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public T this[Index iRow, Index iCol, bool checkBound = true] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //public long Rows { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public long Cols { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
