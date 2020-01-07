using System.Collections.ObjectModel;
using StdOttStandard.AsyncResult;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using StdOttStandard;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TimetableFH
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private ViewModel viewModel;

        public SettingsPage()
        {
            Resources.Add("ignore", CompareType.Ignore);

            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = viewModel = (ViewModel)e.Parameter;

            base.OnNavigatedTo(e);
        }

        private void IbnAddClasses_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Settings.NotAdmittedClasses.Add(new NameCompare());
        }

        private void IbnRemoveClass_Click(object sender, RoutedEventArgs e)
        {
            NameCompare nameCompare = (NameCompare)((FrameworkElement)sender).DataContext;

            viewModel.Settings.NotAdmittedClasses.Remove(nameCompare);
        }

        private void IbnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            EventGroup eventGroup = new EventGroup();
            EventGroupEditPageViewModel eventGroupViewModel = new EventGroupEditPageViewModel(eventGroup, viewModel.GetAllGroups());

            viewModel.Settings.Groups.Collection.Add(eventGroup);

            Frame.Navigate(typeof(EventGroupEditPage), eventGroupViewModel);
        }

        private void EleGroup_EditClick(object sender, RoutedEventArgs e)
        {
            EventGroup eventGroup = (EventGroup)((FrameworkElement)sender).DataContext;
            EventGroupEditPageViewModel eventGroupViewModel = new EventGroupEditPageViewModel(eventGroup, viewModel.GetAllGroups());

            Frame.Navigate(typeof(EventGroupEditPage), eventGroupViewModel);
        }

        private void EleGroup_RemoveClick(object sender, RoutedEventArgs e)
        {
            EventGroup eventGroup = (EventGroup)((FrameworkElement)sender).DataContext;

            viewModel.Settings.Groups.Collection.Remove(eventGroup);
        }

        private void EleGroup_UpClick(object sender, RoutedEventArgs e)
        {
            EventGroup eventGroup = (EventGroup)((FrameworkElement)sender).DataContext;

            Move(viewModel.Settings.Groups.Collection, eventGroup, -1);
        }

        private void EleGroup_DownClick(object sender, RoutedEventArgs e)
        {
            EventGroup eventGroup = (EventGroup)((FrameworkElement)sender).DataContext;

            Move(viewModel.Settings.Groups.Collection, eventGroup, 1);
        }

        private void BtnAddColor_Click(object sender, RoutedEventArgs e)
        {
            EventColor eventColor = new EventColor();
            EventColorEditPageViewModel eventColorViewModel =
                new EventColorEditPageViewModel(eventColor, viewModel.GetAllGroups(), viewModel.GetAllNames());

            viewModel.Settings.EventColors.Collection.Add(eventColor);

            Frame.Navigate(typeof(EventColorEditPage), eventColorViewModel);
        }

        private void EleColor_EditClick(object sender, RoutedEventArgs e)
        {
            EventColor eventColor = (EventColor)((FrameworkElement)sender).DataContext;
            EventColorEditPageViewModel eventColorViewModel =
                new EventColorEditPageViewModel(eventColor, viewModel.GetAllGroups(), viewModel.GetAllNames());

            Frame.Navigate(typeof(EventColorEditPage), eventColorViewModel);
        }

        private void EleColor_RemoveClick(object sender, RoutedEventArgs e)
        {
            EventColor eventColor = (EventColor)((FrameworkElement)sender).DataContext;

            viewModel.Settings.EventColors.Collection.Remove(eventColor);
        }

        private void EleColor_UpClick(object sender, RoutedEventArgs e)
        {
            EventColor eventGroup = (EventColor)((FrameworkElement)sender).DataContext;

            Move(viewModel.Settings.EventColors.Collection, eventGroup, -1);
        }

        private void EleColor_DownClick(object sender, RoutedEventArgs e)
        {
            EventColor eventGroup = (EventColor)((FrameworkElement)sender).DataContext;

            Move(viewModel.Settings.EventColors.Collection, eventGroup, 1);
        }

        private async void RectChangeColor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AsyncResult<Color?, Color> setableValue =
                new AsyncResult<Color?, Color>(viewModel.Settings.EventColors.DefaultColor);

            Frame.Navigate(typeof(ColorPickerPage), setableValue);

            Color? color = await setableValue.Task;

            if (color.HasValue) viewModel.Settings.EventColors.DefaultColor = color.Value;
        }

        private void IbnAddName_Click(object sender, RoutedEventArgs e)
        {
            EventName eventName = new EventName();
            EventNameEditPageViewModel eventNameViewModel = new EventNameEditPageViewModel(eventName, viewModel.GetAllNames());

            viewModel.Settings.EventNames.Add(eventName);

            Frame.Navigate(typeof(EventNameEditPage), eventNameViewModel);
        }

        private void EleName_EditClick(object sender, RoutedEventArgs e)
        {
            EventName eventName = (EventName)((FrameworkElement)sender).DataContext;
            EventNameEditPageViewModel eventNameViewModel = new EventNameEditPageViewModel(eventName, viewModel.GetAllNames());

            Frame.Navigate(typeof(EventNameEditPage), eventNameViewModel);
        }

        private void EleName_RemoveClick(object sender, RoutedEventArgs e)
        {
            EventName eventName = (EventName)((FrameworkElement)sender).DataContext;

            viewModel.Settings.EventNames.Remove(eventName);
        }

        private void EleName_UpClick(object sender, RoutedEventArgs e)
        {
            EventName eventGroup = (EventName)((FrameworkElement)sender).DataContext;

            Move(viewModel.Settings.EventNames, eventGroup, -1);
        }

        private void EleName_DownClick(object sender, RoutedEventArgs e)
        {
            EventName eventGroup = (EventName)((FrameworkElement)sender).DataContext;

            Move(viewModel.Settings.EventNames, eventGroup, 1);
        }

        private void IbnAddRoom_Click(object sender, RoutedEventArgs e)
        {
            ReplaceValue replaceValue = new ReplaceValue();
            ReplaceValueEditPageViewModel replaceValueViewModel =
                new ReplaceValueEditPageViewModel(replaceValue, viewModel.Settings.Rooms.Examples);

            viewModel.Settings.Rooms.Add(replaceValue);

            Frame.Navigate(typeof(ReplaceValueEditPage), replaceValueViewModel);
        }

        private void IbnRemovePostData_Click(object sender, RoutedEventArgs e)
        {
            StringKeyValuePair pair = (StringKeyValuePair)((FrameworkElement)sender).DataContext;

            viewModel.Settings.PostDataPairs.Remove(pair);
        }

        private void IbnAddPostData_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Settings.PostDataPairs.Add(new StringKeyValuePair());
        }

        private void EleRoom_EditClick(object sender, RoutedEventArgs e)
        {
            ReplaceValue replaceValue = (ReplaceValue)((FrameworkElement)sender).DataContext;
            ReplaceValueEditPageViewModel replaceValueViewModel =
                new ReplaceValueEditPageViewModel(replaceValue, viewModel.Settings.Rooms.Examples);

            Frame.Navigate(typeof(ReplaceValueEditPage), replaceValueViewModel);
        }

        private void EleRoom_RemoveClick(object sender, RoutedEventArgs e)
        {
            ReplaceValue replaceValue = (ReplaceValue)((FrameworkElement)sender).DataContext;

            viewModel.Settings.Rooms.Remove(replaceValue);
        }

        private void EleRoom_UpClick(object sender, RoutedEventArgs e)
        {
            ReplaceValue replaceValue = (ReplaceValue)((FrameworkElement)sender).DataContext;

            Move(viewModel.Settings.Rooms, replaceValue, -1);
        }

        private void EleRoom_DownClick(object sender, RoutedEventArgs e)
        {
            ReplaceValue replaceValue = (ReplaceValue)((FrameworkElement)sender).DataContext;

            Move(viewModel.Settings.Rooms, replaceValue, 1);
        }

        private void AbbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private static void Move<T>(ObservableCollection<T> collection, T item, int offset)
        {
            int oldIndex = collection.IndexOf(item);

            if (oldIndex == -1) return;

            int newIndex = Utils.OffsetIndex(oldIndex, collection.Count, offset).index;

            collection.Move(oldIndex, newIndex);
        }

        private void Lvw_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer;
            if (!TryFindScrollView((DependencyObject) sender, out scrollViewer)) return;

            scrollViewer.VerticalScrollMode = ScrollMode.Disabled;
            scrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
        }

        private static bool TryFindScrollView(DependencyObject db, out ScrollViewer sv)
        {
            if (db is ScrollViewer)
            {
                sv = (ScrollViewer)db;
                return true;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(db); i++)
            {
                if (TryFindScrollView(VisualTreeHelper.GetChild(db, i), out sv)) return true;
            }

            sv = null;
            return false;
        }
    }
}
