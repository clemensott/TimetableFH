using System;
using System.ComponentModel;
using Windows.UI;

namespace TimetableFH.Models
{
    public class Event : INotifyPropertyChanged
    {
        private bool isAdmittedClass, isCurrentGroup;
        private string shortName, shortRoom;
        private Color color;

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

        public Color Color
        {
            get => color;
            set
            {
                if (value == color) return;

                color = value;
                OnPropertyChanged(nameof(Color));
            }
        }


        public EventBase Base { get; }

        public DateTime Begin => Base.Begin;

        public DateTime End => Base.End;

        public string Name => Base.Name;

        public string Professor => Base.Professor;

        public string Room => Base.Room;

        public string Group => Base.Group;

        public Event(EventBase @base)
        {
            Base = @base;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return $"{Name} @ {Begin}";
        }
    }
}
