using System.ComponentModel;

namespace TimtableFH
{
    public class NameCompare : INotifyPropertyChanged
    {
        private string name;
        private CompareType compareType;

        public string Name
        {
            get { return name; }
            set
            {
                if (value == name) return;

                name = value;
                OnPropertyChanged(nameof(Name));
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}