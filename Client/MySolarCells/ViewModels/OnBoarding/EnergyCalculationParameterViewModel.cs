
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
    public ICommand AddParametersCommand => new Command( () =>  AddParameters());

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
            SettingsService.OnboardingStatus = OnboardingStatusEnum.EnergyCalculationparametersSelected;
            await GoToAsync(nameof(FirstSyncView));
            //TODO:Kolla så vi inte får med konsting time stamp på fromDate
            //var parametersExist = await dbContext.EnergyCalculationParameter.FirstOrDefaultAsync(x => x.EnergyCalculationParameterId == selectedParameters.EnergyCalculationParameterId && x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
            //if (parametersExist == null)
            //{
            //    parametersExist = new Services.Sqlite.Models.EnergyCalculationParameter
            //    {
            //        FromDate = selectedParameters.FromDate,
            //        ProdCompensationElectricityLowload = selectedParameters.ProdCompensationElectricityLowload,
            //        TransferFee = selectedParameters.TransferFee,
            //        TaxReduction = selectedParameters.TaxReduction,
            //        EnergyTax = selectedParameters.EnergyTax,
            //        UseSpotPrice = selectedParameters.UseSpotPrice,
            //        FixedPriceKwh = selectedParameters.FixedPriceKwh,
            //        HomeId = MySolarCellsGlobals.SelectedHome.HomeId

            //    };
            //    //TODO:Do we neeed more info from tibber homes
            //    await dbContext.EnergyCalculationParameter.AddAsync(parametersExist);
            //    await dbContext.SaveChangesAsync();
            //    SettingsService.OnboardingStatus = OnboardingStatusEnum.EnergyCalculationparametersSelected;
            //    await GoToAsync(nameof(FirstSyncView));
            //}
            //else
            //{
            //    await DialogService.ShowAlertAsync("You cant save the same from date as exist", AppResources.My_Solar_Cells, AppResources.Ok);
            //}
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
        set { SetProperty(ref selectedParameters, value); }
    }
    

}

