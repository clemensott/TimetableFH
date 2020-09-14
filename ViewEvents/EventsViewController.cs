using StdOttStandard.Linq;
using StdOttStandard.Linq.DataStructures.Observable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TimetableFH.Helpers;
using TimetableFH.Models;
using Windows.Foundation;

namespace TimetableFH.ViewEvents
{
    public class EventsViewController : INotifyPropertyChanged
    {
        private DateTime referenceDate;
        private DaysOfWeek viewDays;
        private bool hideAdmittedEvents;
        private Event[] allEvents;
        private ILookup<DateTime, Event> eventsLookup;
        private readonly ObservableCollection<EventDay> eventDays;

        public DateTime ReferenceDate
        {
            get => referenceDate;
            set
            {
                value = value.Date;
                if (value == referenceDate) return;

                referenceDate = value;
                OnPropertyChanged(nameof(ReferenceDate));

                Update();
            }
        }

        public DaysOfWeek ViewDays
        {
            get => viewDays;
            set
            {
                if (value == viewDays) return;

                viewDays = value;
                OnPropertyChanged(nameof(ViewDays));

                Update();
            }
        }

        public bool HideAdmittedEvents
        {
            get => hideAdmittedEvents;
            set
            {
                if (value == hideAdmittedEvents) return;

                hideAdmittedEvents = value;
                OnPropertyChanged(nameof(HideAdmittedEvents));

                Update(eventDays.Where(d => d.Events.Any(e => e.Event.IsAdmittedClass)).Select(d => d.Date));
            }
        }

        public Event[] AllEvents
        {
            get => allEvents;
            set
            {
                if (value == allEvents) return;

                IEnumerable<Event> oldValue = allEvents;

                allEvents = value;
                OnPropertyChanged(nameof(AllEvents));

                if (!oldValue.BothNullOrSequenceEqual(allEvents)) UpdateAll();
            }
        }

        public IReadOnlyObservableCollection<EventDay> EventDays { get; }

        public EventsViewController()
        {
            eventDays = new ObservableCollection<EventDay>();
            EventDays = eventDays.AsReadonly();
        }

        public void Update()
        {
            Update(Enumerable.Empty<DateTime>());
        }

        private void UpdateAll()
        {
            eventsLookup = allEvents.ToLookup(e => e.Begin.Date);
            Update(null);
        }

        private void Update(DateTime dayToUpdate)
        {
            Update(new DateTime[] { dayToUpdate });
        }

        private void Update(IEnumerable<DateTime> daysToUpdate)
        {
            EventDay[] oldDays = daysToUpdate != null ?
                eventDays.Where(d => !daysToUpdate.Contains(d.Date)).ToArray() : new EventDay[0];

            (DateTime d, Event[])[] newDays = GetOrderedViewDays()
                .Select(GetDateOfNext).Select(d => (d, GetEventsForDate(d))).ToArray();

            eventDays.RemoveLast<EventDay>(eventDays.Count - newDays.Length);

            int index = 0;
            foreach ((DateTime date, Event[] events) in newDays)
            {
                EventDay eventDay = oldDays.FirstOrDefault(d => d.Date == date);

                if (eventDay == null || !eventDay.Events.Select(e => e.Event).SequenceEqual(events))
                {
                    eventDay = CreateDay(date, events);
                }

                if (index < eventDays.Count) eventDays[index] = eventDay;
                else eventDays.Add(eventDay);

                index++;
            }
        }

        private IEnumerable<DayOfWeek> GetOrderedViewDays()
        {
            DayOfWeek refDayOfWeek = ReferenceDate.DayOfWeek;
            return ViewDays.GetDaysOfWeek().OrderBy(d => d >= refDayOfWeek).ThenBy(d => d);
        }

        private DateTime GetDateOfNext(DayOfWeek day)
        {
            DateTime date = ReferenceDate.Date;

            while (date.DayOfWeek != day)
            {
                date = date.AddDays(1);
            }

            return date;
        }

        private Event[] GetEventsForDate(DateTime date)
        {
            return eventsLookup?.Contains(date) == true ? eventsLookup[date].Where(ViewEvent).ToArray() : new Event[0];
        }

        private bool ViewEvent(Event fhEvent)
        {
            return fhEvent.IsCurrentGroup && (!HideAdmittedEvents || !fhEvent.IsAdmittedClass);
        }

        public static EventDay CreateDay(DateTime date, IList<Event> events)
        {
            List<EventPosition> positions = new List<EventPosition>();
            List<IList<Event>> simultaneousGroups = new List<IList<Event>>();

            if (events.Count > 0)
            {
                FindGroups(new Queue<Event>(events), new Stack<Event>(), simultaneousGroups);

                IEnumerable<(Event, IList<Event>[])> ordered = events
                    .Select(e => (fhEvent: e, groups: simultaneousGroups.Where(g => g.Contains(e)).ToArray()))
                    .OrderByDescending(t => t.groups.Max(a => a.Count))
                    .ThenByDescending(t => t.groups.Length)
                    .ThenBy(t => t.fhEvent.Begin);

                foreach ((Event fhEvent, IList<Event>[] groups) in ordered)
                {
                    double minWidth = double.MaxValue;

                    foreach (IList<Event> group in groups)
                    {
                        double remainingWidth = 1;
                        int remainingEvents = group.Count;

                        foreach (Event simultaneousEvent in group)
                        {
                            EventPosition pos;
                            if (!positions.TryFirst(p => p.Event.Equals(simultaneousEvent), out pos)) continue;

                            remainingWidth -= pos.Rect.Width;
                            remainingEvents--;
                        }

                        double width = remainingWidth / remainingEvents;
                        if (minWidth > width) minWidth = width;
                    }

                    Rect rect = new Rect(0, fhEvent.Begin.TimeOfDay.TotalHours,
                        minWidth, (fhEvent.End - fhEvent.Begin).TotalHours);

                    foreach (EventPosition position in positions)
                    {
                        Rect intersect = position.Rect;
                        intersect.Intersect(rect);

                        if (intersect.IsEmpty || intersect.Width == 0 || intersect.Height == 0) continue;

                        rect = new Rect(position.Rect.Right, rect.Y, rect.Width, rect.Height);
                    }

                    positions.Add(new EventPosition(rect, fhEvent));
                }
            }

            EventDay eventDay = new EventDay(date, positions.ToArray());
            return eventDay;
        }

        private static bool FindGroups(Queue<Event> remainingEvents, Stack<Event> group, IList<IList<Event>> groups)
        {
            for (int i = remainingEvents.Count - 1; i >= 0; i--)
            {
                Event fhEvent = remainingEvents.Dequeue();
                if (!IsTimeOverlapping(group, fhEvent)) continue;

                group.Push(fhEvent);
                FindGroups(new Queue<Event>(remainingEvents), group, groups);
                group.Pop();
            }

            if (ContainsGroup(groups, group)) return false;

            groups.Add(group.ToArray());
            return true;
        }

        private static bool IsTimeOverlapping(IReadOnlyCollection<Event> events, Event fhEvent)
        {
            return events.Count == 0 || events.All(e => IsTimeOverlapping(e, fhEvent));
        }

        private static bool IsTimeOverlapping(Event e1, Event e2)
        {
            return
                (e1.Begin == e2.Begin && e1.End == e2.End) ||
                (e1.Begin <= e2.Begin && e1.End > e2.Begin) ||
                (e2.Begin <= e1.Begin && e2.End > e1.Begin);
        }

        private static bool ContainsGroup(IList<IList<Event>> groups, IReadOnlyCollection<Event> group)
        {
            foreach (IList<Event> existingGroup in groups.ReverseAsIList())
            {
                bool equals = true;
                foreach (Event fhEvent in group)
                {
                    if (existingGroup.Contains(fhEvent)) continue;

                    equals = false;
                    break;
                }

                if (equals) return true;
            }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
