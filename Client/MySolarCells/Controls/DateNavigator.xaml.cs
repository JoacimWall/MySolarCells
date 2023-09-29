namespace MySolarCells.Controls;

public partial class DateNavigator : ContentView
{
	public DateNavigator()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty TodayCommandProperty = BindableProperty.Create(nameof(TodayCommand), typeof(ICommand), typeof(DateNavigator), null);
    public static readonly BindableProperty DayCommandProperty = BindableProperty.Create(nameof(DayCommand), typeof(ICommand), typeof(DateNavigator), null);
    public static readonly BindableProperty WeekCommandProperty = BindableProperty.Create(nameof(WeekCommand), typeof(ICommand), typeof(DateNavigator), null);
    public static readonly BindableProperty MonthCommandProperty = BindableProperty.Create(nameof(MonthCommand), typeof(ICommand), typeof(DateNavigator), null);
    public static readonly BindableProperty YearCommandProperty = BindableProperty.Create(nameof(YearCommand), typeof(ICommand), typeof(DateNavigator), null);
    public static readonly BindableProperty BackCommandProperty = BindableProperty.Create(nameof(BackCommand), typeof(ICommand), typeof(DateNavigator), null);
    public static readonly BindableProperty ForwardCommandProperty = BindableProperty.Create(nameof(ForwardCommand), typeof(ICommand), typeof(DateNavigator), null);
    public static readonly BindableProperty ShowUnitCommandProperty = BindableProperty.Create(nameof(ShowUnitCommand), typeof(ICommand), typeof(DateNavigator), null);
    public static readonly BindableProperty ShowCurrencyCommandProperty = BindableProperty.Create(nameof(ShowCurrencyCommand), typeof(ICommand), typeof(DateNavigator), null);

    public ICommand TodayCommand
    {
        get { return (ICommand)GetValue(TodayCommandProperty); }
        set { SetValue(TodayCommandProperty, value); }
    }
    public ICommand DayCommand
    {
        get { return (ICommand)GetValue(DayCommandProperty); }
        set { SetValue(DayCommandProperty, value); }
    }
    public ICommand WeekCommand
    {
        get { return (ICommand)GetValue(WeekCommandProperty); }
        set { SetValue(WeekCommandProperty, value); }
    }
    public ICommand MonthCommand
    {
        get { return (ICommand)GetValue(MonthCommandProperty); }
        set { SetValue(MonthCommandProperty, value); }
    }
    public ICommand YearCommand
    {
        get { return (ICommand)GetValue(YearCommandProperty); }
        set { SetValue(YearCommandProperty, value); }
    }
    public ICommand BackCommand
    {
        get { return (ICommand)GetValue(BackCommandProperty); }
        set { SetValue(BackCommandProperty, value); }
    }
    public ICommand ForwardCommand
    {
        get { return (ICommand)GetValue(ForwardCommandProperty); }
        set { SetValue(ForwardCommandProperty, value); }
    }
    public ICommand ShowUnitCommand
    {
        get { return (ICommand)GetValue(ShowUnitCommandProperty); }
        set { SetValue(ShowUnitCommandProperty, value); }
    }
    public ICommand ShowCurrencyCommand
    {
        get { return (ICommand)GetValue(ShowCurrencyCommandProperty); }
        set { SetValue(ShowCurrencyCommandProperty, value); }
    }


    public bool ShowUnitCurrencySeletor
    {
        get { return (bool)GetValue(ShowUnitCurrencySeletorProperty); }
        set
        {
            SetValue(ShowUnitCurrencySeletorProperty, value);
        }
    }
    public static BindableProperty ShowUnitCurrencySeletorProperty = BindableProperty.Create(propertyName: nameof(ShowUnitCurrencySeletor),
   returnType: typeof(bool), declaringType: typeof(ContentView),
   defaultValue: true, defaultBindingMode: BindingMode.OneWay, propertyChanged : ShowUnitCurrencySeletorPropertyChanged);
   

    private static void ShowUnitCurrencySeletorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {

        var control = (DateNavigator)bindable;
        var value = (bool)newValue;
        control.UnitButton.IsVisible = value;
        control.CurrencyButton.IsVisible = value;
    }
}
