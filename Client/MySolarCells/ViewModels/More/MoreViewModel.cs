using Syncfusion.XlsIO;

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
        AppInfoVersion =  AppInfo.VersionString + "(" + AppInfo.BuildString + ")";

    }

    public ICommand ShowInvestAndLonCommand => new Command(async () => await ShowInvestAndLon());
    public ICommand ShowCalcParametersCommand => new Command(async () => await ShowCalcParameters());
    public ICommand ElectricitySupplierCommand => new Command(async () => await ShowElectricitySupplier());
    public ICommand ExportExcelCommand => new Command(async () => await ExportExcel(MySolarCellsGlobals.SelectedHome.HomeId));
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

            //worksheet.Range["A14"].Text = AppResources.If_Battery_Is_Not_Used;
            //worksheet.Range["A14"].CellStyle.Font.Bold = true;
            //worksheet.Range["A15"].Text = AppResources.kWh;
            //worksheet.Range["A16"].Text = AppResources.Currency;
            //worksheet.Range["A17"].Text = AppResources.Production_To_Grid;
            //worksheet.Range["A18"].Text = AppResources.Energy_Tax;
            //worksheet.Range["A19"].Text = AppResources.Amount;
            //worksheet.Range["A19"].CellStyle.Font.Bold = true;
            //worksheet.Range["A19"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range["A20"].Text = AppResources.Production_Own_Use;
            worksheet.Range["A20"].CellStyle.Font.Bold = true;
            worksheet.Range["A21"].Text = AppResources.kWh;
            worksheet.Range["A22"].Text = AppResources.KWh_From_Battery;
            worksheet.Range["A23"].Text = AppResources.Currency;
            worksheet.Range["A24"].Text = AppResources.Battery_Currency;
            worksheet.Range["A25"].Text = AppResources.Transfer_Fee;
            worksheet.Range["A26"].Text = AppResources.Battery_Transfer_Fee;
            worksheet.Range["A27"].Text = AppResources.Energy_Tax;
            worksheet.Range["A28"].Text = AppResources.Battery_Energy_Tax;
            worksheet.Range["A29"].Text = AppResources.Amount;
            worksheet.Range["A29"].CellStyle.Font.Bold = true;
            worksheet.Range["A29"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range["A30"].Text = AppResources.Total;
            worksheet.Range["A30"].CellStyle.Font.Bold = true;
            worksheet.Range["A31"].Text = AppResources.kWh;
            worksheet.Range["A32"].Text = AppResources.Interest;
            worksheet.Range["A33"].Text = AppResources.Result;
            worksheet.Range["A33"].CellStyle.Font.Bold = true;
            worksheet.Range["A33"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;
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
                        worksheet.Range[prefixValue + c.ToString() + "1"].Text = result.Model[index].FromDate.ToString("yyyy").ToUpper();
                        worksheet.Range[prefixValue + c.ToString() + "1"].CellStyle.Font.Bold = true;
                        worksheet.Range[prefixValue + c.ToString() + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                    }
                    //Månad
                    worksheet.Range[prefixValue + c.ToString() + "2"].Text = result.Model[index].FromDate.ToString("MMM").ToUpper();
                    worksheet.Range[prefixValue + c.ToString() + "2"].CellStyle.Font.Bold = true;
                    worksheet.Range[prefixValue + c.ToString() + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                    worksheet.Range[prefixValue + c.ToString() + "3"].Value = result.Model[index].RoiStats.Purchased.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "3"].NumberFormat = "###,##";
                    worksheet.Range[prefixValue + c.ToString() + "4"].Value = result.Model[index].RoiStats.PurchasedCost.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "5"].Value = result.Model[index].RoiStats.PurchasedTransferFeeCost.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "6"].Value = result.Model[index].RoiStats.PurchasedTaxCost.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "7"].Value = result.Model[index].RoiStats.SumPurchasedCost.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "7"].CellStyle.Font.Bold = true;
                    worksheet.Range[prefixValue + c.ToString() + "7"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    worksheet.Range[prefixValue + c.ToString() + "9"].Value = result.Model[index].RoiStats.ProductionSold.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "10"].Value = result.Model[index].RoiStats.ProductionSoldProfit.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "11"].Value = result.Model[index].RoiStats.ProductionSoldGridCompensationProfit.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "12"].Value = result.Model[index].RoiStats.ProductionSoldTaxReductionProfit.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "13"].Value = result.Model[index].RoiStats.SumProductionSoldProfit.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "13"].CellStyle.Font.Bold = true;
                    worksheet.Range[prefixValue + c.ToString() + "13"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    //worksheet.Range[prefixValue + c.ToString() + "15"].Value = result.Model[index].RoiStats.TotalBatteryCharge.ToString();
                    //worksheet.Range[prefixValue + c.ToString() + "16"].Value = result.Model[index].RoiStats.TotalBatteryChargeProfitFake.ToString();
                    //worksheet.Range[prefixValue + c.ToString() + "17"].Value = result.Model[index].RoiStats.TotalCompensationForProductionToGridChargeBatteryFake.ToString();
                    //worksheet.Range[prefixValue + c.ToString() + "18"].Value = result.Model[index].RoiStats.TotalSavedEnergyTaxReductionBatteryChargeFakeToGrid.ToString();
                    //worksheet.Range[prefixValue + c.ToString() + "19"].Value = result.Model[index].RoiStats.SumProductionBatteryChargeFakeSold.ToString();
                    //worksheet.Range[prefixValue + c.ToString() + "19"].CellStyle.Font.Bold = true;
                    //worksheet.Range[prefixValue + c.ToString() + "19"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    worksheet.Range[prefixValue + c.ToString() + "21"].Value = result.Model[index].RoiStats.ProductionOwnUse.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "22"].Value = result.Model[index].RoiStats.BatteryUsed.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "23"].Value = result.Model[index].RoiStats.ProductionOwnUseSaved.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "24"].Value = result.Model[index].RoiStats.BatteryUsedSaved.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "25"].Value = result.Model[index].RoiStats.ProductionOwnUseTransferFeeSaved.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "26"].Value = result.Model[index].RoiStats.BatteryUseTransferFeeSaved.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "27"].Value = result.Model[index].RoiStats.ProductionOwnUseEnergyTaxSaved.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "28"].Value = result.Model[index].RoiStats.BatteryUseEnergyTaxSaved.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "29"].Value = result.Model[index].RoiStats.SumProductionOwnUseAndBatterySaved.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "29"].CellStyle.Font.Bold = true;
                    worksheet.Range[prefixValue + c.ToString() + "29"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    worksheet.Range[prefixValue + c.ToString() + "31"].Value = result.Model[index].RoiStats.SumAllProduction.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "32"].Value = result.Model[index].RoiStats.InterestCost.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "33"].Value = result.Model[index].RoiStats.BalanceProfitAndSaved_Minus_InterestCost.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "33"].CellStyle.Font.Bold = true;
                    worksheet.Range[prefixValue + c.ToString() + "33"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

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

           return ((InverterTyp)inverter.InverterTyp).ToString();
        }

    }
    private string _appInfoVersion;
    public String AppInfoVersion
    {
        get { return _appInfoVersion; }
        set { SetProperty(ref _appInfoVersion, value); }
    }
}

