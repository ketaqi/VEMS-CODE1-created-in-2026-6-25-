using System;
using System.Text;

using System.IO;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;

namespace VEMS.WorkBench
{
    /// <summary>
    /// VEMS console class
    /// </summary>
    public class VEMSConsole : TextWriter, INotifyPropertyChanged
    {
        #region PropertyChanged ...
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion


        private string _content { get; set; }
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        private FontFamily _fontFamily { get; set; }
        public FontFamily FontFamily
        {
            get => _fontFamily;
            set
            {
                _fontFamily = value;
                OnPropertyChanged(nameof(FontFamily));
                TxtBox.FontFamily = FontFamily;
            }
        }

        private double _fontSize { get; set; }
        public double FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                OnPropertyChanged(nameof(FontSize));
                TxtBox.FontSize = FontSize;
            }
        }

        public TextBox TxtBox { get; set; }

        /// <summary>
        /// override the Write method
        /// </summary>
        /// <param name="value"> input char </param>
        public override void Write(char value)
        {
            //Content += value.ToString();
            //TxtBox.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    TxtBox.AppendText(value.ToString());
            //    TxtBox.ScrollToEnd();
            //}));
            WriteWithCarriageReturn(TxtBox, value.ToString());
        }

        /// <summary>
        /// override the WriteLine method
        /// </summary>
        /// <param name="value"> input string </param>
        public override void WriteLine(string value)
        {
            //Content += value;
            TxtBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                TxtBox.AppendText(value + "\n");
                TxtBox.ScrollToEnd();
            }));
        }

        private static void WriteWithCarriageReturn(TextBox textBox, 
            string value)
        {
            if (textBox == null || value == null) return;

            textBox.Dispatcher.Invoke(() =>
            {
                // Handle carriage return
                int crIndex = value.LastIndexOf('\r');
                if (crIndex >= 0)
                {
                    // Get the text up to the current caret position
                    int lineIndex = textBox.GetLineIndexFromCharacterIndex(textBox.CaretIndex);
                    int lineStart = textBox.GetCharacterIndexFromLineIndex(lineIndex);

                    // Remove current line from lineStart to end of line
                    int lineLength = textBox.GetLineLength(lineIndex);
                    textBox.Select(lineStart, lineLength);
                    // Insert the text after the last \r
                    string afterCR = value.Substring(crIndex + 1).Replace("\n", "");
                    textBox.SelectedText = afterCR;
                    textBox.CaretIndex = lineStart + afterCR.Length;
                }
                else
                {
                    // Normal append
                    textBox.AppendText(value);
                    textBox.CaretIndex = textBox.Text.Length;
                }
                textBox.ScrollToEnd();
            });
        }

        public override Encoding Encoding => Encoding.Default;

    }


    [Obsolete("Use VEMSConsole instead")]
    public class VConsole : TextWriter
    {
        public TextBox txtBox { get; set; }

        public VConsole() { }
        public VConsole(TextBox output)
        {
            txtBox = output;
        }

        public override void Write(char value)
        {
            base.Write(value);

            //txtBox.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    txtBox.AppendText(value.ToString());
            //    txtBox.ScrollToEnd();
            //}));
            txtBox.AppendText(value.ToString());
            txtBox.ScrollToEnd();
        }

        public override void WriteLine(char value)
        {
            base.WriteLine(value);

            txtBox.AppendText(value.ToString());
            txtBox.ScrollToEnd();
        }

        public override Encoding Encoding
        {
            get => Encoding.Default;
        }

    }



}
