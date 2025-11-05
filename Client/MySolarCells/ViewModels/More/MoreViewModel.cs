using System.Globalization;
using MySolarCellsSQLite.Sqlite;
using MySolarCellsSQLite.Sqlite.Models;
using Syncfusion.XlsIO;

namespace MySolarCells.ViewModels.More;

public class MoreViewModel : BaseViewModel
{
    private readonly IHistoryDataService historyService;
    private readonly IRoiService roiService;
    private readonly IDataSyncService dataSyncService;
    private readonly IThemeService themeService;
    private readonly MscDbContext mscDbContext;

    public MoreViewModel(IHistoryDataService historyService, IDataSyncService dataSyncService, IRoiService roiService, MscDbContext mscDbContext, IDialogService dialogService,
        IAnalyticsService analyticsService, IInternetConnectionService internetConnectionService, ILogService logService, ISettingsService settingsService, IHomeService homeService, IThemeService themeService) : base(dialogService, analyticsService, internetConnectionService,
        logService, settingsService, homeService)
    {
        this.historyService = historyService;
        this.roiService = roiService;
        this.mscDbContext = mscDbContext;
        this.dataSyncService = dataSyncService;
        this.themeService = themeService;
        electricitySupplier = homeService.FirstElectricitySupplier();
        AppInfoVersion = AppInfo.VersionString + "(" + AppInfo.BuildString + ")";

        // Subscribe to theme changes
        themeService.ThemeChanged += OnThemeChanged;
    }

    private void OnThemeChanged(object sender, AppTheme theme)
    {
        OnPropertyChanged(nameof(CurrentThemeText));
        OnPropertyChanged(nameof(CurrentThemeDisplayText));
        OnPropertyChanged(nameof(ThemeIcon));
    }

    public ICommand ShowInvestAndLonCommand => new Command(async () => await ShowInvestAndLon());
    public ICommand ShowCalcParametersCommand => new Command(async () => await ShowCalcParameters());
    public ICommand ElectricitySupplierCommand => new Command(async () => await ShowElectricitySupplier());
    public ICommand ExportExcelCommand => new Command(async () => await ExportExcelGroupYear(HomeService.CurrentHome().HomeId));
    public ICommand InverterSettingsCommand => new Command(async () => await ShowInverterSettings());
    public ICommand ShowReportCommand => new Command(async () => await ShowReport());
    public ICommand ShowPowerTariffCommand => new Command(async () => await ShowPowerTariff());
    public ICommand ShowPickCountryCommand => new Command(async () => await ShowPickCountry());
    public ICommand FixTimeGampInDbCommand => new Command(async () => await FixTimeGampInDb());
    public ICommand ToggleThemeCommand => new Command(async () => await ToggleTheme());

    private async Task FixTimeGampInDb()
    {
        using var dlg = (ProgressDialog)DialogService.GetProgress(AppResources.Import_Data);
        await Task.Delay(200);
        var result = await dataSyncService.Sync(true);
        if (result is { WasSuccessful: false })
        {
            await DialogService.ShowAlertAsync(result.ErrorMessage, AppResources.My_Solar_Cells, AppResources.Ok);
        }
        else
        {
            if (result.Model != null) DialogService.ShowToast(result.Model.Message);
        }
    }

    private async Task ShowPickCountry()
    {
        //await GoToAsync(nameof(SelectLanguageCountryView));
        var view = ServiceHelper.GetService<SelectLanguageCountryView>();
        //await ((ReportViewModel)view.BindingContext).RefreshAsync(ViewState.Refreshing);
        await PushModal(view);
    }

    private async Task ShowPowerTariff()
    {
        await GoToAsync(nameof(PowerTariffParameterView));
    }

    private async Task ShowReport()
    {
        //using var dlg = DialogService.GetProgress(AppResources.Generating_Report_Please_Wait);
        //await Task.Delay(200);
        var view = ServiceHelper.GetService<ReportView>();
        await ((ReportViewModel)view.BindingContext).RefreshAsync();
        await PushModal(view);
    }

    private async Task ShowElectricitySupplier()
    {
        await GoToAsync(nameof(ElectricitySupplierView));
    }


    private async Task ShowInverterSettings()
    {
        await GoToAsync(nameof(InverterView));
    }

    private async Task ShowCalcParameters()
    {
        await GoToAsync(nameof(EnergyCalculationParameterView));
        //await GoToAsync(nameof(ParametersOverviewView));
    }
    public override async Task OnAppearingAsync()
    {
        //Show log
        logText.Clear();

        var logs = mscDbContext.Log.OrderByDescending(x => x.CreateDate);
        //await this.mscDbContext.Log.ExecuteDelete(x => x.);
        foreach (var item in logs)
        {
            logText.Add(item.CreateDate.ToString(CultureInfo.InvariantCulture) + " " + item.LogTitle);
        }
        OnPropertyChanged(nameof(LogText));
        await base.OnAppearingAsync();
    }
    private ObservableCollection<string> logText = new ObservableCollection<string>();
    public ObservableCollection<string> LogText
    {
        get => logText;
        set => SetProperty(ref logText, value);
    }
    private async Task<bool> GenerateYearWorkSheet(IWorksheet worksheet, List<ReportHistoryStats> roiStats, int electricitySupplierId, bool isOverviewSheet)
    {
        //Disable gridlines in the worksheet
        worksheet.IsGridLinesVisible = true;

        #region Year or month rows
        //< !--PRODUCTION AND CONSUMPTION-->
        worksheet.Range["A1"].Text = AppResources.Production_And_Consumption;
        worksheet.Range["A2"].Text = AppResources.Sold;
        worksheet.Range["A3"].Text = AppResources.Battery_Charge;
        worksheet.Range["A4"].Text = AppResources.Own_Use;
        worksheet.Range["A5"].Text = AppResources.Purchased;
        worksheet.Range["A6"].Text = AppResources.Battery_used;
        worksheet.Range["A7"].Text = AppResources.Energy_Peak_Reduction;
        worksheet.Range["A8"].Text = AppResources.Production;
        worksheet.Range["A9"].Text = AppResources.Consumption;
        worksheet.Range["A10"].Text = AppResources.Balance_Production_Minus_Consumption;
        //<!-- COSTS AND REVENUES-->
        worksheet.Range["A11"].Text = AppResources.Costs_And_Revenues;
        worksheet.Range["A12"].Text = AppResources.Production_Profit_Sold;
        worksheet.Range["A13"].Text = AppResources.Production_Grid_Compensation;
        worksheet.Range["A14"].Text = AppResources.Production_Saved_Energy_Tax;
        worksheet.Range["A15"].Text = AppResources.Own_Use_Saved_Cost;
        worksheet.Range["A16"].Text = AppResources.Own_Use_Saved_Transfer_Fee;
        worksheet.Range["A17"].Text = AppResources.Own_Use_Saved_Energy_Tax;
        worksheet.Range["A18"].Text = AppResources.Saved_Cost_Battery;
        worksheet.Range["A19"].Text = AppResources.Saved_Transfer_Fee_Battery;
        worksheet.Range["A20"].Text = AppResources.Saved_Energy_Tax_Battery;
        worksheet.Range["A21"].Text = AppResources.Purchase_Cost;
        worksheet.Range["A22"].Text = AppResources.Purchase_Transfer_Fee;
        worksheet.Range["A23"].Text = AppResources.Purchase_Energy_Tax;
        worksheet.Range["A24"].Text = AppResources.Energy_Peak_Reduction_Saved;
        worksheet.Range["A25"].Text = AppResources.Production;
        worksheet.Range["A26"].Text = AppResources.Interest;
        worksheet.Range["A27"].Text = AppResources.Consumption;
        worksheet.Range["A28"].Text = AppResources.Balance_Production_Minus_Consumption;
        // <!--FUN FACTS-->

        worksheet.Range["A29"].Text = AppResources.Fun_Facts;
        worksheet.Range["A30"].Text = AppResources.Production_Index_Desc_Prod_Per_Day_KWh;
        worksheet.Range["A31"].Text = AppResources.Average_Purchased_Cost_Per_Kwh;
        worksheet.Range["A32"].Text = AppResources.Average_Production_Sold_Profit_Per_Kwh;
        worksheet.Range["A33"].Text = AppResources.Average_Production_Own_Use_Saved_Per_Kwh;
        worksheet.Range["A34"].Text = AppResources.Average_Battery_Used_Saved_Per_Kwh;

        //if (isOverviewSheet)
        //{
        //    worksheet.Range["A35"].Text = AppResources.ROI;
        //    worksheet.Range["A35"].CellStyle.Font.Bold = true;
        //    worksheet.Range["A36"].Text = AppResources.Years_Left_Until_ROI;
        //    worksheet.Range["A37"].Text = AppResources.Total_Investment_And_Loan;
        //    worksheet.Range["A38"].Text = AppResources.Total_Produced_And_Saved;
        //    worksheet.Range["A39"].Text = AppResources.Left_Of_Investment_Cost;
        //    worksheet.Range["A39"].CellStyle.Font.Bold = true;
        //    worksheet.Range["A39"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

        ////}

        worksheet.Range["A1:A34"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
        worksheet.Range["A1"].ColumnWidth = 36;

        int index = 0;

        bool valueMode = true;
        string lastC = "";
        //title is in row A so we start in B
        for (char c = 'B'; c <= 'Z'; c++)
        {
            lastC = c.ToString();


            if (index == roiStats.Count)
                valueMode = false;

            //< !--PRODUCTION AND CONSUMPTION-->
            //Month/Year
            worksheet.Range[c + "1"].Text = !valueMode ? "SUM" : isOverviewSheet ? roiStats[index].FromDate.ToString("yyyy").ToUpper() : roiStats[index].FromDate.ToString("MMM").ToUpper();
            worksheet.Range[c + "2"].Value = valueMode ? roiStats[index].HistoryStats.ProductionSold.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.ProductionSold).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "3"].Value = valueMode ? roiStats[index].HistoryStats.BatteryCharge.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.BatteryCharge).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "4"].Value = valueMode ? roiStats[index].HistoryStats.ProductionOwnUse.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.ProductionOwnUse).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "5"].Value = valueMode ? roiStats[index].HistoryStats.Purchased.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.Purchased).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "6"].Value = valueMode ? roiStats[index].HistoryStats.BatteryUsed.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.BatteryUsed).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "7"].Value = valueMode ? roiStats[index].HistoryStats.PeakEnergyReduction.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.PeakEnergyReduction).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "8"].Value = valueMode ? roiStats[index].HistoryStats.SumAllProduction.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.SumAllProduction).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "9"].Value = valueMode ? roiStats[index].HistoryStats.SumAllConsumption.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.SumAllConsumption).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "10"].Value = valueMode ? roiStats[index].HistoryStats.BalanceProductionMinusConsumption.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.BalanceProductionMinusConsumption).ToString(CultureInfo.InvariantCulture);
            //<!-- COSTS AND REVENUES-->
            worksheet.Range[c + "12"].Value = valueMode ? roiStats[index].HistoryStats.ProductionSoldProfit.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.ProductionSoldProfit).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "13"].Value = valueMode ? roiStats[index].HistoryStats.ProductionSoldGridCompensationProfit.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.ProductionSoldGridCompensationProfit).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "14"].Value = valueMode ? roiStats[index].HistoryStats.ProductionSoldTaxReductionProfit.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.ProductionSoldTaxReductionProfit).ToString(CultureInfo.InvariantCulture);
            if (valueMode && !string.IsNullOrEmpty(roiStats[index].HistoryStats.ProductionSoldTaxReductionProfitComment))
            {
                worksheet.Range[c + "14"].AddThreadedComment(roiStats[index].HistoryStats.ProductionSoldTaxReductionProfitComment, DateTime.Now);
            }
            worksheet.Range[c + "15"].Value = valueMode ? roiStats[index].HistoryStats.ProductionOwnUseSaved.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.ProductionOwnUseSaved).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "16"].Value = valueMode ? roiStats[index].HistoryStats.ProductionOwnUseTransferFeeSaved.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.ProductionOwnUseTransferFeeSaved).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "17"].Value = valueMode ? roiStats[index].HistoryStats.ProductionOwnUseEnergyTaxSaved.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.ProductionOwnUseEnergyTaxSaved).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "18"].Value = valueMode ? roiStats[index].HistoryStats.BatteryUsedSaved.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.BatteryUsedSaved).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "19"].Value = valueMode ? roiStats[index].HistoryStats.BatteryUseTransferFeeSaved.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.BatteryUseTransferFeeSaved).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "20"].Value = valueMode ? roiStats[index].HistoryStats.BatteryUseEnergyTaxSaved.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.BatteryUseEnergyTaxSaved).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "21"].Value = valueMode ? roiStats[index].HistoryStats.PurchasedCost.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.PurchasedCost).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "22"].Value = valueMode ? roiStats[index].HistoryStats.PurchasedTransferFeeCost.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.PurchasedTransferFeeCost).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "23"].Value = valueMode ? roiStats[index].HistoryStats.PurchasedTaxCost.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.PurchasedTaxCost).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "24"].Value = valueMode ? roiStats[index].HistoryStats.PeakEnergyReductionSaved.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.PeakEnergyReductionSaved).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "25"].Value = valueMode ? roiStats[index].HistoryStats.SumAllProductionSoldAndSaved.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.SumAllProductionSoldAndSaved).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "26"].Value = valueMode ? roiStats[index].HistoryStats.InterestCost.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.InterestCost).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "27"].Value = valueMode ? roiStats[index].HistoryStats.SumPurchasedCost.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.SumPurchasedCost).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "28"].Value = valueMode ? roiStats[index].HistoryStats.BalanceProductionProfitMinusConsumptionCost.ToString(CultureInfo.InvariantCulture) : roiStats.Sum(x => x.HistoryStats.BalanceProductionProfitMinusConsumptionCost).ToString(CultureInfo.InvariantCulture);
            //< !--FUN FACTS-- >
            worksheet.Range[c + "30"].Value = valueMode ? roiStats[index].HistoryStats.FactsProductionIndex.ToString(CultureInfo.InvariantCulture) : Math.Round(roiStats.Average(x => x.HistoryStats.FactsProductionIndex), 2).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "31"].Value = valueMode ? roiStats[index].HistoryStats.FactsPurchasedCostAveragePerKwhPurchased.ToString(CultureInfo.InvariantCulture) : Math.Round(roiStats.Average(x => x.HistoryStats.FactsPurchasedCostAveragePerKwhPurchased), 2).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "32"].Value = valueMode ? roiStats[index].HistoryStats.FactsProductionSoldAveragePerKwhProfit.ToString(CultureInfo.InvariantCulture) : Math.Round(roiStats.Average(x => x.HistoryStats.FactsProductionSoldAveragePerKwhProfit), 2).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "33"].Value = valueMode ? roiStats[index].HistoryStats.FactsProductionOwnUseAveragePerKwhSaved.ToString(CultureInfo.InvariantCulture) : Math.Round(roiStats.Average(x => x.HistoryStats.FactsProductionOwnUseAveragePerKwhSaved), 2).ToString(CultureInfo.InvariantCulture);
            worksheet.Range[c + "34"].Value = valueMode ? roiStats[index].HistoryStats.FactsBatteryUsedAveragePerKwhSaved.ToString(CultureInfo.InvariantCulture) : Math.Round(roiStats.Average(x => x.HistoryStats.FactsBatteryUsedAveragePerKwhSaved), 2).ToString(CultureInfo.InvariantCulture);

            if (index == roiStats.Count)
                break;

            index++;
        }
        //production and Consumption Title
        worksheet.Range["B1:" + lastC + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
        worksheet.Range["A1:" + lastC + "1"].CellStyle.Font.Bold = true;
        worksheet.Range["A1:" + lastC + "1"].CellStyle.Font.Size = 14;
        //production and Consumption result rows
        //worksheet.Range["A7:" + lastC + "8"].CellStyle.Color = Syncfusion.Drawing.Color.Transparent;
        worksheet.Range["A10:" + lastC + "10"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;
        worksheet.Range["A8:" + lastC + "10"].CellStyle.Font.Bold = true;

        //Costs and Revenues Title
        worksheet.Range["A11:" + lastC + "11"].CellStyle.Font.Bold = true;
        worksheet.Range["A11:" + lastC + "11"].CellStyle.Font.Size = 14;
        //production and Consumption result rows
        //worksheet.Range["A23:" + lastC + "24"].CellStyle.Color = Syncfusion.Drawing.Color.Transparent;
        worksheet.Range["A28:" + lastC + "28"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;
        worksheet.Range["A25:" + lastC + "28"].CellStyle.Font.Bold = true;

        //FunFacts
        worksheet.Range["A29:" + lastC + "29"].CellStyle.Font.Bold = true;
        worksheet.Range["A29:" + lastC + "29"].CellStyle.Font.Size = 14;


        worksheet.Range[lastC + "2:" + lastC + "39"].CellStyle.Font.Bold = true;





        #endregion


        #region Calculation_Parameters
        if (!isOverviewSheet)
        {
            var calculationParameter = await mscDbContext.EnergyCalculationParameter.Where(x => x.ElectricitySupplierId == electricitySupplierId && x.FromDate < roiStats.Last().FromDate).OrderBy(o => o.FromDate).ToListAsync();
            //remove parameters that have no impact on current data
            bool existOne = false;
            for (int i = calculationParameter.Count - 1; i >= 0; i--)
            {   //if true remove all before 
                if (calculationParameter[i].FromDate < roiStats.First().FromDate && existOne)
                {
                    calculationParameter = calculationParameter.Where(x => x.FromDate > calculationParameter[i].FromDate).ToList();
                    break;
                } //ve must have one that is same date or before first date
                else if (calculationParameter[i].FromDate <= roiStats.First().FromDate)
                {
                    existOne = true;
                }
                
            }
            var indexParameter = 0;

            //headlines
            worksheet.Range["A41"].Text = AppResources.Calculation_Parameters;
            worksheet.Range["A41"].CellStyle.Font.Bold = true;
            worksheet.Range["A42"].Text = AppResources.From_Date;
            worksheet.Range["A43"].Text = AppResources.Compensation_Electricity_Load;
            worksheet.Range["A44"].Text = AppResources.Transfer_Fee;
            worksheet.Range["A45"].Text = AppResources.Tax_Reduction;
            worksheet.Range["A46"].Text = AppResources.Energy_Tax;
            worksheet.Range["A47"].Text = AppResources.Total_Installed_Kwh;
            worksheet.Range["A48"].Text = AppResources.Use_Spot_Prices;
            worksheet.Range["A49"].Text = AppResources.Fixed_Price_Kwh;


            for (char c = 'B'; c <= 'Z'; c++)
            {
                if (indexParameter == calculationParameter.Count)
                    break;

                worksheet.Range[c + "42"].Value = calculationParameter[indexParameter].FromDate.ToString("yyyy-MM");
                worksheet.Range[c + "42"].CellStyle.Font.Bold = true;
                worksheet.Range[c + "42"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                //Grid benefit SEK 0.078/kWh
                worksheet.Range[c + "43"].Value = calculationParameter[indexParameter].ProdCompensationElectricityLowLoad.ToString(CultureInfo.InvariantCulture);

                //Any transfer fee that costs you if we buy from or (we save for our own use) Ellevio SEK 0.3
                worksheet.Range[c + "44"].Value = calculationParameter[indexParameter].TransferFee.ToString(CultureInfo.InvariantCulture);

                //0.60/kWh electricity sold Max SEK 18,0000 and not for more kWh than the house buys in
                worksheet.Range[c + "45"].Value = calculationParameter[indexParameter].TaxReduction.ToString(CultureInfo.InvariantCulture);
                //0.49/kWh sold (we save for own use)
                worksheet.Range[c + "46"].Value = calculationParameter[indexParameter].EnergyTax.ToString(CultureInfo.InvariantCulture);

                //10.5 Kwh
                worksheet.Range[c + "47"].Value = calculationParameter[indexParameter].TotalInstallKwhPanels.ToString(CultureInfo.InvariantCulture);
                //SpotPrice
                worksheet.Range[c + "48"].Value = calculationParameter[indexParameter].UseSpotPrice.ToString();

                //fixed price
                worksheet.Range[c + "49"].Value = calculationParameter[indexParameter].FixedPriceKwh.ToString(CultureInfo.InvariantCulture);

                indexParameter++;
            }
        }
        #endregion
        return true;
    }



    private Task<bool> GenerateSavingEstimateWorksheet(IWorksheet worksheet, List<EstimateRoi> estimateRois)
    {
        //Disable gridlines in the worksheet
        worksheet.IsGridLinesVisible = true;
        worksheet.Range["A1"].Text = AppResources.Year;
        worksheet.Range["B1"].Text = "AveragePriceSold";
        worksheet.Range["C1"].Text = "AveragePrisOwnUse";
        worksheet.Range["D1"].Text = "ProductionSold";
        worksheet.Range["E1"].Text = "ProductionOwnUse";
        worksheet.Range["F1"].Text = "YearSavingsSold";
        worksheet.Range["G1"].Text = "YearSavingsOwnUse";
        worksheet.Range["H1"].Text = "ReturnPercentage";
        worksheet.Range["I1"].Text = "RemainingOnInvestment";


        //worksheet.Range["A1:A32"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //worksheet.Range["A1"].ColumnWidth = 36;
        int index = 0;
        for (int i = 2; i < estimateRois.Count + 2; i++)
        {
            worksheet.Range["A" + i].Value = estimateRois[index].Year.ToString();
            worksheet.Range["B" + i].Value = estimateRois[index].AveragePriceSold.ToString(CultureInfo.InvariantCulture);
            worksheet.Range["C" + i].Value = estimateRois[index].AveragePrisOwnUse.ToString(CultureInfo.InvariantCulture);
            worksheet.Range["D" + i].Value = estimateRois[index].ProductionSold.ToString(CultureInfo.InvariantCulture);
            worksheet.Range["E" + i].Value = estimateRois[index].ProductionOwnUse.ToString(CultureInfo.InvariantCulture);
            worksheet.Range["F" + i].Value = estimateRois[index].YearSavingsSold.ToString(CultureInfo.InvariantCulture);
            worksheet.Range["G" + i].Value = estimateRois[index].YearSavingsOwnUse.ToString(CultureInfo.InvariantCulture);
            worksheet.Range["H" + i].Value = estimateRois[index].ReturnPercentage.ToString(CultureInfo.InvariantCulture);
            worksheet.Range["I" + i].Value = estimateRois[index].RemainingOnInvestment.ToString(CultureInfo.InvariantCulture);
            index++;
        }

        //
        worksheet.Range["A1:I1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
        worksheet.Range["A1:I1"].CellStyle.Font.Bold = true;
        worksheet.Range["A1:I1"].CellStyle.Font.Size = 14;
        worksheet.Range["A1:I1"].ColumnWidth = 20;


        return Task.FromResult(true);
    }
    private async Task<bool> ExportExcelGroupYear(int homeId)
    {
        using var dlg = (ProgressDialog)DialogService.GetProgress(AppResources.Generating_Report_Please_Wait);
        await Task.Delay(200);
        var result = await historyService.GenerateTotalPerMonthReport(HomeService.FirstElectricitySupplier().FromDate, DateTime.Today,new HistorySimulate());
        
        var savingEstimate = mscDbContext.SavingEstimateParameters.FirstOrDefault() ?? new SavingEstimateParameters();
        if (result.Model == null) return false;
        var resultRoi = roiService.CalcSavingsEstimate(result.Model, savingEstimate);
        if (resultRoi.Model == null)
            return false;

        using ExcelEngine excelEngine = new ExcelEngine();
        Syncfusion.XlsIO.IApplication application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        //Create a workbook with a worksheet
        string[] nameOverview = new string[2];
        nameOverview[0] = "Savings estimate";
        nameOverview[1] = "Overview";
        IWorkbook workbook = application.Workbooks.Create(nameOverview);
        await GenerateSavingEstimateWorksheet(workbook.Worksheets[0], resultRoi.Model);
        await GenerateYearWorkSheet(workbook.Worksheets[1], result.Model.Item1, homeId, true);
        foreach (var itemList in result.Model.Item2)
        {
            workbook.Worksheets.Create(itemList.First().FromDate.Year.ToString());
            await GenerateYearWorkSheet(workbook.Worksheets[workbook.Worksheets.Count - 1], itemList, homeId, false);
        } 
        MemoryStream ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;

        var saveService = ServiceHelper.GetService<ISaveAndView>();
        await saveService.SaveAndView("report.xls", "", ms);
        return true;
    }


    private async Task ShowInvestAndLon()
    {
        await GoToAsync(nameof(InvestmentAndLoanView));
    }
    private readonly ElectricitySupplier electricitySupplier;

    private ElectricitySupplier ElectricitySupplier
    {
        get => electricitySupplier;
        init => SetProperty(ref electricitySupplier, value);
    }

    public string HomeImageUrl => "MySolarCells.Resources.EmbeddedImages.house_with_solar_cells.png";

    public string LanguageImage => SettingsService.UserCountry == CountryEnum.SvSe ? "se.png":"us.png";

    public string ElectricitySupplierText
    {
        get
        {
            if (electricitySupplier != null)
                return ((ElectricitySupplierEnum)electricitySupplier.ElectricitySupplierType).ToString();
            else
                return "";
        }
    }

    public string InverterText
    {
        get
        {

            var inverter = mscDbContext.Inverter.OrderByDescending(s => s.FromDate).FirstOrDefault(x => x.HomeId == HomeService.CurrentHome().HomeId);
            if (inverter != null)
            {
                inverterModelText = inverter.Name;
                OnPropertyChanged(nameof(InverterModelText));
                return ((InverterTypeEnum)inverter.InverterTyp).ToString();
            }

            return "";
        }

    }
    private string inverterModelText = "";
    public string InverterModelText => inverterModelText;
    private string appInfoVersion="";
    public string AppInfoVersion
    {
        get => appInfoVersion;
        set => SetProperty(ref appInfoVersion, value);
    }

    public string CurrentThemeText
    {
        get
        {
            return themeService.UserThemePreference switch
            {
                AppTheme.Light => AppResources.Light_Theme,
                AppTheme.Dark => AppResources.Dark_Theme,
                _ => AppResources.System_Theme
            };
        }
    }

    public string CurrentThemeDisplayText
    {
        get
        {
            return $"{AppResources.Theme}: {CurrentThemeText}";
        }
    }

    public string ThemeIcon
    {
        get
        {
            return themeService.UserThemePreference switch
            {
                AppTheme.Light => IconFont.Sun,
                AppTheme.Dark => IconFont.Moon,
                _ => IconFont.Settings
            };
        }
    }

    private async Task ToggleTheme()
    {
        var currentTheme = themeService.UserThemePreference;
        var newTheme = currentTheme switch
        {
            AppTheme.Unspecified => AppTheme.Light,
            AppTheme.Light => AppTheme.Dark,
            AppTheme.Dark => AppTheme.Unspecified,
            _ => AppTheme.Unspecified
        };

        themeService.UserThemePreference = newTheme;
        await Task.CompletedTask;
    }
}

