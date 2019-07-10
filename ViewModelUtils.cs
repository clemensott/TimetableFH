using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace TimetableFH
{
    static class ViewModelUtils
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(ViewModel));

        public static IEnumerable<Event> GetAdmittedEvents(this IEnumerable<Event> events, IEnumerable<NameCompare> notAdmittedClasses)
        {
            return events.Where(e => !IsNotAdmittedEvent(notAdmittedClasses, e));
        }

        private static bool IsNotAdmittedEvent(this IEnumerable<NameCompare> notAdmittedClasses, Event fhEvent)
        {
            return notAdmittedClasses.Any(c => Compare(c.CompareType, fhEvent.Name, c.Name));
        }

        public static IEnumerable<Event> GetGroupEvents(this IEnumerable<Event> events, EventGroups groups)
        {
            EventGroup group = groups?.CurrentGroup;

            return group == null ? events : events.Where(e => IsGroupEvent(group, e));
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

        private static bool IsEvent(EventColor ec, string group, string name)
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

        private static bool IsEvent(EventName en, string name)
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

        private static bool Compare(CompareType type, string value, string reference)
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

        public static Event SetDetails(this ViewModel viewModel, Event fhEvent)
        {
            IEnumerable<string> roomTypes;

            fhEvent.IsAdmittedClass = !viewModel.NotAdmittedClasses.IsNotAdmittedEvent(fhEvent);
            fhEvent.IsCurrentGroup = viewModel.Groups.CurrentGroup.IsGroupEvent(fhEvent);
            fhEvent.ShortName = viewModel.EventNames.GetName(fhEvent.Name);
            fhEvent.ShortRoom = viewModel.Rooms.GetShortRoomName(fhEvent.Room, out roomTypes);
            fhEvent.Brush = viewModel.EventColors.GetBrush(fhEvent.Group, fhEvent.Name);

            foreach (string roomType in roomTypes)
            {
                CheckForRoomTypes(viewModel, roomType);
            }

            return fhEvent;
        }

        private static void CheckForRoomTypes(ViewModel viewModel, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || viewModel.Rooms.Examples.Contains(value)) return;

            viewModel.Rooms.Examples.Add(value);
        }

        public static string ToPostData(this IEnumerable<StringKeyValuePair> pairs)
        {
            return string.Join("&", pairs.Select(p => p.Key + "=" + p.Value));
        }

        public static async Task<ViewModel> Load(string fileName)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);

            return await Load(file);
        }

        public static async Task<ViewModel> Load(StorageFile file)
        {
            string xmlText = await FileIO.ReadTextAsync(file);

            return (ViewModel)serializer.Deserialize(new StringReader(xmlText));
        }

        public static Task Save(this ViewModel viewModel, string fileName)
        {
            IAsyncOperation<StorageFile> fileOperation = ApplicationData.Current.LocalFolder
                .CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

            return Save(viewModel, fileOperation);
        }

        public static async Task Save(this ViewModel viewModel, IAsyncOperation<StorageFile> fileOperation)
        {
            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, viewModel);

            await FileIO.WriteTextAsync(await fileOperation, writer.ToString());
        }
    }
}
