using System.ComponentModel;
using System.Xml.Serialization;
using Windows.UI;
using Windows.UI.Xaml.Media;
using TimetableFH.Controls.Compare;

namespace TimetableFH.Coloring
{
    public class EventColor : INotifyPropertyChanged
    {
        private string group, name;
        private CompareType groupCompareType, nameCompareType;
        private Windows.UI.Color color;

        public string Group
        {
            get { return group; }
            set
            {
                if (value == group) return;

                group = value;
                OnPropertyChanged(nameof(Group));
            }
        }

        public CompareType GroupCompareType
        {
            get { return groupCompareType; }
            set
            {
                if (value == groupCompareType) return;

                groupCompareType = value;
                OnPropertyChanged(nameof(GroupCompareType));
            }
        }

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

        public CompareType NameCompareType
        {
            get { return nameCompareType; }
            set
            {
                if (value == nameCompareType) return;

                nameCompareType = value;
                OnPropertyChanged(nameof(NameCompareType));
            }
        }

        public Windows.UI.Color Color
        {
            get { return color; }
            set
            {
                if (value == color) return;

                color = value;
                Brush = new SolidColorBrush(color);

                OnPropertyChanged(nameof(Color));
                OnPropertyChanged(nameof(Brush));
            }
        }

        [XmlIgnore]
        public Brush Brush { get; private set; }

        public EventColor()
        {
            GroupCompareType = CompareType.Ignore;
            NameCompareType = CompareType.Ignore;
            Color = Colors.Gray;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
