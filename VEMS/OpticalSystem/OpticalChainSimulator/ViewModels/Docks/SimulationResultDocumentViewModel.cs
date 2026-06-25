using Dock.Model.Mvvm.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OpticalChainSimulator.ViewModels.Docks;

public partial class SimulationResultDocumentViewModel : Document
{
    [ObservableProperty]
    private string resultText = string.Empty;

    [ObservableProperty]
    private string asciiPath = string.Empty;

    public SimulationResultDocumentViewModel(string title, string result, string ascii)
    {
        Id = $"SimResult_{Guid.NewGuid()}";
        Title = title;
        ResultText = result;
        AsciiPath = ascii;
    }
}