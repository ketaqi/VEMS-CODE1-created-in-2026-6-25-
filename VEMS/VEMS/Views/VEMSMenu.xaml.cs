using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

//using VEMS.EditorALT;
//using VEMS.Imager;

namespace VEMS
{
    /// <summary>
    /// VEMSMenu.xaml 的交互逻辑
    /// </summary>
    public partial class VEMSMenu : Window
    {
        private VEMSMenuModel vMenuModel;

        public VEMSMenu()
        {
            InitializeComponent();

            vMenuModel = new VEMSMenuModel();
            DataContext = vMenuModel;

        }

        /*private void Button_VEditorWPF(object sender, RoutedEventArgs e)
        {
            Window w = new VEditorWPF();
            w.Show();
        }

        private void Button_VEditorALT(object sender, RoutedEventArgs e)
        {
            //Window w = new VEditorALT();
            //w.Show();
        }

        private void Button_VImager(object sender, RoutedEventArgs e)
        {
            Window w = new VImager();
            w.Show();
        }*/
    }
}
