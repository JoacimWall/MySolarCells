using System.Collections.ObjectModel;
using MySolarCells.Services.Sqlite.Models;
using NetTopologySuite.Index.HPRtree;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation;
using static System.Net.Mime.MediaTypeNames;

namespace MySolarCells.ViewModels.More;

public class MoreViewModel : BaseViewModel
{
    private readonly IRoiService roiService;
    private readonly MscDbContext mscDbContext;
    public MoreViewModel(IRoiService roiService, MscDbContext mscDbContext)
    {
        this.roiService = roiService;
        this.mscDbContext = mscDbContext;
        Home = MySolarCellsGlobals.SelectedHome;
        AppInfoVersion = AppInfo.VersionString + "(" + AppInfo.BuildString + ")";

    }

    public ICommand ShowInvestAndLonCommand => new Command(async () => await ShowInvestAndLon());
    public ICommand ShowCalcParametersCommand => new Command(async () => await ShowCalcParameters());
    public ICommand ElectricitySupplierCommand => new Command(async () => await ShowElectricitySupplier());
    public ICommand ExportExcelCommand => new Command(async () => await ExportExcelGroupYear(MySolarCellsGlobals.SelectedHome.HomeId));
    public ICommand InverterSettingsCommand => new Command(async () => await ShowInverterSettings());

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
    private async Task<bool> GenerateYearWorkSheet(IWorksheet worksheet, List<ReportRoiStats> roiStats, int homeId, bool isOverviewSheet)
    {
        //Disable gridlines in the worksheet
        worksheet.IsGridLinesVisible = true;

        #region Year or month rows
        //Titles
        worksheet.Range["A2"].Text = AppResources.Purchased;
        worksheet.Range["A2"].CellStyle.Font.Bold = true;
        worksheet.Range["A3"].Text = AppResources.kWh;
        worksheet.Range["A4"].Text = AppResources.Currency;
        worksheet.Range["A5"].Text = AppResources.Transfer_Fee;
        worksheet.Range["A6"].Text = AppResources.Energy_Tax;
        worksheet.Range["A7"].Text = AppResources.Amount;
        worksheet.Range["A7"].CellStyle.Font.Bold = true;
        worksheet.Range["A7"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

        worksheet.Range["A8"].Text = AppResources.Production_To_Grid;
        worksheet.Range["A8"].CellStyle.Font.Bold = true;
        worksheet.Range["A9"].Text = AppResources.kWh;
        worksheet.Range["A10"].Text = AppResources.Currency;
        worksheet.Range["A11"].Text = AppResources.Production_To_Grid;
        worksheet.Range["A12"].Text = AppResources.Energy_Tax;
        worksheet.Range["A13"].Text = AppResources.Amount;
        worksheet.Range["A13"].CellStyle.Font.Bold = true;
        worksheet.Range["A13"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

        worksheet.Range["A14"].Text = AppResources.Production_Own_Use;
        worksheet.Range["A14"].CellStyle.Font.Bold = true;
        worksheet.Range["A15"].Text = AppResources.kWh;
        worksheet.Range["A16"].Text = AppResources.KWh_From_Battery;
        worksheet.Range["A17"].Text = AppResources.Currency;
        worksheet.Range["A18"].Text = AppResources.Battery_Currency;
        worksheet.Range["A19"].Text = AppResources.Transfer_Fee;
        worksheet.Range["A20"].Text = AppResources.Battery_Transfer_Fee;
        worksheet.Range["A21"].Text = AppResources.Energy_Tax;
        worksheet.Range["A22"].Text = AppResources.Battery_Energy_Tax;
        worksheet.Range["A23"].Text = AppResources.Amount;
        worksheet.Range["A23"].CellStyle.Font.Bold = true;
        worksheet.Range["A23"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

        worksheet.Range["A24"].Text = AppResources.Total;
        worksheet.Range["A24"].CellStyle.Font.Bold = true;
        worksheet.Range["A25"].Text = AppResources.kWh;
        worksheet.Range["A26"].Text = AppResources.Interest;
        worksheet.Range["A27"].Text = AppResources.Result;
        worksheet.Range["A27"].CellStyle.Font.Bold = true;
        worksheet.Range["A27"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

        worksheet.Range["A28"].Text = AppResources.Fun_Facts;
        worksheet.Range["A28"].CellStyle.Font.Bold = true;
        worksheet.Range["A29"].Text = AppResources.Production_Index_Desc_Prod_Per_Day_KWh;
        worksheet.Range["A30"].Text = AppResources.Average_Purchased_Cost_Per_Kwh;
        worksheet.Range["A31"].Text = AppResources.Average_Production_Sold_Profit_Per_Kwh;
        worksheet.Range["A32"].Text = AppResources.Average_Production_Own_Use_Saved_Per_Kwh;
        worksheet.Range["A33"].Text = AppResources.Average_Battery_Used_Saved_Per_Kwh;

        worksheet.Range["A1:A33"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
        worksheet.Range["A1"].ColumnWidth = 36;

        int index = 0;
        Single productionindex = -1; //used for geting the first month with production
        double totalProducedSaved = 0;
        bool firstRound = true;
        bool skipAfirstRound = true;
        string prefixValue = "";
        //for (char prefix = 'A'; prefix <= 'Z';)
        //{
        //this is for fix so that we print in AA,AB, Av and ... 
        //if (!firstRound)
        //{
        //    prefixValue = prefix.ToString();
        //    prefix++;
        //}
        bool valueMode = true;
        string lastC = "";
        for (char c = 'A'; c <= 'Z'; c++)
        {
            lastC = c.ToString();
            //title is in row A so we start in b but only first time then we vant AA,AB
            if (skipAfirstRound)
            {
                skipAfirstRound = false;
                continue;
            }
            if (index == roiStats.Count)
                valueMode = false;


            //Månad/Yeear
            worksheet.Range[c.ToString() + "2"].Text = !valueMode ? "SUM" : isOverviewSheet ? roiStats[index].FromDate.ToString("yyyy").ToUpper() : roiStats[index].FromDate.ToString("MMM").ToUpper();
            worksheet.Range[c.ToString() + "2"].CellStyle.Font.Bold = true;
            worksheet.Range[c.ToString() + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet.Range[c.ToString() + "3"].Value = valueMode ? roiStats[index].RoiStats.Purchased.ToString() : roiStats.Sum(x => x.RoiStats.Purchased).ToString();
            //worksheet.Range[c.ToString() + "3"].NumberFormat = "###,##";
            worksheet.Range[c.ToString() + "4"].Value = valueMode ? roiStats[index].RoiStats.PurchasedCost.ToString() : roiStats.Sum(x => x.RoiStats.PurchasedCost).ToString();
            worksheet.Range[c.ToString() + "5"].Value = valueMode ? roiStats[index].RoiStats.PurchasedTransferFeeCost.ToString() : roiStats.Sum(x => x.RoiStats.PurchasedTransferFeeCost).ToString();
            worksheet.Range[c.ToString() + "6"].Value = valueMode ? roiStats[index].RoiStats.PurchasedTaxCost.ToString() : roiStats.Sum(x => x.RoiStats.PurchasedTaxCost).ToString();
            worksheet.Range[c.ToString() + "7"].Value = valueMode ? roiStats[index].RoiStats.SumPurchasedCost.ToString() : roiStats.Sum(x => x.RoiStats.SumPurchasedCost).ToString();
            worksheet.Range[c.ToString() + "7"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;
            worksheet.Range[c.ToString() + "7"].CellStyle.Font.Bold = true;

            worksheet.Range[c.ToString() + "9"].Value = valueMode ? roiStats[index].RoiStats.ProductionSold.ToString() : roiStats.Sum(x => x.RoiStats.ProductionSold).ToString();
            worksheet.Range[c.ToString() + "10"].Value = valueMode ? roiStats[index].RoiStats.ProductionSoldProfit.ToString() : roiStats.Sum(x => x.RoiStats.ProductionSoldProfit).ToString();
            worksheet.Range[c.ToString() + "11"].Value = valueMode ? roiStats[index].RoiStats.ProductionSoldGridCompensationProfit.ToString() : roiStats.Sum(x => x.RoiStats.ProductionSoldGridCompensationProfit).ToString();
            worksheet.Range[c.ToString() + "12"].Value = valueMode ? roiStats[index].RoiStats.ProductionSoldTaxReductionProfit.ToString() : roiStats.Sum(x => x.RoiStats.ProductionSoldTaxReductionProfit).ToString();
            worksheet.Range[c.ToString() + "13"].Value = valueMode ? roiStats[index].RoiStats.SumProductionSoldProfit.ToString() : roiStats.Sum(x => x.RoiStats.SumProductionSoldProfit).ToString();
            worksheet.Range[c.ToString() + "13"].CellStyle.Font.Bold = true;
            worksheet.Range[c.ToString() + "13"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range[c.ToString() + "15"].Value = valueMode ? roiStats[index].RoiStats.ProductionOwnUse.ToString() : roiStats.Sum(x => x.RoiStats.ProductionOwnUse).ToString();
            worksheet.Range[c.ToString() + "16"].Value = valueMode ? roiStats[index].RoiStats.BatteryUsed.ToString() : roiStats.Sum(x => x.RoiStats.BatteryUsed).ToString();
            worksheet.Range[c.ToString() + "17"].Value = valueMode ? roiStats[index].RoiStats.ProductionOwnUseSaved.ToString() : roiStats.Sum(x => x.RoiStats.ProductionOwnUseSaved).ToString();
            worksheet.Range[c.ToString() + "18"].Value = valueMode ? roiStats[index].RoiStats.BatteryUsedSaved.ToString() : roiStats.Sum(x => x.RoiStats.BatteryUsedSaved).ToString();
            worksheet.Range[c.ToString() + "19"].Value = valueMode ? roiStats[index].RoiStats.ProductionOwnUseTransferFeeSaved.ToString() : roiStats.Sum(x => x.RoiStats.ProductionOwnUseTransferFeeSaved).ToString();
            worksheet.Range[c.ToString() + "20"].Value = valueMode ? roiStats[index].RoiStats.BatteryUseTransferFeeSaved.ToString() : roiStats.Sum(x => x.RoiStats.BatteryUseTransferFeeSaved).ToString();
            worksheet.Range[c.ToString() + "21"].Value = valueMode ? roiStats[index].RoiStats.ProductionOwnUseEnergyTaxSaved.ToString() : roiStats.Sum(x => x.RoiStats.ProductionOwnUseEnergyTaxSaved).ToString();
            worksheet.Range[c.ToString() + "22"].Value = valueMode ? roiStats[index].RoiStats.BatteryUseEnergyTaxSaved.ToString() : roiStats.Sum(x => x.RoiStats.BatteryUseEnergyTaxSaved).ToString();
            worksheet.Range[c.ToString() + "23"].Value = valueMode ? roiStats[index].RoiStats.SumProductionOwnUseAndBatterySaved.ToString() : roiStats.Sum(x => x.RoiStats.SumProductionOwnUseAndBatterySaved).ToString();
            worksheet.Range[c.ToString() + "23"].CellStyle.Font.Bold = true;
            worksheet.Range[c.ToString() + "23"].CellStyle.Color =  Syncfusion.Drawing.Color.Orange;

            worksheet.Range[c.ToString() + "25"].Value = valueMode ? roiStats[index].RoiStats.SumAllProduction.ToString() : roiStats.Sum(x => x.RoiStats.SumAllProduction).ToString();
            worksheet.Range[c.ToString() + "26"].Value = valueMode ? roiStats[index].RoiStats.InterestCost.ToString() : roiStats.Sum(x => x.RoiStats.InterestCost).ToString();
            worksheet.Range[c.ToString() + "27"].Value = valueMode ? roiStats[index].RoiStats.BalanceProfitAndSaved_Minus_InterestCost.ToString() : roiStats.Sum(x => x.RoiStats.BalanceProfitAndSaved_Minus_InterestCost).ToString();
            worksheet.Range[c.ToString() + "27"].CellStyle.Font.Bold = true;
            worksheet.Range[c.ToString() + "27"].CellStyle.Color =  Syncfusion.Drawing.Color.Orange;

            worksheet.Range[c.ToString() + "29"].Value = valueMode ? roiStats[index].RoiStats.FactsProductionIndex.ToString() : Math.Round(roiStats.Average(x => x.RoiStats.FactsProductionIndex),2).ToString();
            worksheet.Range[c.ToString() + "30"].Value = valueMode ? roiStats[index].RoiStats.FactsPurchasedCostAveragePerKwhPurchased.ToString() : Math.Round(roiStats.Average(x => x.RoiStats.FactsPurchasedCostAveragePerKwhPurchased), 2).ToString();
            worksheet.Range[c.ToString() + "31"].Value = valueMode ? roiStats[index].RoiStats.FactsProductionSoldAveragePerKwhProfit.ToString() : Math.Round(roiStats.Average(x => x.RoiStats.FactsProductionSoldAveragePerKwhProfit), 2).ToString();
            worksheet.Range[c.ToString() + "32"].Value = valueMode ? roiStats[index].RoiStats.FactsProductionOwnUseAveragePerKwhSaved.ToString() : Math.Round(roiStats.Average(x => x.RoiStats.FactsProductionOwnUseAveragePerKwhSaved), 2).ToString();
            worksheet.Range[c.ToString() + "33"].Value = valueMode ? roiStats[index].RoiStats.FactsBatteryUsedAveragePerKwhSaved.ToString() : Math.Round(roiStats.Average(x => x.RoiStats.FactsBatteryUsedAveragePerKwhSaved), 2).ToString();

            //worksheet.Range[c.ToString() + "3:23"].HorizontalAlignment = ExcelHAlign.HAlignRight;
            if (index == roiStats.Count)
                break;



            totalProducedSaved = totalProducedSaved + roiStats[index].RoiStats.BalanceProfitAndSaved_Minus_InterestCost;
            if (productionindex == -1 && roiStats[index].RoiStats.SumAllProduction > 0)
                productionindex = 1;
            else if (productionindex > 0)
            {
                var relDates = Helpers.DateHelper.GetRelatedDates(DateTime.Today);
                //check current month
                if (roiStats[index].FromDate == relDates.ThisMonthStart)
                {
                    TimeSpan difference = new TimeSpan();
                    difference = DateTime.Today - roiStats[index].FromDate;
                    TimeSpan mothDays = relDates.ThisMonthStart.AddMonths(1) - relDates.ThisMonthStart;
                    productionindex = productionindex + (Convert.ToSingle(difference.Days) / mothDays.Days);
                }
                else
                {
                    productionindex++;
                }
            }

            index++;
        }
        worksheet.Range[lastC + "3:" + lastC + "33"].CellStyle.Font.Bold = true;
        //worksheet.Range[lastC + "3:" + lastC + "6"].CellStyle.Color = Syncfusion.Drawing.Color.Green;
        //worksheet.Range[lastC + "9:" + lastC + "12"].CellStyle.Color = Syncfusion.Drawing.Color.Green;
        //worksheet.Range[lastC + "15:" + lastC + "22"].CellStyle.Color = Syncfusion.Drawing.Color.Green;
        //worksheet.Range[lastC + "25:" + lastC + "26"].CellStyle.Color = Syncfusion.Drawing.Color.Green;
        //worksheet.Range[lastC + "29:" + lastC + "32"].CellStyle.Color = Syncfusion.Drawing.Color.Green;
        // firstRound = false;
        //}
        //sumerice horizontal



        #endregion
        #region ROI and investment part
        var invest = await this.mscDbContext.InvestmentAndLon.Where(x => x.HomeId == homeId).ToListAsync();
        int toalInvestLon = 0;
        foreach (var item in invest)
        {
            toalInvestLon = toalInvestLon + item.Investment + item.Loan;
        }

        worksheet.Range["A35"].Text = AppResources.ROI;
        worksheet.Range["A35"].CellStyle.Font.Bold = true;
        worksheet.Range["A36"].Text = AppResources.Years_Left_Until_ROI;
        worksheet.Range["A37"].Text = AppResources.Total_Investment_And_Loan;
        worksheet.Range["A38"].Text = AppResources.Total_Produced_And_Saved;
        worksheet.Range["A39"].Text = AppResources.Left_Of_Investment_Cost;
        worksheet.Range["A39"].CellStyle.Font.Bold = true;
        worksheet.Range["A39"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

        worksheet.Range["B36"].Value = Math.Round(((toalInvestLon - totalProducedSaved) / (totalProducedSaved / productionindex) / 12), 2).ToString();
        worksheet.Range["B37"].Value = toalInvestLon.ToString();
        worksheet.Range["B38"].Value = Math.Round(totalProducedSaved, 0).ToString();
        worksheet.Range["B39"].Value = (toalInvestLon - Math.Round(totalProducedSaved, 0)).ToString();
        worksheet.Range["B39"].CellStyle.Font.Bold = true;
        worksheet.Range["B39"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;
        #endregion

        #region Calculation_Parameters
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

            worksheet.Range[c.ToString() + "42"].Text = parmas[indexParm].FromDate.ToString("yyyy-MM");
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
        #endregion
        return true;
    }
    private async Task<bool> ExportExcelGroupYear(int homeId)
    {
        using var dlg = DialogService.GetProgress(AppResources.Generating_Report_Please_Wait);
        await Task.Delay(200);

        var result = await this.roiService.GenerateTotalPermonthReport();


        using (ExcelEngine excelEngine = new ExcelEngine())
        {
            Syncfusion.XlsIO.IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Xlsx;

            //Create a workbook with a worksheet
            string[] nameOverview = new string[1];
            nameOverview[0] = "Overview";
            IWorkbook workbook = application.Workbooks.Create(nameOverview);



            //    --------- Seperate Year worksheets ------------------ 
            int year = result.Model.First().FromDate.Year;
            List<ReportRoiStats> roiStatsGrpYear = new List<ReportRoiStats>();
            List<ReportRoiStats> roiStatsOvervView = new List<ReportRoiStats>();
            int worksheetsCount = 1;
            for (int i = 0; i < result.Model.Count; i++)
            {
                if (year == result.Model[i].FromDate.Year)
                    roiStatsGrpYear.Add(result.Model[i]);

                if (year != result.Model[i].FromDate.Year || i == result.Model.Count - 1)
                {
                    workbook.Worksheets.Create(roiStatsGrpYear.First().FromDate.Year.ToString());
                    await GenerateYearWorkSheet(workbook.Worksheets[worksheetsCount], roiStatsGrpYear, homeId, false);
                    TimeSpan timeSpanYear = roiStatsGrpYear.Last().FromDate - roiStatsGrpYear.First().FromDate;
                    roiStatsOvervView.Add(new ReportRoiStats
                    {
                        FromDate = roiStatsGrpYear.First().FromDate,
                        RoiStats = roiService.SummerizeToOneRoiStats(roiStatsGrpYear.Select(x => x.RoiStats).ToList(), timeSpanYear)
                    });
                    worksheetsCount++;
                    year = result.Model[i].FromDate.Year;
                    roiStatsGrpYear = new List<ReportRoiStats>();
                    roiStatsGrpYear.Add(result.Model[i]);
                }
            }
            await GenerateYearWorkSheet(workbook.Worksheets[0], roiStatsOvervView, homeId, true);

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
    private async Task<bool> ExportExcel(int homeId)
    {
        using var dlg = DialogService.GetProgress(AppResources.Generating_Report_Please_Wait);
        await Task.Delay(200);

        var result = await this.roiService.GenerateTotalPermonthReport();


        using (ExcelEngine excelEngine = new ExcelEngine())
        {
            Syncfusion.XlsIO.IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Xlsx;

            //Create a workbook with a worksheet
            IWorkbook workbook = application.Workbooks.Create(1);

            //Access first worksheet from the workbook instance.
            IWorksheet worksheet = workbook.Worksheets[0];

            //Assembly executingAssembly = typeof(App).GetTypeInfo().Assembly;
            //Stream inputStream = executingAssembly.GetManifestResourceStream("MAUISample.AdventureCycles-Logo.png");

            ////Add a picture
            //IPictureShape shape = worksheet.Pictures.AddPicture(1, 1, inputStream, 20, 20);

            //Disable gridlines in the worksheet
            worksheet.IsGridLinesVisible = true;


            //Titles
            worksheet.Range["A2"].Text = AppResources.Purchased;
            worksheet.Range["A2"].CellStyle.Font.Bold = true;
            worksheet.Range["A3"].Text = AppResources.kWh;
            worksheet.Range["A4"].Text = AppResources.Currency;
            worksheet.Range["A5"].Text = AppResources.Transfer_Fee;
            worksheet.Range["A6"].Text = AppResources.Energy_Tax;
            worksheet.Range["A7"].Text = AppResources.Amount;
            worksheet.Range["A7"].CellStyle.Font.Bold = true;
            worksheet.Range["A7"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range["A8"].Text = AppResources.Production_To_Grid;
            worksheet.Range["A8"].CellStyle.Font.Bold = true;
            worksheet.Range["A9"].Text = AppResources.kWh;
            worksheet.Range["A10"].Text = AppResources.Currency;
            worksheet.Range["A11"].Text = AppResources.Production_To_Grid;
            worksheet.Range["A12"].Text = AppResources.Energy_Tax;
            worksheet.Range["A13"].Text = AppResources.Amount;
            worksheet.Range["A13"].CellStyle.Font.Bold = true;
            worksheet.Range["A13"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range["A14"].Text = AppResources.Production_Own_Use;
            worksheet.Range["A14"].CellStyle.Font.Bold = true;
            worksheet.Range["A15"].Text = AppResources.kWh;
            worksheet.Range["A16"].Text = AppResources.KWh_From_Battery;
            worksheet.Range["A17"].Text = AppResources.Currency;
            worksheet.Range["A18"].Text = AppResources.Battery_Currency;
            worksheet.Range["A19"].Text = AppResources.Transfer_Fee;
            worksheet.Range["A20"].Text = AppResources.Battery_Transfer_Fee;
            worksheet.Range["A21"].Text = AppResources.Energy_Tax;
            worksheet.Range["A22"].Text = AppResources.Battery_Energy_Tax;
            worksheet.Range["A23"].Text = AppResources.Amount;
            worksheet.Range["A23"].CellStyle.Font.Bold = true;
            worksheet.Range["A23"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range["A24"].Text = AppResources.Total;
            worksheet.Range["A24"].CellStyle.Font.Bold = true;
            worksheet.Range["A25"].Text = AppResources.kWh;
            worksheet.Range["A26"].Text = AppResources.Interest;
            worksheet.Range["A27"].Text = AppResources.Result;
            worksheet.Range["A27"].CellStyle.Font.Bold = true;
            worksheet.Range["A27"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range["A28"].Text = AppResources.Fun_Facts;
            worksheet.Range["A28"].CellStyle.Font.Bold = true;
            worksheet.Range["A29"].Text = AppResources.Production_Index_Desc_Prod_Per_Day_KWh;
            worksheet.Range["A30"].Text = AppResources.Average_Purchased_Cost_Per_Kwh;
            worksheet.Range["A31"].Text = AppResources.Average_Production_Sold_Profit_Per_Kwh;
            worksheet.Range["A32"].Text = AppResources.Average_Production_Own_Use_Saved_Per_Kwh;
            worksheet.Range["A33"].Text = AppResources.Average_Battery_Used_Saved_Per_Kwh;

            worksheet.Range["A1:A33"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range["A1"].ColumnWidth = 36;

            int index = 0;
            Single productionindex = -1; //used for geting the first month with production
            double totalProducedSaved = 0;
            bool firstRound = true;
            bool skipAfirstRound = true;
            string prefixValue = "";
            for (char prefix = 'A'; prefix <= 'Z';)
            {
                //this is for fix so that we print in AA,AB, Av and ... 
                if (!firstRound)
                {
                    prefixValue = prefix.ToString();
                    prefix++;
                }

                for (char c = 'A'; c <= 'Z'; c++)
                {
                    //title is in row A so we start in b but only first time then we vant AA,AB
                    if (skipAfirstRound)
                    {
                        skipAfirstRound = false;
                        continue;
                    }
                    if (index == result.Model.Count)
                        break;

                    //Year if jan
                    if (result.Model[index].FromDate.Month == 1)
                    {
                        worksheet.Range[c.ToString() + "1"].Text = result.Model[index].FromDate.ToString("yyyy").ToUpper();
                        worksheet.Range[c.ToString() + "1"].CellStyle.Font.Bold = true;
                        worksheet.Range[c.ToString() + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                    }
                    //Månad
                    worksheet.Range[c.ToString() + "2"].Text = result.Model[index].FromDate.ToString("MMM").ToUpper();
                    worksheet.Range[c.ToString() + "2"].CellStyle.Font.Bold = true;
                    worksheet.Range[c.ToString() + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet.Range[c.ToString() + "3"].Value = result.Model[index].RoiStats.Purchased.ToString();
                    worksheet.Range[c.ToString() + "3"].NumberFormat = "###,##";
                    worksheet.Range[c.ToString() + "4"].Value = result.Model[index].RoiStats.PurchasedCost.ToString();
                    worksheet.Range[c.ToString() + "5"].Value = result.Model[index].RoiStats.PurchasedTransferFeeCost.ToString();
                    worksheet.Range[c.ToString() + "6"].Value = result.Model[index].RoiStats.PurchasedTaxCost.ToString();
                    worksheet.Range[c.ToString() + "7"].Value = result.Model[index].RoiStats.SumPurchasedCost.ToString();
                    worksheet.Range[c.ToString() + "7"].CellStyle.Font.Bold = true;
                    worksheet.Range[c.ToString() + "7"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    worksheet.Range[c.ToString() + "9"].Value = result.Model[index].RoiStats.ProductionSold.ToString();
                    worksheet.Range[c.ToString() + "10"].Value = result.Model[index].RoiStats.ProductionSoldProfit.ToString();
                    worksheet.Range[c.ToString() + "11"].Value = result.Model[index].RoiStats.ProductionSoldGridCompensationProfit.ToString();
                    worksheet.Range[c.ToString() + "12"].Value = result.Model[index].RoiStats.ProductionSoldTaxReductionProfit.ToString();
                    worksheet.Range[c.ToString() + "13"].Value = result.Model[index].RoiStats.SumProductionSoldProfit.ToString();
                    worksheet.Range[c.ToString() + "13"].CellStyle.Font.Bold = true;
                    worksheet.Range[c.ToString() + "13"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    worksheet.Range[c.ToString() + "15"].Value = result.Model[index].RoiStats.ProductionOwnUse.ToString();
                    worksheet.Range[c.ToString() + "16"].Value = result.Model[index].RoiStats.BatteryUsed.ToString();
                    worksheet.Range[c.ToString() + "17"].Value = result.Model[index].RoiStats.ProductionOwnUseSaved.ToString();
                    worksheet.Range[c.ToString() + "18"].Value = result.Model[index].RoiStats.BatteryUsedSaved.ToString();
                    worksheet.Range[c.ToString() + "19"].Value = result.Model[index].RoiStats.ProductionOwnUseTransferFeeSaved.ToString();
                    worksheet.Range[c.ToString() + "20"].Value = result.Model[index].RoiStats.BatteryUseTransferFeeSaved.ToString();
                    worksheet.Range[c.ToString() + "21"].Value = result.Model[index].RoiStats.ProductionOwnUseEnergyTaxSaved.ToString();
                    worksheet.Range[c.ToString() + "22"].Value = result.Model[index].RoiStats.BatteryUseEnergyTaxSaved.ToString();
                    worksheet.Range[c.ToString() + "23"].Value = result.Model[index].RoiStats.SumProductionOwnUseAndBatterySaved.ToString();
                    worksheet.Range[c.ToString() + "23"].CellStyle.Font.Bold = true;
                    worksheet.Range[c.ToString() + "23"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    worksheet.Range[c.ToString() + "25"].Value = result.Model[index].RoiStats.SumAllProduction.ToString();
                    worksheet.Range[c.ToString() + "26"].Value = result.Model[index].RoiStats.InterestCost.ToString();
                    worksheet.Range[c.ToString() + "27"].Value = result.Model[index].RoiStats.BalanceProfitAndSaved_Minus_InterestCost.ToString();
                    worksheet.Range[c.ToString() + "27"].CellStyle.Font.Bold = true;
                    worksheet.Range[c.ToString() + "27"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    worksheet.Range[c.ToString() + "29"].Value = result.Model[index].RoiStats.FactsProductionIndex.ToString();
                    worksheet.Range[c.ToString() + "30"].Value = result.Model[index].RoiStats.FactsPurchasedCostAveragePerKwhPurchased.ToString();
                    worksheet.Range[c.ToString() + "31"].Value = result.Model[index].RoiStats.FactsProductionSoldAveragePerKwhProfit.ToString();
                    worksheet.Range[c.ToString() + "32"].Value = result.Model[index].RoiStats.FactsProductionOwnUseAveragePerKwhSaved.ToString();
                    worksheet.Range[c.ToString() + "33"].Value = result.Model[index].RoiStats.FactsBatteryUsedAveragePerKwhSaved.ToString();

                    //worksheet.Range[c.ToString() + "3:23"].HorizontalAlignment = ExcelHAlign.HAlignRight;




                    totalProducedSaved = totalProducedSaved + result.Model[index].RoiStats.BalanceProfitAndSaved_Minus_InterestCost;
                    if (productionindex == -1 && result.Model[index].RoiStats.SumAllProduction > 0)
                        productionindex = 1;
                    else if (productionindex > 0)
                    {
                        var relDates = Helpers.DateHelper.GetRelatedDates(DateTime.Today);
                        //check current month
                        if (result.Model[index].FromDate == relDates.ThisMonthStart)
                        {
                            TimeSpan difference = new TimeSpan();
                            difference = DateTime.Today - result.Model[index].FromDate;
                            TimeSpan mothDays = relDates.ThisMonthStart.AddMonths(1) - relDates.ThisMonthStart;
                            productionindex = productionindex + (Convert.ToSingle(difference.Days) / mothDays.Days);
                        }
                        else
                        {
                            productionindex++;
                        }
                    }

                    index++;
                }

                firstRound = false;
            }
            var invest = await this.mscDbContext.InvestmentAndLon.Where(x => x.HomeId == homeId).ToListAsync();
            int toalInvestLon = 0;
            foreach (var item in invest)
            {
                toalInvestLon = toalInvestLon + item.Investment + item.Loan;
            }

            worksheet.Range["A35"].Text = AppResources.ROI;
            worksheet.Range["A35"].CellStyle.Font.Bold = true;
            worksheet.Range["A36"].Text = AppResources.Years_Left_Until_ROI;
            worksheet.Range["A37"].Text = AppResources.Total_Investment_And_Loan;
            worksheet.Range["A38"].Text = AppResources.Total_Produced_And_Saved;
            worksheet.Range["A39"].Text = AppResources.Left_Of_Investment_Cost;
            worksheet.Range["A39"].CellStyle.Font.Bold = true;
            worksheet.Range["A39"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range["B36"].Value = Math.Round(((toalInvestLon - totalProducedSaved) / (totalProducedSaved / productionindex) / 12), 2).ToString();
            worksheet.Range["B37"].Value = toalInvestLon.ToString();
            worksheet.Range["B38"].Value = Math.Round(totalProducedSaved, 0).ToString();
            worksheet.Range["B39"].Value = (toalInvestLon - Math.Round(totalProducedSaved, 0)).ToString();
            worksheet.Range["B39"].CellStyle.Font.Bold = true;
            worksheet.Range["B39"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            var parmas = await this.mscDbContext.EnergyCalculationParameter.Where(x => x.HomeId == homeId).ToListAsync();
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

                worksheet.Range[c.ToString() + "42"].Text = parmas[indexParm].FromDate.ToString("yyyy-MM");
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
    private Services.Sqlite.Models.Home home;
    public Services.Sqlite.Models.Home Home
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
    public string ElectricitySupplierText
    {
        get => ((ElectricitySupplier)home.ElectricitySupplier).ToString();

    }
    public string InverterText
    {
        get
        {

            var inverter = this.mscDbContext.Inverter.FirstOrDefault(x => x.HomeId == Home.HomeId);
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
    public String AppInfoVersion
    {
        get { return _appInfoVersion; }
        set { SetProperty(ref _appInfoVersion, value); }
    }
}

