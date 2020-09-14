using System;
using Windows.UI.Xaml.Controls;

namespace TimetableFH.Controls.Compare
{
    class CompareTypeComboBox : ComboBox
    {
        public CompareTypeComboBox()
        {
            ItemsSource = Enum.GetValues(typeof(CompareType));
        }
    }
}
