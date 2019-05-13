using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml.Media;

namespace TimetableFH
{
    public class Event : EventBase, INotifyPropertyChanged
    {
        private bool isAdmittedClass, isCurrentGroup;
        private string shortName, shortRoom;
        private Brush brush;

        public bool IsAdmittedClass
        {
            get => isAdmittedClass;
            set
            {
                if (value == isAdmittedClass) return;

                isAdmittedClass = value;
                OnPropertyChanged(nameof(IsAdmittedClass));
            }
        }

        public string ShortName
        {
            get => shortName;
            set
            {
                if (value == shortName) return;

                shortName = value;
                OnPropertyChanged(nameof(ShortName));
            }
        }

        public string ShortRoom
        {
            get => shortRoom;
            set
            {
                if (value == shortRoom) return;

                shortRoom = value;
                OnPropertyChanged(nameof(ShortRoom));
            }
        }

        public bool IsCurrentGroup
        {
            get => isCurrentGroup;
            set
            {
                if (value == isCurrentGroup) return;

                isCurrentGroup = value;
                OnPropertyChanged(nameof(IsCurrentGroup));
            }
        }

        public Brush Brush
        {
            get => brush;
            set
            {
                if (value == brush) return;

                brush = value;
                OnPropertyChanged(nameof(Brush));
            }
        }

        public Event()
        {
        }

        public Event(Dictionary<string, string> dict) : base(dict)
        {
        }
    }
}
