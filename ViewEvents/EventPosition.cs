using TimetableFH.Models;
using Windows.Foundation;

namespace TimetableFH.ViewEvents
{
    public struct EventPosition
    {
        public Rect Rect { get; }

        public Event Event { get; }

        public EventPosition(Rect rect, Event @event) : this()
        {
            Rect = rect;
            Event = @event;
        }
    }
}
