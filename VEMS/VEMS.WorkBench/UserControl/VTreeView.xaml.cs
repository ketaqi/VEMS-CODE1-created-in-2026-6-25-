using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VEMS.WorkBench
{
    /// <summary>
    /// VTreeView.xaml 的交互逻辑
    /// </summary>
    public partial class VTreeView : UserControl
    {

        //public IEnumerable<ItemsControl> ItemsSource
        //{
        //    set
        //    {
        //        ItemsSource = value;
        //        VTrv.ItemsSource = value;
        //    }
        //}


        public VTreeView()
        {
            InitializeComponent();

            //

            TreeViewItem MainNode = new() { Header = "ab-Main Node" };
            VTrv.ItemsSource = new ObservableCollection<TreeViewItem> { MainNode };

            TreeViewItem SubNode1 = new() { Header = "ab-Sub Node #1" };
            TreeViewItem SubNode2 = new() { Header = "ab-Sub Node #2" };
            TreeViewItem SubNode3 = new() { Header = "ab-Sub Node #3" };
            MainNode.Items.Add(SubNode1);
            MainNode.Items.Add(SubNode2);
            MainNode.Items.Add(SubNode3);

            SubNode2.Items.Add(new TreeViewItem() { Header = "cd-Sub Node #2.1" });
            SubNode2.Items.Add(new TreeViewItem() { Header = "cd-Sub Node #2.2" });

            //
        }
    }
}
