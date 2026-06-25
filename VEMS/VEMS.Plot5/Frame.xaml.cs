using System.Windows.Controls;

namespace VEMS.Plot5
{
    /// <summary>
    /// Frame5
    /// </summary>
    public partial class Frame : UserControl
    {

        /// <summary>
        /// the view model
        /// </summary>
        public FrameViewModel ViewModel;

        /// <summary>
        /// constructor
        /// </summary>
        public Frame()
        {
            InitializeComponent();
            ViewModel = new(Dock);
            DataContext = ViewModel;
        }
    
    
    }
}
