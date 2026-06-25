using CommunityToolkit.Mvvm.ComponentModel;

namespace VEMS.WorkBench
{
    public partial class SearchModel : ObservableObject
    {

        [ObservableProperty]
        private string? searchName = "";

        [ObservableProperty]
        private bool isCapital = true;
    }
}
