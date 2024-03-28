using System.Globalization;

namespace MySolarCells.Converters;

public class ImageHeightFromDisplayWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value == null)
            {
                return 50;
            }
            var width = (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density);
            //percent of width
            if (parameter != null && int.TryParse(parameter.ToString(), out var percentOut))
            {
                float percent = percentOut / (float)100;
                return width * percent;
            }

            if (parameter == null)
                return width;
            
            if (parameter.ToString() == "Width_Margin_0")
                return width;

            if (parameter.ToString() == "Width_Margin_15")
                return width - (15 * 2);

            if (parameter.ToString() == "Width_Margin_16")
                return width - (16 * 2);

            if (parameter.ToString() == "Width_Dev_2_Margin_8")
                return (width / 2) - 8;

            if (parameter.ToString() == "Width_Dev_2_Margin_16")
                return (width / 2) - 16;

            if (parameter.ToString() == "Width_Dev_2_Margin_32")
                return (width / 2) -32;
            return width;

        }
        catch
        {
            // ignored
        }


        return 0;


    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Usually unused, but inverse the above logic if needed
        throw new NotImplementedException();
    }
}

