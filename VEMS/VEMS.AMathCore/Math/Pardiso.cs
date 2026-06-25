using System.Numerics;

namespace VEMS.AMathCore
{

    /// <summary>
    /// Managed wrapper for the Intel MKL PARDISO sparse direct solver.
    /// Provides a thin, type-safe interface around PARDISO calls for numeric
    /// matrix types that implement <see cref="INumber{T}"/>.
    /// </summary>
    /// <typeparam name="T">Numeric type of matrix entries. Supported types are project-specific aliases such as <c>Real</c> and <c>Cplx</c>.</typeparam>
    public unsafe class Pardiso<T>
        where T : INumber<T>
    {

        #region properties

        /// <summary>
        /// Maximum number of factors with identical sparsity structure 
        /// that must be kept in memory at the same time.
        /// Default is 1.
        /// </summary>
        public Int MaxFct { get; set; } = 1;

        /// <summary>
        /// Indicates the actual matrix for the solution phase.
        /// Default is 1.
        /// </summary>
        public Int MNum { get; set; } = 1;

        /// <summary>
        /// Defines the matrix type, which influences the pivoting method.
        /// Set automatically by <see cref="Solve"/> based on the type parameter <typeparamref name="T"/>.
        /// Typical values: 11 = unsymmetric real, 13 = unsymmetric complex.
        /// </summary>
        public Int MType { get; set; }

        /// <summary>
        /// Number of right-hand sides that need to be solved for.
        /// Default is 1.
        /// </summary>
        public Int NRhs { get; set; } = 1;

        /// <summary>
        /// Message level information passed to PARDISO. Zero suppresses output.
        /// </summary>
        public Int MsgLvl { get; set; } = 0;

        /// <summary>
        /// The error indicator. This property mirrors the error code returned by PARDISO calls.
        /// </summary>
        public Int Error { get; set; } = 0;


        /// <summary>
        /// Internal handle array passed to PARDISO to maintain solver state between phases.
        /// Allocated with a size of 64 entries.
        /// </summary>
        internal DenseArray<Int> Handle { get; set; }

        /// <summary>
        /// Integer parameter array controlling PARDISO behaviour.
        /// The array is pre-initialized with recommended defaults in the constructor.
        /// </summary>
        internal DenseArray<Int> IParam { get; set; }

        /// <summary>
        /// Raw pointer to the <see cref="Handle"/> data used to call the native PARDISO API.
        /// </summary>
        private void* pt { get; set; }


        #endregion
        #region constructors

        /// <summary>
        /// Creates a new <see cref="Pardiso{T}"/> instance and initializes internal
        /// PARDISO handles and parameter array with recommended defaults.
        /// </summary>
        /// <param name="indexing">
        /// Specifies whether the sparse matrix exported to PARDISO uses zero-based or one-based indexing.
        /// When <see cref="SPARSE_IndexBase.ZeroBase"/> is provided, PARDISO is configured to expect
        /// zero-based CSR indexing; otherwise one-based indexing is selected.
        /// </param>
        /// <remarks>
        /// The constructor allocates internal arrays used by the native solver and populates
        /// <see cref="IParam"/> with recommended default options. The internal handle array
        /// (<see cref="Handle"/>) is zero-initialized and <see cref="pt"/> is set to point to its data.
        /// </remarks>
        public Pardiso(SPARSE_IndexBase indexing = SPARSE_IndexBase.ZeroBase)
        {
            // initialize properties
            Handle = new DenseArray<Int>(count: 64, initMode: ArrayInitMode.Calloc);
            pt = Handle.DataPtr.ToPointer();
            //for (int i = 0; i < Handle.Count; i++) { ((long*)pt)[i] = 0; }
            IParam = new DenseArray<Int>(count: 64, initMode: ArrayInitMode.Calloc)
            {
                [0] = 1, /* No solver default */
                [1] = 2, /* Fill-in reordering from METIS */
                [3] = 0, /* No iterative-direct algorithm */
                [4] = 0, /* No user fill-in reducing permutation */
                [5] = 0, /* Write solution into x */
                [7] = 2, /* Max numbers of iterative refinement steps */
                [9] = 13, /* Perturb the pivot elements with 1E-13 */
                [10] = 1, /* Use nonsymmetric permutation and scaling MPS */
                [11] = 0, /* Conjugate transposed/transpose solve */
                [12] = 1, /* Maximum weighted matching algorithm is switched-on (default for non-symmetric) */
                [17] = -1, /* Output: Number of nonzeros in the factor LU */
                [18] = -1, /* Output: Mflops for LU factorization */
                [19] = 0, /* Output: Numbers of CG Iterations */
                // ...
                [34] = (indexing == SPARSE_IndexBase.ZeroBase) ? 1 : 0, // zero-based indexing
            };
        }

        #endregion
        #region methods

        /// <summary>
        /// Solves the linear system A * x = b using the PARDISO direct solver.
        /// </summary>
        /// <param name="a">
        /// Sparse matrix in project-specific representation (<see cref="SPMatx{T}"/>).
        /// The matrix is exported to CSR format before calling the native solver.
        /// </param>
        /// <param name="b">
        /// Right-hand side vector(s). The pointer from <paramref name="b"/> is passed to PARDISO
        /// as the right-hand side input for phase 33.
        /// </param>
        /// <param name="x">
        /// Solution vector(s). On successful return this contains the computed solution(s).
        /// The caller is responsible for allocating <paramref name="x"/> with the correct size
        /// and number of right-hand sides (<see cref="NRhs"/>).
        /// </param>
        /// <remarks>
        /// The method automatically selects <see cref="MType"/> based on the generic type parameter <typeparamref name="T"/>.
        /// It performs the following PARDISO phases:
        /// - Phase 11: reorder and symbolic factorization.
        /// - Phase 22: numerical factorization.
        /// - Phase 33: back substitution and iterative refinement (solve).
        /// The sparse matrix is expected to be square; a non-square matrix triggers an <see cref="ArgumentException"/>.
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// Thrown when <typeparamref name="T"/> is not a supported numeric type (neither <c>Real</c> nor <c>Cplx</c>).
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the provided matrix <paramref name="a"/> is not square.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when any of the PARDISO phases (11, 22 or 33) returns a non-zero error code.
        /// The error code returned by the native library is included in the exception message.
        /// </exception>
        public void Solve(SPMatx<T> a, Vect<T> b,
            ref Vect<T> x)
        {
            // MType
            if (typeof(T) == typeof(Real)) { MType = 11; /* unsymmetric real */ }
            else if (typeof(T) == typeof(Cplx)) { MType = 13; /* unsymmetric complex */ }
            else throw new NotSupportedException($"Type {typeof(T)} not supported.");

            // export the sparse matrix (CSR format)
            a.ExportCSR(out SPARSE_IndexBase indexing,
                out Int rows, out Int cols, out Int nnz,
                out Int* rowPtr, out Int* colIdx, out T* nzv);
            if (rows != cols) { throw new ArgumentException(); }

            // local variables
            Int maxfct = MaxFct;
            Int mnum = MNum;
            long n = rows;
            Int mtype = MType;
            Int idum = 0;
            Int nrhs = NRhs;
            Int msglvl = MsgLvl;
            Int error = Error;

            // Phase 11: reordering and symbolic factorization
            Int phase = 11;
            IntelMKLNative.PARDISO_64(pt, &maxfct, &mnum, &mtype, &phase,
                &n, nzv, rowPtr, colIdx, &idum, &nrhs, IParam.TPtr, &msglvl, null, null, &error);
            if (error != 0) throw new InvalidOperationException($"PARDISO phase 11 failed: {error}");

            // Phase 22: numerical factorization
            phase = 22;
            IntelMKLNative.PARDISO_64(pt, &maxfct, &mnum, &mtype, &phase,
                &n, nzv, rowPtr, colIdx, &idum, &nrhs, IParam.TPtr, &msglvl, null, null, &error);
            if (error != 0) throw new InvalidOperationException($"PARDISO phase 22 failed: {error}");

            // Phase 33: back substitution & iterative refinement (solve)
            phase = 33;
            IntelMKLNative.PARDISO_64(pt, &maxfct, &mnum, &mtype, &phase,
                &n, nzv, rowPtr, colIdx, &idum, &nrhs, IParam.TPtr, &msglvl,
                b.VPtr, x.VPtr, &error);
            if (error != 0) throw new InvalidOperationException($"PARDISO phase 33 failed: {error}");
        }


        /// <summary>
        /// Releases internal memory allocated by PARDISO for the current solver instance.
        /// </summary>
        /// <remarks>
        /// This method invokes PARDISO with phase = -1 which instructs the native library
        /// to free internal data structures associated with the solver handle.
        /// After calling <see cref="Release"/>, the instance should not be used for further solves
        /// unless it is re-initialized.
        /// </remarks>
        public void Release()
        {
            // Phase -1: release internal memory
            Int maxfct = MaxFct;
            Int mnum = MNum;
            Int mtype = MType;
            Int phase = -1;
            long n = 0;
            Int idum = 0;
            Int nrhs = 0;
            Int msglvl = MsgLvl;
            Int error = Error;
            IntelMKLNative.PARDISO_64(pt, &maxfct, &mnum, &mtype, &phase,
                &n, null, null, null, &idum, &nrhs, IParam.TPtr, &msglvl, 
                null, null, &error);
        }

        #endregion

    }

}


