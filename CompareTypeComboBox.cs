using System;
using Windows.UI.Xaml.Controls;

namespace TimetableFH
{
    class CompareTypeComboBox : ComboBox
    {
        public CompareTypeComboBox()
        {
            ItemsSource = Enum.GetValues(typeof(CompareType));
        }
    }
}
