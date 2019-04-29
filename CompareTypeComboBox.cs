using System;
using Windows.UI.Xaml.Controls;

namespace TimtableFH
{
    class CompareTypeComboBox : ComboBox
    {
        public CompareTypeComboBox()
        {
            ItemsSource = Enum.GetValues(typeof(CompareType));
        }
    }
}
