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
                ReportPageTyp reportPageTyp = (ReportPageTyp)values[1];
                if (reportPageTyp == ReportPageTyp.YearsOverview)
                    return dateTime.ToString("yyyy");
                if (reportPageTyp == ReportPageTyp.YearDetails)
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

