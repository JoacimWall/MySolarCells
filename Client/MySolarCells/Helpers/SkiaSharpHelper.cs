using System;
using SkiaSharp;
using System.Reflection;

namespace MySolarCells.Helpers;

    public static class SkiaSharpHelper
    {
    private static SKTypeface open_sans_regular;
    public static SKTypeface OpenSansRegular
    {
        get
        {
            if (open_sans_regular == null)
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(SkiaSharpHelper)).Assembly;
                Stream stream = assembly.GetManifestResourceStream("MySolarCells.Resources.Fonts.OpenSans-Regular.ttf");

                open_sans_regular = SKTypeface.FromStream(stream);
            }
            return open_sans_regular;
        }
    }
}

