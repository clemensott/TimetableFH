using StdOttStandard;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TimetableFH
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ColorPickerPage : Page
    {
        private SetableValue<Color?> setableValue;

        public ColorPickerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            (Color color, SetableValue<Color?> value) = ((Color, SetableValue<Color?>))e.Parameter;

            setableValue = value;
            colorPicker.SelectedColor = color;

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
