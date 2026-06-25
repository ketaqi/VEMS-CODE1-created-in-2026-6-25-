using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEMS.MLayers
{
    public class LayerInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        private int index { get; set; }
        private double thickness { get; set; }
        private string material { get; set; }
        private double nRe { get; set; }
        private double nIm { get; set; }

        /// <summary>
        /// layer index, counting from 0
        /// 0-index for the embedding medium
        /// last0index for the substrate
        /// </summary>
        public int Index
        {
            get => index;
            set
            {
                index = value;
                OnPropertyChanged(nameof(Index));
            }
        }

        /// <summary>
        /// thickness of the layer
        /// </summary>
        public double Thickness
        {
            get => thickness;
            set
            {
                thickness = value;
                OnPropertyChanged(nameof(Thickness));
            }
        }

        /// <summary>
        /// material of the layer
        /// </summary>
        public string Material
        {
            get => material;
            set
            {
                material = value;
                OnPropertyChanged(nameof(Material));
            }
        }

        /// <summary>
        /// real-part of the refractive index
        /// </summary>
        public double NRe
        {
            get => nRe;
            set
            {
                nRe = value;
                OnPropertyChanged(nameof(NRe));
            }
        }

        /// <summary>
        /// imaginary part of the refractive index
        /// </summary>
        public double NIm
        {
            get => nIm;
            set
            {
                nIm = value;
                OnPropertyChanged(nameof(NIm));
            }
        }

    }
}
