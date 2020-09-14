using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace TimetableFH.Controls
{
    public sealed partial class EditListElementControl : UserControl
    {
        public static readonly DependencyProperty IsViewMoveIconsProperty = DependencyProperty.Register("IsViewMoveIcons",
            typeof(bool), typeof(EditListElementControl), new PropertyMetadata(default(bool)));

        public event EventHandler<RoutedEventArgs> EditClick;
        public event EventHandler<RoutedEventArgs> UpClick;
        public event EventHandler<RoutedEventArgs> RemoveClick;
        public event EventHandler<RoutedEventArgs> DownClick;

        public bool IsViewMoveIcons
        {
            get => (bool)GetValue(IsViewMoveIconsProperty);
            set => SetValue(IsViewMoveIconsProperty, value);
        }

        public EditListElementControl()
        {
            this.InitializeComponent();
        }

        private void SinEdit_Click(object sender, RoutedEventArgs e)
        {
            EditClick?.Invoke(this, e);
        }

        private void SinUp_Click(object sender, RoutedEventArgs e)
        {
            UpClick?.Invoke(this, e);
        }

        private void SinRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveClick?.Invoke(this, e);
        }

        private void SinDown_Click(object sender, RoutedEventArgs e)
        {
            DownClick?.Invoke(this, e);
        }
    }
}
