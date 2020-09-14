using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace TimetableFH.Rooms
{
    public class EventRooms : ObservableCollection<ReplaceValue>
    {
        [XmlIgnore]
        public ObservableCollection<string> Examples { get; set; }

        public EventRooms()
        {
            Examples = new ObservableCollection<string>();
        }
    }
}
