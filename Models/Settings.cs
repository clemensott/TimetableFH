using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Windows.UI.Xaml;
using TimetableFH.AdmittedClasses;
using TimetableFH.Coloring;
using TimetableFH.Groups;
using TimetableFH.Helpers;
using TimetableFH.Names;
using TimetableFH.PostData;
using TimetableFH.Rooms;

namespace TimetableFH.Models
{
    public class Settings : INotifyPropertyChanged
    {
        public const string BaseFhUrl = "http://stundenplan.fh-joanneum.at/";

        private bool hideAdmittedClasses, useCustomOptions;
        private ApplicationTheme theme;
        private DaysOfWeek daysOfWeek;
        private DateTime refTime;
        private TimeSpan viewDuration;
        private uint beginYear;
        private string majorShortName, customBaseUrl, customRequestUrlAddition;

        public bool HideAdmittedClasses
        {
            get { return hideAdmittedClasses; }
            set
            {
                if (value == hideAdmittedClasses) return;

                hideAdmittedClasses = value;
                OnPropertyChanged(nameof(HideAdmittedClasses));
            }
        }

        public ApplicationTheme Theme
        {
            get => theme;
            set
            {
                if (value == theme) return;

                theme = value;
                OnPropertyChanged(nameof(Theme));
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

        public bool UseCustomOptions
        {
            get => useCustomOptions;
            set
            {
                if (value == useCustomOptions) return;

                useCustomOptions = value;
                OnPropertyChanged(nameof(UseCustomOptions));
            }
        }

        public string MajorShortName
        {
            get => majorShortName;
            set
            {
                value = value?.Trim();

                if (value == majorShortName) return;

                majorShortName = value;
                OnPropertyChanged(nameof(MajorShortName));
            }
        }

        public uint BeginYear
        {
            get => beginYear;
            set
            {
                if (value == beginYear) return;

                beginYear = value;
                OnPropertyChanged(nameof(BeginYear));
            }
        }

        public string CustomBaseUrl
        {
            get => customBaseUrl;
            set
            {
                if (value == customBaseUrl) return;

                customBaseUrl = value;
                OnPropertyChanged(nameof(CustomBaseUrl));
            }
        }

        public string CustomRequestUrlAddition
        {
            get => customRequestUrlAddition;
            set
            {
                if (value == customRequestUrlAddition) return;

                customRequestUrlAddition = value;
                OnPropertyChanged(nameof(CustomRequestUrlAddition));
            }
        }

        public EventClasses AdmittedClasses { get; set; }

        public EventGroups Groups { get; set; }

        public EventColors EventColors { get; set; }

        public EventNames EventNames { get; set; }

        public EventRooms Rooms { get; set; }

        public StringKeyValuePairs CustomPostDataPairs { get; set; }

        public Settings()
        {
            Theme = Application.Current?.RequestedTheme ?? default(ApplicationTheme);
            SetCurrentMondayMorning(TimeSpan.FromHours(7));
            ViewDuration = TimeSpan.FromHours(8);
            DaysOfWeek = GetWeekDays();
            UseCustomOptions = true;
            CustomBaseUrl = BaseFhUrl;
            CustomRequestUrlAddition = "?new_stg=MSD&new_jg=2018&new_date=1569830400&new_viewmode=matrix_vertical";

            AdmittedClasses = new EventClasses();
            Groups = new EventGroups();
            EventColors = new EventColors();
            EventNames = new EventNames();
            Rooms = new EventRooms();
            CustomPostDataPairs = new StringKeyValuePairs();
        }

        private static DaysOfWeek GetWeekDays()
        {
            return DaysOfWeek.Monday | DaysOfWeek.Tuesday | DaysOfWeek.Wednesday | DaysOfWeek.Thursday | DaysOfWeek.Friday;
        }

        public void SetCurrentMondayMorning()
        {
            SetCurrentMondayMorning(RefTime.TimeOfDay);
        }

        public void SetCurrentMondayMorning(TimeSpan time)
        {
            DateTime monday = GetCurrentMonday(DateTime.Now, DaysOfWeek);
            RefTime = monday.Date + time;
        }

        public static DateTime GetCurrentMonday(DateTime date, DaysOfWeek daysOfWeek)
        {
            bool weekEnded = true;

            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                if (daysOfWeek.Contains(date.DayOfWeek)) weekEnded = false;

                date = date.AddDays(1);
            }

            if (!weekEnded) date = date.AddDays(-7);

            return date;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
