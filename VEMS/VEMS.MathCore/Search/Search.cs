namespace VEMS.MathCore
{

    /// <summary>
    /// search methods
    /// </summary>
    public class Search
    {

        /// <summary>
        /// performs binary search for given span and location
        /// </summary>
        /// <param name="xs"> input span </param>
        /// <param name="x"> location </param>
        /// <param name="k"> index in the span </param>
        /// <returns> whether found or not </returns>
        public static bool Binary(VectorD xs, double x,
            out long k)
        {
            long n = xs.Count;
            k = n - 1;
            if (n == 0) { return false; }

            double first = xs[0, false];
            double last = xs[k, false];
            if (x < first || x > last) { return false; }
            if (x == last) { return true; }

            long iBegin = 0;
            long iEnd = k;
            while (iBegin < iEnd)
            {
                //long iMid = iBegin + (iEnd - iBegin) / 2;
                long iMid = iBegin + ((iEnd - iBegin) >> 1);
                double midVal = xs[iMid, false];

                if (x < midVal)
                { iEnd = iMid; }
                else
                { iBegin = iMid + 1; }
            }
            k = iBegin - 1;
            if (k < 0) k = 0;
            return true;
        }

    }
}
