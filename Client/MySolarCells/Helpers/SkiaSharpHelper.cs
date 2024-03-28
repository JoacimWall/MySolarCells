using SkiaSharp;
using System.Reflection;

namespace MySolarCells.Helpers;

    public static class SkiaSharpHelper
    {
    private static SKTypeface? openSansRegular;
    public static SKTypeface? OpenSansRegular
    {
        get
        {
            if (openSansRegular == null)
            {
                var assembly = typeof(SkiaSharpHelper).GetTypeInfo().Assembly;
                Stream? stream = assembly.GetManifestResourceStream("MySolarCells.Resources.Fonts.OpenSans-Regular.ttf");

                openSansRegular = SKTypeface.FromStream(stream);
            }
            return openSansRegular;
        }
    }
}

