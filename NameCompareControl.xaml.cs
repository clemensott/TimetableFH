using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace TimtableFH
{
    public sealed partial class NameCompareControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(NameCompare), typeof(NameCompareControl),
                new PropertyMetadata(null, new PropertyChangedCallback(OnValuePropertyChanged)));

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            NameCompareControl s = (NameCompareControl)sender;
            NameCompare oldValue = (NameCompare)e.OldValue;
            NameCompare newValue = (NameCompare)e.NewValue;

            if (oldValue != null) oldValue.PropertyChanged -= s.Value_PropertyChanged;
            if (newValue != null) newValue.PropertyChanged += s.Value_PropertyChanged;

            s.SetSuggetions();
        }

        public static readonly DependencyProperty NamesProperty =
            DependencyProperty.Register("Names", typeof(IEnumerable<string>), typeof(NameCompareControl),
                new PropertyMetadata(null, new PropertyChangedCallback(OnNamesPropertyChanged)));

        private static void OnNamesPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((NameCompareControl)sender).SetSuggetions();
        }

        public NameCompare Value
        {
            get { return (NameCompare)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public IEnumerable<string> Names
        {
            get { return (IEnumerable<string>)GetValue(NamesProperty); }
            set { SetValue(NamesProperty, value); }
        }

        public NameCompareControl()
        {
            this.InitializeComponent();
        }

        private void Value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Value.Name)) SetSuggetions();
        }

        private void SetSuggetions()
        {
            asbName.ItemsSource = Names.Filter(Value?.Name?.ToLower());
        }

        private void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((AutoSuggestBox)sender).IsSuggestionListOpen = true;
        }
    }
}
