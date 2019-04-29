using System.ComponentModel;

namespace TimtableFH
{
    public class EventName : INotifyPropertyChanged
    {
        private string reference, shortName;
        private CompareType compareType;

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

        public CompareType CompareType
        {
            get { return compareType; }
            set
            {
                if (value == compareType) return;

                compareType = value;
                OnPropertyChanged(nameof(CompareType));
            }
        }

        public string Short
        {
            get { return shortName; }
            set
            {
                if (value == shortName) return;

                shortName = value;
                OnPropertyChanged(nameof(Short));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}