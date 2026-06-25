using System.Composition;
using RoslynPad.Utilities;

namespace RoslynPad.UI;

[Export]
public sealed class NuGetDocumentViewModel : NotificationObject
{
    private readonly NuGetViewModel _nuGetViewModel;
    private readonly ITelemetryProvider _telemetryProvider;

    private string? _searchTerm;
    private bool _isSearching;
    private CancellationTokenSource? _searchCts;
    private bool _isPackagesMenuOpen;
    private bool _prerelease;
    private IReadOnlyList<PackageData> _packages;

    public IReadOnlyList<PackageData> Packages
    {
        get => _packages;
        private set => SetProperty(ref _packages, value);
    }

    [ImportingConstructor]
    public NuGetDocumentViewModel(NuGetViewModel nuGetViewModel, ICommandProvider commands, ITelemetryProvider telemetryProvider)
    {
        _nuGetViewModel = nuGetViewModel;
        _telemetryProvider = telemetryProvider;
        _packages = [];

        InstallPackageCommand = commands.Create<PackageData>(InstallPackage);
    }

    private void InstallPackage(PackageData? package)
    {
        if (package == null)
        {
            return;
        }

        OnPackageInstalled(package);
    }

    public IDelegateCommand<PackageData> InstallPackageCommand { get; }

    private void OnPackageInstalled(PackageData package)
    {
        PackageInstalled?.Invoke(package);
    }

    public event Action<PackageData>? PackageInstalled;

    public bool IsSearching
    {
        get => _isSearching;
        private set => SetProperty(ref _isSearching, value);
    }

    public string? SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                PerformSearch();
            }
        }
    }

    public bool IsPackagesMenuOpen
    {
        get => _isPackagesMenuOpen;
        set => SetProperty(ref _isPackagesMenuOpen, value);
    }

    public bool ExactMatch { get; set; }

    public bool Prerelease
    {
        get => _prerelease;
        set
        {
            if (SetProperty(ref _prerelease, value))
            {
                PerformSearch();
            }
        }
    }

    private void PerformSearch()
    {
        //IsSearching = true;
        if (string.IsNullOrEmpty(SearchTerm))
        {
            return;
        }

        _searchCts?.Cancel();
        var searchCts = new CancellationTokenSource();
        var cancellationToken = searchCts.Token;
        _searchCts = searchCts;

        _ = Task.Run(() => PerformSearch(SearchTerm, cancellationToken), cancellationToken);

        //try
        //{
        //    // 直接调用同步实现（阻塞）
        //    PerformSearch1(SearchTerm, cancellationToken);
        //}
        //catch (OperationCanceledException)
        //{
        //    // 处理取消
        //}
    }

    private async Task PerformSearch(string searchTerm, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            Packages = [];
            IsPackagesMenuOpen = false;
            return;
        }

        IsSearching = true;
        try
        {
            try
            {
                var packages = await Task.Run(() =>
                        _nuGetViewModel.GetPackagesAsync(searchTerm, includePrerelease: Prerelease,
                            exactMatch: ExactMatch, cancellationToken: cancellationToken), cancellationToken)
                    .ConfigureAwait(true);

                cancellationToken.ThrowIfCancellationRequested();

                foreach (var package in packages)
                {
                    package.InstallPackageCommand = InstallPackageCommand;
                }

                Packages = packages;
                IsPackagesMenuOpen = Packages.Count > 0;
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                _telemetryProvider.ReportError(e);
            }
        }
        finally
        {
            IsSearching = false;
        }
    }

    private void PerformSearch1(string searchTerm, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            Packages = Array.Empty<PackageData>();
            IsPackagesMenuOpen = false;
            return;
        }

        IsSearching = true;
        try
        {
            try
            {
                // Execute the async fetch synchronously on the thread-pool to avoid deadlocks with UI sync context.
                // Using Task.Run here ensures the async operation runs on a thread-pool thread and GetAwaiter().GetResult() unwraps exceptions.
                var packages = Task.Run(() =>
                        _nuGetViewModel.GetPackagesAsync(searchTerm, includePrerelease: Prerelease,
                            exactMatch: ExactMatch, cancellationToken: cancellationToken), cancellationToken)
                    .GetAwaiter().GetResult();

                cancellationToken.ThrowIfCancellationRequested();

                // Prevent an earlier search result from overwriting a newer search:
                if (!string.Equals(searchTerm, SearchTerm, StringComparison.Ordinal))
                {
                    return;
                }

                foreach (var package in packages)
                {
                    package.InstallPackageCommand = InstallPackageCommand;
                }

                Packages = packages;
                IsPackagesMenuOpen = Packages.Count > 0;
            }
            catch (OperationCanceledException)
            {
                // Cancellation is expected - do nothing
            }
            catch (Exception e)
            {
                _telemetryProvider.ReportError(e);
            }
        }
        finally
        {
            IsSearching = false;
        }
    }

}
