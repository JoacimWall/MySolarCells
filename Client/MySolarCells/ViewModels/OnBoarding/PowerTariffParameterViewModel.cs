
using Microsoft.EntityFrameworkCore;


namespace MySolarCells.ViewModels.OnBoarding;

public class PowerTariffParameterViewModel : BaseViewModel
{
    private readonly MscDbContext mscDbContext;
    public PowerTariffParameterViewModel(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
        var list = this.mscDbContext.PowerTariffParameters.Where(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId).OrderBy(o => o.FromDate).ToList();
        if (list != null && list.Count > 0)
        {
            Parameters = new ObservableCollection<PowerTariffParameters>(list);
            selectedParameters = parameters.Last();
        }
        else //Add default first one
            AddParameters(true);
    }
    public ICommand AddParametersCommand => new Command(() => AddParameters());
    public ICommand ShowDatePickerCommand => new Command(() => ShowDatePickerDlg());

    private void ShowDatePickerDlg()
    {
        ShowDatePicker = false;
        ShowDatePicker = true;

    }
    private bool showDatePicker;
    public bool ShowDatePicker
    {
        get => showDatePicker;
        set { SetProperty(ref showDatePicker, value); }
    }
    private void AddParameters(bool firstTime = false)
    {
        //Clone previus
        if (firstTime)
        {
            Parameters.Add(new PowerTariffParameters
            {
                HomeId = MySolarCellsGlobals.SelectedHome.HomeId,
                FromDate = MySolarCellsGlobals.SelectedHome.FromDate,
            });
        }
        else
        { 
        var paramLast = Parameters.Last();
        Parameters.Add(new PowerTariffParameters
        {
            HomeId = MySolarCellsGlobals.SelectedHome.HomeId,
            FromDate = paramLast.FromDate.AddMonths(1),
            //EnergyTax = paramLast.EnergyTax,
            //FixedPriceKwh = paramLast.FixedPriceKwh,
            //ProdCompensationElectricityLowload = paramLast.ProdCompensationElectricityLowload,
            //TaxReduction = paramLast.TaxReduction,
            //TotalInstallKwhPanels = paramLast.TotalInstallKwhPanels,
            //TransferFee = paramLast.TransferFee,
            //UseSpotPrice = paramLast.UseSpotPrice

        });
        }
        this.mscDbContext.PowerTariffParameters.Add(Parameters.Last());
        SelectedParameters = Parameters.Last();
    }

    public ICommand SaveCommand => new Command(async () => await Save());

    private async Task Save()
    {
        try
        {
            await this.mscDbContext.SaveChangesAsync();
            if (SettingsService.OnboardingStatus == OnboardingStatusEnum.OnboardingDone)
            {
                await GoBack();
            }
            else
            {
                SettingsService.OnboardingStatus = OnboardingStatusEnum.EnergyCalculationparametersSelected;
                await GoToAsync(nameof(InvestmentAndLoanView));
            }
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(ex.Message, AppResources.My_Solar_Cells, AppResources.Ok);
        }

    }

    private ObservableCollection<PowerTariffParameters> parameters = new ObservableCollection<PowerTariffParameters>();
    public ObservableCollection<PowerTariffParameters> Parameters
    {
        get => parameters;
        set
        {
            SetProperty(ref parameters, value);

        }
    }
    private PowerTariffParameters selectedParameters = new PowerTariffParameters();
    public PowerTariffParameters SelectedParameters
    {
        get => selectedParameters;
        set
        {
            SetProperty(ref selectedParameters, value);
            //SelectedFromDate = value.FromDate;
        }
    }
    //private DateTime selectedFromDate = DateTime.Today;
    //public DateTime SelectedFromDate
    //{
    //    get { return selectedFromDate; }
    //    set
    //    {
    //        SetProperty(ref selectedFromDate, value);
    //    }
    //}

}

