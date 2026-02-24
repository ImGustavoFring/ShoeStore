using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ShoeStore.WpfApp.Converters
{
    public class DiscountToTextDecorationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal discount && discount > 0)
                return TextDecorations.Strikethrough;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}