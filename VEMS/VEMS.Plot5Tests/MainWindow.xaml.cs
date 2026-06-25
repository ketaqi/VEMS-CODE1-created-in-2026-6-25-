using System.Windows;

namespace VEMS.Plot5Tests
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string TitleLeft { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            MainViewModel vm = new(FLeft);
            DataContext = vm;

        }
    }
}