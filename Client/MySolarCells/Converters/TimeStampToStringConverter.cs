using System.Globalization;

namespace MySolarCells.Converters
{
    public class TimeStampToStringConverter : IMultiValueConverter
    {
        public object? Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is { Length: < 3 })
                return null;

            if (values != null)
            {
                DateTime dateTime = (DateTime)values[0];
                ReportPageType reportPageType = (ReportPageType)values[1];
                if (reportPageType == ReportPageType.YearsOverview)
                    return dateTime.ToString("yyyy");
                if (reportPageType == ReportPageType.YearDetails)
                    return dateTime.ToString("MMM").ToUpper();
            }

            return null;
        }



        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

