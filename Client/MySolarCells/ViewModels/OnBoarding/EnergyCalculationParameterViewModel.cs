
namespace MySolarCells.ViewModels.OnBoarding;

public class EnergyCalculationParameterViewModel : BaseViewModel
{
    public EnergyCalculationParameterViewModel()
    {

    }

    public ICommand SaveCommand => new Command(async () => await Save());

    private async Task Save()
    {
        try
        {
            using var dbContext = new MscDbContext();
            //TODO:Kolla så vi inte får med konsting time stamp på fromDate
            var parametersExist = await dbContext.EnergyCalculationParameter.FirstOrDefaultAsync(x => x.FromDate == parameters.FromDate && x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);
            if (parametersExist == null)
            {
                parametersExist = new Services.Sqlite.Models.EnergyCalculationParameter
                {
                    FromDate = parameters.FromDate,
                    ProdCompensationElectricityLowload = parameters.ProdCompensationElectricityLowload,
                    TransferFee = parameters.TransferFee,
                    TaxReduction = parameters.TaxReduction,
                    EnergyTax = parameters.EnergyTax,
                    HomeId = MySolarCellsGlobals.SelectedHome.HomeId

                };
                //TODO:Do we neeed more info from tibber homes
                await dbContext.EnergyCalculationParameter.AddAsync(parametersExist);
                await dbContext.SaveChangesAsync();
                SettingsService.OnboardingStatus = OnboardingStatusEnum.EnergyCalculationparametersSelected;
                await GoToAsync(nameof(FirstSyncView));
            }
            else
            {
                await DialogService.ShowAlertAsync("You cant ahve the same from date as exist", AppResources.My_Solar_Cells, AppResources.Ok);
            }
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(ex.Message, AppResources.My_Solar_Cells, AppResources.Ok);
        }

    }

    private Services.Sqlite.Models.EnergyCalculationParameter parameters = new Services.Sqlite.Models.EnergyCalculationParameter();
    public Services.Sqlite.Models.EnergyCalculationParameter Parameters
    {
        get => parameters;
        set { SetProperty(ref parameters, value); }
    }

}

