using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace TimtableFH
{
    static class ViewModelUtils
    {
        public static IEnumerable<Event> GetAdmittedEvents(this IEnumerable<Event> events, IEnumerable<NameCompare> notAdmittedClasses)
        {
            return events.Where(e => !IsNotAdmittedEvent(notAdmittedClasses, e));
        }

        private static bool IsNotAdmittedEvent(IEnumerable<NameCompare> notAdmittedClasses, Event fhEvent)
        {
            return notAdmittedClasses.Any(c => Compare(c.CompareType, fhEvent.Name, c.Name));
        }

        public static IEnumerable<Event> GetGroupEvents(this IEnumerable<Event> events, EventGroups groups)
        {
            if (groups?.CurrentGroup == null) return events.Where(e => e.Group != "B1" && e.Group != "B2" && e.Group != "M1");

            EventGroup group = groups?.CurrentGroup;

            return group == null ? events : events.Where(e => IsGroupEvent(e, group));
        }

        private static bool IsGroupEvent(Event fhEvent, EventGroup group)
        {
            return group.Collection.All(n => !Compare(n.CompareType, fhEvent.Group, n.Name));
        }

        public static Brush GetBrush(this EventColors eventColors, string group, string name)
        {
            foreach (EventColor eventColor in eventColors.Collection)
            {
                if (IsEvent(eventColor, group, name)) return eventColor.Brush;
            }

            return eventColors.DefaultBrush;
        }

        public static bool IsEvent(EventColor ec, string group, string name)
        {
            return Compare(ec.GroupCompareType, group, ec.Group) && Compare(ec.NameCompareType, name, ec.Name);
        }

        public static string GetName(this IEnumerable<EventName> items, string name)
        {
            foreach (EventName eventName in items)
            {
                if (IsEvent(eventName, name)) return eventName.Short;
            }

            return name;
        }

        public static bool IsEvent(EventName en, string name)
        {
            return Compare(en.CompareType, name, en.Reference);
        }

        public static string Replace(this IEnumerable<ReplaceValue> items, string value)
        {
            foreach (ReplaceValue replaceValue in items)
            {
                value = value.Replace(replaceValue.Reference, replaceValue.Replacement);
            }

            return value;
        }

        public static bool Compare(CompareType type, string value, string reference)
        {
            switch (type)
            {
                case CompareType.Equals:
                    return value.Equals(reference);

                case CompareType.StartsWith:
                    return value.StartsWith(reference);

                case CompareType.EndsWith:
                    return value.EndsWith(reference);

                case CompareType.Contains:
                    return value.Contains(reference);

                case CompareType.Ignore:
                    return true;
            }

            throw new ArgumentException("CompareType \"" + type + "\" is not implemented.", nameof(type));
        }

        public static IEnumerable<string> GetAllGroups(this ViewModel viewModel)
        {
            return viewModel.AllEvents.Select(e => e.Group).Distinct().Where(g => !string.IsNullOrWhiteSpace(g)).OrderBy(g => g);
        }

        public static IEnumerable<string> GetAllNames(this ViewModel viewModel)
        {
            return viewModel.AllEvents.Select(e => e.Name).Distinct().Where(n => !string.IsNullOrWhiteSpace(n)).OrderBy(n => n);
        }

        public static IEnumerable<string> Filter(this IEnumerable<string> items, string key)
        {
            if (string.IsNullOrEmpty(key) || items == null) return items;

            string lowerKey = key.ToLower();
            string[] suggestion = items.Where(i => IsGoodSuggestion(i, lowerKey)).ToArray();

            return suggestion.Length > 0 ? suggestion : items;
        }

        private static bool IsGoodSuggestion(string item, string lowerKey)
        {
            if (string.IsNullOrEmpty(item)) return false;

            string lowerItem = item.ToLower();

            return lowerItem != lowerKey && lowerItem.Contains(lowerKey);
        }
    }
}
