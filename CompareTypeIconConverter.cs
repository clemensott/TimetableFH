using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace TimtableFH
{
    class CompareTypeIconConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((CompareType)value)
            {
                case CompareType.Equals:
                    return Symbol.List;

                case CompareType.StartsWith:
                    return Symbol.AlignLeft;

                case CompareType.EndsWith:
                    return Symbol.AlignRight;

                case CompareType.Contains:
                    return Symbol.AlignCenter;
                    
                case CompareType.Ignore:
                    return Symbol.Remove;
            }

            return Symbol.Help;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
