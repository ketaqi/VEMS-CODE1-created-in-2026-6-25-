using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEMS.MLayers
{
    public class OpticalInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        private double wavelength { get; set; }
        private double minIncidenceAngle { get; set; }
        private double maxIncidenceAngle { get; set; }

        /// <summary>
        /// wavelength in vacuum
        /// </summary>
        public double Wavelength 
        {
            get => wavelength;
            set
            {
                wavelength = value;
                OnPropertyChanged(nameof(Wavelength));
            }
        }

        /// <summary>
        /// minimum angle of incidence
        /// under investigation
        /// </summary>
        public double MinIncidenceAngle
        {
            get => minIncidenceAngle;
            set
            {
                minIncidenceAngle = value;
                OnPropertyChanged(nameof(MinIncidenceAngle));
            }
        }

        /// <summary>
        /// maximum angle of incidence
        /// under investigation
        /// </summary>
        public double MaxIncidenceAngle
        {
            get => maxIncidenceAngle;
            set
            {
                maxIncidenceAngle = value;
                OnPropertyChanged(nameof(MaxIncidenceAngle));
            }
        }

    }
}
