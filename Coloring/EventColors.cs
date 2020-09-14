using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI;

namespace TimetableFH.Coloring
{
    public class EventColors : INotifyPropertyChanged
    {
        private Color defaultColor;

        public Color DefaultColor
        {
            get => defaultColor;
            set
            {
                if (value == defaultColor) return;

                defaultColor = value;
                OnPropertyChanged(nameof(DefaultColor));
            }
        }

        public ObservableCollection<EventColor> Collection { get; set; }

        public EventColors()
        {
            DefaultColor = Colors.LightGray;
            Collection = new ObservableCollection<EventColor>();
        }

		public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
