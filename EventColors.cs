using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace TimetableFH
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
                DefaultBrush = new SolidColorBrush(defaultColor);

                OnPropertyChanged(nameof(DefaultColor));
                OnPropertyChanged(nameof(DefaultBrush));
            }
        }

        [XmlIgnore]
        public Brush DefaultBrush { get; private set; }

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
