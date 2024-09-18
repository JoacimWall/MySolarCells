namespace MySolarCells.ViewModels.Roi;

public class ReportViewModel : BaseViewModel
{
    readonly IHistoryDataService historyDataService;
    readonly IRoiService roiService;
    readonly ISetOrientationService setOrientationService;
    private readonly MscDbContext mscDbContext;
    public ReportViewModel(ISetOrientationService setOrientationService,IHistoryDataService historyDataService, IRoiService roiService,  MscDbContext mscDbContext,IDialogService dialogService,
        IAnalyticsService analyticsService, IInternetConnectionService internetConnectionService, ILogService logService,ISettingsService settingsService,IHomeService homeService): base(dialogService, analyticsService, internetConnectionService,
        logService,settingsService,homeService)
    {
        this.setOrientationService = setOrientationService;
        this.historyDataService = historyDataService;
        this.roiService = roiService;
        this.mscDbContext = mscDbContext;
        
        var exist = this.mscDbContext.SavingEstimateParameters.FirstOrDefault();
        if (exist is null)
        {
            exist = new SavingEstimateParameters();
            this.mscDbContext.SavingEstimateParameters.Add(exist);
            this.mscDbContext.SaveChanges();
        }
        savingEstimateParameters = exist;
        selectedReportPage = new ReportModel();
        
    }
    
    private Task MoreButton()
    {
        ShowSavingEstimateIsVisible = !showSavingEstimateIsVisible;
        return Task.CompletedTask;
    }
    
    public ICommand RefreshSavingEstimateCommand => new Command(async () => await ReloadSavingsEstimate(false));
    public ICommand ReloadGraphDataCommand => new Command(async () => await RefreshAsync( ));
    public ICommand PrevPageCommand => new Command(async () => await PrevPage());
    public ICommand NextPageCommand => new Command(async () => await NextPage());
    public ICommand MoreButtonCommand => new Command(async () => await MoreButton());
    public ICommand FullScreenCommand => new Command(async () => await FullScreenButton());

    private async Task FullScreenButton()
    {
        FullScreen = true;
        ReportView? view = ServiceHelper.GetService<ReportView>();
        ((ReportViewModel)view.BindingContext).SavingEstimateParameters = savingEstimateParameters;
        ((ReportViewModel)view.BindingContext).SelectedReportPage = selectedReportPage;
        ((ReportViewModel)view.BindingContext).ReportData = reportData;
        ((ReportViewModel)view.BindingContext).FullScreen = true;
        ((ReportViewModel)view.BindingContext).FirstTimeAppearing = false;
        await PushModal(view);
        FullScreen = false;
    }

    private async Task PrevPage()
    {
        using var dlg  = (ProgressDialog)DialogService.GetProgress(AppResources.Loading);
        await Task.Delay(300);
        for (int i = ReportData.Count - 1; i >= 0; i--)
        {
            if (ReportData[i] == SelectedReportPage && i > 0)
            {
                SelectedReportPage = ReportData[i - 1];
                return;
            }
        }

    }
    
    public override async Task OnAppearingAsync()
    {
        if (fullScreen)
        {
            setOrientationService.OnlyLandscape();
        }
        if (FirstTimeAppearing)
        {
            

            await RefreshAsync();
        }
        
        await base.OnAppearingAsync();
    }

    public override async Task OnDisappearingAsync()
    {
        await mscDbContext.SaveChangesAsync();
        if (fullScreen)
        {
            setOrientationService.OnlyPortrait();
        }
    }

    private async Task NextPage()
    {
        using var dlg = (ProgressDialog)DialogService.GetProgress(AppResources.Loading);
        await Task.Delay(300);
        for (int i = 0; i < ReportData.Count; i++)
        {
            if (ReportData[i] == SelectedReportPage && i < ReportData.Count - 1)
            {
                SelectedReportPage = ReportData[i + 1];
                return;
            }
        }
    }

    public override async Task RefreshAsync(TsViewState layoutState = TsViewState.Refreshing, bool showProgress = true)
    {
        using var dlg = (ProgressDialog)DialogService.GetProgress(AppResources.Generating_Report_Please_Wait);
        await Task.Delay(300);
        await ReloadHistoryData();
        await ReloadSavingsEstimate(true);
    }

    private Result<Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>>>? resultHistory;
    private async Task ReloadHistoryData()
    {
        resultHistory = await historyDataService.GenerateTotalPerMonthReport(HomeService.FirstElectricitySupplier().FromDate, DateTime.Today,new HistorySimulate());
        if (!resultHistory.WasSuccessful)
        {
            await DialogService.ShowAlertAsync(resultHistory.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
        }
    }
    private async Task<bool> ReloadSavingsEstimate(bool firstTime)
    {
      
        
        using var dlg = (ProgressDialog)DialogService.GetProgress(AppResources.Generating_Report_Please_Wait);
        await Task.Delay(300);

        if (resultHistory is { Model: not null })
        {
           
                var resultRoi = roiService.CalcSavingsEstimate(resultHistory.Model, SavingEstimateParameters);
                if (!resultRoi.WasSuccessful)
                {
                    await DialogService.ShowAlertAsync(resultRoi.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
                }
                else
                {
                    if (resultRoi.Model != null)
                    {
                        if (firstTime)
                        {
                            ReportData.Add(new ReportModel
                            {
                                EstimateRoi = resultRoi.Model, ReportPageType = ReportPageType.SavingEstimate,
                                ReportTitle = AppResources.Savings_Calculation
                            });
                            ReportData.Add(new ReportModel
                            {
                                Stats = resultHistory.Model.Item1, ReportPageType = ReportPageType.YearsOverview,
                                ReportTitle = AppResources.Year_Overview
                            });
                            foreach (var item in resultHistory.Model.Item2)
                            {
                                ReportData.Add(new ReportModel
                                {
                                    Stats = item, ReportPageType = ReportPageType.YearDetails,
                                    ReportTitle = string.Format(AppResources.Details_Year, item.First().FromDate.Year)
                                });
                            }
                            await Task.Delay(300);
                            SelectedReportPage = ReportData.First();
                        }
                        else
                        {
                            ReportData[0] = new ReportModel { EstimateRoi = resultRoi.Model, ReportPageType = ReportPageType.SavingEstimate, ReportTitle = AppResources.Savings_Calculation };
                            SelectedReportPage = ReportData.First();
                        }
                    }
                }
            
        }

        return true;
    }

    
    //private HistorySimulate roiSimulate = new HistorySimulate();
    //public HistorySimulate RoiSimulate
    //{
    //    get { return roiSimulate; }
    //    set { SetProperty(ref roiSimulate, value); }
    //}
    private float scaleFactor = 14;
    public float ScaleFactor
    {
        get => scaleFactor;
        set => SetProperty(ref scaleFactor, value);
    }
    private bool fullScreen = false;
    public bool FullScreen
    {
        get => fullScreen;
        set => SetProperty(ref fullScreen, value);
    }
    public ColumnDefinitionCollection Columns
    {
        get
        {   //64,44,44,64,64,64,64,88,108"
            var columns = new ColumnDefinitionCollection();
            var column1=new ColumnDefinition();
            var column2 = new ColumnDefinition();
            var column3 = new ColumnDefinition();
            var column4 = new ColumnDefinition();
            var column5 = new ColumnDefinition();
            var column6 = new ColumnDefinition();
            var column7 = new ColumnDefinition();
            var column8 = new ColumnDefinition();
            var column9 = new ColumnDefinition();
            column1.Width = 6 * scaleFactor;
            column2.Width = 4 * scaleFactor;
            column3.Width =  4 * scaleFactor;
            column4.Width =  6 * scaleFactor;
            column5.Width =  6 * scaleFactor;
            column6.Width =  6 * scaleFactor;
            column7.Width =  6 * scaleFactor;
            column8.Width =  9 * scaleFactor;
            column9.Width =  11 * scaleFactor;
            columns.Add(column1);
            columns.Add(column2);
            columns.Add(column3);
            columns.Add(column4);
            columns.Add(column5);
            columns.Add(column6);
            columns.Add(column7);
            columns.Add(column8);
            columns.Add(column9);
            return columns;
        }

    }
    private bool showSavingEstimateIsVisible;
    public bool ShowSavingEstimateIsVisible
    {
        get => showSavingEstimateIsVisible;
        set => SetProperty(ref showSavingEstimateIsVisible, value);
    }

    private SavingEstimateParameters savingEstimateParameters;
    public SavingEstimateParameters SavingEstimateParameters
    {
        get => savingEstimateParameters;
        set => SetProperty(ref savingEstimateParameters, value);
    }
    private ReportModel selectedReportPage;
    public ReportModel SelectedReportPage
    {
        get => selectedReportPage;
        set => SetProperty(ref selectedReportPage, value);
    }
    private List<ReportModel> reportData = new List<ReportModel>();
    public List<ReportModel> ReportData
    {
        get => reportData;
        set => SetProperty(ref reportData, value);
    }

}

