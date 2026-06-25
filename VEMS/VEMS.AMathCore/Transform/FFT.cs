using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEMS.AMathCore
{

    /// <summary>
    /// FFT factory
    /// </summary>
    internal class FFTFactory
    {

        internal IFFT iFFT { get; set; }

        internal IVMF iVMF { get; set; }

        internal IBLAS iBLAS { get; set; }

        internal FFTFactory()
        {
            iFFT = Defaults.IFFT;
            iVMF = Defaults.IVMF;
            iBLAS = Defaults.IBLAS;
        }
    }


    internal class FFT
    {
    }
}
