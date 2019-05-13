using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace TimetableFH
{
    public sealed partial class CompareTypePicker : UserControl
    {
        public static readonly DependencyProperty CompareTypeProperty =
            DependencyProperty.Register("CompareType", typeof(CompareType), typeof(CompareTypePicker),
                new PropertyMetadata(CompareType.StartsWith, new PropertyChangedCallback(OnCompareTypePropertyChanged)));

        private static void OnCompareTypePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (CompareTypePicker)sender;
            var value = (CompareType)e.NewValue;
        }

        public CompareType CompareType
        {
            get { return (CompareType)GetValue(CompareTypeProperty); }
            set { SetValue(CompareTypeProperty, value); }
        }

        public CompareTypePicker()
        {
            this.InitializeComponent();
        }
    }
}
