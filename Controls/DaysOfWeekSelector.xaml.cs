using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TimetableFH.Models;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace TimetableFH.Controls
{
    public sealed partial class DaysOfWeekSelector : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(DaysOfWeek), typeof(DaysOfWeekSelector),
                new PropertyMetadata(default(DaysOfWeek), OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DaysOfWeekSelector s = (DaysOfWeekSelector)sender;

            s.UpdateCheckBoxes();
        }

        public DaysOfWeek Value
        {
            get => (DaysOfWeek)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public DaysOfWeekSelector()
        {
            this.InitializeComponent();
        }

        private void UpdateCheckBoxes()
        {
            cbxMonday.IsChecked = (Value & DaysOfWeek.Monday) > 0;
            cbxTuesday.IsChecked = (Value & DaysOfWeek.Tuesday) > 0;
            cbxWednesday.IsChecked = (Value & DaysOfWeek.Wednesday) > 0;
            cbxThursday.IsChecked = (Value & DaysOfWeek.Thursday) > 0;
            cbxFriday.IsChecked = (Value & DaysOfWeek.Friday) > 0;
            cbxSaturday.IsChecked = (Value & DaysOfWeek.Saturday) > 0;
            cbxSunday.IsChecked = (Value & DaysOfWeek.Sunday) > 0;
        }

        private void CbxMonday_Checked(object sender, RoutedEventArgs e)
        {
            Value |= DaysOfWeek.Monday;
        }

        private void CbxMonday_Unchecked(object sender, RoutedEventArgs e)
        {
            Value -= DaysOfWeek.Monday;
        }

        private void CbxTuesday_Checked(object sender, RoutedEventArgs e)
        {
            Value |= DaysOfWeek.Tuesday;
        }

        private void CbxTuesday_Unchecked(object sender, RoutedEventArgs e)
        {
            Value -= DaysOfWeek.Tuesday;
        }

        private void CbxWednesday_Checked(object sender, RoutedEventArgs e)
        {
            Value |= DaysOfWeek.Wednesday;
        }

        private void CbxWednesday_Unchecked(object sender, RoutedEventArgs e)
        {
            Value -= DaysOfWeek.Wednesday;
        }

        private void CbxThursday_Checked(object sender, RoutedEventArgs e)
        {
            Value |= DaysOfWeek.Thursday;
        }

        private void CbxThursday_Unchecked(object sender, RoutedEventArgs e)
        {
            Value -= DaysOfWeek.Thursday;
        }

        private void CbxFriday_Checked(object sender, RoutedEventArgs e)
        {
            Value |= DaysOfWeek.Friday;
        }

        private void CbxFriday_Unchecked(object sender, RoutedEventArgs e)
        {
            Value -= DaysOfWeek.Friday;
        }

        private void CbxSaturday_Checked(object sender, RoutedEventArgs e)
        {
            Value |= DaysOfWeek.Saturday;
        }

        private void CbxSaturday_Unchecked(object sender, RoutedEventArgs e)
        {
            Value -= DaysOfWeek.Saturday;
        }

        private void CbxSunday_Checked(object sender, RoutedEventArgs e)
        {
            Value |= DaysOfWeek.Sunday;
        }

        private void CbxSunday_Unchecked(object sender, RoutedEventArgs e)
        {
            Value -= DaysOfWeek.Sunday;
        }
    }
}
