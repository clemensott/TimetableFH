using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using StdOttStandard;

namespace TimetableFH
{
    public class Settings : INotifyPropertyChanged
    {
        private bool viewGroupEvents, isSingleDay;
        private DaysOfWeek daysOfWeek;
        private DateTime refTime;
        private TimeSpan viewDuration;
        private string baseUrl, requestUrlAddition;

        public bool ViewGroupEvents
        {
            get { return viewGroupEvents; }
            set
            {
                if (value == viewGroupEvents) return;

                viewGroupEvents = value;
                OnPropertyChanged(nameof(ViewGroupEvents));
            }
        }

        public bool IsSingleDay
        {
            get => isSingleDay;
            set
            {
                if (value == isSingleDay) return;

                isSingleDay = value;
                OnPropertyChanged(nameof(IsSingleDay));
            }
        }

        public DaysOfWeek DaysOfWeek
        {
            get => daysOfWeek;
            set
            {
                if (value == daysOfWeek) return;

                daysOfWeek = value;
                OnPropertyChanged(nameof(DaysOfWeek));
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

        public string BaseUrl
        {
            get => baseUrl;
            set
            {
                if (value == baseUrl) return;

                baseUrl = value;
                OnPropertyChanged(nameof(BaseUrl));
            }
        }

        public string RequestUrlAddition
        {
            get => requestUrlAddition;
            set
            {
                if (value == requestUrlAddition) return;

                requestUrlAddition = value;
                OnPropertyChanged(nameof(RequestUrlAddition));
            }
        }

        public EventClasses NotAdmittedClasses { get; set; }

        public EventGroups Groups { get; set; }

        public EventColors EventColors { get; set; }

        public EventNames EventNames { get; set; }

        public EventRooms Rooms { get; set; }

        public StringKeyValuePairs PostDataPairs { get; set; }

        public Settings()
        {
            RefTime = GetLastMondayMorning();
            ViewDuration = TimeSpan.FromHours(8);
            IsSingleDay = false;
            DaysOfWeek = GetWeekDays();
            BaseUrl = "http://stundenplan.fh-joanneum.at/";
            RequestUrlAddition = "?new_stg=MSD&new_jg=2018&new_date=1569830400&new_viewmode=matrix_vertical";

            NotAdmittedClasses = new EventClasses();
            Groups = new EventGroups();
            EventColors = new EventColors();
            EventNames = new EventNames();
            Rooms = new EventRooms();
            PostDataPairs = new StringKeyValuePairs();
        }

        private static DaysOfWeek GetWeekDays()
        {
            return DaysOfWeek.Monday | DaysOfWeek.Tuesday | DaysOfWeek.Wednesday | DaysOfWeek.Thursday | DaysOfWeek.Friday;
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
