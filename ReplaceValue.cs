using System.ComponentModel;

namespace TimtableFH
{
    public class ReplaceValue : INotifyPropertyChanged
    {
        private string reference, replacement;

        public string Reference
        {
            get { return reference; }
            set
            {
                if (value == reference) return;

                reference = value;
                OnPropertyChanged(nameof(Reference));
            }
        }

        public string Replacement
        {
            get { return replacement; }
            set
            {
                if (value == replacement) return;

                replacement = value;
                OnPropertyChanged(nameof(Replacement));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}