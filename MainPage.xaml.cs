using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TimetableFH.AdmittedClasses;
using TimetableFH.Coloring;
using TimetableFH.Helpers;
using TimetableFH.Models;
using TimetableFH.Names;
using TimetableFH.Controls.Compare;
using Windows.UI.Xaml.Media;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace TimetableFH
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string dataFileName = "data.csv";

        private ViewModel viewModel;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = viewModel = (ViewModel)e.Parameter;
            viewModel.ApplySettings();

            base.OnNavigatedTo(e);

            try
            {
                IStorageItem item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(dataFileName);
                if (item is StorageFile) await SetEventsFromFile(item as StorageFile);
            }
            catch (Exception exc)
            {
                await new MessageDialog(exc.ToString(), "OnNavigatedTo").ShowAsync();
            }

            try
            {
                await DownloadCsv();
            }
            catch (WebException)
            {
            }
            catch (Exception exc)
            {
                await new MessageDialog(exc.ToString(), "AutoDownload").ShowAsync();
            }
        }

        private async Task SetEventsFromFile(StorageFile file)
        {
            viewModel.AllEvents = (await CsvParser.GetEvents(file)).ToArray();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataContext = null;

            base.OnNavigatedFrom(e);
        }

        private void AbbPreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            DaysOfWeek days = viewModel.Controller.ViewDays;
            if (days.GetDaysOfWeek().Count() == 1)
            {
                if (days.Contains(viewModel.Settings.RefTime.DayOfWeek))
                {
                    viewModel.Settings.RefTime = viewModel.Settings.RefTime.AddDays(-7);
                }

                if (days == DaysOfWeek.Monday) days = DaysOfWeek.Sunday;
                else days = (DaysOfWeek)((int)days >> 1);

                viewModel.Controller.ViewDays = days;
            }
            else viewModel.Settings.RefTime = viewModel.Settings.RefTime.AddDays(-7);
        }

        private void AbbThisWeek_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Settings.SetCurrentMondayMorning();
        }

        private void AbbNextWeek_Click(object sender, RoutedEventArgs e)
        {
            DaysOfWeek days = viewModel.Controller.ViewDays;
            if (days.GetDaysOfWeek().Count() == 1)
            {
                if (days == DaysOfWeek.Sunday) days = DaysOfWeek.Monday;
                else days = (DaysOfWeek)((int)days << 1);

                if (days.Contains(viewModel.Settings.RefTime.DayOfWeek))
                {
                    viewModel.Settings.RefTime = viewModel.Settings.RefTime.AddDays(7);
                }

                viewModel.Controller.ViewDays = days;
            }
            else viewModel.Settings.RefTime = viewModel.Settings.RefTime.AddDays(7);
        }

        private async void AbbDownloadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await DownloadCsv();

                await new MessageDialog("Updated data").ShowAsync();
            }
            catch (Exception exc)
            {
                await new MessageDialog(exc.ToString()).ShowAsync();
            }
        }

        private async Task DownloadCsv()
        {
            const string urlAdditionFormat = "?new_stg={0}&new_jg={1}&new_date=1569830400&new_viewmode=matrix_vertical";
            const string postDataFormat = "user={0}&pass={0}&login=Login&spanne_start=01.08.{1}&spanne_end=01.11.{2}&write_spanne=Von-Bis-Datum";

            string baseUrl, urlAddition, postData;
            if (!viewModel.Settings.UseCustomOptions)
            {
                if (string.IsNullOrWhiteSpace(viewModel.Settings.MajorShortName) || viewModel.Settings.BeginYear == 0) return;

                string majorShortName = viewModel.Settings.MajorShortName;
                uint beginYear = viewModel.Settings.BeginYear;

                baseUrl = Settings.BaseFhUrl;
                urlAddition = string.Format(urlAdditionFormat, majorShortName?.ToUpper(), beginYear);
                postData = string.Format(postDataFormat, majorShortName?.ToLower(), beginYear, DateTime.Now.Year + 1);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(viewModel.Settings.CustomBaseUrl) ||
                    string.IsNullOrWhiteSpace(viewModel.Settings.CustomRequestUrlAddition)) return;

                baseUrl = viewModel.Settings.CustomBaseUrl;
                urlAddition = viewModel.Settings.CustomRequestUrlAddition;
                postData = viewModel.Settings.CustomPostDataPairs.ToPostData();
            }
            StorageFile srcFile = await EventRequester.DownloadCsv(baseUrl, urlAddition, postData);

            await SetEventsFromFile(srcFile);

            StorageFile destFile = await ApplicationData.Current.LocalFolder
                .CreateFileAsync(dataFileName, CreationCollisionOption.OpenIfExists);
            await srcFile.MoveAndReplaceAsync(destFile);
        }

        private async void AbbOpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileOpenPicker picker = new FileOpenPicker()
                {
                    SuggestedStartLocation = PickerLocationId.Downloads,
                    ViewMode = PickerViewMode.List
                };

                picker.FileTypeFilter.Add(".csv");

                StorageFile srcFile = await picker.PickSingleFileAsync();
                await SetEventsFromFile(srcFile);

                StorageFile destFile = await ApplicationData.Current.LocalFolder
                    .CreateFileAsync(dataFileName, CreationCollisionOption.OpenIfExists);
                await srcFile.CopyAndReplaceAsync(destFile);
            }
            catch (Exception exc)
            {
                await new MessageDialog(exc.ToString(), "Open_File").ShowAsync();
            }
        }

        private void AbbSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage), viewModel);
        }

        private void AbbMoreTime_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan newDuration = viewModel.Settings.ViewDuration.Add(TimeSpan.FromHours(1));
            DateTime refTime = viewModel.Settings.RefTime;

            if (refTime.Add(newDuration).Date == refTime.Date) viewModel.Settings.ViewDuration = newDuration;
        }

        private void AbbLessTime_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan newDuration = viewModel.Settings.ViewDuration.Add(TimeSpan.FromHours(-1));

            if (newDuration > TimeSpan.Zero) viewModel.Settings.ViewDuration = newDuration;
        }

        private void AbbRefUp_Click(object sender, RoutedEventArgs e)
        {
            DateTime newRef = viewModel.Settings.RefTime.AddMinutes(-15);

            if (newRef.Date == viewModel.Settings.RefTime.Date) viewModel.Settings.RefTime = newRef;
        }

        private void AbbRefDown_Click(object sender, RoutedEventArgs e)
        {
            DateTime newRef = viewModel.Settings.RefTime.AddMinutes(15);
            TimeSpan duration = viewModel.Settings.ViewDuration;

            if (newRef.Add(duration).Date == viewModel.Settings.RefTime.Date) viewModel.Settings.RefTime = newRef;
        }

        private void EventView_DayHeaderClick(object sender, DateTime e)
        {
            Settings settings = viewModel.Settings;
            if (viewModel.Controller.ViewDays == settings?.DaysOfWeek)
            {
                viewModel.Controller.ViewDays = e.DayOfWeek.ToDaysOfWeek();
            }
            else if (settings != null)
            {
                viewModel.Controller.ViewDays = settings.DaysOfWeek;
            }
        }

        private void EventsView_SetColorClick(object sender, Event e)
        {
            foreach (EventColor eventColor in viewModel.Settings.EventColors.Collection)
            {
                if (!Utils.IsEvent(eventColor, e.Group, e.Name)) continue;

                EventColorEditPageViewModel eventColorViewModel =
                    new EventColorEditPageViewModel(eventColor, viewModel.GetAllGroups(), viewModel.GetAllNames());

                Frame.Navigate(typeof(EventColorEditPage), eventColorViewModel);
                return;
            }

            EventColor newEventColor = new EventColor()
            {
                NameCompareType = CompareType.StartsWith,
                Name = e.Name,
            };
            EventColorEditPageViewModel newEventColorViewModel =
                new EventColorEditPageViewModel(newEventColor, viewModel.GetAllGroups(), viewModel.GetAllNames());

            viewModel.Settings.EventColors.Collection.Add(newEventColor);
            Frame.Navigate(typeof(EventColorEditPage), newEventColorViewModel);
        }

        private void EventsView_SetNameClick(object sender, Event e)
        {
            foreach (EventName eventName in viewModel.Settings.EventNames)
            {
                if (!Utils.IsEvent(eventName, e.Name)) continue;

                EventNameEditPageViewModel eventNameViewModel = new EventNameEditPageViewModel(eventName, viewModel.GetAllNames());

                viewModel.Settings.EventNames.Add(eventName);
                Frame.Navigate(typeof(EventNameEditPage), eventNameViewModel);
                return;
            }

            EventName newEventName = new EventName()
            {
                CompareType = CompareType.StartsWith,
                Reference = e.Name,
            };
            EventNameEditPageViewModel newEventNameViewModel =
                new EventNameEditPageViewModel(newEventName, viewModel.GetAllNames());

            viewModel.Settings.EventNames.Add(newEventName);
            Frame.Navigate(typeof(EventNameEditPage), newEventNameViewModel);
        }

        private void EventsView_AddAdmittedClick(object sender, Event e)
        {
            foreach (NameCompare admittedClass in viewModel.Settings.AdmittedClasses)
            {
                if (Utils.Compare(admittedClass.CompareType,
                    e.Name, admittedClass.Name)) return;
            }

            NameCompare newAdmittedClass = new NameCompare()
            {
                CompareType = CompareType.StartsWith,
                Name = e.Name,
            };

            viewModel.Settings.AdmittedClasses.Add(newAdmittedClass);
        }
    }
}
