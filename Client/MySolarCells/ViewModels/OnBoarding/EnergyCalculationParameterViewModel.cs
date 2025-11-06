using MySolarCellsSQLite.Sqlite;
using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells.ViewModels.OnBoarding;

public class EnergyCalculationParameterViewModel : BaseViewModel
{
    private readonly MscDbContext mscDbContext;
    public EnergyCalculationParameterViewModel(MscDbContext mscDbContext, IDialogService dialogService,
        IAnalyticsService analyticsService, IInternetConnectionService internetConnectionService, ILogService logService, ISettingsService settingsService, IHomeService homeService) : base(dialogService, analyticsService, internetConnectionService,
        logService, settingsService, homeService)
    {
        this.mscDbContext = mscDbContext;
        var list = this.mscDbContext.EnergyCalculationParameter.Where(x => x.ElectricitySupplierId == HomeService.FirstElectricitySupplier().ElectricitySupplierId).OrderBy(o => o.FromDate).ToList();
        if (list.Count > 0)
        {
            Parameters = new ObservableCollection<EnergyCalculationParameter>(list);
            selectedParameters = parameters.Last();
        }
        else //Add default first one
            AddParameters(true);
    }
    public ICommand AddParametersCommand => new Command(() => AddParameters());
    public ICommand ShowDatePickerCommand => new Command(ShowDatePickerDlg);

    private void ShowDatePickerDlg()
    {
        ShowDatePicker = false;
        ShowDatePicker = true;

    }
    private bool showDatePicker;
    public bool ShowDatePicker
    {
        get => showDatePicker;
        set => SetProperty(ref showDatePicker, value);
    }
    private void AddParameters(bool firstTime = false)
    {
        //Clone previous
        if (firstTime)
        {
            Parameters.Add(new EnergyCalculationParameter
            {
                ElectricitySupplierId = HomeService.FirstElectricitySupplier().ElectricitySupplierId,
                FromDate = HomeService.FirstElectricitySupplier().FromDate,
            });
        }
        else
        {
            var paramLast = Parameters.Last();
            Parameters.Add(new EnergyCalculationParameter
            {
                ElectricitySupplierId = HomeService.FirstElectricitySupplier().ElectricitySupplierId,
                FromDate = paramLast.FromDate.AddMonths(1),
                EnergyTax = paramLast.EnergyTax,
                FixedPriceKwh = paramLast.FixedPriceKwh,
                ProdCompensationElectricityLowLoad = paramLast.ProdCompensationElectricityLowLoad,
                TaxReduction = paramLast.TaxReduction,
                TotalInstallKwhPanels = paramLast.TotalInstallKwhPanels,
                TransferFee = paramLast.TransferFee,
                UseSpotPrice = paramLast.UseSpotPrice

            });
        }
        mscDbContext.EnergyCalculationParameter.Add(Parameters.Last());
        SelectedParameters = Parameters.Last();
    }

    public ICommand SaveCommand => new Command(async () => await Save());

    private async Task Save()
    {
        try
        {
            await mscDbContext.SaveChangesAsync();
            if (SettingsService.OnboardingStatus == OnboardingStatusEnum.OnboardingDone)
            {
                await GoBack();
            }
            else
            {
                SettingsService.OnboardingStatus = OnboardingStatusEnum.EnergyCalculationParameterSelected;
                await GoToAsync(nameof(InvestmentAndLoanView));
            }
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(ex.Message, AppResources.My_Solar_Cells, AppResources.Ok);
        }

    }

    private ObservableCollection<EnergyCalculationParameter> parameters = new ObservableCollection<EnergyCalculationParameter>();
    public ObservableCollection<EnergyCalculationParameter> Parameters
    {
        get => parameters;
        set => SetProperty(ref parameters, value);
    }
    private EnergyCalculationParameter selectedParameters = new EnergyCalculationParameter();
    public EnergyCalculationParameter SelectedParameters
    {
        get => selectedParameters;
        set => SetProperty(ref selectedParameters, value);
        //SelectedFromDate = value.FromDate;
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

