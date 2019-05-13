using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TimetableFH
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class EventGroupEditPage : Page
    {
        private EventGroupEditPageViewModel viewModel;

        public EventGroupEditPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = viewModel = (EventGroupEditPageViewModel)e.Parameter;

            base.OnNavigatedTo(e);
        }

        private void BtnAddGroup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            viewModel.EventGroup.Collection.Add(new NameCompare());
        }

        private void SinRemoveGroup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NameCompare nameCompare = (NameCompare)((FrameworkElement)sender).DataContext;

            viewModel.EventGroup.Collection.Remove(nameCompare);
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

    class EventGroupEditPageViewModel : INotifyPropertyChanged
    {
        private EventGroup eventGroup;
        private IEnumerable<string> suggestedGroups;

        public EventGroup EventGroup
        {
            get { return eventGroup; }
            set
            {
                if (value == eventGroup) return;

                if (eventGroup != null) eventGroup.PropertyChanged -= EventGroup_PropertyChanged;
                eventGroup = value;
                if (eventGroup != null) eventGroup.PropertyChanged += EventGroup_PropertyChanged;

                OnPropertyChanged(nameof(EventGroup));

                SetSuggestedGroups();
            }
        }

        public IEnumerable<string> Groups { get; }

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

        public EventGroupEditPageViewModel(EventGroup eventGroup, IEnumerable<string> groups)
        {
            Groups = groups;
            EventGroup = eventGroup;
        }

        private void EventGroup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EventGroup.Name)) SetSuggestedGroups();
        }

        private void SetSuggestedGroups()
        {
            SuggestedGroups = Groups.Filter(EventGroup?.Name?.ToLower());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
