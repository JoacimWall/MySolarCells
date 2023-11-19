using System;
using System.Globalization;

namespace MySolarCells.Converters
{
    public class StringToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null)
                return null;

            var someValue = (string)value; // Convert 'object' to whatever type you are expecting

            if (someValue.Contains("Resources.EmbeddedImages."))
            {
                return ImageSource.FromResource(someValue, MySolarCellsGlobals.App.GetType().Assembly);
            }

            if (someValue.StartsWith("http"))
                return someValue;

            //if (!string.IsNullOrEmpty(someValue))
            //{
            //    var filePath = Path.Combine(TietoGlobals.PrivMediaFolder(), someValue);
            //    return FileImageSource.FromFile(filePath);
            //}

            return null;
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Usually unused, but inverse the above logic if needed
            throw new NotImplementedException();
        }
    }
}

