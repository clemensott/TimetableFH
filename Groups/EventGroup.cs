using System.Collections.ObjectModel;
using System.ComponentModel;
using TimetableFH.AdmittedClasses;

namespace TimetableFH.Groups
{
    public class EventGroup : INotifyPropertyChanged
    {
        private string name;

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

        public ObservableCollection<NameCompare> Collection { get; set; }

        public EventGroup()
        {
            Collection = new ObservableCollection<NameCompare>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}