using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace VEMS.WorkBench
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private VTreeItem tree = new();

        /// <summary>
        /// the root node of the tree 
        /// including folders and files
        /// </summary>
        public VTreeItem Tree
        {
            get => tree;
            set => tree = value;
        }


        private ObservableCollection<VFileItem> files = new();

        /// <summary>
        /// collection of VFileItems
        /// </summary>
        public ObservableCollection<VFileItem> Files
        {
            get => files;
            set => files = value;
        }


        private ObservableCollection<VFrame> frames = new();

        /// <summary>
        /// collection of VFrames
        /// </summary>
        public ObservableCollection<VFrame> Frames
        {
            get => frames;
            set => frames = value;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // checks expiration first
            CheckExpirationDate();
            // starts app
            VEditor mainWindow = new ();
            mainWindow.Show();
        }

        /// <summary>
        /// defines expiration date here
        /// </summary>
        internal DateTime ExpireDate = new(year: 2026, month: 12, day: 31);

        /// <summary>
        /// checks if the license expires 
        /// </summary>
        private void CheckExpirationDate()
        {
            // compares dates
            if (DateTime.Now.Date >= ExpireDate)
            {
                MessageBox.Show(messageBoxText: $"License has been expired on {ExpireDate}", 
                    caption: "License Information", 
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Warning);
                // shuts down the app
                Shutdown();
                return;
            }
            else if((ExpireDate - DateTime.Now.Date).Days <= 5)
            {
                MessageBox.Show(messageBoxText: $"License will expire on {ExpireDate}", 
                    caption: "License Information", 
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Warning);
            }

        }


    }
}
