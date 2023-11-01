using System.Globalization;

namespace MySolarCells.Converters;

public class ImageHeightFromDisplayWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var width = (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density);
            //procent av bredd
            int procentOut;
            if (int.TryParse(parameter.ToString(), out procentOut))
            {
                float procent = (float)procentOut / (float)100;
                return width * procent;
            }

            //if (filename.StartsWith("http"))
            //    image.Source = filename;
            //if (parameter.ToString() == "Height_LandscapeXXX_Margin_0")
            //    return width / MediaHelper.LandscapeXXXCropRatio;

            //if (parameter.ToString() == "Height_LandscapeXX_Margin_0")
            //    return width / MediaHelper.LandscapeXXCropRatio;

            //if (parameter.ToString() == "Height_LandscapeX_Margin_0")
            //    return width / MediaHelper.LandscapeXCropRatio;

            //if (parameter.ToString() == "StageVideoHeightWidthRatio")
            //    return (width - 15 * 2) / MediaHelper.StageVideoHeightWidthRatio;

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
        catch (Exception)
        {

        }


        return 0;


    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Usually unused, but inverse the above logic if needed
        throw new NotImplementedException();
    }
}

