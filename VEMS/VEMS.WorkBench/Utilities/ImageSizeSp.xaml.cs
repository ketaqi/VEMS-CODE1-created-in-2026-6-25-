using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VEMS.WorkBench
{
    /// <summary>
    /// ImageSizeSp.xaml 的交互逻辑
    /// </summary>
    public partial class ImageSizeSp : Window
    {
        /*public ImageSizeSp()
        {
            InitializeComponent();
        }*/

        public string SavePath { get; set; }
        private VFrame frameObject { get; set; }
        public ImageSizeSp(String savePath,VFrame frame)
        {
            InitializeComponent();
            SavePath = savePath;
            frameObject = frame;
            View_Width.Text = frameObject.GetActualWidth().ToString();
            View_Height.Text = frameObject.GetActualHeight().ToString();
        }

        #region ===== save Figure =====

        private void ViewWidth_Auto(object sender, RoutedEventArgs e)
        => View_Width.Text = frameObject.GetActualWidth().ToString();
        
        private void ViewHeight_Auto(object sender, RoutedEventArgs e)
        => View_Height.Text = frameObject.GetActualHeight().ToString();

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            //Only "Numbers" can be accepted in both textbox
            //返回false可以输入，返回true不可以输入
            e.Handled = new Regex("[^0-9]").IsMatch(e.Text);
        }
        
        /// <summary>
        /// save the figure of the frame（Support .jpg/.JPEG/.PNG/.TIF/.TIFF/.BMP format）
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {            
            frameObject.SaveFrame(SavePath, int.Parse(View_Width.Text), int.Parse(View_Height.Text), bool.Parse(View_lowQuality.Text), int.Parse(View_Scale.Text));
            this.Close();
        }
        #endregion
    }
}
