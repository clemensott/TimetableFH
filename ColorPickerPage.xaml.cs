using StdOttStandard.AsyncResult;
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
    public sealed partial class ColorPickerPage : Page
    {
        private AsyncResult<Color?, Color> setableValue;

        public ColorPickerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            setableValue = (AsyncResult<Color?, Color>)e.Parameter;

            colorPicker.SelectedColor = setableValue.Input;

            base.OnNavigatedTo(e);
        }

        private void AbbAccept_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setableValue.SetValue(colorPicker.SelectedColor);
            Frame.GoBack();
        }

        private void AbbBack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setableValue.SetValue(null);
            Frame.GoBack();
        }
    }
}
