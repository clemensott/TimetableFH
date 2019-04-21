using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace TimtableFH
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string settingsFileName = "settings.xml", dataFileName = "data.csv";

        private ViewModel viewModel;

        public MainPage()
        {
            this.InitializeComponent();

            DataContext = viewModel = new ViewModel();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = viewModel = await ViewModel.Load(settingsFileName);

            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(dataFileName);

                await SetEventsFromFile(file);
            }
            catch (Exception exc)
            {
                await new MessageDialog(exc.ToString(), "Loaded_Page").ShowAsync();
            }

            Application.Current.EnteredBackground += Application_EnteredBackground;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.EnteredBackground -= Application_EnteredBackground;
        }

        private async void Application_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            await viewModel.Save(settingsFileName);
        }

        private async Task SetEventsFromFile(StorageFile file)
        {
            viewModel.AllEvents = (await CsvParser.GetEvents(file)).ToArray();
        }

        private void AbbPreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.ViewDaysCount == 1) viewModel.RefTime = viewModel.RefTime.AddDays(-1);
            else viewModel.RefTime = viewModel.RefTime.AddDays(-7);
        }

        private void AbbThisWeek_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RefTime = ViewModel.GetLastMondayMorning();
        }

        private void AbbNextWeek_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.ViewDaysCount == 1) viewModel.RefTime = viewModel.RefTime.AddDays(1);
            else viewModel.RefTime = viewModel.RefTime.AddDays(7);
        }

        private async void AbbDownloadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFile srcFile = await EventRequestor.DownloadCsv();
                await SetEventsFromFile(srcFile);

                StorageFile destFile = await GetOrCreateFileAsync(ApplicationData.Current.LocalFolder, dataFileName);
                await srcFile.MoveAndReplaceAsync(destFile);

                await new MessageDialog("Updated data").ShowAsync();
            }
            catch (Exception exc)
            {
                await new MessageDialog(exc.ToString()).ShowAsync();
            }
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

                StorageFile destFile = await GetOrCreateFileAsync(ApplicationData.Current.LocalFolder, dataFileName);
                await srcFile.CopyAndReplaceAsync(destFile);
            }
            catch (Exception exc)
            {
                await new MessageDialog(exc.ToString(), "Open_File").ShowAsync();
            }
        }

        private async Task<StorageFile> GetOrCreateFileAsync(StorageFolder folder, string fileName)
        {
            try
            {
                return await folder.GetFileAsync(fileName);
            }
            catch { }

            return await folder.CreateFileAsync(fileName);
        }

        private void AbbMoreTime_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan newDuration = viewModel.ViewDuration.Add(TimeSpan.FromHours(1));
            DateTime refTime = viewModel.RefTime;

            if (refTime.Add(newDuration).Date == refTime.Date) viewModel.ViewDuration = newDuration;
        }

        private void AbbLessTime_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan newDuration = viewModel.ViewDuration.Add(TimeSpan.FromHours(-1));

            if (newDuration > TimeSpan.Zero) viewModel.ViewDuration = newDuration;
        }

        private void AbbRefUp_Click(object sender, RoutedEventArgs e)
        {
            DateTime newRef = viewModel.RefTime.AddMinutes(-15);

            if (newRef.Date == viewModel.RefTime.Date) viewModel.RefTime = newRef;
        }

        private void AbbRefDown_Click(object sender, RoutedEventArgs e)
        {
            DateTime newRef = viewModel.RefTime.AddMinutes(15);
            TimeSpan duration = viewModel.ViewDuration;

            if (newRef.Add(duration).Date == viewModel.RefTime.Date) viewModel.RefTime = newRef;
        }
    }
}
