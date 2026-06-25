namespace VEMS.AMathCore
{
    /// <summary>
    /// collection of FFT-related optinos
    /// </summary>
    public struct FFTOptions
    {
        /// <summary>
        /// direction of the transform
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// forward transform
            /// </summary>
            Forward,

            /// <summary>
            /// backward transform
            /// </summary>
            Backward
        }

        /// <summary>
        /// zero-convention
        /// </summary>
        public enum Convention
        {
            /// <summary>
            /// zero-centered: central value at index zero
            /// </summary>
            ZeroCentered,

            /// <summary>
            /// zero-based: first value at index zero
            /// </summary>
            ZeroBased
        }

        /// <summary>
        /// for zero-centered convention, specifies the preferred conversion method
        /// <para> option #1: conversion by linear phase </para>
        /// <para> option #2: conversion by data shift </para>
        /// </summary>
        public enum Conversion
        {
            /// <summary>
            /// linear phase: this is a generally valid method
            /// </summary>
            LinearPhase,

            /// <summary>
            /// data shift: this only works for even number of samples
            /// </summary>
            DataShift
        }

        /// <summary>
        /// for BlockShift, specifies the copy mode
        /// <para> option #1: copy by block(s) via BLAS </para>
        /// <para> option #2: copy by element via for loop </para>
        /// </summary>
        public enum CopyMode
        {
            /// <summary>
            /// copy by block via BLAS
            /// </summary>
            Block,

            /// <summary>
            /// copy by element via for-loop
            /// </summary>
            Element
        }

        /// <summary>
        /// for-loop mode options
        /// <para> for BlockShift case, applies when element-wise copy is used </para>
        /// <para> for LinearPhase case, applies for the linear phase calculation </para>
        /// </summary>
        public enum LoopMode
        {
            /// <summary>
            /// sequential for-loop
            /// </summary>
            Sequential,

            /// <summary>
            /// parallel for-loop
            /// </summary>
            Parallel
        }

    }

}
