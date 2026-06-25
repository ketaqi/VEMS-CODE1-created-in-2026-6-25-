using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VEMS.MathCore;

namespace VEMS.Plot
{

    /// <summary>
    /// class for range values, including
    /// name, min value, max value ...
    /// </summary>
    [Obsolete]
    public class RangeValue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #region properties

        private string _name { get; set; }
        private double _minValue { get; set; }
        private double _maxValue { get; set; }


        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public double MinValue
        {
            get => _minValue;
            set
            {
                //if (value > MaxValue)
                //    throw new ArgumentOutOfRangeException(nameof(value), value, "Min is larger than the Max");
                _minValue = value;
                OnPropertyChanged(nameof(MinValue));
            }
        }

        public double MaxValue
        {
            get => _maxValue;
            set
            {
                //if (value < MinValue)
                //    throw new ArgumentOutOfRangeException(nameof(value), value, "Max is smaller than the Min");
                _maxValue = value;
                OnPropertyChanged(nameof(MaxValue));
            }
        }

        #endregion

    }

    /// <summary>
    /// class for number value, including
    /// display name, number value
    /// </summary>
    [Obsolete]
    public class NumberTuple : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #endregion
        #region properties

        private string? _displayName { get; set; }
        private double _numValue { get; set; }

        /// <summary>
        /// display name
        /// </summary>
        public string DisplayName
        {
            get => _displayName != null ? _displayName : "";
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        /// <summary>
        /// number value 
        /// </summary>
        public double NumValue
        {
            get => _numValue;
            set
            {
                _numValue = value;
                OnPropertyChanged(nameof(NumValue));
            }
        }

        #endregion
    }

    /// <summary>
    /// class for one-number value, including
    /// display name, number value
    /// </summary>
    [Obsolete]
    public class OneNumberTuple : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #endregion
        #region properties

        private string? _displayName { get; set; }
        private double _numberValue { get; set; }

        /// <summary>
        /// display name
        /// </summary>
        public string DisplayName
        {
            get => _displayName != null ? _displayName : "";
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        /// <summary>
        /// number value 
        /// </summary>
        public double NumberValue
        {
            get => _numberValue;
            set
            {
                _numberValue = value;
                OnPropertyChanged(nameof(NumberValue));
            }
        }

        #endregion
    }


    /// <summary>
    /// class for text value, including
    /// display name, text value
    /// </summary>
    //public class TextTuple : INotifyPropertyChanged
    //{
    //    #region INotifyPropertyChanged Interface

    //    public event PropertyChangedEventHandler? PropertyChanged;
    //    protected virtual void OnPropertyChanged(string properyName)
    //        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

    //    #endregion
    //    #region properties

    //    private string? _displayName { get; set; }
    //    private string? _txtValue { get; set; }

    //    /// <summary>
    //    /// display name
    //    /// </summary>
    //    public string DisplayName
    //    {
    //        get => _displayName != null ? _displayName : "";
    //        set
    //        {
    //            _displayName = value;
    //            OnPropertyChanged(nameof(DisplayName));
    //        }
    //    }

    //    /// <summary>
    //    /// text value 
    //    /// </summary>
    //    public string TxtValue
    //    {
    //        get => _txtValue != null ? _txtValue : "";
    //        set
    //        {
    //            _txtValue = value;
    //            OnPropertyChanged(nameof(TxtValue));
    //        }
    //    }

    //    #endregion

    //}


    /// <summary>
    /// class for range tuple, including
    /// display name, min value, max value ...
    /// </summary>
    [Obsolete]
    public class RangeTuple : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #endregion
        #region properties

        private string? _displayName { get; set; }
        private double _minValue { get; set; }
        private double _maxValue { get; set; }

        /// <summary>
        /// display name
        /// </summary>
        public string DisplayName
        {
            get => _displayName != null ? _displayName : ""; 
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }
        
        /// <summary>
        /// minimum value
        /// </summary>
        public double MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                OnPropertyChanged(nameof(MinValue));
            }
        }

        /// <summary>
        /// maximum value
        /// </summary>
        public double MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue= value;
                OnPropertyChanged(nameof(MaxValue));
            }
        }

        #endregion
    }

    /// <summary>
    /// class for tick tuple, including
    /// display name, tick density, font size
    /// </summary>
    [Obsolete]
    public class TickTuple : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #endregion
        #region properties

        private string? _displayName { get; set; }
        private double _tickDensity { get; set; }
        private double _fontSize { get; set; }

        /// <summary>
        /// display name
        /// </summary>
        public string DisplayName
        {
            get => _displayName != null ? _displayName : "";
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        /// <summary>
        /// tick density value
        /// </summary>
        public double TickDensity
        {
            get => _tickDensity;
            set
            {
                _tickDensity = value;
                OnPropertyChanged(nameof(TickDensity));
            }
        }

        /// <summary>
        /// font size value
        /// </summary>
        public double FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        #endregion
    }

    /// <summary>
    /// class for text tuple, including
    /// display name, text content, font size
    /// </summary>
    [Obsolete]
    public class TextTuple : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #endregion
        #region properties

        private string? _displayName { get; set; }
        private string? _content { get; set; }
        private double _fontSize { get; set; }

        /// <summary>
        /// display name
        /// </summary>
        public string DisplayName
        {
            get => _displayName != null ? _displayName : "";
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        /// <summary>
        /// text content value
        /// </summary>
        public string Content
        {
            get => _content != null ? _content : "";
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        /// <summary>
        /// font size value
        /// </summary>
        public double FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        #endregion
    }

    [Obsolete]
    public class LocationTuple : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #endregion
        #region properties

        private string? _displayName { get; set; }
        private double _x { get; set; }
        private double _y { get; set; }

        /// <summary>
        /// display name
        /// </summary>
        public string DisplayName
        {
            get => _displayName != null ? _displayName : "";
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        /// <summary>
        /// horizontal location
        /// </summary>
        public double X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }

        /// <summary>
        /// vertical location
        /// </summary>
        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }

        #endregion

    }

    /// <summary>
    /// number quantity tuple
    /// including: name, double value, unit
    /// </summary>
    public class NumberQuantity : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #endregion
        #region properties

        private string? _name { get; set; }
        private double _value { get; set; }
        private double _unit { get; set; }
        /// <summary>
        /// display name
        /// </summary>
        public string Name
        {
            get => _name ?? ""; // _name != null ? _name : "";
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// number value 
        /// </summary>
        public double Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(DisplayValue));
            }
        }
        /// <summary>
        /// display of value
        /// </summary>
        public string DisplayValue
        {
            get =>Converter.NumberToString(_value);
            //set
            //{
            //    _value = value;
            //    OnPropertyChanged(nameof(DisplayValue));
            //}
        }
        /// <summary>
        /// (Not Implemented) unit
        /// </summary>
        public double Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }
        /// <summary>
        /// Comparative value size
        /// </summary>
        /// <param name="otherNumberQuantity"> other class of NumberQuantity </param>
        /// <returns></returns>
        public bool LessThan(NumberQuantity otherNumberQuantity)
        {
            bool flg = true;
            if (this.Value >= otherNumberQuantity.Value)
            {
                MessageBox.Show($"The {this.Name} must be greater than the {otherNumberQuantity.Name}");
                flg = false;
            }
            return flg;
        }
        #endregion
    }

    /// <summary>
    /// text quantity tuple
    /// including: name, string value, unit
    /// </summary>
    public class TextQuantity : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));

        #endregion
        #region properties

        private string? _name { get; set; }
        private string? _value { get; set; }
        private double _unit { get; set; }
        /// <summary>
        /// display name
        /// </summary>
        public string Name
        {
            get => _name ?? ""; // _name != null ? _name : "";
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// number value 
        /// </summary>
        public string Value
        {
            get => _value ?? "";
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        /// <summary>
        /// (Not Implemented) unit => font size
        /// </summary>
        public double Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

        #endregion
    }

}
