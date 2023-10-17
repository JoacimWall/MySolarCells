
using Microsoft.EntityFrameworkCore;

namespace MySolarCells.ViewModels.OnBoarding;

public class EnergyCalculationParameterViewModel : BaseViewModel
{
    MscDbContext dbContext = new MscDbContext();
    public EnergyCalculationParameterViewModel()
    {
        var list = dbContext.EnergyCalculationParameter.Where(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId).OrderBy(o => o.FromDate).ToList();
        if (list != null && list.Count > 0)
        {
            Parameters = new ObservableCollection<Services.Sqlite.Models.EnergyCalculationParameter>(list);
            selectedParameters = parameters.Last();
        }
        else //Add default first one
            AddParameters();
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
    private void AddParameters()
    {
        Parameters.Add(new Services.Sqlite.Models.EnergyCalculationParameter { HomeId = MySolarCellsGlobals.SelectedHome.HomeId, FromDate = MySolarCellsGlobals.SelectedHome.FromDate });
        dbContext.EnergyCalculationParameter.Add(Parameters.Last());
        SelectedParameters = Parameters.Last();
    }

    public ICommand SaveCommand => new Command(async () => await Save());

    private async Task Save()
    {
        try
        {
            await this.dbContext.SaveChangesAsync();
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

    private ObservableCollection<Services.Sqlite.Models.EnergyCalculationParameter> parameters = new ObservableCollection<Services.Sqlite.Models.EnergyCalculationParameter>();
    public ObservableCollection<Services.Sqlite.Models.EnergyCalculationParameter> Parameters
    {
        get => parameters;
        set
        {
            SetProperty(ref parameters, value);

        }
    }
    private Services.Sqlite.Models.EnergyCalculationParameter selectedParameters = new Services.Sqlite.Models.EnergyCalculationParameter();
    public Services.Sqlite.Models.EnergyCalculationParameter SelectedParameters
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

