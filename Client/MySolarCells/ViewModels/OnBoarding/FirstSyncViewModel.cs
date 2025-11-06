namespace MySolarCells.ViewModels.OnBoarding;

public class FirstSyncViewModel : BaseViewModel
{
    private readonly IGridSupplierInterface gridSupplierService;
    private readonly IInverterServiceInterface inverterService;
    private readonly MscDbContext mscDbContext;
    public FirstSyncViewModel(MscDbContext mscDbContext, IDialogService dialogService,
        IAnalyticsService analyticsService, IInternetConnectionService internetConnectionService, ILogService logService, ISettingsService settingsService, IHomeService homeService) : base(dialogService, analyticsService, internetConnectionService,
        logService, settingsService, homeService)
    {
        this.mscDbContext = mscDbContext;
        gridSupplierService = ServiceHelper.GetGridSupplierService(this.mscDbContext.ElectricitySupplier.First().ElectricitySupplierType);
        inverterService = ServiceHelper.GetInverterService(this.mscDbContext.Inverter.First().InverterTyp);
    }

    public ICommand SyncCommand => new Command(async () => await Sync());

    private async Task Sync()
    {
        StartButtonEnable = false;
        var difference = DateTime.Now - HomeService.FirstElectricitySupplier().FromDate;

        var days = difference.Days;
        var hours = difference.Hours;
        var totalHours = (days * 24) + hours;

        var progress = new Progress<int>(currentDay =>
        {
            CalculateProgress(currentDay, totalHours);
        });

        await Task.Delay(200);
        //keepUploading = true;
        if (SettingsService.OnboardingStatus == OnboardingStatusEnum.InvestmentAndLoanDone)
        {
            ShowProgressStatus = true;
            ProgressStatus = string.Format(AppResources.Step_Nr_of_Nr_InfoText, "1", "2", AppResources.Import_Data_From_Electricity_Supplier);
            ProgressSubStatus = string.Format(AppResources.Saved_Rows_Amount, "0");
            await Task.Delay(200);
            var result = await gridSupplierService.Sync(HomeService.FirstElectricitySupplier().FromDate, progress, 0);
            if (!result.WasSuccessful)
            {
                await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
                StartButtonEnable = true;
                return;
            }
            else
            {
                SettingsService.OnboardingStatus = OnboardingStatusEnum.FirstImportElectricitySupplierIsDone;
            }
        }

        if (SettingsService.OnboardingStatus == OnboardingStatusEnum.FirstImportElectricitySupplierIsDone)
        {
            ShowProgressStatus = true;
            ProgressStatus = string.Format(AppResources.Step_Nr_of_Nr_InfoText, "2", "2", AppResources.Import_Data_From_Inverter);
            ProgressSubStatus = string.Format(AppResources.Saved_Rows_Amount, "0");
            await Task.Delay(200);

            var inverter = await mscDbContext.Inverter.OrderByDescending(s => s.FromDate).FirstAsync(x => x.HomeId == HomeService.CurrentHome().HomeId);

            var differenceInverter = DateTime.Now - inverter.FromDate;

            var inverterDays = differenceInverter.Days;
            var inverterHours = differenceInverter.Hours;
            var totalHoursInverter = (inverterDays * 24) + inverterHours;
            progress = new Progress<int>(currentDay =>
           {
               CalculateProgress(currentDay, totalHoursInverter);
           });
            var result = await inverterService.Sync(inverter.FromDate, progress, 0);
            if (!result.WasSuccessful)
            {
                if (result.Model != null && !string.IsNullOrEmpty(result.Model.Message))
                    await DialogService.ShowAlertAsync(result.Model.Message, AppResources.My_Solar_Cells, AppResources.Ok);

                await DialogService.ShowAlertAsync(AppResources.Error_Import_Data_From_Inverter, AppResources.My_Solar_Cells, AppResources.Ok);
                StartButtonEnable = true;
            }
            else
            {
                if (result.Model != null && !string.IsNullOrEmpty(result.Model.Message))
                    await DialogService.ShowAlertAsync(result.Model.Message, AppResources.My_Solar_Cells, AppResources.Ok);

                SettingsService.OnboardingStatus = OnboardingStatusEnum.OnboardingDone;
                if (Application.Current != null)
                    Application.Current.MainPage = new AppShell();
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
        ProgressPercent = (float)percentage * 100;
        ProgressSubStatus = string.Format(AppResources.Saved_Rows_Amount, completed.ToString());
    }
    private bool startButtonEnable = true;
    public bool StartButtonEnable
    {
        get => startButtonEnable;
        set => SetProperty(ref startButtonEnable, value);
    }

    private bool showProgressStatus;
    public bool ShowProgressStatus
    {
        get => showProgressStatus;
        set => SetProperty(ref showProgressStatus, value);
    }
    private string progressStatus = "";
    public string ProgressStatus
    {
        get => progressStatus;
        set => SetProperty(ref progressStatus, value);
    }
    private string progressSubStatus = "";
    public string ProgressSubStatus
    {
        get => progressSubStatus;
        set => SetProperty(ref progressSubStatus, value);
    }
    private float progressPercent;
    public float ProgressPercent
    {
        get => progressPercent;
        set => SetProperty(ref progressPercent, value);
    }
}

