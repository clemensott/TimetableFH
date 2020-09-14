using System;

namespace TimetableFH.ViewEvents
{
    public class EventDay
    {
        public DateTime Date { get; }

        public DayOfWeek Weekday => Date.DayOfWeek;

        public EventPosition[] Events { get; }

        public EventDay(DateTime date, EventPosition[] events)
        {
            Date = date;
            Events = events;
        }
    }
}
