using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace TimetableFH
{
    public class EventGroups : INotifyPropertyChanged
    {
        private int currentGroupIndex;
        private EventGroup currentGroup;
        private ObservableCollection<EventGroup> collection;

        public int CurrentGroupIndex
        {
            get { return currentGroupIndex; }
            set
            {
                if (value == currentGroupIndex) return;

                currentGroupIndex = value;
                OnPropertyChanged(nameof(CurrentGroupIndex));

                UpdateCurrentGroup();
            }
        }

        [XmlIgnore]
        public EventGroup CurrentGroup
        {
            get { return currentGroup; }
            private set
            {
                if (value == currentGroup) return;

                currentGroup = value;
                OnPropertyChanged(nameof(CurrentGroup));
            }
        }

        public ObservableCollection<EventGroup> Collection
        {
            get { return collection; }
            set
            {
                if (value == collection) return;

                if (collection != null) collection.CollectionChanged -= Collection_CollectionChanged;
                collection = value;
                if (collection != null) collection.CollectionChanged += Collection_CollectionChanged;

                OnPropertyChanged(nameof(Collection));

                UpdateCurrentGroup();
            }
        }

        public EventGroups()
        {
            CurrentGroupIndex = -1;
            Collection = new ObservableCollection<EventGroup>();
        }

        private void Collection_CollectionChanged(object sender, EventArgs e)
        {
            UpdateCurrentGroup();
        }

        private void UpdateCurrentGroup()
        {
            CurrentGroup = Collection?.ElementAtOrDefault(CurrentGroupIndex);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}