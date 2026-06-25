namespace VEMS.AMathCore
{

    /// <summary>
    /// FFT interface
    /// </summary>
    public interface IFFT 
    {

        /// <summary>
        /// DftiCreateDescriptor wrapper (1D)
        /// </summary>
        /// <param name="desc"> DFTI descriptor </param>
        /// <param name="precision"> precision (ConfigValue) </param>
        /// <param name="domain"> domain (ConfigValue) </param>
        /// <param name="dimension"> dimension of the transform </param>
        /// <param name="length"> length given in long format </param>
        /// <returns> error information </returns>
        int DftiCreateDescriptor(ref IntPtr desc,
            FFTConfigValue precision, FFTConfigValue domain, int dimension, long length);

        /// <summary>
		/// DftiCreateDescriptor wrapper (2D)
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
		/// <param name="precision"> precision (ConfigValue) </param>
		/// <param name="domain"> domain (ConfigValue) </param>
		/// <param name="dimension"> dimension of the transform </param>
		/// <param name="lengths"> lengths given in long format </param>
		/// <returns> error information </returns>
        int DftiCreateDescriptor(ref IntPtr desc,
            FFTConfigValue precision, FFTConfigValue domain, int dimension, long[] lengths);

        /// <summary>
        /// DftiSetValue wrapper
        /// </summary>
        /// <param name="desc"> DFTI descriptor </param>
        /// <param name="config_param"> config_param (ConfigParam) </param>
        /// <param name="config_val"> config_val (int)</param>
        /// <returns> error information </returns>
        int DftiSetValue(IntPtr desc,
            FFTConfigParam config_param, FFTConfigValue config_val);

        /// <summary>
        /// DftiSetValue wrapper
        /// </summary>
		/// <param name="desc"> DFTI descriptor </param>
		/// <param name="config_param"> config_param (ConfigParam) </param>
		/// <param name="config_val"> config_val (double)</param>
		/// <returns> error information </returns> 
        int DftiSetValue(IntPtr desc,
            FFTConfigParam config_param, double config_val);

        /// <summary>
		/// DftiCommitDescriptor wrapper
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
		/// <returns> error information </returns>
        int DftiCommitDescriptor(IntPtr desc);

        ///// <summary>
        ///// DftiComputeForward wrapper
        ///// </summary>
        ///// <param name="desc"> DFTI descriptor </param>
        ///// <param name="x"> array data x (in / out) </param>
        ///// <returns> error information </returns>
        //int DftiComputeForward(IntPtr desc,
        //    DenseArrayBase<Complex> x);

  //      /// <summary>
		///// DftiComputeForward wrapper
		///// </summary>
		///// <param name="desc"> DFTI descriptor </param>
  //      /// <param name="x_in"> input array data </param>
  //      /// <param name="x_out"> output array data </param>
  //      /// <returns> error information </returns>
  //      int DftiComputeForward(IntPtr desc,
  //          DenseArrayBase<Complex> x_in, DenseArrayBase<Complex> x_out);

  //      /// <summary>
		///// DftiComputeBackward wrapper
		///// </summary>
		///// <param name="desc"> DFTI descriptor </param>
  //      /// <param name="x"> array data x (in / out) </param>
  //      /// <returns> error information </returns>
  //      int DftiComputeBackward(IntPtr desc,
  //          DenseArrayBase<Complex> x);

  //      /// <summary>
		///// DftiComputeBackward wrapper
		///// </summary>
		///// <param name="desc"> DFTI descriptor </param>
  //      /// <param name="x_in"> input array data </param>
  //      /// <param name="x_out"> output array data </param>
  //      /// <returns> error information </returns>
  //      int DftiComputeBackward(IntPtr desc,
  //          DenseArrayBase<Complex> x_in, DenseArrayBase<Complex> x_out);

        /// <summary>
		/// DftiFreeDescriptor wrapper
		/// </summary>
		/// <param name="desc"> DFTI descriptor </param>
		/// <returns> error information </returns>
        int DftiFreeDescriptor(ref IntPtr desc);

    }

}
