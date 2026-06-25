using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using VEMS.MathCore;

namespace VEMS.WorkBench
{

    /// <summary>
    /// linear algebra benchmark model
    /// </summary>
    public partial class Benchmark : ObservableObject
    {
        #region properties

        private int numberOfTestRuns = 3;

        /// <summary>
        /// number of test runs for benchmark
        /// </summary>
        public int NumberOfTestRuns 
        {   
            get => numberOfTestRuns;
            set 
            {
                numberOfTestRuns = value;
                OnPropertyChanged(nameof(NumberOfTestRuns));
            } 
        }


        private bool isMatrixProductComplex = true;

        /// <summary>
        /// whether to use complex numbers for matrix product
        /// </summary>
        public bool IsMatrixProductComplex
        {
            get => isMatrixProductComplex;
            set
            {
                isMatrixProductComplex = value;
                OnPropertyChanged(nameof(IsMatrixProductComplex));
            }
        }


        private bool isLinearSolveComplex = true;

        /// <summary>
        /// whether to use complex numbers for linear solve
        /// </summary>
        public bool IsLinearSolveComplex
        {
            get => isLinearSolveComplex;
            set
            {
                isLinearSolveComplex = value;
                OnPropertyChanged(nameof(IsLinearSolveComplex));
            }
        }


        private bool isMatrixSVDComplex = true;

        /// <summary>
        /// whether to use complex numbers for matrix SVD
        /// </summary>
        public bool IsMatrixSVDComplex
        {
            get => isMatrixSVDComplex;
            set
            {
                isMatrixSVDComplex = value;
                OnPropertyChanged(nameof(IsMatrixSVDComplex));
            }
        }


        private bool isMatrixEigenComplex = true;

        /// <summary>
        /// whether to use complex numbers for matrix eigen
        /// </summary>
        public bool IsMatrixEigenComplex
        {
            get => isMatrixEigenComplex;
            set
            {
                isMatrixEigenComplex = value;
                OnPropertyChanged(nameof(IsMatrixEigenComplex));
            }
        }


        private bool isMatrixFFTComplex = true;

        /// <summary>
        /// whether to use complex numbers for matrix FFT
        /// </summary>
        public bool IsMatrixFFTComplex
        {
            get => isMatrixFFTComplex;
            set
            {
                isMatrixFFTComplex = value;
                OnPropertyChanged(nameof(IsMatrixFFTComplex));
            }
        }

        #endregion
        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        public Benchmark() { }

        #endregion
        #region methods

        /// <summary>
        /// load default settings
        /// </summary>
        [RelayCommand]
        public void LoadDefaultSettings()
        {
            NumberOfTestRuns = 3;
            IsMatrixProductComplex = true;
            IsLinearSolveComplex = true;
            IsMatrixSVDComplex = true;
            IsMatrixEigenComplex = true;
            IsMatrixFFTComplex = true;
        }

        /// <summary>
        /// checks the number of test runs for benchmark
        /// </summary>
        [RelayCommand]
        public void CheckNumberOfTestRuns()
        {
            // exception handling
            if (NumberOfTestRuns < 1)
            { NumberOfTestRuns = 1; }
            Printer.Write($"Number of test runs for benchmark: {NumberOfTestRuns}");
        }

        /// <summary>
        /// performs matrix product benchmark
        /// </summary>
        [RelayCommand]
        public async Task MatrixProduct()
        {
            MatrixBench.MatrixProduct bm = new(runs: NumberOfTestRuns,
                isComplex: IsMatrixProductComplex);
            await Task.Run(() => bm.Run());
        }

        /// <summary>
        /// performs linear solve benchmark
        /// </summary>
        [RelayCommand]
        public async Task LinearSolve()
        {
            MatrixBench.LinearSolve bm = new(runs: NumberOfTestRuns,
                isComplex: IsLinearSolveComplex);
            await Task.Run(() => bm.Run());
        }

        /// <summary>
        /// performs matrix SVD benchmark
        /// </summary>
        [RelayCommand]
        public async Task MatrixSVD()
        {
            MatrixBench.SVD bm = new(runs: NumberOfTestRuns,
                isComplex: IsMatrixSVDComplex);
            await Task.Run(() => bm.Run());
        }

        /// <summary>
        /// performs matrix eigen benchmark
        /// </summary>
        [RelayCommand]
        public async Task MatrixEigen()
        {
            MatrixBench.Eigen bm = new(runs: NumberOfTestRuns,
                isComplex: IsMatrixEigenComplex);
            await Task.Run(() => bm.Run());
        }

        /// <summary>
        /// performs matrix FFT benchmark
        /// </summary>
        [RelayCommand]
        public async Task MatrixFFT()
        {
            MatrixBench.FFT bm = new(runs: NumberOfTestRuns,
                isComplex: IsMatrixFFTComplex);
            await Task.Run(() => bm.Run());
        }

        #endregion
    }

}
