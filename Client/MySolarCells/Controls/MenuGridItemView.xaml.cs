namespace MySolarCells.Controls;

public partial class MenuGridItemView : ContentView
{
    public MenuGridItemView()
    {
        InitializeComponent();
    }
    public static readonly BindableProperty TitleTextProperty = BindableProperty.Create(
                                                  propertyName: nameof(TitleText),
                                                  returnType: typeof(string),
                                                  declaringType: typeof(MenuGridItemView),
                                                  defaultValue: "",
                                                  defaultBindingMode: BindingMode.TwoWay,
                                                  propertyChanged: TitleTextPropertyChanged);

    public string TitleText
    {
        get => (string)GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    private static void TitleTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (MenuGridItemView)bindable;
        control.Title.Text = newValue.ToString() ?? string.Empty;
    }

    public static readonly BindableProperty FontIconProperty = BindableProperty.Create(
                                                    propertyName: nameof(FontIcon),
                                                    returnType: typeof(string),
                                                    declaringType: typeof(MenuGridItemView),
                                                    defaultValue: "",
                                                    defaultBindingMode: BindingMode.TwoWay,
                                                    propertyChanged: FontIconPropertyChanged);

    public string FontIcon
    {
        get => (string)GetValue(FontIconProperty);
        set => SetValue(FontIconProperty, value);
    }

    private static void FontIconPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (MenuGridItemView)bindable;
        control.IconLabel.Text = newValue.ToString() ?? string.Empty;
    }

    public static readonly BindableProperty FontIconExternalProperty = BindableProperty.Create(
                                                   propertyName: nameof(FontIconExternal),
                                                   returnType: typeof(string),
                                                   declaringType: typeof(MenuGridItemView),
                                                   defaultValue: "",
                                                   defaultBindingMode: BindingMode.TwoWay,
                                                   propertyChanged: FontIconExternalPropertyChanged);

    public string FontIconExternal
    {
        get => (string)GetValue(FontIconExternalProperty);
        set => SetValue(FontIconExternalProperty, value);
    }

    private static void FontIconExternalPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (MenuGridItemView)bindable;
        control.IconLabelExternal.Text = newValue.ToString() ?? "";
    }


    public static BindableProperty DateImplementedProperty = BindableProperty.Create(propertyName: nameof(DateImplemented),
      returnType: typeof(DateTime), declaringType: typeof(MenuGridItemView),
      defaultValue: DateTime.Now.AddYears(-1), defaultBindingMode: BindingMode.OneWay,
      propertyChanged: DateImplementedChanged);

    private static void DateImplementedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // MenuGridItemView control;
        //
        // control = (MenuGridItemView)bindable;
        // if (control != null && newValue != null)
        // {
        //     //control.IsNewLabel.IsVisible = (DateTime.Now - (DateTime)newValue).TotalDays < 30 ? true : false;
        // }
    }

    public DateTime DateImplemented
    {
        get => (DateTime)GetValue(DateImplementedProperty);
        set => SetValue(DateImplementedProperty, value);
    }

}