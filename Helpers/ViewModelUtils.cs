using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using TimetableFH.AdmittedClasses;
using TimetableFH.Coloring;
using TimetableFH.Groups;
using TimetableFH.Models;
using TimetableFH.Names;
using TimetableFH.PostData;
using TimetableFH.Rooms;
using TimetableFH.Controls.Compare;

namespace TimetableFH.Helpers
{
    static class ViewModelUtils
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(Settings));
        
        private static bool IsAdmittedEvent(this IEnumerable<NameCompare> admittedClasses, Event fhEvent)
        {
            return admittedClasses.Any(c => Compare(c.CompareType, fhEvent.Name, c.Name));
        }
        
        private static bool IsGroupEvent(this EventGroup group, Event fhEvent)
        {
            return group?.Collection?.All(n => !Compare(n.CompareType, fhEvent.Group, n.Name)) ?? false;
        }

        private static Brush GetBrush(this EventColors eventColors, string group, string name)
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

        private static string GetName(this IEnumerable<EventName> items, string name)
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

        private static string Replace(this IEnumerable<ReplaceValue> items, string value)
        {
            foreach (ReplaceValue replaceValue in items)
            {
                if (string.IsNullOrEmpty(replaceValue.Reference)) continue;

                value = value.Replace(replaceValue.Reference, replaceValue.Replacement);
            }

            return value;
        }

        private static string GetShortRoomName(this IEnumerable<ReplaceValue> items, string rawRoom, out IEnumerable<string> types)
        {
            List<string> roomTypes = new List<string>();

            string completeShortName = string.Join("/", rawRoom.Split('/').Select(r =>
            {
                string type;
                string shortName = items.GetShortRoomName(r, out type);
                roomTypes.Add(type);

                return shortName;
            }));

            types = roomTypes;

            return completeShortName;
        }

        private static string GetShortRoomName(this IEnumerable<ReplaceValue> items, string rawRoom, out string type)
        {
            string location, building, roomNumber;

            location = building = roomNumber = type = string.Empty;

            IEnumerator<char> enumerator = ((IEnumerable<char>)rawRoom).GetEnumerator();

            while (true)
            {
                if (!enumerator.MoveNext()) return rawRoom;
                if (enumerator.Current == '.') break;

                location += enumerator.Current;
            }

            while (true)
            {
                if (!enumerator.MoveNext()) return rawRoom;
                if (enumerator.Current == '.') break;

                building += enumerator.Current;
            }

            while (true)
            {
                if (!enumerator.MoveNext()) return rawRoom;
                if (enumerator.Current == ' ') break;

                roomNumber += enumerator.Current;
            }

            while (enumerator.MoveNext())
            {
                type += enumerator.Current;
            }

            enumerator.Dispose();

            return Replace(items, type + roomNumber);
        }

        public static bool Compare(CompareType type, string value, string reference)
        {
            if (type == CompareType.Ignore) return true;
            if (value == null || reference == null) return false;

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

        public static Event SetDetails(this ViewModel viewModel, Event fhEvent)
        {
            IEnumerable<string> roomTypes;

            fhEvent.IsAdmittedClass = !viewModel.Settings.AdmittedClasses.IsAdmittedEvent(fhEvent);
            fhEvent.IsCurrentGroup = viewModel.Settings.Groups.CurrentGroup.IsGroupEvent(fhEvent);
            fhEvent.ShortName = viewModel.Settings.EventNames.GetName(fhEvent.Name);
            fhEvent.ShortRoom = viewModel.Settings.Rooms.GetShortRoomName(fhEvent.Room, out roomTypes);
            fhEvent.Brush = viewModel.Settings.EventColors.GetBrush(fhEvent.Group, fhEvent.Name);

            foreach (string roomType in roomTypes)
            {
                CheckForRoomTypes(viewModel, roomType);
            }

            return fhEvent;
        }

        private static void CheckForRoomTypes(ViewModel viewModel, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || viewModel.Settings.Rooms.Examples.Contains(value)) return;

            viewModel.Settings.Rooms.Examples.Add(value);
        }

        public static string ToPostData(this IEnumerable<StringKeyValuePair> pairs)
        {
            return string.Join("&", pairs.Select(p => p.Key + "=" + p.Value));
        }

        public static async Task<Settings> Load(string fileName)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);

            return await Load(file);
        }

        public static async Task<Settings> Load(StorageFile file)
        {
            string xmlText = await FileIO.ReadTextAsync(file);

            return (Settings)serializer.Deserialize(new StringReader(xmlText));
        }

        public static async Task Save(this Settings settings, string fileName)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder
                .CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

            await Save(settings, file);
        }

        public static async Task Save(this Settings settings, StorageFile file)
        {
            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, settings);

            await FileIO.WriteTextAsync(file, writer.ToString());
        }

        public static bool Contains(this DaysOfWeek daysOfWeek, DayOfWeek day)
        {
            return (daysOfWeek & Convert(day)) > 0;
        }

        public static DaysOfWeek Convert(this DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Friday:
                    return DaysOfWeek.Friday;

                case DayOfWeek.Monday:
                    return DaysOfWeek.Monday;

                case DayOfWeek.Saturday:
                    return DaysOfWeek.Saturday;

                case DayOfWeek.Sunday:
                    return DaysOfWeek.Sunday;

                case DayOfWeek.Thursday:
                    return DaysOfWeek.Thursday;

                case DayOfWeek.Tuesday:
                    return DaysOfWeek.Tuesday;

                case DayOfWeek.Wednesday:
                    return DaysOfWeek.Wednesday;

                default:
                    throw new ArgumentOutOfRangeException(nameof(day), day, null);
            }
        }

        public static IEnumerable<DayOfWeek> GetDaysOfWeek(this DaysOfWeek days)
        {
            if ((days & DaysOfWeek.Monday) > 0) yield return DayOfWeek.Monday;
            if ((days & DaysOfWeek.Tuesday) > 0) yield return DayOfWeek.Tuesday;
            if ((days & DaysOfWeek.Wednesday) > 0) yield return DayOfWeek.Wednesday;
            if ((days & DaysOfWeek.Thursday) > 0) yield return DayOfWeek.Thursday;
            if ((days & DaysOfWeek.Friday) > 0) yield return DayOfWeek.Friday;
            if ((days & DaysOfWeek.Saturday) > 0) yield return DayOfWeek.Saturday;
            if ((days & DaysOfWeek.Sunday) > 0) yield return DayOfWeek.Sunday;
        }
    }
}
