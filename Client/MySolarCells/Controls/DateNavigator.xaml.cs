namespace MySolarCells.Controls;

public partial class DateNavigator : ContentView
{
    public DateNavigator()
    {
        InitializeComponent();
        SimulateSettings.IsVisible = false;
        ShowSimulateButton.IsVisible = true;
    }

    public static readonly BindableProperty GraphDataChangedCommandProperty = BindableProperty.Create(nameof(GraphDataChangedCommand), typeof(ICommand), typeof(DateNavigator));
    public ICommand? GraphDataChangedCommand
    {
        get => (ICommand)GetValue(GraphDataChangedCommandProperty);
        set => SetValue(GraphDataChangedCommandProperty, value);
    }


    public static readonly BindableProperty ChartDataProperty = BindableProperty.Create(propertyName: nameof(ChartData),
    returnType: typeof(ChartDataRequest), declaringType: typeof(ContentView),
    defaultValue: new ChartDataRequest(), defaultBindingMode: BindingMode.TwoWay);
    public ChartDataRequest ChartData
    {
        get => (ChartDataRequest)GetValue(ChartDataProperty);
        set => SetValue(ChartDataProperty, value);
    }


    public static BindableProperty ShowUnitCurrencySelectorProperty = BindableProperty.Create(propertyName: nameof(ShowUnitCurrencySelector),
    returnType: typeof(bool), declaringType: typeof(ContentView),
    defaultValue: true, defaultBindingMode: BindingMode.OneWay, propertyChanged: ShowUnitCurrencySelectorPropertyChanged);

    public bool ShowUnitCurrencySelector
    {
        get => (bool)GetValue(ShowUnitCurrencySelectorProperty);
        set => SetValue(ShowUnitCurrencySelectorProperty, value);
    }


    private static void ShowUnitCurrencySelectorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {

        var control = (DateNavigator)bindable;
        var value = (bool)newValue;
        control.UnitButton.IsVisible = value;
        control.CurrencyButton.IsVisible = value;
    }

    public static BindableProperty RoiSimulateProperty = BindableProperty.Create(propertyName: nameof(RoiSimulate),
    returnType: typeof(HistorySimulate), declaringType: typeof(ContentView),
    defaultValue: new HistorySimulate(), defaultBindingMode: BindingMode.TwoWay, propertyChanged: RoiSimulatePropertyChanged);

    public HistorySimulate RoiSimulate
    {
        get => (HistorySimulate)GetValue(RoiSimulateProperty);
        set => SetValue(RoiSimulateProperty, value);
    }
    private static void RoiSimulatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (DateNavigator)bindable;
        var value = (HistorySimulate)newValue;
        control.RoisBatteryKwh.SetBinding(Slider.ValueProperty, new Binding("MaxBatteryPower", source: value));
        
    }

    public static BindableProperty ShowRoiSimulateProperty = BindableProperty.Create(propertyName: nameof(ShowRoiSimulate),
     returnType: typeof(bool), declaringType: typeof(ContentView),
     defaultValue: true, defaultBindingMode: BindingMode.TwoWay, propertyChanged: ShowRoiSimulatePropertyChanged);

    private static void ShowRoiSimulatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (DateNavigator)bindable;
        var value = (bool)newValue;
        control.ShowSimulateButton.IsVisible = value;

    }

    public bool ShowRoiSimulate
    {
        get => (bool)GetValue(ShowRoiSimulateProperty);
        set
        {
            SetValue(ShowRoiSimulateProperty, value);
            ShowSimulateButton.IsVisible = value;
        }
    }

    void Back_Tapped(Object sender, TappedEventArgs e)
    {
        switch (ChartData.ChartDataRange)
        {
            case ChartDataRange.Today:
            case ChartDataRange.Day:
                ChartData.TimeStamp = ChartData.TimeStamp.AddDays(-1);
                ChartData.ChartDataRange = ChartData.TimeStamp.Date == DateTime.Now.Date ? ChartDataRange.Today : ChartDataRange.Day;
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
        if (ChartData.TimeStamp.Date < MySolarCellsGlobals.SelectedHome.FromDate)
            ChartData.TimeStamp = MySolarCellsGlobals.SelectedHome.FromDate;

        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);

    }

    void Forward_Tapped(Object sender, TappedEventArgs e)
    {
        switch (ChartData.ChartDataRange)
        {
            case ChartDataRange.Today:
            case ChartDataRange.Day:
                ChartData.TimeStamp = ChartData.TimeStamp.AddDays(1);
                ChartData.ChartDataRange = ChartData.TimeStamp.Date == DateTime.Now.Date ? ChartDataRange.Today : ChartDataRange.Day;
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
        //Check that we not go forward in time 
        if (ChartData.TimeStamp.Date > DateTime.Today)
            ChartData.TimeStamp = DateTime.Today;

        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);

    }

    void Today_Tapped(Object sender, TappedEventArgs e)
    {

        ChartData.TimeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        ChartData.ChartDataRange = ChartDataRange.Today;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);

    }
    void Day_Tapped(Object sender, TappedEventArgs e)
    {
        ChartData.ChartDataRange = ChartDataRange.Day;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);

    }
    void Week_Tapped(Object sender, TappedEventArgs e)
    {
        ChartData.ChartDataRange = ChartDataRange.Week;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
    }

    void Month_Tapped(Object sender, TappedEventArgs e)
    {
        ChartData.ChartDataRange = ChartDataRange.Month;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
    }

    void Year_Tapped(Object sender, TappedEventArgs e)
    {
        ChartData.ChartDataRange = ChartDataRange.Year;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
    }

    void Unit_Tapped(Object sender, TappedEventArgs e)
    {
        ChartData.ChartDataUnit = ChartDataUnit.KWh;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
    }

    void Currency_Tapped(Object sender, TappedEventArgs e)
    {
        ChartData.ChartDataUnit = ChartDataUnit.Currency;
        if (GraphDataChangedCommand != null && GraphDataChangedCommand.CanExecute(null))
            GraphDataChangedCommand.Execute(null);
    }

    private bool simSettingsIsVisible;
    void ShowSimulate_Tapped(Object sender, TappedEventArgs e)
    {
        if (simSettingsIsVisible)
        {
            simSettingsIsVisible = false;
            SimulateSettings.IsVisible = false;
            RoiSimulate.DoSimulate = false;
            WeakReferenceMessenger.Default.Send(new RefreshRoiViewMessage(true));
        }
        else
        {
            simSettingsIsVisible = true;
            SimulateSettings.IsVisible = true;
            RoiSimulate.DoSimulate = true;
            WeakReferenceMessenger.Default.Send(new RefreshRoiViewMessage(true));
        }
    }
    
    void AddBattery_Tapped(Object sender, TappedEventArgs e)
    {
        RoiSimulate.AddBattery = !RoiSimulate.AddBattery;
        WeakReferenceMessenger.Default.Send(new RefreshRoiViewMessage(true));
    }

    private void RemoveBattery_Tapped(Object sender, TappedEventArgs e)
    {
        RoiSimulate.RemoveBattery = !RoiSimulate.RemoveBattery;
        WeakReferenceMessenger.Default.Send(new RefreshRoiViewMessage(true));
    }

    private bool moreStackIsVisible;
    void More_Tapped(Object sender, TappedEventArgs e)
    {

        moreStackIsVisible = MoreStack.IsVisible = !moreStackIsVisible;
    }

   

}
