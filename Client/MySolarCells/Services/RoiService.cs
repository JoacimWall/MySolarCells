using System;
using CoreVideo;
using NetTopologySuite.Index.HPRtree;

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
