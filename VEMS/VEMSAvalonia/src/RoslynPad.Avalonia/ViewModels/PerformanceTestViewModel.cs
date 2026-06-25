using System.Diagnostics;
using System.Numerics;
using System.Windows.Input;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using RoslynPad.ViewModels;

namespace RoslynPad.UI;

/// <summary>
/// 性能测试视图模型：提供矩阵运算、FFT、SVD 等数值计算性能测试。
/// </summary>
public class PerformanceTestViewModel : NotificationObject
{
    private readonly ICommandProvider _commands;
    private readonly Action<string, string, string?, int?> _outputResult;

    public PerformanceTestViewModel(ICommandProvider commands, Action<string, string, string?, int?> outputResult)
    {
        _commands = commands;
        _outputResult = outputResult;
    }

    #region 测试开关属性

    private bool _isComplexMatrixMultiplyEnabled;
    public bool IsComplexMatrixMultiplyEnabled
    {
        get => _isComplexMatrixMultiplyEnabled;
        set => SetProperty(ref _isComplexMatrixMultiplyEnabled, value);
    }

    private bool _isComplexLinearSolveEnabled;
    public bool IsComplexLinearSolveEnabled
    {
        get => _isComplexLinearSolveEnabled;
        set => SetProperty(ref _isComplexLinearSolveEnabled, value);
    }

    private bool _isComplexSvdEnabled;
    public bool IsComplexSvdEnabled
    {
        get => _isComplexSvdEnabled;
        set => SetProperty(ref _isComplexSvdEnabled, value);
    }

    private bool _isComplexEigenEnabled;
    public bool IsComplexEigenEnabled
    {
        get => _isComplexEigenEnabled;
        set => SetProperty(ref _isComplexEigenEnabled, value);
    }

    private bool _isComplexFftEnabled;
    public bool IsComplexFftEnabled
    {
        get => _isComplexFftEnabled;
        set => SetProperty(ref _isComplexFftEnabled, value);
    }

    private int _testRunCount = 3;
    public int TestRunCount
    {
        get => _testRunCount;
        set
        {
            if (value < 1) value = 1;
            if (SetProperty(ref _testRunCount, value))
            {
                _outputResult("[TestRunCount]", $"Number of tests modified to {value}", "[Info]", null);
            }
        }
    }

    #endregion

    #region 库信息属性

    public string FftLibraryInfo =>
        MathNet.Numerics.Providers.FourierTransform.FourierTransformControl.Provider.GetType().Assembly.GetName().Name ?? "Unknown";

    public string LapackLibraryInfo =>
        MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.Provider.GetType().Assembly.GetName().Name ?? "Unknown";

    public string BlasLibraryInfo =>
        MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.Provider.GetType().Assembly.GetName().Name ?? "Unknown";

    #endregion

    #region 测试命令

    public ICommand TestMatrixMultiplyCommand => _commands.CreateAsync(ExecuteMatrixMultiplyTest);
    public ICommand TestLinearSolveCommand => _commands.CreateAsync(ExecuteLinearSolveTest);
    public ICommand TestSvdCommand => _commands.CreateAsync(ExecuteSvdTest);
    public ICommand TestEigenCommand => _commands.CreateAsync(ExecuteEigenTest);
    public ICommand TestFftCommand => _commands.CreateAsync(ExecuteFftTest);

    #endregion

    #region 测试执行

    private async Task ExecuteMatrixMultiplyTest()
    {
        await RunTestSuite("Matrix Multiply", TestMatrixMultiply).ConfigureAwait(false);
    }

    private async Task ExecuteLinearSolveTest()
    {
        await RunTestSuite("Linear Solve", TestLinearSystemSolve).ConfigureAwait(false);
    }

    private async Task ExecuteSvdTest()
    {
        await RunTestSuite("SVD", TestSvd).ConfigureAwait(false);
    }

    private async Task ExecuteEigenTest()
    {
        await RunTestSuite("Eigen", TestEigen).ConfigureAwait(false);
    }

    private async Task ExecuteFftTest()
    {
        await RunTestSuite("FFT", TestFft).ConfigureAwait(false);
    }

    private async Task RunTestSuite(string testName, Func<int, bool, int, (double avg, double min, double max)> testFunc)
    {
        _outputResult("[TestRunCount]", $"Number of tests: {TestRunCount}", "[Info]", null);

        int[] sizes = { 101, 201, 501, 1001, 2001 };
        var results = new List<(int size, double avg, double min, double max)>();

        foreach (var size in sizes)
        {
            _outputResult($"[{testName}]", $"Running test for size {size}x{size} ... IsComplex={IsComplexFftEnabled}", "[Info]", null);

            var (avg, min, max) = await Task.Run(() => testFunc(size, IsComplexFftEnabled, TestRunCount)).ConfigureAwait(false);
            results.Add((size, avg, min, max));
        }

        _outputResult($"[{testName}]", "Size\t||\tAVG [ms]\t||\tMIN [ms]\t||\tMAX [ms]", "[Summary]", null);

        foreach (var r in results)
        {
            _outputResult($"[{testName}]", $"[{r.size}x{r.size}]\t||\t{r.avg:F10}\t||\t{r.min:F10}\t||\t{r.max:F10}", "[Result]", null);
        }
    }

    #endregion

    #region 核心测试方法

    private (double avg, double min, double max) TestMatrixMultiply(int size, bool complex, int runs)
    {
        return RunBenchmark(runs, () =>
        {
            if (complex)
            {
                var a = Matrix<Complex>.Build.Random(size, size);
                var b = Matrix<Complex>.Build.Random(size, size);
                _ = a * b;
            }
            else
            {
                var a = Matrix<double>.Build.Random(size, size);
                var b = Matrix<double>.Build.Random(size, size);
                _ = a * b;
            }
        });
    }

    private (double avg, double min, double max) TestLinearSystemSolve(int size, bool complex, int runs)
    {
        return RunBenchmark(runs, () =>
        {
            if (complex)
            {
                var a = Matrix<Complex>.Build.Random(size, size);
                var b = MathNet.Numerics.LinearAlgebra.Vector<Complex>.Build.Random(size);
                _ = a.Solve(b);
            }
            else
            {
                var a = Matrix<double>.Build.Random(size, size);
                var b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Random(size);
                _ = a.Solve(b);
            }
        });
    }

    private (double avg, double min, double max) TestSvd(int size, bool complex, int runs)
    {
        return RunBenchmark(runs, () =>
        {
            if (complex)
            {
                var a = Matrix<Complex>.Build.Random(size, size);
                _ = a.Svd();
            }
            else
            {
                var a = Matrix<double>.Build.Random(size, size);
                _ = a.Svd();
            }
        });
    }

    private (double avg, double min, double max) TestEigen(int size, bool complex, int runs)
    {
        return RunBenchmark(runs, () =>
        {
            if (complex)
            {
                var a = Matrix<Complex>.Build.Random(size, size);
                _ = a.Evd();
            }
            else
            {
                var a = Matrix<double>.Build.Random(size, size);
                _ = a.Evd();
            }
        });
    }

    private (double avg, double min, double max) TestFft(int size, bool complex, int runs)
    {
        var rand = new Random();

        return RunBenchmark(runs, () =>
        {
            if (complex)
            {
                var samples = new Complex[size];
                for (int i = 0; i < size; i++)
                    samples[i] = new Complex(rand.NextDouble(), rand.NextDouble());

                MathNet.Numerics.IntegralTransforms.Fourier.Forward(samples);
            }
            else
            {
                var samples = new Complex32[size];
                for (int i = 0; i < size; i++)
                    samples[i] = new Complex32((float)rand.NextDouble(), 0f);

                MathNet.Numerics.IntegralTransforms.Fourier.Forward(samples);
            }
        });
    }

    private static (double avg, double min, double max) RunBenchmark(int runs, Action action)
    {
        double min = double.MaxValue, max = double.MinValue, sum = 0;

        for (int i = 0; i < runs; i++)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();

            double ms = sw.Elapsed.TotalMilliseconds;
            min = Math.Min(min, ms);
            max = Math.Max(max, ms);
            sum += ms;
        }

        return (sum / runs, min, max);
    }

    #endregion
}
