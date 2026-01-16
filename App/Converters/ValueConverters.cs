using BusinessLayer;
using System.Globalization;

namespace App.Converters
{
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BusinessTripStatus status)
            {
                return status switch
                {
                    BusinessTripStatus.Pending => "Чакаща",
                    BusinessTripStatus.Approved => "Одобрена",
                    BusinessTripStatus.Rejected => "Отхвърлена",
                    _ => "Неизвестен"
                };
            }
            return "Неизвестен";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BusinessTripStatus status)
            {
                return status switch
                {
                    BusinessTripStatus.Pending => Colors.Orange, // Pending
                    BusinessTripStatus.Approved => Colors.Green,  // Approved
                    BusinessTripStatus.Rejected => Colors.Red,    // Rejected
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                bool result = intValue > 0;

                // Check if we need to invert the result
                if (parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
                {
                    result = !result;
                }

                return result;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return !string.IsNullOrWhiteSpace(stringValue);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToEditTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditing)
            {
                return isEditing ? "Отказ" : "Редактирай";
            }
            return "Редактирай";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnabledToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Always return bright colors regardless of enabled state
            return Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}