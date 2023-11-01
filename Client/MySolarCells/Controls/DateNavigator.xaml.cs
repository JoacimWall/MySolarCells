namespace MySolarCells.Controls;

public partial class DateNavigator : ContentView
{
    public DateNavigator()
    {
        InitializeComponent();
        SimulateSettings.IsVisible = false;
        ROISimulateOn.IsToggled = false;
        ShowSimulateButton.IsVisible = true;
        
        // ROISimulateBatteryKwh.SetBinding(Label.TextProperty, new Binding("MaxBatteryPower", source: RoiSimulate));
        //ROISimulateBatteryKwh.Text = value.MaxBatteryPower.ToString();
    }

    public static readonly BindableProperty GraphDataChangedCommandProperty = BindableProperty.Create(nameof(GraphDataChangedCommand), typeof(ICommand), typeof(DateNavigator), null);
    public ICommand GraphDataChangedCommand
    {
        get { return (ICommand)GetValue(GraphDataChangedCommandProperty); }
        set { SetValue(GraphDataChangedCommandProperty, value); }
    }


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

    public static BindableProperty RoiSimulateProperty = BindableProperty.Create(propertyName: nameof(RoiSimulate),
    returnType: typeof(RoiSimulate), declaringType: typeof(ContentView),
    defaultValue: new RoiSimulate(), defaultBindingMode: BindingMode.TwoWay, propertyChanged: RoiSimulatePropertyChanged);

    public RoiSimulate RoiSimulate
    {
        get { return (RoiSimulate)GetValue(RoiSimulateProperty); }
        set
        {
            SetValue(RoiSimulateProperty, value);
        }
    }
    private static void RoiSimulatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (DateNavigator)bindable;
        var value = (RoiSimulate)newValue;
        control.ROISimulateOn.SetBinding(Switch.IsToggledProperty, new Binding("DoSimulate", source: value));
        control.ROISimulateOn.SetBinding(Switch.IsToggledProperty, new Binding("DoSimulate", source: value));
        control.ROIAddBatterySwith.SetBinding(Switch.IsToggledProperty, new Binding("AddBattery", source: value));
        control.ROIRemoveBatterySwitch.SetBinding(Switch.IsToggledProperty, new Binding("RemoveBattery", source: value));
        control.ROISBatteryKwh.SetBinding(Microsoft.Maui.Controls.Slider.ValueProperty, new Binding("MaxBatteryPower", source: value));
        
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
        get
        {
            return (bool)GetValue(ShowRoiSimulateProperty);
        }
        set
        {
            SetValue(ShowRoiSimulateProperty, value);
            ShowSimulateButton.IsVisible = value;
        }
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
        if (ChartData.TimeStamp.Date < MySolarCellsGlobals.SelectedHome.FromDate)
            ChartData.TimeStamp = MySolarCellsGlobals.SelectedHome.FromDate;

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
        //Check that we not go forward in time 
        if (ChartData.TimeStamp.Date > DateTime.Today)
            ChartData.TimeStamp = DateTime.Today;

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

    private bool simSettingsIsVisible = false;
    void ShowSimulate_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        if (simSettingsIsVisible)
        {
            simSettingsIsVisible = false;
            SimulateSettings.IsVisible = false;
        }
        else
        {
            simSettingsIsVisible = true;
            SimulateSettings.IsVisible = true;
        }
    }

    void ROISimulateBatteryKwh_TextChanged(System.Object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
    {
        RoiSimulate.MaxBatteryPower = Convert.ToDouble(e.NewTextValue);
    }
  
    void ROISimulateOn_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new RefreshRoiViewMessage(true));
    }
    
    void ROIAddBatterySwith_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {
        //if (simSettingsAddbattery)
        //{
        //    simSettingsAddbattery = false;
        //    //ROIAddBatterySwith.IsToggled = false;
        //    RoiSimulate.AddBattery = false;
        //}
        //else
        //{
        //    simSettingsAddbattery = true;
        //    //ROIAddBatterySwith.IsToggled = true;
        //    RoiSimulate.AddBattery = true;
           
        //}
        WeakReferenceMessenger.Default.Send(new RefreshRoiViewMessage(true));
    }
    //private bool simSettingsRemoveBattery = false;
    void ROIRemoveBatterySwitch_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {

        //if (simSettingsRemoveBattery)
        //{
        //    simSettingsRemoveBattery = false;
        //    //ROIRemoveBatterySwitch.IsToggled = false;
        //    RoiSimulate.RemoveBattery = false;
        //}
        //else
        //{
        //    simSettingsRemoveBattery = true;
        //    //ROIRemoveBatterySwitch.IsToggled = true;
        //    RoiSimulate.RemoveBattery = true;
        //}
        WeakReferenceMessenger.Default.Send(new RefreshRoiViewMessage(true));
    }
}
