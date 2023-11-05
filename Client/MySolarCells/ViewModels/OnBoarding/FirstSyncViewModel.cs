namespace MySolarCells.ViewModels.OnBoarding;

public class FirstSyncViewModel : BaseViewModel
{
    private readonly IGridSupplierInterface gridSupplierService;
    private IInverterServiceInterface inverterService;
    private readonly MscDbContext mscDbContext;
    public FirstSyncViewModel(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
        this.gridSupplierService = ServiceHelper.GetGridSupplierService(this.mscDbContext.Home.First().ElectricitySupplier); ;
        this.inverterService = ServiceHelper.GetInverterService(this.mscDbContext.Inverter.First().InverterTyp);
        //DateTime dateTime = new DateTime(2023, 11, 1);
        //long milliseconds = DateHelper.DateTimeToMillis(dateTime);
        //DateTime dattest = DateHelper.MillisToDateTime(milliseconds);
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
        if (SettingsService.OnboardingStatus == OnboardingStatusEnum.InvestmentAndLoanDone)
        {
            ShowProgressStatus = true;
            ProgressStatus =  AppResources.Import_Data_From_Electricity_Supplier;
            ProgressSubStatus = string.Format(AppResources.Saved_Rows_Amount,"0");
            await Task.Delay(200);
            var result = await this.gridSupplierService.Sync(MySolarCellsGlobals.SelectedHome.FromDate, progress, 0);
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
            ProgressStatus = AppResources.Import_Data_From_Inverter;
            ProgressSubStatus = string.Format(AppResources.Saved_Rows_Amount, "0");
            await Task.Delay(200);
            
            var inverter = await this.mscDbContext.Inverter.FirstOrDefaultAsync(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId);

            var differenceInverter = DateTime.Now - inverter.FromDate;

            var daysInv = differenceInverter.Days;
            var hoursInv = differenceInverter.Hours;
            var totalhoursInv = (daysInv * 24) + hoursInv;
            progress = new Progress<int>(currentDay =>
           {
               CalculateProgress(currentDay, totalhoursInv);
           });
            var result = await this.inverterService.Sync(inverter.FromDate, progress, 0);
            if (!result.WasSuccessful)
            {
                if (result.Model != null && !string.IsNullOrEmpty(result.Model.Message))
                    await DialogService.ShowAlertAsync(result.Model.Message, AppResources.My_Solar_Cells, AppResources.Ok);

                await DialogService.ShowAlertAsync(AppResources.Error_Import_Data_From_Inverter, AppResources.My_Solar_Cells, AppResources.Ok);

            }
            else
            {
                if (result.Model != null && !string.IsNullOrEmpty(result.Model.Message))
                    await DialogService.ShowAlertAsync(result.Model.Message, AppResources.My_Solar_Cells, AppResources.Ok);

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
        ProgressSubStatus = string.Format(AppResources.Saved_Rows_Amount, completed.ToString()); 
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

