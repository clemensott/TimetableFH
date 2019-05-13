using StdOttStandard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Storage;

namespace TimtableFH
{
    public enum CompareType { Equals, StartsWith, EndsWith, Contains, Ignore }

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(ViewModel));

        private bool viewGroupEvents;
        private DateTime refTime;
        private TimeSpan viewDuration;
        private Event[] allEvents, groupEvents, admittedEvents;
        private int viewDaysCount;

        public bool ViewGroupEvents
        {
            get { return viewGroupEvents; }
            set
            {
                if (value == viewGroupEvents) return;

                viewGroupEvents = value;
                OnPropertyChanged(nameof(ViewGroupEvents));
                OnPropertyChanged(nameof(ViewEvents));
            }
        }

        public long RefTimeTicks
        {
            get => RefTime.Ticks;
            set => RefTime = new DateTime(value);
        }

        [XmlIgnore]
        public DateTime RefTime
        {
            get { return refTime; }
            set
            {
                if (value == refTime) return;

                refTime = value;
                OnPropertyChanged(nameof(RefTime));
            }
        }

        public long ViewDurationTicks
        {
            get => ViewDuration.Ticks;
            set => ViewDuration = new TimeSpan(value);
        }

        [XmlIgnore]
        public TimeSpan ViewDuration
        {
            get { return viewDuration; }
            set
            {
                if (value == viewDuration) return;

                viewDuration = value;
                OnPropertyChanged(nameof(ViewDuration));
            }
        }

        [XmlIgnore]
        public Event[] AllEvents
        {
            get { return allEvents; }
            set
            {
                if (value == allEvents) return;

                Rooms?.Examples?.Clear();
                foreach (Event fhEvent in value)
                {
                    this.SetDetails(fhEvent);
                }

                allEvents = value;
                OnPropertyChanged(nameof(AllEvents));

                GroupEvents = AllEvents.Where(e => e.IsCurrentGroup).ToArray();
            }
        }

        [XmlIgnore]
        public Event[] GroupEvents
        {
            get { return groupEvents; }
            private set
            {
                if (value.BothNullOrSequenceEqual(groupEvents)) return;

                groupEvents = value;
                OnPropertyChanged(nameof(GroupEvents));

                if (ViewGroupEvents) OnPropertyChanged(nameof(ViewEvents));

                AdmittedEvents = GroupEvents.Where(e => e.IsAdmittedClass).ToArray();
            }
        }

        [XmlIgnore]
        public Event[] AdmittedEvents
        {
            get { return admittedEvents; }
            private set
            {
                if (value.BothNullOrSequenceEqual(admittedEvents)) return;

                admittedEvents = value;
                OnPropertyChanged(nameof(AdmittedEvents));

                if (!ViewGroupEvents) OnPropertyChanged(nameof(ViewEvents));
            }
        }

        [XmlIgnore]
        public Event[] ViewEvents => ViewGroupEvents ? GroupEvents : AdmittedEvents;

        public int ViewDaysCount
        {
            get { return viewDaysCount; }
            set
            {
                if (value == viewDaysCount) return;

                viewDaysCount = value;
                OnPropertyChanged(nameof(ViewDaysCount));
            }
        }

        public EventClasses NotAdmittedClasses { get; set; }

        public EventGroups Groups { get; set; }

        public EventColors EventColors { get; set; }

        public EventNames EventNames { get; set; }

        public EventRooms Rooms { get; set; }

        [XmlIgnore]
        public IEnumerable<string> Names => this.GetAllNames();

        public ViewModel()
        {
            AllEvents = new Event[0];

            RefTime = GetLastMondayMorning();
            ViewDuration = TimeSpan.FromHours(8);
            ViewDaysCount = 5;

            NotAdmittedClasses = new EventClasses();
            Groups = new EventGroups();
            EventColors = new EventColors();
            EventNames = new EventNames();
            Rooms = new EventRooms();
        }

        public static DateTime GetLastMondayMorning()
        {
            return GetMondayMorningBefore(DateTime.Now);
        }

        public static DateTime GetMondayMorningBefore(DateTime date)
        {
            date = date.Date;

            while (date.DayOfWeek != DayOfWeek.Monday) date = date.AddDays(-1);

            return date.AddHours(7);
        }

        public static async Task<ViewModel> Load(string fileName)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                string xmlText = await FileIO.ReadTextAsync(file);

                return (ViewModel)serializer.Deserialize(new StringReader(xmlText));
            }
            catch (Exception exc)
            {
                return new ViewModel();
            }
        }

        public async Task Save(string fileName)
        {
            await Save(fileName, this);
        }

        public async static Task Save(string fileName, ViewModel viewModel)
        {
            try
            {
                IAsyncOperation<StorageFile> fileOperation = ApplicationData.Current.LocalFolder
                    .CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, viewModel);

                await FileIO.WriteTextAsync(await fileOperation, writer.ToString());
            }
            catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
