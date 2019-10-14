using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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

            base.OnNavigatedTo(e);

            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(dataFileName);

                await SetEventsFromFile(file);
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

        private void AbbPreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Settings.RefTime = viewModel.Settings.RefTime.AddDays(viewModel.Settings.IsSingleDay ? -1 : -7);
        }

        private void AbbThisWeek_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Settings.SetCurrentMondayMorning();
        }

        private void AbbNextWeek_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Settings.RefTime = viewModel.Settings.RefTime.AddDays(viewModel.Settings.IsSingleDay ? 1 : 7);
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
            string baseUrl = viewModel.Settings.BaseUrl;
            string urlAddition = viewModel.Settings.RequestUrlAddition;
            string postData = viewModel.Settings.PostDataPairs.ToPostData();
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

        private async void AbbExportSettings_Click(object sender, RoutedEventArgs e)
        {
            const string fileName = "TimetableFhSettings.xml";
            const CreationCollisionOption option = CreationCollisionOption.GenerateUniqueName;

            try
            {
                StorageFolder folder = KnownFolders.DocumentsLibrary;
                IAsyncOperation<StorageFile> fileOperation = folder.CreateFileAsync(fileName, option);

                await viewModel.Settings.Save(fileOperation);
            }
            catch (Exception exc)
            {
                await new MessageDialog(exc.ToString(), "ExportSettings").ShowAsync();
            }
        }

        private async void AbbImportSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileOpenPicker picker = new FileOpenPicker()
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    ViewMode = PickerViewMode.List
                };

                picker.FileTypeFilter.Add(".xml");

                StorageFile srcFile = await picker.PickSingleFileAsync();

                viewModel.Settings = await ViewModelUtils.Load(srcFile);
            }
            catch (Exception exc)
            {
                await new MessageDialog(exc.ToString(), "ImportSettings").ShowAsync();
            }
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
    }
}
