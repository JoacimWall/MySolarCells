namespace MySolarCells.Controls;

public partial class DateNavigator : ContentView
{
	public DateNavigator()
	{
		InitializeComponent();
	}

    //public static readonly BindableProperty TodayCommandProperty = BindableProperty.Create(nameof(TodayCommand), typeof(ICommand), typeof(DateNavigator), null);
    //public static readonly BindableProperty DayCommandProperty = BindableProperty.Create(nameof(DayCommand), typeof(ICommand), typeof(DateNavigator), null);
    //public static readonly BindableProperty WeekCommandProperty = BindableProperty.Create(nameof(WeekCommand), typeof(ICommand), typeof(DateNavigator), null);
    //public static readonly BindableProperty MonthCommandProperty = BindableProperty.Create(nameof(MonthCommand), typeof(ICommand), typeof(DateNavigator), null);
    //public static readonly BindableProperty YearCommandProperty = BindableProperty.Create(nameof(YearCommand), typeof(ICommand), typeof(DateNavigator), null);
    //public static readonly BindableProperty BackCommandProperty = BindableProperty.Create(nameof(BackCommand), typeof(ICommand), typeof(DateNavigator), null);
    //public static readonly BindableProperty ForwardCommandProperty = BindableProperty.Create(nameof(ForwardCommand), typeof(ICommand), typeof(DateNavigator), null);
    //public static readonly BindableProperty ShowUnitCommandProperty = BindableProperty.Create(nameof(ShowUnitCommand), typeof(ICommand), typeof(DateNavigator), null);
    //public static readonly BindableProperty ShowCurrencyCommandProperty = BindableProperty.Create(nameof(ShowCurrencyCommand), typeof(ICommand), typeof(DateNavigator), null);
    public static readonly BindableProperty GraphDataChangedCommandProperty = BindableProperty.Create(nameof(GraphDataChangedCommand), typeof(ICommand), typeof(DateNavigator), null);
    public ICommand GraphDataChangedCommand
    {
        get { return (ICommand)GetValue(GraphDataChangedCommandProperty); }
        set { SetValue(GraphDataChangedCommandProperty, value); }
    }
    //public ICommand DayCommand
    //{
    //    get { return (ICommand)GetValue(DayCommandProperty); }
    //    set { SetValue(DayCommandProperty, value); }
    //}
    //public ICommand WeekCommand
    //{
    //    get { return (ICommand)GetValue(WeekCommandProperty); }
    //    set { SetValue(WeekCommandProperty, value); }
    //}
    //public ICommand MonthCommand
    //{
    //    get { return (ICommand)GetValue(MonthCommandProperty); }
    //    set { SetValue(MonthCommandProperty, value); }
    //}
    //public ICommand YearCommand
    //{
    //    get { return (ICommand)GetValue(YearCommandProperty); }
    //    set { SetValue(YearCommandProperty, value); }
    //}
    //public ICommand BackCommand
    //{
    //    get { return (ICommand)GetValue(BackCommandProperty); }
    //    set { SetValue(BackCommandProperty, value); }
    //}
    //public ICommand ForwardCommand
    //{
    //    get { return (ICommand)GetValue(ForwardCommandProperty); }
    //    set { SetValue(ForwardCommandProperty, value); }
    //}
    //public ICommand ShowUnitCommand
    //{
    //    get { return (ICommand)GetValue(ShowUnitCommandProperty); }
    //    set { SetValue(ShowUnitCommandProperty, value); }
    //}
    //public ICommand ShowCurrencyCommand
    //{
    //    get { return (ICommand)GetValue(ShowCurrencyCommandProperty); }
    //    set { SetValue(ShowCurrencyCommandProperty, value); }
    //}

    public static BindableProperty ChartDataProperty = BindableProperty.Create(propertyName: nameof(ChartData),
    returnType: typeof(ChartDataRequest), declaringType: typeof(ContentView),
    defaultValue: new ChartDataRequest(), defaultBindingMode: BindingMode.TwoWay);
    public ChartDataRequest ChartData
    {
        get { return (ChartDataRequest)GetValue(ChartDataProperty); }
        set
        {
            SetValue(ChartDataProperty, value);
        }
    }
    

    public static BindableProperty ShowUnitCurrencySeletorProperty = BindableProperty.Create(propertyName: nameof(ShowUnitCurrencySeletor),
    returnType: typeof(bool), declaringType: typeof(ContentView),
    defaultValue: true, defaultBindingMode: BindingMode.OneWay, propertyChanged: ShowUnitCurrencySeletorPropertyChanged);

    public bool ShowUnitCurrencySeletor
    {
        get { return (bool)GetValue(ShowUnitCurrencySeletorProperty); }
        set
        {
            SetValue(ShowUnitCurrencySeletorProperty, value);
        }
    }


    private static void ShowUnitCurrencySeletorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {

        var control = (DateNavigator)bindable;
        var value = (bool)newValue;
        control.UnitButton.IsVisible = value;
        control.CurrencyButton.IsVisible = value;
    }

   

    void Back_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        switch (ChartData.ChartDataRange)
        {
            case ChartDataRange.Today:
            case ChartDataRange.Day:
                ChartData.TimeStamp = ChartData.TimeStamp.AddDays(-1);
                if (ChartData.TimeStamp.Date == DateTime.Now.Date)
                    ChartData.ChartDataRange = ChartDataRange.Today;
                else
                    ChartData.ChartDataRange = ChartDataRange.Day;
                break;
            case ChartDataRange.Week:
                ChartData.TimeStamp = ChartData.TimeStamp.AddDays(-7);
                break;

            case ChartDataRange.Month:
                ChartData.TimeStamp = ChartData.TimeStamp.AddMonths(-1);
                break;

            case ChartDataRange.Year:
                ChartData.TimeStamp = ChartData.TimeStamp.AddYears(-1);
                break;

        }
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
        
    }

    void Forward_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        switch (ChartData.ChartDataRange)
        {
            case ChartDataRange.Today:
            case ChartDataRange.Day:
                ChartData.TimeStamp = ChartData.TimeStamp.AddDays(1);
                if (ChartData.TimeStamp.Date == DateTime.Now.Date)
                    ChartData.ChartDataRange = ChartDataRange.Today;
                else
                    ChartData.ChartDataRange = ChartDataRange.Day;
                break;
            case ChartDataRange.Week:
                ChartData.TimeStamp = ChartData.TimeStamp.AddDays(7);
                break;

            case ChartDataRange.Month:
                ChartData.TimeStamp = ChartData.TimeStamp.AddMonths(1);
                break;

            case ChartDataRange.Year:
                ChartData.TimeStamp = ChartData.TimeStamp.AddYears(1);
                break;

        }
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
        
    }

    void Today_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {

        ChartData.TimeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        ChartData.ChartDataRange = ChartDataRange.Today;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
        
    }
    void Day_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        ChartData.ChartDataRange = ChartDataRange.Day;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
        
    }
    void Week_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        ChartData.ChartDataRange = ChartDataRange.Week;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
    }

    void Month_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        ChartData.ChartDataRange = ChartDataRange.Month;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
    }

    void Year_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        ChartData.ChartDataRange = ChartDataRange.Year;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
    }

    void Unit_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        ChartData.ChartDataUnit = ChartDataUnit.kWh;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
    }

    void Currency_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        ChartData.ChartDataUnit = ChartDataUnit.Currency;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
    }
}
