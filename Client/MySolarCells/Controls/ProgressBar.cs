using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace MySolarCells.Controls;

public class ProgressBar : SKCanvasView
{
    private Animation? animation;

    public static BindableProperty PercentageProperty = BindableProperty.Create(nameof(Percentage), typeof(float),
        typeof(ProgressBar), 0f,
        validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

    public float Percentage
    {
        get => (float)GetValue(PercentageProperty);
        set => SetValue(PercentageProperty, value);
    }


    public static BindableProperty PercentageAnimateProperty = BindableProperty.Create(nameof(PercentageAnimate), typeof(float),
        typeof(ProgressBar), 0f,
        validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedPercentAnimate);

    public float PercentageAnimate
    {
        get => (float)GetValue(PercentageAnimateProperty);
        set => SetValue(PercentageAnimateProperty, value);
    }


    public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
        typeof(ProgressBar), 5f,
        validateValue: (_, value) => value != null && (float)value >= 0,
        propertyChanged: OnPropertyChangedInvalidate);

    public float CornerRadius
    {
        get => (float)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(Color),
        typeof(ProgressBar), AppColors.WhiteColor,
        validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

    public Color BarBackgroundColor
    {
        get => (Color)GetValue(BarBackgroundColorProperty);
        set => SetValue(BarBackgroundColorProperty, value);
    }

    public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
        typeof(ProgressBar), 12f,
        validateValue: (_, value) => value != null && (float)value >= 0,
        propertyChanged: OnPropertyChangedInvalidate);

    public float FontSize
    {
        get => (float)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(Color),
        typeof(ProgressBar), AppColors.Primary500Color,
        validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

    public Color GradientStartColor
    {
        get => (Color)GetValue(GradientStartColorProperty);
        set => SetValue(GradientStartColorProperty, value);
    }

    public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(Color),
        typeof(ProgressBar), AppColors.Primary500Color,
        validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

    public Color GradientEndColor
    {
        get => (Color)GetValue(GradientEndColorProperty);
        set => SetValue(GradientEndColorProperty, value);
    }

    public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color),
        typeof(ProgressBar), AppColors.WhiteColor,
        validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(Color),
        typeof(ProgressBar), AppColors.Primary500Color,
        validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

    public Color AlternativeTextColor
    {
        get => (Color)GetValue(AlternativeTextColorProperty);
        set => SetValue(AlternativeTextColorProperty, value);
    }

    public static BindableProperty ShowPercentTextProperty = BindableProperty.Create(nameof(ShowPercentText), typeof(bool),
    typeof(ProgressBar), true,
    validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

    public bool ShowPercentText
    {
        get => (bool)GetValue(ShowPercentTextProperty);
        set => SetValue(ShowPercentTextProperty, value);
    }
   
    private static void OnPropertyChangedPercentAnimate(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (ProgressBar)bindable;

        if (oldValue != newValue)
        {
            var startValue = 0;
            var desiredValue = (float)newValue;

            if (control.animation != null)
            {
                control.AbortAnimation("Percentage");
            }

            control.animation = new Animation(v =>
            {
                if (v == 0)
                {
                    control.Percentage = 0;
                    return;
                }

                control.Percentage = (float)(v / 100);
            }, startValue, desiredValue * 100, easing: Easing.SinInOut);

           
            control.animation.Commit(control, "Percentage", length: 4000,
                finished: (_, _) => { control.animation = null; });


        }
           
    }
    private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
    {
        var control = (ProgressBar)bindable;

        if (oldvalue != newvalue)
            control.InvalidateSurface();
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var info = e.Info;
        var canvas = e.Surface.Canvas;
      
        float width = (float)Width;
        var scale = CanvasSize.Width / width;

        var percentage = Percentage/100;

        var cornerRadius = CornerRadius * scale;

        var textSize = FontSize * scale;

        var height = e.Info.Height;

        var str = percentage.ToString("0%");

        var percentageWidth = (int)Math.Floor(info.Width * percentage);

        canvas.Clear();

        var backgroundBar = new SKRoundRect(new SKRect(0, 0, info.Width, height), cornerRadius, cornerRadius);
        var progressBar = new SKRoundRect(new SKRect(0, 0, percentageWidth, height), cornerRadius, cornerRadius);

        var background = new SKPaint { Color = BarBackgroundColor.ToSKColor(), IsAntialias = true };

        canvas.DrawRoundRect(backgroundBar, background);

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            float x = percentageWidth;
            float y = info.Height;
            var rect = new SKRect(0, 0, x, y);

            paint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(rect.Left, rect.Top),
                new SKPoint(rect.Right, rect.Top),
                new[]
                {
                    GradientStartColor.ToSKColor(),
                    GradientEndColor.ToSKColor()
                },
                new float[] { 0, 1 },
                SKShaderTileMode.Clamp);

            canvas.DrawRoundRect(progressBar, paint);
        }
        
        

        var textPaint = new SKPaint { Color = TextColor.ToSKColor(), TextSize = textSize };

        var textBounds = new SKRect();

        textPaint.MeasureText(str, ref textBounds);

        var xText = percentageWidth / 2f - textBounds.MidX;
        if (xText < 0)
        {
            xText = info.Width / 2f - textBounds.MidX;
            textPaint.Color = AlternativeTextColor.ToSKColor();
        }

        var yText = info.Height / 2f - textBounds.MidY;
        if (ShowPercentText)
            canvas.DrawText(str, xText, yText, textPaint);
    }
}
