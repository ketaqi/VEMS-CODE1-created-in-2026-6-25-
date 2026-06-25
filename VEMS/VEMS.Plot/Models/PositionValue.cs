using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VEMS.MathCore;

namespace VEMS.Plot
{
    /// <summary>
    /// class for 2D position value, including
    /// name, x, y, x string, y string, ...
    /// </summary>
    public class PositionValue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #region properties

        public string Name { get; set; }

        private double _x { get; set; }
        private double _y { get; set; }
        private string _xString { get; set; }
        private string _yString { get; set; }
        public double X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged(nameof(X));
                XString = Converter.NumberToString(X);
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged(nameof(Y));
                YString = Converter.NumberToString(Y);
            }
        }

        public string XString
        {
            get => _xString;
            set
            {
                _xString = value;
                OnPropertyChanged(nameof(XString));
            }
        }
        public string YString
        {
            get => _yString;
            set
            {
                _yString = value;
                OnPropertyChanged(nameof(YString));
            }
        }

        #endregion

    }

}
