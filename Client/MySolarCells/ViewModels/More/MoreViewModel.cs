using Azure;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation;
using System.IO;
using System.Reflection;


namespace MySolarCells.ViewModels.More;

public class MoreViewModel : BaseViewModel
{
    private IRoiService roiService;
    //private bool keepUploading = true;
    public MoreViewModel(IRoiService roiService)
    {
        this.roiService = roiService;

    }

    public ICommand ShowInvestAndLonCommand => new Command(async () => await ShowInvestAndLon());
    public ICommand ExportExcelCommand => new Command(async () => await ExportExcel(MySolarCellsGlobals.SelectedHome.HomeId));

    private async Task<bool> ExportExcel(int homeId)
    {
        using var dlg = DialogService.GetProgress("");
        using var dbContext = new MscDbContext();
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
            worksheet.Range["A3"].Text = "Purchased kWh";
            worksheet.Range["A4"].Text = "Purchased cost (SEK)";
            worksheet.Range["A5"].Text = "Purchased transfer fee";
            worksheet.Range["A6"].Text = "Purchased energy tax";
            worksheet.Range["A7"].Text = "Sum";
            worksheet.Range["A7"].CellStyle.Font.Bold = true;
            worksheet.Range["A7"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range["A9"].Text = "Production sold kWh";
            worksheet.Range["A10"].Text = "Production sold profit";
            worksheet.Range["A11"].Text = "Compensation for production to grid";
            worksheet.Range["A12"].Text = "Saved energy tax";
            worksheet.Range["A13"].Text = "Sum";
            worksheet.Range["A13"].CellStyle.Font.Bold = true;
            worksheet.Range["A13"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range["A15"].Text = "Production own use kWh";
            worksheet.Range["A16"].Text = "Production own use battery kWh";
            worksheet.Range["A17"].Text = "Saved cost";
            worksheet.Range["A18"].Text = "Saved cost battery";
            worksheet.Range["A19"].Text = "Saved transfer fee";
            worksheet.Range["A20"].Text = "Saved transfer fee battery";
            worksheet.Range["A21"].Text = "Saved energy tax";
            worksheet.Range["A22"].Text = "Saved energy tax battery";
            worksheet.Range["A23"].Text = "Sum";
            worksheet.Range["A23"].CellStyle.Font.Bold = true;
            worksheet.Range["A23"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range["A25"].Text = "Total production kWh";
            worksheet.Range["A26"].Text = "Total interest";
            worksheet.Range["A27"].Text = "Result";
            worksheet.Range["A27"].CellStyle.Font.Bold = true;
            worksheet.Range["A27"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;
            worksheet.Range["A1:A27"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
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
                    worksheet.Range[prefixValue + c.ToString() + "3"].Value = result.Model[index].RoiStats.TotalPurchased.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "3"].NumberFormat = "###,##";
                    worksheet.Range[prefixValue + c.ToString() + "4"].Value = result.Model[index].RoiStats.TotalPurchasedCost.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "5"].Value = result.Model[index].RoiStats.TotalTransferFeePurchased.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "6"].Value = result.Model[index].RoiStats.TotalTaxPurchased.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "7"].Value = result.Model[index].RoiStats.SumPurchased.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "7"].CellStyle.Font.Bold = true;
                    worksheet.Range[prefixValue + c.ToString() + "7"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    worksheet.Range[prefixValue + c.ToString() + "9"].Value = result.Model[index].RoiStats.TotalProductionSold.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "10"].Value = result.Model[index].RoiStats.TotalProductionSoldProfit.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "11"].Value = result.Model[index].RoiStats.TotalCompensationForProductionToGrid.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "12"].Value = result.Model[index].RoiStats.TotalSavedEnergyTaxReductionProductionToGrid.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "13"].Value = result.Model[index].RoiStats.SumProductionSold.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "13"].CellStyle.Font.Bold = true;
                    worksheet.Range[prefixValue + c.ToString() + "13"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    worksheet.Range[prefixValue + c.ToString() + "15"].Value = result.Model[index].RoiStats.TotalProductionOwnUse.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "16"].Value = result.Model[index].RoiStats.TotalBatteryUsed.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "17"].Value = result.Model[index].RoiStats.TotalProductionOwnUseProfit.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "18"].Value = result.Model[index].RoiStats.TotalBatteryUsedProfit.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "19"].Value = result.Model[index].RoiStats.TotalSavedTransferFeeProductionOwnUse.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "20"].Value = result.Model[index].RoiStats.TotalSavedTransferFeeBatteryUse.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "21"].Value = result.Model[index].RoiStats.TotalSavedEnergyTaxProductionOwnUse.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "22"].Value = result.Model[index].RoiStats.TotalSavedEnergyTaxBatteryUse.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "23"].Value = result.Model[index].RoiStats.SumProductionOwnUseAndBattery.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "23"].CellStyle.Font.Bold = true;
                    worksheet.Range[prefixValue + c.ToString() + "23"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    worksheet.Range[prefixValue + c.ToString() + "25"].Value = result.Model[index].RoiStats.TotalProduction.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "26"].Value = result.Model[index].RoiStats.TotalInterest.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "27"].Value = result.Model[index].RoiStats.TotalSaved.ToString();
                    worksheet.Range[prefixValue + c.ToString() + "27"].CellStyle.Font.Bold = true;
                    worksheet.Range[prefixValue + c.ToString() + "27"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

                    //worksheet.Range[c.ToString() + "3:23"].HorizontalAlignment = ExcelHAlign.HAlignRight;



                    totalProducedSaved = totalProducedSaved + result.Model[index].RoiStats.TotalSaved;
                    if (productionindex == -1 && result.Model[index].RoiStats.TotalProduction > 0)
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
            var invest = await dbContext.InvestmentAndLon.Where(x => x.HomeId == homeId).ToListAsync();
            int toalInvestLon = 0;
            foreach (var item in invest)
            {
                toalInvestLon = toalInvestLon + item.Investment + item.Lon;
            }

            worksheet.Range["A29"].Text = "ROI";
            worksheet.Range["A30"].CellStyle.Font.Bold = true;
            worksheet.Range["A30"].Text = "Years until ROI";
            worksheet.Range["A31"].Text = "Total investment & lon";
            worksheet.Range["A32"].Text = "Total produced & saved";
            worksheet.Range["A33"].Text = "Left of investment cost";
            worksheet.Range["A33"].CellStyle.Font.Bold = true;
            worksheet.Range["A33"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            worksheet.Range["B30"].Value = Math.Round((toalInvestLon / (totalProducedSaved / productionindex) / 12), 2).ToString();
            worksheet.Range["B31"].Value = toalInvestLon.ToString();
            worksheet.Range["B32"].Value = Math.Round(totalProducedSaved, 0).ToString();
            worksheet.Range["B33"].Value = (toalInvestLon - Math.Round(totalProducedSaved, 0)).ToString();
            worksheet.Range["B33"].CellStyle.Font.Bold = true;
            worksheet.Range["B33"].CellStyle.Color = Syncfusion.Drawing.Color.Orange;

            var parmas = await dbContext.EnergyCalculationParameter.Where(x => x.HomeId == homeId).ToListAsync();
            var indexParm = 0;

            //rubriker
            worksheet.Range["A35"].Text = "Calcualtion parameters";
            worksheet.Range["A36"].CellStyle.Font.Bold = true;
            worksheet.Range["A37"].Text = "From Date";
            worksheet.Range["A38"].Text = "Compensation electricity load";
            worksheet.Range["A39"].Text = "Transfer fee";
            worksheet.Range["A40"].Text = "Tax reduction";
            worksheet.Range["A41"].Text = "Energy tax";
            worksheet.Range["A42"].Text = "Total installed Kwh";
            worksheet.Range["A43"].Text = "Use Spot prices";
            worksheet.Range["A44"].Text = "Fixed price/Kwh";


            for (char c = 'B'; c <= 'Z'; c++)
            {
                if (indexParm == parmas.Count)
                    break;

                worksheet.Range[c.ToString() + "37"].Text = parmas[indexParm].FromDate.ToString("yyyy-MM");
                worksheet.Range[c.ToString() + "37"].CellStyle.Font.Bold = true;
                worksheet.Range[c.ToString() + "37"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                //Nätnytta 0.078 kr/kWh
                worksheet.Range[c.ToString() + "38"].Value = parmas[indexParm].ProdCompensationElectricityLowload.ToString();

                //Eventuell överföringsavgift som du kostar vi köp av eller (sparar vi egen användning) Ellevio 0.3 kr
                worksheet.Range[c.ToString() + "39"].Value = parmas[indexParm].TransferFee.ToString();

                //0.60/kWh såld el Max 18 0000 kr och inte för fler kWh än huset köper in
                worksheet.Range[c.ToString() + "40"].Value = parmas[indexParm].TaxReduction.ToString();
                //0.49/kWh såld (sparar vi egen användning)
                worksheet.Range[c.ToString() + "41"].Value = parmas[indexParm].EnergyTax.ToString();

                //10.5 Kwh
                worksheet.Range[c.ToString() + "42"].Value = parmas[indexParm].TotalInstallKwhPanels.ToString();
                //SpotPrice
                worksheet.Range[c.ToString() + "43"].Value = parmas[indexParm].UseSpotPrice.ToString();

                //fixed price
                worksheet.Range[c.ToString() + "44"].Value = parmas[indexParm].FixedPriceKwh.ToString();

                indexParm++;
            }

//for (char c = 'B'; c <= 'Z'; c++)
//{
//    if (index == result.Model.Count)
//        continue;
////Enter values to the cells from A3 to A5
//worksheet.Range["A3"].Text = "46036 Michigan Ave";
//worksheet.Range["A4"].Text = "Canton, USA";
//worksheet.Range["A5"].Text = "Phone: +1 231-231-2310";

////Make the text bold
//worksheet.Range["A3:A5"].CellStyle.Font.Bold = true;

////Merge cells
// worksheet.Range["D1:E1"].Merge();

////Enter text to the cell D1 and apply formatting.
//worksheet.Range["D1"].Text = "INVOICE";
//worksheet.Range["D1"].CellStyle.Font.Bold = true;
//worksheet.Range["D1"].CellStyle.Font.RGBColor = Syncfusion.Drawing.Color.FromArgb(0, 42, 118, 189);
//worksheet.Range["D1"].CellStyle.Font.Size = 35;

////Apply alignment in the cell D1
//worksheet.Range["D1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
//worksheet.Range["D1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

////Enter values to the cells from D5 to E8
//worksheet.Range["D5"].Text = "INVOICE#";
//worksheet.Range["E5"].Text = "DATE";
//worksheet.Range["D6"].Number = 1028;
//worksheet.Range["E6"].Value = "12/31/2018";
//worksheet.Range["D7"].Text = "CUSTOMER ID";
//worksheet.Range["E7"].Text = "TERMS";
//worksheet.Range["D8"].Number = 564;
//worksheet.Range["E8"].Text = "Due Upon Receipt";

////Apply RGB backcolor to the cells from D5 to E8
//worksheet.Range["D5:E5"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(0, 42, 118, 189);
//worksheet.Range["D7:E7"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(0, 42, 118, 189);

////Apply known colors to the text in cells D5 to E8
//worksheet.Range["D5:E5"].CellStyle.Font.Color = ExcelKnownColors.White;
//worksheet.Range["D7:E7"].CellStyle.Font.Color = ExcelKnownColors.White;

////Make the text as bold from D5 to E8
//worksheet.Range["D5:E8"].CellStyle.Font.Bold = true;

////Apply alignment to the cells from D5 to E8
//worksheet.Range["D5:E8"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
//worksheet.Range["D5:E5"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
//worksheet.Range["D7:E7"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
//worksheet.Range["D6:E6"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

////Enter value and applying formatting in the cell A7
//worksheet.Range["A7"].Text = "  BILL TO";
//worksheet.Range["A7"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(0, 42, 118, 189);
//worksheet.Range["A7"].CellStyle.Font.Bold = true;
//worksheet.Range["A7"].CellStyle.Font.Color = ExcelKnownColors.White;

////Apply alignment
//worksheet.Range["A7"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
//worksheet.Range["A7"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

////Enter values in the cells A8 to A12
//worksheet.Range["A8"].Text = "Steyn";
//worksheet.Range["A9"].Text = "Great Lakes Food Market";
//worksheet.Range["A10"].Text = "20 Whitehall Rd";
//worksheet.Range["A11"].Text = "North Muskegon,USA";
//worksheet.Range["A12"].Text = "+1 231-654-0000";

////Create a Hyperlink for e-mail in the cell A13
//IHyperLink hyperlink = worksheet.HyperLinks.Add(worksheet.Range["A13"]);
//hyperlink.Type = ExcelHyperLinkType.Url;
//hyperlink.Address = "Steyn@greatlakes.com";
//hyperlink.ScreenTip = "Send Mail";

////Enter details of products and prices
//worksheet.Range["A15"].Text = "DESCRIPTION";
//worksheet.Range["C15"].Text = "QTY";
//worksheet.Range["D15"].Text = "UNIT PRICE";
//worksheet.Range["E15"].Text = "AMOUNT";
//worksheet.Range["A16"].Text = "Cabrales Cheese";
//worksheet.Range["A17"].Text = "Chocos";
//worksheet.Range["A18"].Text = "Pasta";
//worksheet.Range["A19"].Text = "Cereals";
//worksheet.Range["A20"].Text = "Ice Cream";
//worksheet.Range["C16"].Number = 3;
//worksheet.Range["C17"].Number = 2;
//worksheet.Range["C18"].Number = 1;
//worksheet.Range["C19"].Number = 4;
//worksheet.Range["C20"].Number = 3;
//worksheet.Range["D16"].Number = 21;
//worksheet.Range["D17"].Number = 54;
//worksheet.Range["D18"].Number = 10;
//worksheet.Range["D19"].Number = 20;
//worksheet.Range["D20"].Number = 30;
//worksheet.Range["D23"].Text = "Total";

////Apply number format
//worksheet.Range["D16:E22"].NumberFormat = "$.00";
//worksheet.Range["E23"].NumberFormat = "$.00";

////Merge column A and B from row 15 to 22
//worksheet.Range["A15:B15"].Merge();
//worksheet.Range["A16:B16"].Merge();
//worksheet.Range["A17:B17"].Merge();
//worksheet.Range["A18:B18"].Merge();
//worksheet.Range["A19:B19"].Merge();
//worksheet.Range["A20:B20"].Merge();
//worksheet.Range["A21:B21"].Merge();
//worksheet.Range["A22:B22"].Merge();

////Apply incremental formula for column Amount by multiplying Qty and UnitPrice
//application.EnableIncrementalFormula = true;
//worksheet.Range["E16:E20"].Formula = "=C16*D16";

////Formula for Sum the total
//worksheet.Range["E23"].Formula = "=SUM(E16:E22)";

////Apply borders
//worksheet.Range["A16:E22"].CellStyle.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
//worksheet.Range["A16:E22"].CellStyle.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
//worksheet.Range["A16:E22"].CellStyle.Borders[ExcelBordersIndex.EdgeTop].Color = ExcelKnownColors.Grey_25_percent;
//worksheet.Range["A16:E22"].CellStyle.Borders[ExcelBordersIndex.EdgeBottom].Color = ExcelKnownColors.Grey_25_percent;
//worksheet.Range["A23:E23"].CellStyle.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
//worksheet.Range["A23:E23"].CellStyle.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
//worksheet.Range["A23:E23"].CellStyle.Borders[ExcelBordersIndex.EdgeTop].Color = ExcelKnownColors.Black;
//worksheet.Range["A23:E23"].CellStyle.Borders[ExcelBordersIndex.EdgeBottom].Color = ExcelKnownColors.Black;

////Apply font setting for cells with product details
//worksheet.Range["A3:E23"].CellStyle.Font.FontName = "Arial";
//worksheet.Range["A3:E23"].CellStyle.Font.Size = 10;
//worksheet.Range["A15:E15"].CellStyle.Font.Color = ExcelKnownColors.White;
//worksheet.Range["A15:E15"].CellStyle.Font.Bold = true;
//worksheet.Range["D23:E23"].CellStyle.Font.Bold = true;

////Apply cell color
//worksheet.Range["A15:E15"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(0, 42, 118, 189);

////Apply alignment to cells with product details
//worksheet.Range["A15"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
//worksheet.Range["C15:C22"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
//worksheet.Range["D15:E15"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;

////Apply row height and column width to look good
//worksheet.Range["A1"].ColumnWidth = 36;
//worksheet.Range["B1"].ColumnWidth = 11;
//worksheet.Range["C1"].ColumnWidth = 8;
//worksheet.Range["D1:E1"].ColumnWidth = 18;
//worksheet.Range["A1"].RowHeight = 47;
//worksheet.Range["A2"].RowHeight = 15;
//worksheet.Range["A3:A4"].RowHeight = 15;
//worksheet.Range["A5"].RowHeight = 18;
//worksheet.Range["A6"].RowHeight = 29;
//worksheet.Range["A7"].RowHeight = 18;
//worksheet.Range["A8"].RowHeight = 15;
//worksheet.Range["A9:A14"].RowHeight = 15;
//worksheet.Range["A15:A23"].RowHeight = 18;

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

    
}

