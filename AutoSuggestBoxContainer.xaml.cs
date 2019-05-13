using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace TimetableFH
{
    public sealed partial class AutoSuggestBoxContainer : UserControl
    {
        private static DateTime lastLostFocusTime;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
            typeof(string), typeof(AutoSuggestBoxContainer), new PropertyMetadata(default(string)));


        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
            typeof(object), typeof(AutoSuggestBoxContainer), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty FocusLockMillisProperty =
            DependencyProperty.Register("FocusLockMillis", typeof(int),
                typeof(AutoSuggestBoxContainer), new PropertyMetadata(default(int)));


        public static readonly DependencyProperty LastLostFocusControlProperty =
            DependencyProperty.Register("LastLostFocusControl", typeof(AutoSuggestBox),
                typeof(AutoSuggestBoxContainer), new PropertyMetadata(default(AutoSuggestBox)));


        public static readonly DependencyProperty LastLostFocusTimeProperty =
            DependencyProperty.Register("LastLostFocusTime", typeof(DateTime),
                typeof(AutoSuggestBoxContainer), new PropertyMetadata(default(DateTime)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public int FocusLockMillis
        {
            get => (int)GetValue(FocusLockMillisProperty);
            set => SetValue(FocusLockMillisProperty, value);
        }

        public AutoSuggestBox LastLostFocusControl
        {
            get => (AutoSuggestBox)GetValue(LastLostFocusControlProperty);
            set => SetValue(LastLostFocusControlProperty, value);
        }

        public DateTime LastLostFocusTime
        {
            get => (DateTime)GetValue(LastLostFocusTimeProperty);
            set => SetValue(LastLostFocusTimeProperty, value);
        }


        public AutoSuggestBoxContainer()
        {
            this.InitializeComponent();
        }

        private void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("GotFocus: {0}; Equals: {1}; {2} ms; Time passed: {3}",
                Name, ReferenceEquals(sender, LastLostFocusControl), DateTime.Now.Millisecond, DateTime.Now > GetMinOpenTime());

            if (!ReferenceEquals(sender, LastLostFocusControl) || DateTime.Now > GetMinOpenTime())
            {
                ((AutoSuggestBox)sender).IsSuggestionListOpen = true;
            }
        }

        private void AutoSuggestBox_LostFocus(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("LostFocus: {0}; {1} ms", Name, DateTime.Now.Millisecond);

            LastLostFocusControl = sender as AutoSuggestBox;
            LastLostFocusTime= lastLostFocusTime = DateTime.Now;
        }

        private DateTime GetMinOpenTime()
        {
            return lastLostFocusTime + TimeSpan.FromMilliseconds(FocusLockMillis);
            return LastLostFocusTime + TimeSpan.FromMilliseconds(FocusLockMillis);
        }
    }
}
