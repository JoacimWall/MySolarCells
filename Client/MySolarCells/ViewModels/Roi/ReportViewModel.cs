using MySolarCellsSQLite.Sqlite;
using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells.ViewModels.Roi;

public class ReportViewModel : BaseViewModel
{
    readonly IHistoryDataService historyDataService;
    readonly IRoiService roiService;
    private readonly MscDbContext mscDbContext;
    public ReportViewModel(IHistoryDataService historyDataService, IRoiService roiService,  MscDbContext mscDbContext,IDialogService dialogService,
        IAnalyticsService analyticsService, IInternetConnectionHelper internetConnectionHelper, ILogService logService,ISettingsService settingsService): base(dialogService, analyticsService, internetConnectionHelper,
        logService,settingsService)
    {
        
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
        selReportPage = new ReportModel();
    }
    public ICommand MoreButtonCommand => new Command(async () => await MoreButton());

    private Task MoreButton()
    {
        ShowSavingEstimateIsVisible = !showSavingEstimateIsVisible;
        return Task.CompletedTask;
    }
    
    public ICommand RefreshSavingEstimateCommand => new Command(async () => await ReloadSavingsEstimate(false));
    public ICommand ReloadGraphDataCommand => new Command(async () => await RefreshAsync( ));
    public ICommand PrevPageCommand => new Command(async () => await PrevPage());
    public ICommand NextPageCommand => new Command(async () => await NextPage());
    private async Task PrevPage()
    {
        _ = (ProgressDialog)DialogService.GetProgress(AppResources.Loading);
        await Task.Delay(300);
        for (int i = ReportData.Count - 1; i >= 0; i--)
        {
            if (ReportData[i] == SelReportPage && i > 0)
            {
                SelReportPage = ReportData[i - 1];
                return;
            }
        }

    }

    public override async Task OnDisappearingAsync()
    {
        await mscDbContext.SaveChangesAsync();
    }

    private async Task NextPage()
    {
        using var dlg = (ProgressDialog)DialogService.GetProgress(AppResources.Loading);
        await Task.Delay(300);
        for (int i = 0; i < ReportData.Count; i++)
        {
            if (ReportData[i] == SelReportPage && i < ReportData.Count - 1)
            {
                SelReportPage = ReportData[i + 1];
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
        resultHistory = await historyDataService.GenerateTotalPerMonthReport(MySolarCellsGlobals.SelectedHome.FromDate, DateTime.Today);
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
                                EstimateRoi = resultRoi.Model, ReportPageTyp = ReportPageTyp.SavingEstimate,
                                ReportTitle = AppResources.Savings_Calculation
                            });
                            ReportData.Add(new ReportModel
                            {
                                Stats = resultHistory.Model.Item1, ReportPageTyp = ReportPageTyp.YearsOverview,
                                ReportTitle = AppResources.Year_Overview
                            });
                            foreach (var item in resultHistory.Model.Item2)
                            {
                                ReportData.Add(new ReportModel
                                {
                                    Stats = item, ReportPageTyp = ReportPageTyp.YearDetails,
                                    ReportTitle = string.Format(AppResources.Details_Year, item.First().FromDate.Year)
                                });
                            }
                            await Task.Delay(300);
                            SelReportPage = ReportData.First();
                        }
                        else
                        {
                            ReportData[0] = new ReportModel { EstimateRoi = resultRoi.Model, ReportPageTyp = ReportPageTyp.SavingEstimate, ReportTitle = AppResources.Savings_Calculation };
                            SelReportPage = ReportData.First();
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
    private ReportModel selReportPage;
    public ReportModel SelReportPage
    {
        get => selReportPage;
        set => SetProperty(ref selReportPage, value);
    }
    private List<ReportModel> reportData = new List<ReportModel>();
    public List<ReportModel> ReportData
    {
        get => reportData;
        set => SetProperty(ref reportData, value);
    }

}

