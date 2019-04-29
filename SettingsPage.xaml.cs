using StdOttStandard;
using Windows.UI;
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

        private void BtnAddClasses_Tapped(object sender, TappedRoutedEventArgs e)
        {
            viewModel.NotAdmittedClasses.Add(new NameCompare());
        }

        private void SinRemoveClass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NameCompare nameCompare = (NameCompare)((FrameworkElement)sender).DataContext;

            viewModel.NotAdmittedClasses.Remove(nameCompare);
        }

        private void BtnAddGroup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EventGroup eventGroup = new EventGroup();
            EventGroupEditPageViewModel eventGroupViewModel = new EventGroupEditPageViewModel(eventGroup, viewModel.GetAllGroups());

            viewModel.Groups.Collection.Add(eventGroup);

            Frame.Navigate(typeof(EventGroupEditPage), eventGroupViewModel);
        }

        private void SinEditGroup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EventGroup eventGroup = (EventGroup)((FrameworkElement)sender).DataContext;
            EventGroupEditPageViewModel eventGroupViewModel = new EventGroupEditPageViewModel(eventGroup, viewModel.GetAllGroups());

            Frame.Navigate(typeof(EventGroupEditPage), eventGroupViewModel);
        }

        private void SinRemoveGroup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EventGroup eventGroup = (EventGroup)((FrameworkElement)sender).DataContext;

            viewModel.Groups.Collection.Remove(eventGroup);
        }

        private void BtnAddColor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EventColor eventColor = new EventColor();
            EventColorEditPageViewModel eventColorViewModel = new EventColorEditPageViewModel(eventColor, viewModel.GetAllGroups(), viewModel.GetAllNames());

            viewModel.EventColors.Collection.Add(eventColor);

            Frame.Navigate(typeof(EventColorEditPage), eventColorViewModel);
        }

        private void SinEditColor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EventColor eventColor = (EventColor)((FrameworkElement)sender).DataContext;
            EventColorEditPageViewModel eventColorViewModel = new EventColorEditPageViewModel(eventColor, viewModel.GetAllGroups(), viewModel.GetAllNames());

            Frame.Navigate(typeof(EventColorEditPage), eventColorViewModel);
        }

        private void SinRemoveColor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EventColor eventColor = (EventColor)((FrameworkElement)sender).DataContext;

            viewModel.EventColors.Collection.Remove(eventColor);
        }

        private async void BtnChangeColor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetableValue<Color?> setableValue = new SetableValue<Color?>();

            Frame.Navigate(typeof(ColorPickerPage), (viewModel.EventColors.DefaultColor, setableValue));

            Color? color = await setableValue.Task;

            if (color.HasValue) viewModel.EventColors.DefaultColor = color.Value;
        }

        private void BtnAddName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EventName eventName = new EventName();
            EventNameEditPageViewModel eventNameViewModel = new EventNameEditPageViewModel(eventName, viewModel.GetAllNames());

            viewModel.EventNames.Add(eventName);

            Frame.Navigate(typeof(EventNameEditPage), eventNameViewModel);
        }

        private void SinEditName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EventName eventName = (EventName)((FrameworkElement)sender).DataContext;
            EventNameEditPageViewModel eventNameViewModel = new EventNameEditPageViewModel(eventName, viewModel.GetAllNames());

            Frame.Navigate(typeof(EventNameEditPage), eventNameViewModel);
        }

        private void SinRemoveName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EventName eventName = (EventName)((FrameworkElement)sender).DataContext;

            viewModel.EventNames.Remove(eventName);
        }

        private void BtnAddReplace_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ReplaceValue replaceValue = new ReplaceValue();
            ReplaceValueEditPageViewModel replaceValueViewModel = new ReplaceValueEditPageViewModel(replaceValue, viewModel.ReplaceValues.Examples);

            viewModel.ReplaceValues.Add(replaceValue);

            Frame.Navigate(typeof(ReplaceValueEditPage), replaceValueViewModel);
        }

        private void SinEditReplace_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ReplaceValue replaceValue = (ReplaceValue)((FrameworkElement)sender).DataContext;
            ReplaceValueEditPageViewModel replaceValueViewModel = new ReplaceValueEditPageViewModel(replaceValue, viewModel.ReplaceValues.Examples);

            Frame.Navigate(typeof(ReplaceValueEditPage), replaceValueViewModel);
        }

        private void SinRemoveReplace_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ReplaceValue replaceValue = (ReplaceValue)((FrameworkElement)sender).DataContext;

            viewModel.ReplaceValues.Remove(replaceValue);
        }

        private void AbbBack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void AbbDefault_Tapped(object sender, TappedRoutedEventArgs e)
        {
            viewModel.EventColors.Collection.Add(new EventColor()
            {
                Group = "PA",
                GroupCompareType = CompareType.Equals,
                Name = "",
                NameCompareType = CompareType.Ignore,
                Color = Colors.Pink
            });

            viewModel.EventColors.Collection.Add(new EventColor()
            {
                Group = "PAB",
                GroupCompareType = CompareType.Equals,
                Name = "",
                NameCompareType = CompareType.Ignore,
                Color = Colors.Pink
            });

            viewModel.EventColors.Collection.Add(new EventColor()
            {
                Group = "PAM",
                GroupCompareType = CompareType.Equals,
                Name = "",
                NameCompareType = CompareType.Ignore,
                Color = Colors.Pink
            });

            viewModel.EventColors.Collection.Add(new EventColor()
            {
                Group = "",
                GroupCompareType = CompareType.Ignore,
                Name = "DATSTR",
                NameCompareType = CompareType.StartsWith,
                Color = Colors.Orange
            });

            viewModel.EventColors.Collection.Add(new EventColor()
            {
                Group = "",
                GroupCompareType = CompareType.Ignore,
                Name = "DBDESIGN",
                NameCompareType = CompareType.StartsWith,
                Color = Colors.Aqua
            });

            viewModel.EventColors.Collection.Add(new EventColor()
            {
                Group = "",
                GroupCompareType = CompareType.Ignore,
                Name = "KONFIG",
                NameCompareType = CompareType.StartsWith,
                Color = Colors.LightBlue
            });

            viewModel.EventColors.Collection.Add(new EventColor()
            {
                Group = "",
                GroupCompareType = CompareType.Ignore,
                Name = "MATH2",
                NameCompareType = CompareType.StartsWith,
                Color = Colors.RosyBrown
            });

            viewModel.EventColors.Collection.Add(new EventColor()
            {
                Group = "",
                GroupCompareType = CompareType.Ignore,
                Name = "PMuAR",
                NameCompareType = CompareType.StartsWith,
                Color = Colors.LightGreen
            });

            viewModel.EventColors.Collection.Add(new EventColor()
            {
                Group = "",
                GroupCompareType = CompareType.Ignore,
                Name = "PROFENG",
                NameCompareType = CompareType.StartsWith,
                Color = Colors.Yellow
            });

            viewModel.EventColors.Collection.Add(new EventColor()
            {
                Group = "",
                GroupCompareType = CompareType.Ignore,
                Name = "PROG2",
                NameCompareType = CompareType.StartsWith,
                Color = Colors.YellowGreen
            });

            viewModel.EventColors.Collection.Add(new EventColor()
            {
                Group = "",
                GroupCompareType = CompareType.Ignore,
                Name = "WEBTECH",
                NameCompareType = CompareType.StartsWith,
                Color = Colors.LightSeaGreen
            });


            viewModel.EventNames.Add(new EventName()
            {
                Reference = "DATSTR",
                CompareType = CompareType.StartsWith,
                Short = "D&A"
            });

            viewModel.EventNames.Add(new EventName()
            {
                Reference = "DBDESIGN",
                CompareType = CompareType.StartsWith,
                Short = "DB"
            });

            viewModel.EventNames.Add(new EventName()
            {
                Reference = "KONFIG",
                CompareType = CompareType.StartsWith,
                Short = "Konf"
            });

            viewModel.EventNames.Add(new EventName()
            {
                Reference = "MATH2",
                CompareType = CompareType.StartsWith,
                Short = "Mathe"
            });

            viewModel.EventNames.Add(new EventName()
            {
                Reference = "PMuAR",
                CompareType = CompareType.StartsWith,
                Short = "WIR"
            });

            viewModel.EventNames.Add(new EventName()
            {
                Reference = "PROFENG",
                CompareType = CompareType.StartsWith,
                Short = "English"
            });

            viewModel.EventNames.Add(new EventName()
            {
                Reference = "PROG2",
                CompareType = CompareType.StartsWith,
                Short = "SwDev"
            });

            viewModel.EventNames.Add(new EventName()
            {
                Reference = "WEBTECH",
                CompareType = CompareType.StartsWith,
                Short = "Web"
            });


            viewModel.ReplaceValues.Add(new ReplaceValue()
            {
                Reference = "Heidemarie K??llinger",
                Replacement = "Heidemarie Köllinger"
            });

            viewModel.ReplaceValues.Add(new ReplaceValue()
            {
                Reference = "H??rsaal",
                Replacement = "Hörsaal"
            });

            viewModel.ReplaceValues.Add(new ReplaceValue()
            {
                Reference = "M??sl??m Atas",
                Replacement = "Müslüm Atas"
            });

            viewModel.ReplaceValues.Add(new ReplaceValue()
            {
                Reference = "Stefan Gr??nwald",
                Replacement = "Stefan Grünwald"
            });

            viewModel.ReplaceValues.Add(new ReplaceValue()
            {
                Reference = "Einf??hrung, Hausf??hrung",
                Replacement = "Einführung, Hausführung"
            });

            viewModel.ReplaceValues.Add(new ReplaceValue()
            {
                Reference = "??KOGL",
                Replacement = "ÖKOGL"
            });
        }
    }
}
