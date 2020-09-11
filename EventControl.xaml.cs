using StdOttStandard.Converter.MultipleInputs;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace TimetableFH
{
    public sealed partial class EventControl : UserControl
    {
        private const double hoverOpacity = 0.15;

        public EventControl()
        {
            this.InitializeComponent();
        }

        private object MulTime_Convert(object sender, MultiplesInputsConvert2EventArgs args)
        {
            DateTime begin = ((DateTime?)args.Input0).GetValueOrDefault();
            DateTime end = ((DateTime?)args.Input1).GetValueOrDefault();

            return string.Format("{0:00}:{1:00} - {2:00}:{3:00}", begin.TimeOfDay.Hours,
                begin.TimeOfDay.Minutes, end.TimeOfDay.Hours, end.TimeOfDay.Minutes);
        }

        protected override async void OnTapped(TappedRoutedEventArgs e)
        {
            Event fhEvent = (Event)DataContext;
            string date = fhEvent.Begin.GetDateTimeFormats()[0];
            string time = tblTime.Text;

            string message = date + " " + time + "\r\n" + fhEvent.Name + "\r\n" +
                fhEvent.Professor + "\r\n" + fhEvent.Room + "\r\n" + fhEvent.Group;

            await new MessageDialog(message).ShowAsync();
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            rectChangeColor.Opacity = hoverOpacity;
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            rectChangeColor.Opacity = 0;
        }

        private void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            rectChangeColor.Opacity = 0;
        }
    }
}
