using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEMS.Plot
{
    public class FrameLiteViewModel :INotifyPropertyChanged
    {
        #region PropertyChanged ...
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region properties

        private WpfPlot plotFrame { get; set; }

        private string? vTitle { get; set; }
        public string? VTitle
        {
            get => vTitle;
            set
            {
                vTitle = value;
                FrameCommons.SetTitle(plotFrame, value?? "default title");
                OnPropertyChanged(nameof(VTitle));
            }
        }

        #endregion
        #region constructor

        public FrameLiteViewModel(WpfPlot plotFrame) 
        {
            this.plotFrame = plotFrame;
            VTitle = "default title";
        }

        #endregion

    }
}
