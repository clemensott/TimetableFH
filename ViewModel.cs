using StdOttStandard;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Storage;

namespace TimtableFH
{
    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(ViewModel));

        private bool viewGroupEvents;
        private DateTime refTime;
        private TimeSpan viewDuration;
        private Event[] allEvents, myGroupEvents, myEvents;
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

                allEvents = value;
                OnPropertyChanged(nameof(AllEvents));

                MyGroupEvents = AllEvents.Where(IsMyGroupEvent).ToArray();
            }
        }

        [XmlIgnore]
        public Event[] MyGroupEvents
        {
            get { return myGroupEvents; }
            private set
            {
                if (value.BothNullOrSequenceEqual(myGroupEvents)) return;

                myGroupEvents = value;
                OnPropertyChanged(nameof(MyGroupEvents));

                if (ViewGroupEvents) OnPropertyChanged(nameof(ViewEvents));

                MyEvents = MyGroupEvents.Where(IsNotAdmittedEvent).ToArray();
            }
        }

        [XmlIgnore]
        public Event[] MyEvents
        {
            get { return myEvents; }
            private set
            {
                if (value.BothNullOrSequenceEqual(myEvents)) return;

                myEvents = value;
                OnPropertyChanged(nameof(MyEvents));

                if (!ViewGroupEvents) OnPropertyChanged(nameof(ViewEvents));
            }
        }

        [XmlIgnore]
        public Event[] ViewEvents => ViewGroupEvents ? MyGroupEvents : MyEvents;

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

        public ViewModel()
        {
            AllEvents = new Event[0];

            RefTime = GetLastMondayMorning();
            ViewDuration = TimeSpan.FromHours(8);
            ViewDaysCount = 5;
        }

        private bool IsMyGroupEvent(Event fhEvent)
        {
            return fhEvent.Group != "B1" && fhEvent.Group != "B2" && fhEvent.Group != "M1";
        }

        private bool IsNotAdmittedEvent(Event fhEvent)
        {
            return !fhEvent.Name.StartsWith("PMuAR") &&
                !fhEvent.Name.StartsWith("PROG2") &&
                !fhEvent.Name.StartsWith("WEBTECH");
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

        public async static Task<ViewModel> Load(string fileName)
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
