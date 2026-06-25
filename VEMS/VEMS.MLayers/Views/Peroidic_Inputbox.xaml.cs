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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

namespace VEMS.MLayers
{
    /// <summary>
    /// Peroidic_Inputbox.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class Peroidic_Inputbox : Window
    {
        public ObservableCollection<LayerInfo> InputPeroidLayers { get; set; }
        public int LayerNumber = 0;// How many layers are there in a layer
        public int PeroidNumber = 0;// How many peroids are there
        public DataGrid inputDataGrid = new();
        Button passInputData = new();
        public Peroidic_Inputbox()
        {
            InitializeComponent();
            InputPeroidLayers = new()
            {
            };                            
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            //Only "Numbers" can be accepted in both textbox
            //返回false可以输入，返回true不可以输入
            e.Handled = new Regex("[^0-9]").IsMatch(e.Text);
            
            
        }

        private void TextBox_PreviewKeyDown1(object sender, KeyEventArgs e)
        {//When "Enter" is pressed in 1st textbox, jump to the 2nd textbox
            if (e.Key == Key.Enter && NumberInPeroid.Text != "")
            {              
                if (int.Parse(NumberInPeroid.Text) <= 5)
                {
                    NumberOfPeroids.Focus();
                }
                else
                {
                    MessageBox.Show("The layer number of each peroid should be less than 5", "Error");
                    NumberInPeroid.Text = "";
                    StackOfData.Children.Clear();
                }
            }
            if (e.Key == Key.Space)
            {               
                e.Handled = true;
            }
        }


        private void TextBox_PreviewKeyDown2(object sender, KeyEventArgs e)
        {// You can enter Peroidic layer number with submit button or key "Enter"
            if (e.Key == Key.Enter && NumberOfPeroids.Text != "" && NumberInPeroid.Text!= "")
            {
                if (int.Parse(NumberInPeroid.Text) <= 5)
                {
                    DeleteInputLayer();
                    LayerNumber = int.Parse(NumberInPeroid.Text);
                    PeroidNumber = int.Parse(NumberOfPeroids.Text);
                    StackOfData.Children.Clear();
                    GenerateLayerInputBox(LayerNumber);
                }
                else
                {
                    MessageBox.Show("The layer number of each peroid should be less than 5","Error");
                    NumberInPeroid.Text = "";
                    StackOfData.Children.Clear();
                }
            }
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void Button_SetPeroidNumber(object sender, RoutedEventArgs e)
        {// You can enter Peroidic layer number with submit button or key "Enter"
            if (NumberInPeroid.Text != "" && NumberOfPeroids.Text !="")
            {
                if (int.Parse(NumberInPeroid.Text) <= 5 && NumberOfPeroids.Text != "")
                {
                    DeleteInputLayer();
                    LayerNumber = int.Parse(NumberInPeroid.Text);
                    PeroidNumber = int.Parse(NumberOfPeroids.Text);
                    StackOfData.Children.Clear();
                    GenerateLayerInputBox(LayerNumber);
                }
                else
                {
                    MessageBox.Show("The layer number of each peroid should be less than 5", "Error");
                    NumberInPeroid.Text = "";
                    StackOfData.Children.Clear();
                }
            }
            
        }
        
    
        private void GenerateLayerInputBox(int N)
        {
            
            inputDataGrid.CanUserAddRows = false;
            inputDataGrid.Margin = new Thickness(30,10,30,5);
            inputDataGrid.CanUserSortColumns = false;
            //Difinite a button to pass layerdata to MLayersAnalyzer
            passInputData.Margin = new Thickness(15);
            passInputData.Width = 100;
            passInputData.Height = 50;
            passInputData.Content = "Create";
            passInputData.Click += Button_PassToMLayerAnalyzer;
            for (int i = 0; i <= N-1; i++)
            {
                InputPeroidLayers.Add(new LayerInfo()
                {
                    Index = InputPeroidLayers.Count+1,
                    Thickness = 0.0,
                    Material = "Null",
                    NRe = 1.0,
                    NIm = 0.0
                });
            }
            inputDataGrid.ItemsSource = InputPeroidLayers;
            this.StackOfData.Children.Add(inputDataGrid);
            this.StackOfData.Children.Add(passInputData);
        }


        private void DeleteInputLayer()
        {//delete input layer data when it is not right
            for (int i = InputPeroidLayers.Count-1; i >= 0; i--)
            {
                InputPeroidLayers.RemoveAt(i);
            }
        }

        private void Button_PassToMLayerAnalyzer(object sneder, RoutedEventArgs e)
        {
            this.Close();
        }

        
    }

}
