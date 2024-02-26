using Syncfusion.XlsIO;

namespace MySolarCells.ViewModels.More;

public class MoreViewModel : BaseViewModel
{
    private readonly IHistoryDataService historyService;
    private readonly IRoiService roiService;
    private readonly ISettingsService settingsService;

    private readonly MscDbContext mscDbContext;
    public MoreViewModel(IHistoryDataService historyService, IRoiService roiService, ISettingsService settingsService, MscDbContext mscDbContext)
    {
        this.historyService = historyService;
        this.roiService = roiService;
        this.settingsService = settingsService;
        this.mscDbContext = mscDbContext;
        Home = MySolarCellsGlobals.SelectedHome;
        AppInfoVersion = AppInfo.VersionString + "(" + AppInfo.BuildString + ")";

    }

    public ICommand ShowInvestAndLonCommand => new Command(async () => await ShowInvestAndLon());
    public ICommand ShowCalcParametersCommand => new Command(async () => await ShowCalcParameters());
    public ICommand ElectricitySupplierCommand => new Command(async () => await ShowElectricitySupplier());
    public ICommand ExportExcelCommand => new Command(async () => await ExportExcelGroupYear(MySolarCellsGlobals.SelectedHome.HomeId));
    public ICommand InverterSettingsCommand => new Command(async () => await ShowInverterSettings());
    public ICommand ShowReportCommand => new Command(async () => await ShowReport());
    public ICommand ShowPowerTariffCommand => new Command(async () => await ShowPowerTariff());
    public ICommand ShowPickCountryCommand => new Command(async () => await ShowPickCountry());

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
        await ((ReportViewModel)view.BindingContext).RefreshAsync(ViewState.Refreshing);
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
    public async override Task OnAppearingAsync()
    {
        //Show log
        logText.Clear();

        var logs = this.mscDbContext.Log.OrderByDescending(x => x.CreateDate);
        //await this.mscDbContext.Log.ExecuteDelete(x => x.);
        foreach (var item in logs)
        {
            logText.Add(item.CreateDate.ToString() + " " + item.LogTitle);
        }
        OnPropertyChanged(nameof(LogText));
        base.OnAppearingAsync();
    }
    private ObservableCollection<string> logText = new ObservableCollection<string>();
    public ObservableCollection<string> LogText
    {
        get { return logText; }
        set { SetProperty(ref logText, value); }
    }
    private async Task<bool> GenerateYearWorkSheet(IWorksheet worksheet, List<ReportHistoryStats> roiStats, int homeId, bool isOverviewSheet)
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
        worksheet.Range["A11"].Text = AppResources.Costs_And_Rvenues;
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
        Single productionindex = -1; //used for geting the first month with production
        double totalProducedSaved = 0;

        bool valueMode = true;
        string lastC = "";
        //title is in row A so we start in B
        for (char c = 'B'; c <= 'Z'; c++)
        {
            lastC = c.ToString();


            if (index == roiStats.Count)
                valueMode = false;

            //< !--PRODUCTION AND CONSUMPTION-->
            //Månad/Yeear
            worksheet.Range[c.ToString() + "1"].Text = !valueMode ? "SUM" : isOverviewSheet ? roiStats[index].FromDate.ToString("yyyy").ToUpper() : roiStats[index].FromDate.ToString("MMM").ToUpper();
            worksheet.Range[c.ToString() + "2"].Value = valueMode ? roiStats[index].HistoryStats.ProductionSold.ToString() : roiStats.Sum(x => x.HistoryStats.ProductionSold).ToString();
            worksheet.Range[c.ToString() + "3"].Value = valueMode ? roiStats[index].HistoryStats.BatteryCharge.ToString() : roiStats.Sum(x => x.HistoryStats.BatteryCharge).ToString();
            worksheet.Range[c.ToString() + "4"].Value = valueMode ? roiStats[index].HistoryStats.ProductionOwnUse.ToString() : roiStats.Sum(x => x.HistoryStats.ProductionOwnUse).ToString();
            worksheet.Range[c.ToString() + "5"].Value = valueMode ? roiStats[index].HistoryStats.Purchased.ToString() : roiStats.Sum(x => x.HistoryStats.Purchased).ToString();
            worksheet.Range[c.ToString() + "6"].Value = valueMode ? roiStats[index].HistoryStats.BatteryUsed.ToString() : roiStats.Sum(x => x.HistoryStats.BatteryUsed).ToString();
            worksheet.Range[c.ToString() + "7"].Value = valueMode ? roiStats[index].HistoryStats.PeakEnergyReduction.ToString() : roiStats.Sum(x => x.HistoryStats.PeakEnergyReduction).ToString();
            worksheet.Range[c.ToString() + "8"].Value = valueMode ? roiStats[index].HistoryStats.SumAllProduction.ToString() : roiStats.Sum(x => x.HistoryStats.SumAllProduction).ToString();
            worksheet.Range[c.ToString() + "9"].Value = valueMode ? roiStats[index].HistoryStats.SumAllConsumption.ToString() : roiStats.Sum(x => x.HistoryStats.SumAllConsumption).ToString();
            worksheet.Range[c.ToString() + "10"].Value = valueMode ? roiStats[index].HistoryStats.BalanceProduction_Minus_Consumption.ToString() : roiStats.Sum(x => x.HistoryStats.BalanceProduction_Minus_Consumption).ToString();
            //<!-- COSTS AND REVENUES-->
            worksheet.Range[c.ToString() + "12"].Value = valueMode ? roiStats[index].HistoryStats.ProductionSoldProfit.ToString() : roiStats.Sum(x => x.HistoryStats.ProductionSoldProfit).ToString();
            worksheet.Range[c.ToString() + "13"].Value = valueMode ? roiStats[index].HistoryStats.ProductionSoldGridCompensationProfit.ToString() : roiStats.Sum(x => x.HistoryStats.ProductionSoldGridCompensationProfit).ToString();
            worksheet.Range[c.ToString() + "14"].Value = valueMode ? roiStats[index].HistoryStats.ProductionSoldTaxReductionProfit.ToString() : roiStats.Sum(x => x.HistoryStats.ProductionSoldTaxReductionProfit).ToString();
            if (valueMode && !string.IsNullOrEmpty(roiStats[index].HistoryStats.ProductionSoldTaxReductionProfitComment))
            {
                worksheet.Range[c.ToString() + "14"].AddThreadedComment(roiStats[index].HistoryStats.ProductionSoldTaxReductionProfitComment, DateTime.Now);
            }
            worksheet.Range[c.ToString() + "15"].Value = valueMode ? roiStats[index].HistoryStats.ProductionOwnUseSaved.ToString() : roiStats.Sum(x => x.HistoryStats.ProductionOwnUseSaved).ToString();
            worksheet.Range[c.ToString() + "16"].Value = valueMode ? roiStats[index].HistoryStats.ProductionOwnUseTransferFeeSaved.ToString() : roiStats.Sum(x => x.HistoryStats.ProductionOwnUseTransferFeeSaved).ToString();
            worksheet.Range[c.ToString() + "17"].Value = valueMode ? roiStats[index].HistoryStats.ProductionOwnUseEnergyTaxSaved.ToString() : roiStats.Sum(x => x.HistoryStats.ProductionOwnUseEnergyTaxSaved).ToString();
            worksheet.Range[c.ToString() + "18"].Value = valueMode ? roiStats[index].HistoryStats.BatteryUsedSaved.ToString() : roiStats.Sum(x => x.HistoryStats.BatteryUsedSaved).ToString();
            worksheet.Range[c.ToString() + "19"].Value = valueMode ? roiStats[index].HistoryStats.BatteryUseTransferFeeSaved.ToString() : roiStats.Sum(x => x.HistoryStats.BatteryUseTransferFeeSaved).ToString();
            worksheet.Range[c.ToString() + "20"].Value = valueMode ? roiStats[index].HistoryStats.BatteryUseEnergyTaxSaved.ToString() : roiStats.Sum(x => x.HistoryStats.BatteryUseEnergyTaxSaved).ToString();
            worksheet.Range[c.ToString() + "21"].Value = valueMode ? roiStats[index].HistoryStats.PurchasedCost.ToString() : roiStats.Sum(x => x.HistoryStats.PurchasedCost).ToString();
            worksheet.Range[c.ToString() + "22"].Value = valueMode ? roiStats[index].HistoryStats.PurchasedTransferFeeCost.ToString() : roiStats.Sum(x => x.HistoryStats.PurchasedTransferFeeCost).ToString();
            worksheet.Range[c.ToString() + "23"].Value = valueMode ? roiStats[index].HistoryStats.PurchasedTaxCost.ToString() : roiStats.Sum(x => x.HistoryStats.PurchasedTaxCost).ToString();
            worksheet.Range[c.ToString() + "24"].Value = valueMode ? roiStats[index].HistoryStats.PeakEnergyReductionSaved.ToString() : roiStats.Sum(x => x.HistoryStats.PeakEnergyReductionSaved).ToString();
            worksheet.Range[c.ToString() + "25"].Value = valueMode ? roiStats[index].HistoryStats.SumAllProductionSoldAndSaved.ToString() : roiStats.Sum(x => x.HistoryStats.SumAllProductionSoldAndSaved).ToString();
            worksheet.Range[c.ToString() + "26"].Value = valueMode ? roiStats[index].HistoryStats.InterestCost.ToString() : roiStats.Sum(x => x.HistoryStats.InterestCost).ToString();
            worksheet.Range[c.ToString() + "27"].Value = valueMode ? roiStats[index].HistoryStats.SumPurchasedCost.ToString() : roiStats.Sum(x => x.HistoryStats.SumPurchasedCost).ToString();
            worksheet.Range[c.ToString() + "28"].Value = valueMode ? roiStats[index].HistoryStats.BalanceProductionProfit_Minus_ConsumptionCost.ToString() : roiStats.Sum(x => x.HistoryStats.BalanceProductionProfit_Minus_ConsumptionCost).ToString();
            //< !--FUN FACTS-- >
            worksheet.Range[c.ToString() + "30"].Value = valueMode ? roiStats[index].HistoryStats.FactsProductionIndex.ToString() : Math.Round(roiStats.Average(x => x.HistoryStats.FactsProductionIndex), 2).ToString();
            worksheet.Range[c.ToString() + "31"].Value = valueMode ? roiStats[index].HistoryStats.FactsPurchasedCostAveragePerKwhPurchased.ToString() : Math.Round(roiStats.Average(x => x.HistoryStats.FactsPurchasedCostAveragePerKwhPurchased), 2).ToString();
            worksheet.Range[c.ToString() + "32"].Value = valueMode ? roiStats[index].HistoryStats.FactsProductionSoldAveragePerKwhProfit.ToString() : Math.Round(roiStats.Average(x => x.HistoryStats.FactsProductionSoldAveragePerKwhProfit), 2).ToString();
            worksheet.Range[c.ToString() + "33"].Value = valueMode ? roiStats[index].HistoryStats.FactsProductionOwnUseAveragePerKwhSaved.ToString() : Math.Round(roiStats.Average(x => x.HistoryStats.FactsProductionOwnUseAveragePerKwhSaved), 2).ToString();
            worksheet.Range[c.ToString() + "34"].Value = valueMode ? roiStats[index].HistoryStats.FactsBatteryUsedAveragePerKwhSaved.ToString() : Math.Round(roiStats.Average(x => x.HistoryStats.FactsBatteryUsedAveragePerKwhSaved), 2).ToString();

            //if (isOverviewSheet)
            //{

            //    worksheet.Range[c.ToString() + "36"].Value = valueMode ? roiStats[index].ROIYearsLeft.HasValue ? Math.Round(roiStats[index].ROIYearsLeft.Value, 2).ToString() : "" : "";
            //    worksheet.Range[c.ToString() + "37"].Value = valueMode ? roiStats[index].HistoryStats.Investment.ToString() : "";
            //    worksheet.Range[c.ToString() + "38"].Value = valueMode ? roiStats[index].ProducedSaved.ToString() : roiStats.Sum(x => x.ProducedSaved).ToString();
            //    worksheet.Range[c.ToString() + "39"].Value = valueMode ? Math.Round(roiStats[index].HistoryStats.Investment - roiStats[index].AcumulatedUntilCurrentYearProducedAndSaved, 0).ToString() : "";
            //    worksheet.Range[c.ToString() + "39"].CellStyle.Font.Bold = true;
            //    worksheet.Range[c.ToString() + "39"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;
            //}

            if (index == roiStats.Count)
                break;

            index++;
        }
        //production and Consumtion Title
        worksheet.Range["B1:" + lastC + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
        worksheet.Range["A1:" + lastC + "1"].CellStyle.Font.Bold = true;
        worksheet.Range["A1:" + lastC + "1"].CellStyle.Font.Size = 14;
        //production and Consumtion result rows
        //worksheet.Range["A7:" + lastC + "8"].CellStyle.Color = Syncfusion.Drawing.Color.Transparent;
        worksheet.Range["A10:" + lastC + "10"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;
        worksheet.Range["A8:" + lastC + "10"].CellStyle.Font.Bold = true;

        //Costs and Revenues Title
        worksheet.Range["A11:" + lastC + "11"].CellStyle.Font.Bold = true;
        worksheet.Range["A11:" + lastC + "11"].CellStyle.Font.Size = 14;
        //production and Consumtion result rows
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
            var parmas = await this.mscDbContext.EnergyCalculationParameter.Where(x => x.HomeId == homeId && x.FromDate < roiStats.Last().FromDate).OrderBy(o => o.FromDate).ToListAsync();
            //remove params that has no impackt on current data
            bool existOne = false;
            for (int i = parmas.Count - 1; i >= 0; i--)
            {   //if true remove all before 
                if (parmas[i].FromDate < roiStats.First().FromDate && existOne)
                {
                    parmas = parmas.Where(x => x.FromDate > parmas[i].FromDate).ToList();
                    break;
                } //ve must have one that is same date or before first date
                else if (parmas[i].FromDate <= roiStats.First().FromDate)
                {
                    existOne = true;
                }
            }
            var indexParm = 0;

            //rubriker
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
                if (indexParm == parmas.Count)
                    break;

                worksheet.Range[c.ToString() + "42"].Value = parmas[indexParm].FromDate.ToString("yyyy-MM");
                worksheet.Range[c.ToString() + "42"].CellStyle.Font.Bold = true;
                worksheet.Range[c.ToString() + "42"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                //Nätnytta 0.078 kr/kWh
                worksheet.Range[c.ToString() + "43"].Value = parmas[indexParm].ProdCompensationElectricityLowload.ToString();

                //Eventuell överföringsavgift som du kostar vi köp av eller (sparar vi egen användning) Ellevio 0.3 kr
                worksheet.Range[c.ToString() + "44"].Value = parmas[indexParm].TransferFee.ToString();

                //0.60/kWh såld el Max 18 0000 kr och inte för fler kWh än huset köper in
                worksheet.Range[c.ToString() + "45"].Value = parmas[indexParm].TaxReduction.ToString();
                //0.49/kWh såld (sparar vi egen användning)
                worksheet.Range[c.ToString() + "46"].Value = parmas[indexParm].EnergyTax.ToString();

                //10.5 Kwh
                worksheet.Range[c.ToString() + "47"].Value = parmas[indexParm].TotalInstallKwhPanels.ToString();
                //SpotPrice
                worksheet.Range[c.ToString() + "48"].Value = parmas[indexParm].UseSpotPrice.ToString();

                //fixed price
                worksheet.Range[c.ToString() + "49"].Value = parmas[indexParm].FixedPriceKwh.ToString();

                indexParm++;
            }
        }
        #endregion
        return true;
    }



    private async Task<bool> GenerateSavingEssitmateWorksheet(IWorksheet worksheet, List<EstimateRoi> estimateRois, int homeId)
    {
        //Disable gridlines in the worksheet
        worksheet.IsGridLinesVisible = true;

       
        // public double AvargePriceSold { get; set; }
        //public double AvargePrisOwnUse { get; set; }
        //public double ProductionSold { get; set; }
        //public double ProductionOwnUse { get; set; }
        //public double YearSavingsSold { get; set; }
        //public double YearSavingsOwnUse { get; set; }
        //public double ReturnPercentage { get; set; }
        //public double RemainingOnInvestment { get; set; }
        worksheet.Range["A1"].Text = AppResources.Year;
        worksheet.Range["B1"].Text = "AvargePriceSold";
        worksheet.Range["C1"].Text = "AvargePrisOwnUse";
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
            worksheet.Range["A" + i.ToString()].Value = estimateRois[index].Year.ToString();
            worksheet.Range["B" + i.ToString()].Value = estimateRois[index].AvargePriceSold.ToString();
            worksheet.Range["C" + i.ToString()].Value = estimateRois[index].AvargePrisOwnUse.ToString();
            worksheet.Range["D" + i.ToString()].Value = estimateRois[index].ProductionSold.ToString();
            worksheet.Range["E" + i.ToString()].Value = estimateRois[index].ProductionOwnUse.ToString();
            worksheet.Range["F" + i.ToString()].Value = estimateRois[index].YearSavingsSold.ToString();
            worksheet.Range["G" + i.ToString()].Value = estimateRois[index].YearSavingsOwnUse.ToString();
            worksheet.Range["H" + i.ToString()].Value = estimateRois[index].ReturnPercentage.ToString();
            worksheet.Range["I" + i.ToString()].Value = estimateRois[index].RemainingOnInvestment.ToString();
            index++;
        }

        //
        worksheet.Range["A1:I1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
        worksheet.Range["A1:I1"].CellStyle.Font.Bold = true;
        worksheet.Range["A1:I1"].CellStyle.Font.Size = 14;
        worksheet.Range["A1:I1"].ColumnWidth = 20;


        return true;
    }
    private async Task<bool> ExportExcelGroupYear(int homeId)
    {
        using var dlg = DialogService.GetProgress(AppResources.Generating_Report_Please_Wait);
        await Task.Delay(200);
        var result = await this.historyService.GenerateTotalPermonthReport(MySolarCellsGlobals.SelectedHome.FromDate, DateTime.Today);
        var savingEsitmate = mscDbContext.SavingEssitmateParameters.FirstOrDefault();
        if (savingEsitmate is null)
            savingEsitmate = new SavingEssitmateParameters();
        var resultRoi = this.roiService.CalcSavingsEstimate(result.Model, savingEsitmate);

        using (ExcelEngine excelEngine = new ExcelEngine())
        {
            Syncfusion.XlsIO.IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Xlsx;

            //Create a workbook with a worksheet
            string[] nameOverview = new string[2];
            nameOverview[0] = "Savings estimate";
            nameOverview[1] = "Overview";
            IWorkbook workbook = application.Workbooks.Create(nameOverview);
            await GenerateSavingEssitmateWorksheet(workbook.Worksheets[0], resultRoi.Model, homeId);
            await GenerateYearWorkSheet(workbook.Worksheets[1], result.Model.Item1, homeId, true);
            foreach (var itemList in result.Model.Item2)
            {
                workbook.Worksheets.Create(itemList.First().FromDate.Year.ToString());
                await GenerateYearWorkSheet(workbook.Worksheets[workbook.Worksheets.Count - 1], itemList, homeId, false);
            }

            //await GenerateYearWorkSheet(workbook.Worksheets[0], roiStatsOvervView, homeId, true);

            //Access first worksheet from the workbook instance.
            //IWorksheet worksheet = workbook.Worksheets[0];

            //Assembly executingAssembly = typeof(App).GetTypeInfo().Assembly;
            //Stream inputStream = executingAssembly.GetManifestResourceStream("MAUISample.AdventureCycles-Logo.png");

            ////Add a picture
            //IPictureShape shape = worksheet.Pictures.AddPicture(1, 1, inputStream, 20, 20);





            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            var saveService = Helpers.ServiceHelper.GetService<ISaveAndView>();
            await saveService.SaveAndView("report.xls", "", ms);
            return true;
            //Saves the memory stream as a file.
            //SaveService saveService = new SaveService();
            //saveService.SaveAndView("Output.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ms);
        }
    }


    private async Task ShowInvestAndLon()
    {
        await GoToAsync(nameof(InvestmentAndLoanView));
    }
    private Home home;
    public Home Home
    {
        get => MySolarCellsGlobals.SelectedHome;
        set
        {
            SetProperty(ref home, value);

        }
    }

    public string HomeImageUrl
    {
        get => "MySolarCells.Resources.EmbeddedImages.house_with_solar_cells.png";

    }
    public string LanguageImage
    {

        get => this.settingsService.UserCountry == CountryEnum.Sv_SE ? "se.png":"us.png";

    }
    public string ElectricitySupplierText
    {
        get => ((ElectricitySupplier)home.ElectricitySupplier).ToString();

    }
    public string InverterText
    {
        get
        {

            var inverter = this.mscDbContext.Inverter.OrderByDescending(s => s.FromDate).FirstOrDefault(x => x.HomeId == Home.HomeId);
            inverterModelText = inverter.Name;
            OnPropertyChanged(nameof(InverterModelText));
            return ((InverterTyp)inverter.InverterTyp).ToString();
        }

    }
    private string inverterModelText = "";
    public string InverterModelText
    {
        get
        {

            return inverterModelText;
        }

    }
    private string _appInfoVersion;
    public string AppInfoVersion
    {
        get { return _appInfoVersion; }
        set { SetProperty(ref _appInfoVersion, value); }
    }
}

