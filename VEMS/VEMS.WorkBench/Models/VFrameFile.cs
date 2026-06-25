using System.ComponentModel;

namespace VEMS.WorkBench
{
    /// <summary>
    /// VFrame file class
    /// </summary>
    public class VFrameFile : INotifyPropertyChanged
    {
        #region property changed ...
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string properyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));
        #endregion

        #region properties

        private int index { get; set; }
        private string docName { get; set; }
        private string usrComment { get; set; }

        public int Index
        {
            get => index;
            set
            {
                index = value;
                OnPropertyChanged($"{nameof(Index)}");
            }
        }

        public string DocName
        {
            get => docName;
            set
            {
                docName = value;
                OnPropertyChanged($"{nameof(DocName)}");
            }
        }

        public string UserComment
        {
            get => usrComment;
            set
            {
                usrComment = value;
                OnPropertyChanged($"{nameof(UserComment)}");
            }
        }

        #endregion

    }
}
