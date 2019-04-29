using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TimtableFH
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class EventNameEditPage : Page
    {
        private EventNameEditPageViewModel viewModel;

        public EventNameEditPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = viewModel = (EventNameEditPageViewModel)e.Parameter;

            base.OnNavigatedTo(e);
        }

        private void AbbBack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((AutoSuggestBox)sender).IsSuggestionListOpen = true;
        }
    }

    class EventNameEditPageViewModel : INotifyPropertyChanged
    {
        private EventName eventName;
        private IEnumerable<string> suggestedNames;

        public EventName EventName
        {
            get { return eventName; }
            set
            {
                if (value == eventName) return;

                if (eventName != null) eventName.PropertyChanged -= EventName_PropertyChanged;
                eventName = value;
                if (eventName != null) eventName.PropertyChanged += EventName_PropertyChanged;

                OnPropertyChanged(nameof(EventName));

                SetSuggestedNames();
            }
        }

        public IEnumerable<string> Names { get; }

        public IEnumerable<string> SuggestedNames
        {
            get { return suggestedNames; }
            set
            {
                if (value == suggestedNames) return;

                suggestedNames = value;
                OnPropertyChanged(nameof(SuggestedNames));
            }
        }

        public EventNameEditPageViewModel(EventName eventName, IEnumerable<string> names)
        {
            Names = names;
            EventName = eventName;
        }

        private void EventName_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EventName.Reference)) SetSuggestedNames();
        }

        private void SetSuggestedNames()
        {
            SuggestedNames = Names.Filter(EventName?.Reference?.ToLower());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
