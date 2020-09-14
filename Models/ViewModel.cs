using StdOttStandard.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TimetableFH.Coloring;
using TimetableFH.Groups;
using TimetableFH.Helpers;
using TimetableFH.ViewEvents;
using Windows.UI;

namespace TimetableFH.Models
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly ThemeChecker themeChecker;
        private EventBase[] allEvents;
        private Settings settings;

        public EventBase[] AllEvents
        {
            get { return allEvents; }
            set
            {
                if (value == allEvents) return;

                EventBase[] oldValue = allEvents;

                allEvents = value;
                OnPropertyChanged(nameof(AllEvents));

                if (!oldValue.BothNullOrSequenceEqual(allEvents)) SetControllerEvents();
            }
        }

        public IEnumerable<string> Names => this.GetAllNames();

        public Settings Settings
        {
            get => settings;
            set
            {
                if (value == null || value == settings) return;

                if (settings != null) Unsubscribe(settings);
                settings = value;
                Subscribe(settings);

                OnPropertyChanged(nameof(Settings));

                ApplySettings();
                CheckTheme();
            }
        }

        public EventsViewController Controller { get; }

        public ViewModel()
        {
            Controller = new EventsViewController();
            themeChecker = new ThemeChecker();
            AllEvents = new EventBase[0];
            Settings = new Settings();
        }

        public async Task CheckTheme()
        {
            Settings currentSettings = Settings;
            if (!await themeChecker.Check(currentSettings) || currentSettings != Settings) return;

            foreach (EventColor eventColor in Settings.EventColors.Collection)
            {
                eventColor.Color = InvertColor(eventColor.Color);
            }

            ApplySettings();
        }

        private static Color InvertColor(Color color)
        {
            return Color.FromArgb(color.A, (byte)(byte.MaxValue - color.R),
                (byte)(byte.MaxValue - color.G), (byte)(byte.MaxValue - color.B));
        }

        private void Subscribe(Settings settings)
        {
            if (settings == null) return;

            settings.PropertyChanged += Settings_PropertyChanged;
        }

        private void Unsubscribe(Settings settings)
        {
            if (settings == null) return;

            settings.PropertyChanged -= Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.HideAdmittedClasses):
                    Controller.HideAdmittedEvents = Settings.HideAdmittedClasses;
                    break;

                case nameof(Settings.RefTime):
                    Controller.ReferenceDate = Settings.RefTime.Date;
                    break;

                case nameof(Settings.DaysOfWeek):
                    Controller.ViewDays = Settings.DaysOfWeek;
                    break;
            }
        }

        public void ApplySettings()
        {
            Settings settings = Settings;
            if (settings == null) return;

            Controller.HideAdmittedEvents = settings.HideAdmittedClasses;
            Controller.ReferenceDate = settings.RefTime.Date;
            Controller.ViewDays = settings.DaysOfWeek;

            UpdateControllerEvents();
        }

        private void UpdateControllerEvents()
        {
            Settings settings = Settings;
            if (settings == null) return;

            settings.Rooms.Clear();
            foreach (Event fhEvent in Controller.AllEvents.ToNotNull())
            {
                settings.UpdateEvent(fhEvent);
            }

            Controller.Update();
        }

        private void SetControllerEvents()
        {
            Settings settings = Settings;
            if (settings == null) return;

            settings.Rooms.Examples.Clear();
            Controller.AllEvents = AllEvents.Select(Settings.CreateEvent).ToArray();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
