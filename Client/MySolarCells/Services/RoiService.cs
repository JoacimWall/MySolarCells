using System.Collections.Generic;

namespace MySolarCells.Services;
public interface IRoiService
{
    Result<List<EstimateRoi>> CalcSavingsEstimate(Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>> historyStats);
}

public class RoiService :IRoiService
{
    private readonly MscDbContext mscDbContext;
    public RoiService(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="historyStats"></param>
    /// Input to collectins tuple 1 is for the overview summirized one Year per row
    /// The second tuple is all months summirzed per year 
    /// <returns></returns>
    public Result<List<EstimateRoi>> CalcSavingsEstimate(Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>> historyStats)
    {
        List<EstimateRoi> list = new List<EstimateRoi>();
        double totalSaving = 0;
        double lastknownInvestment = 0;
        //History Rows
        foreach (var item in historyStats.Item1)
        {
            //if current year 
            if (item.FromDate.Year == DateTime.Now.Year)
            {
                List<ReportHistoryStats> listCurrentYear=null;
                //Get months for current year
                foreach (var itemMonth in historyStats.Item2)
                {
                    if (itemMonth.Any(x => x.FromDate.Year == item.FromDate.Year))
                    {
                        listCurrentYear = itemMonth;
                        break;
                    }
                }
                //Add missing production
                double fakeproduction = 0;
                for (int i = 1; i < 13; i++)
                {
                    var afterCurrentDay = i > listCurrentYear.Count ? true : false;
                    var beforeFirstDay = i < listCurrentYear.Count ? listCurrentYear[i].FromDate < item.FirstProductionDay : false;
                    if (beforeFirstDay || afterCurrentDay)
                    {
                        var calcparmas = listCurrentYear.First().HistoryStats.EnergyCalculationParameter;
                        fakeproduction = fakeproduction + SnittProductionMonth.GetSnitMonth(i, calcparmas.TotalInstallKwhPanels);
                        //if (listCurrentYear[i].HistoryStats.FactsProductionSoldAveragePerKwhProfit == 0)
                        //    listCurrentYear[i].HistoryStats.FactsProductionSoldAveragePerKwhProfit = 
                    }

                }

                item.HistoryStats.ProductionSold = item.HistoryStats.ProductionSold + (fakeproduction * 0.5);
                item.HistoryStats.ProductionOwnUse = item.HistoryStats.ProductionOwnUse + (fakeproduction * 0.5);
            }
            var avargePrisOwnUse = item.HistoryStats.FactsBatteryUsedAveragePerKwhSaved == 0 ?
                                   item.HistoryStats.FactsProductionOwnUseAveragePerKwhSaved :
                                   Math.Round((item.HistoryStats.FactsBatteryUsedAveragePerKwhSaved + item.HistoryStats.FactsProductionOwnUseAveragePerKwhSaved) / 2, 2);

            var yearSavingsSold = Math.Round(item.HistoryStats.FactsProductionSoldAveragePerKwhProfit * item.HistoryStats.ProductionSold, 2);
            var yearSavingsOwnUse = Math.Round((item.HistoryStats.ProductionOwnUse + item.HistoryStats.BatteryUsed) * avargePrisOwnUse, 2);
            var savingThisYear = yearSavingsSold + yearSavingsOwnUse;
            totalSaving = totalSaving + savingThisYear;

            list.Add(new EstimateRoi
            {
                Year = item.FromDate.Year,
                AvargePriceSold = item.HistoryStats.FactsProductionSoldAveragePerKwhProfit,
                AvargePrisOwnUse = avargePrisOwnUse,
                ProductionSold = item.HistoryStats.ProductionSold,
                ProductionOwnUse = item.HistoryStats.ProductionOwnUse + item.HistoryStats.BatteryUsed,
                YearSavingsSold = yearSavingsSold,
                YearSavingsOwnUse = yearSavingsOwnUse,
                RemainingOnInvestment = Math.Round(item.HistoryStats.Investment - totalSaving,0),
                 ReturnPercentage = Math.Round((savingThisYear/ item.HistoryStats.Investment)*100,1)
            });

             lastknownInvestment = item.HistoryStats.Investment;
        }
        //Future years
        int startYear = historyStats.Item1.Last().FromDate.AddYears(1).Year;
        int endYear = historyStats.Item1.First().FromDate.AddYears(30).Year;
       
        double realutvecklingElpris = 1.05;
        double degrageringPerAr = 0.0025;
       
        //Räkna upp alla värden


        for (int i = startYear; i < endYear; i++)
        {
            var newYear = new EstimateRoi
            {
                Year = i,
                AvargePriceSold = Math.Round(list.Last().AvargePriceSold * realutvecklingElpris, 2),
                AvargePrisOwnUse = Math.Round(list.Last().AvargePrisOwnUse * realutvecklingElpris, 2),
                ProductionSold = Math.Round(list.Last().ProductionSold * (1 - degrageringPerAr), 2),
                ProductionOwnUse = Math.Round(list.Last().ProductionOwnUse * (1 - degrageringPerAr), 2),
              
            };
            
            newYear.YearSavingsSold = Math.Round( newYear.ProductionSold * newYear.AvargePriceSold,2);
            newYear.YearSavingsOwnUse = Math.Round(newYear.ProductionOwnUse * newYear.AvargePrisOwnUse, 2);
            var savingThisYear = Math.Round(newYear.YearSavingsSold + newYear.YearSavingsOwnUse,2);
            newYear.RemainingOnInvestment = Math.Round(lastknownInvestment - totalSaving, 0);
            newYear.ReturnPercentage = Math.Round((savingThisYear / lastknownInvestment) * 100, 1);

            totalSaving = totalSaving + savingThisYear;
            list.Add(newYear);

        }
        return new Result<List<EstimateRoi>>(list); 
    }

}

public class EstimateRoi
{
    public int Year { get; set; }
    public double AvargePriceSold { get; set; }
    public double AvargePrisOwnUse { get; set; }
    public double ProductionSold { get; set; }
    public double ProductionOwnUse { get; set; }
    public double YearSavingsSold { get; set; }
    public double YearSavingsOwnUse { get; set; }
    public double ReturnPercentage { get; set; }
    public double RemainingOnInvestment { get; set; }

}
public static class SnittProductionMonth
{
    public static double GetSnitMonth(int month, double installedKwh)
    {
        switch (month)
        {
            case 1:
                return 13.9 * installedKwh;
            case 2:
                return 32.3 * installedKwh;
            case 3:
                return 85.05 * installedKwh;
            case 4:
                return 120.75 * installedKwh;
            case 5:
                return 137.95 * installedKwh;
            case 6:
                return 136.55 * installedKwh;
            case 7:
                return 134.9 * installedKwh;
            case 8:
                return 115.65 * installedKwh;
            case 9:
                return 89.9 * installedKwh;
            case 10:
                return 54.05 * installedKwh;
            case 11:
                return 19.6 * installedKwh;
            case 12:
                return 9.6 * installedKwh;
            default:
                return 0;

        }

    }

}