using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EsportsTournamentManager.Converters
{
    public class BooleanToStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "Đang hoạt động" : "Dự bị / Khóa";
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToStatusBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                // Return hex color brush
                // Green #10B981 for active, grey/red #6B7280 for inactive
                string hexColor = isActive ? "#10B981" : "#EF4444";
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
