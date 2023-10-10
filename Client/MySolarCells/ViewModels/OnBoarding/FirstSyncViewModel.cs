namespace MySolarCells.ViewModels.OnBoarding;

public class FirstSyncViewModel : BaseViewModel
{
    private readonly ITibberService tibberService;
    private IInverterServiceInterface inverterService;
    //private bool keepUploading = true;
    public FirstSyncViewModel(ITibberService tibberService)
    {
        this.tibberService = tibberService;
        using var dbContext = new MscDbContext();
        this.inverterService = ServiceHelper.GetInverterService(dbContext.Inverter.First().InverterTyp);

    }

    public ICommand SyncCommand => new Command(async () => await Sync());

    private async Task Sync()
    {
        StartbuttonEnable = false;
        var difference = DateTime.Now - MySolarCellsGlobals.SelectedHome.FromDate;

        var days = difference.Days;
        var hours = difference.Hours;
        var totalhours = (days * 24) + hours;

        var progress = new Progress<int>(currentDay =>
        {
            CalculateProgress(currentDay, totalhours);
        });

        await Task.Delay(200);
        //keepUploading = true;
        if (SettingsService.OnboardingStatus == OnboardingStatusEnum.EnergyCalculationparametersSelected)
        {
            ShowProgressStatus = true;
            ProgressStatus = "Import consumation and sold production.";
            ProgressSubStatus = "saved rows 0";
            await Task.Delay(200);
            var result = await this.tibberService.SyncConsumptionAndProductionFirstTime(MySolarCellsGlobals.SelectedHome.FromDate, progress, 0);
            if (!result)
            {

                await DialogService.ShowAlertAsync("Error import consumation and sold production.", AppResources.My_Solar_Cells, AppResources.Ok);

            }
            else
            {
                SettingsService.OnboardingStatus = OnboardingStatusEnum.FirstImportElectricitySupplierIsDone;
            }
        }

        if (SettingsService.OnboardingStatus == OnboardingStatusEnum.FirstImportElectricitySupplierIsDone)
        {
            ShowProgressStatus = true;
            ProgressStatus = "Import solar own use and calculate profit.";
            ProgressSubStatus = "saved rows 0";
            await Task.Delay(200);
            using var dbContext = new MscDbContext();
            var inverter = await dbContext.Inverter.FirstOrDefaultAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);

            var differenceInverter = DateTime.Now - inverter.FromDate;

            var daysInv = differenceInverter.Days;
            var hoursInv = differenceInverter.Hours;
            var totalhoursInv = (daysInv * 24) + hoursInv;
            progress = new Progress<int>(currentDay =>
           {
               CalculateProgress(currentDay, totalhoursInv);
           });
            var result = await this.inverterService.SyncProductionOwnUse(inverter.FromDate, progress, 0);
            if (!result)
            {
                await DialogService.ShowAlertAsync("Error import solar own use and calculate profit", AppResources.My_Solar_Cells, AppResources.Ok);
            }
            else
            {
                SettingsService.OnboardingStatus = OnboardingStatusEnum.OnboardingDone;
                App.Current.MainPage = new AppShell();
            }
            
        }
    }

    /// <summary>
    /// Calculates the progress to show on the upload progress bar using the total file size and the amount already uploaded.
    /// </summary>
    /// <param name="completed">The amount of bytes that have already been uploaded to the server.</param>
    /// <param name="total">The total file size in bytes of the file being uploaded to the server.</param>
    private void CalculateProgress(long completed, long total)
    {
        var comp = Convert.ToDouble(completed);
        var tot = Convert.ToDouble(total);
        var percentage = comp / tot;
        //UploadProgress.ProgressTo(percentage, 100, Easing.Linear);
        ProgressProcent = (float)percentage * 100;
        ProgressSubStatus = "saved rows " + completed.ToString();
    }
    private bool _startbuttonEnable = true;
    public bool StartbuttonEnable
    {
        get { return _startbuttonEnable; }
        set { SetProperty(ref _startbuttonEnable, value); }
    }

    private bool _showProgressStatus;
    public bool ShowProgressStatus
    {
        get { return _showProgressStatus; }
        set { SetProperty(ref _showProgressStatus, value); }
    }
    private string _progessStatus;
    public string ProgressStatus
    {
        get { return _progessStatus; }
        set { SetProperty(ref _progessStatus, value); }
    }
    private string _progressSubStatus;
    public string ProgressSubStatus
    {
        get { return _progressSubStatus; }
        set { SetProperty(ref _progressSubStatus, value); }
    }
    private float _progressProcent;
    public float ProgressProcent
    {
        get { return _progressProcent; }
        set { SetProperty(ref _progressProcent, value); }
    }
}

