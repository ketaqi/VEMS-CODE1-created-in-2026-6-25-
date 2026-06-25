using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

using VEMS.CodeEditor;
//using VEMS.Imager;
using VEMS.MLayers;

namespace VEMS
{
    public class VEMSMenuModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        public VEMSMenuModel()
        {
            // initiates

            // relay commands
            RelayCommands();

            // other settins ...
        }

        #region commands
        public ICommand NewVEditorWPFCommand { get; set; }
        public ICommand NewVEditorALTCommand { get; set; }
        public ICommand NewImagerCommand{ get; set; }
        public ICommand NewRayTracerCommand { get; set; }
        public ICommand NewEMFieldTracerCommand { get; set; }

        public ICommand NewCoatingAnalyzerCommand { get; set; }
        public ICommand NewFiberAnalyzerCommand { get; set; }
        public ICommand NewGratingAnalyzerCommand { get; set; }

        private void RelayCommands()
        {
            NewVEditorWPFCommand = new RelayCommand(NewVEditorWPF);
            NewVEditorALTCommand = new RelayCommand(NewVEditorALT);
            NewImagerCommand = new RelayCommand(NewImager);
            NewCoatingAnalyzerCommand = new RelayCommand(NewCoatingAnalyzer);
        }

        #endregion
        #region methods

        public void NewVEditorWPF()
            => new VEditor().Show();
        public void NewVEditorALT()
            => MessageBox.Show("Please contact us for more information");
        public void NewImager()
            => MessageBox.Show("..."); // new VImager().Show();
        public void NewCoatingAnalyzer()
            => new MLayersAnalyzer().Show();// MessageBox.Show("Not implemented ...");


        #endregion

    }
}
