using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace TimetableFH
{
    public sealed partial class EditListElementControl : UserControl
    {
        public static readonly DependencyProperty IsViewMoveIconsProperty = DependencyProperty.Register("IsViewMoveIcons",
            typeof(bool), typeof(EditListElementControl), new PropertyMetadata(default(bool)));

        public event EventHandler<TappedRoutedEventArgs> EditTapped;
        public event EventHandler<TappedRoutedEventArgs> UpTapped;
        public event EventHandler<TappedRoutedEventArgs> RemoveTapped;
        public event EventHandler<TappedRoutedEventArgs> DownTapped;

        public bool IsViewMoveIcons
        {
            get => (bool)GetValue(IsViewMoveIconsProperty);
            set => SetValue(IsViewMoveIconsProperty, value);
        }

        public EditListElementControl()
        {
            this.InitializeComponent();
        }

        private void SinEdit_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EditTapped?.Invoke(this, e);
        }

        private void SinUp_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UpTapped?.Invoke(this, e);
        }

        private void SinRemove_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RemoveTapped?.Invoke(this, e);
        }

        private void SinDown_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DownTapped?.Invoke(this, e);
        }
    }
}
