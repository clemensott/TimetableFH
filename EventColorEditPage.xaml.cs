using StdOttStandard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TimetableFH
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class EventColorEditPage : Page
    {
        private EventColorEditPageViewModel viewModel;

        public EventColorEditPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = viewModel = (EventColorEditPageViewModel)e.Parameter;

            base.OnNavigatedTo(e);
        }

        private async void RectColor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetableValue<Color?> setableValue = new SetableValue<Color?>();

            Frame.Navigate(typeof(ColorPickerPage), (viewModel.EventColor.Color, setableValue));

            Color? color = await setableValue.Task;

            if (color.HasValue) viewModel.EventColor.Color = color.Value;
        }

        private void AbbBack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void AutoSuggestBox_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ((AutoSuggestBox)sender).IsSuggestionListOpen = true;
        }
    }

    public class EventColorEditPageViewModel : INotifyPropertyChanged
    {
        private EventColor eventColor;
        private IEnumerable<string> suggestedGroups, suggestedNames;

        public EventColor EventColor
        {
            get { return eventColor; }
            set
            {
                if (value == eventColor) return;

                if (eventColor != null) eventColor.PropertyChanged -= EventColor_PropertyChanged;
                eventColor = value;
                if (eventColor != null) eventColor.PropertyChanged += EventColor_PropertyChanged;

                OnPropertyChanged(nameof(EventColor));

                SetSuggestedGroups();
                SetSuggestedName();
            }
        }

        public IEnumerable<string> Groups { get; }

        public IEnumerable<string> Names { get; }

        public IEnumerable<string> SuggestedGroups
        {
            get { return suggestedGroups; }
            private set
            {
                if (value == suggestedGroups) return;

                suggestedGroups = value;
                OnPropertyChanged(nameof(SuggestedGroups));
            }
        }

        public IEnumerable<string> SuggestedNames
        {
            get { return suggestedNames; }
            private set
            {
                if (value == suggestedNames) return;

                suggestedNames = value;
                OnPropertyChanged(nameof(SuggestedNames));
            }
        }

        public EventColorEditPageViewModel(EventColor eventColor, IEnumerable<string> groups, IEnumerable<string> names)
        {
            Groups = groups;
            Names = names;
            EventColor = eventColor;
        }

        private void EventColor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EventColor.Group)) SetSuggestedGroups();
            else if (e.PropertyName == nameof(EventColor.Name)) SetSuggestedName();
        }

        private void SetSuggestedGroups()
        {
            SuggestedGroups = Groups.Filter(EventColor?.Group?.ToLower());
        }

        private void SetSuggestedName()
        {
            SuggestedNames = Names.Filter(EventColor?.Name?.ToLower());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
