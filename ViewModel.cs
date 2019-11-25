using StdOttStandard;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace TimetableFH
{
    public enum CompareType { Equals, StartsWith, EndsWith, Contains, Ignore }

    public class ViewModel : INotifyPropertyChanged
    {
        private readonly ThemeChecker themeChecker;
        private Event[] allEvents, groupEvents, admittedEvents;
        private Settings settings;

        public Event[] AllEvents
        {
            get { return allEvents; }
            set
            {
                if (value == allEvents) return;

                Settings?.Rooms?.Examples?.Clear();
                foreach (Event fhEvent in value)
                {
                    this.SetDetails(fhEvent);
                }

                allEvents = value;
                OnPropertyChanged(nameof(AllEvents));

                GroupEvents = AllEvents.Where(e => e.IsCurrentGroup).ToArray();

                foreach (Event fhEvent in AllEvents) this.SetDetails(fhEvent);
            }
        }

        public Event[] GroupEvents
        {
            get { return groupEvents; }
            private set
            {
                if (value.BothNullOrSequenceEqual(groupEvents)) return;

                groupEvents = value;
                OnPropertyChanged(nameof(GroupEvents));

                if (Settings?.ViewGroupEvents == true) OnPropertyChanged(nameof(ViewEvents));

                AdmittedEvents = GroupEvents.Where(e => e.IsAdmittedClass).ToArray();
            }
        }

        public Event[] AdmittedEvents
        {
            get { return admittedEvents; }
            private set
            {
                if (value.BothNullOrSequenceEqual(admittedEvents)) return;

                admittedEvents = value;
                OnPropertyChanged(nameof(AdmittedEvents));

                if (!Settings?.ViewGroupEvents == true) OnPropertyChanged(nameof(ViewEvents));
            }
        }

        public Event[] ViewEvents => Settings?.ViewGroupEvents == true ? GroupEvents : AdmittedEvents;

        public IEnumerable<string> Names => this.GetAllNames();

        public Settings Settings
        {
            get => settings;
            set
            {
                if (value == settings) return;

                if (settings != null) Settings.PropertyChanged -= Settings_PropertyChanged;
                settings = value;
                if (settings != null) Settings.PropertyChanged += Settings_PropertyChanged;

                OnPropertyChanged(nameof(Settings));

                foreach (Event fhEvent in AllEvents) this.SetDetails(fhEvent);
                CheckTheme();
            }
        }

        public ViewModel()
        {
            themeChecker = new ThemeChecker();
            AllEvents = new Event[0];
            Settings = new Settings();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.ViewGroupEvents)) OnPropertyChanged(nameof(ViewEvents));
        }

        public async Task CheckTheme()
        {
            Settings currentSettings = Settings;
            if (!await themeChecker.Check(currentSettings) || currentSettings != Settings) return;

            foreach (EventColor eventColor in Settings.EventColors.Collection)
            {
                eventColor.Color = InvertColor(eventColor.Color);
            }

            foreach (Event fhEvent in AllEvents) this.SetDetails(fhEvent);
        }

        private static Color InvertColor(Color color)
        {
            return Color.FromArgb(color.A, (byte)(byte.MaxValue - color.R),
                (byte)(byte.MaxValue - color.G), (byte)(byte.MaxValue - color.B));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
