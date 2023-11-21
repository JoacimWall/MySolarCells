namespace MySolarCells.Services;
public interface IRoiService
{
    Result<List<EstimateRoi>> CalcSavingsEstimate(Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>> historyStats, SavingEssitmateParameters savingEssitmateParameters);
}

public class RoiService :IRoiService
{
    public RoiService()
    {

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="historyStats"></param>
    /// Input to collectins tuple 1 is for the overview summirized one Year per row
    /// The second tuple is all months summirzed per year 
    /// <returns></returns>
    public Result<List<EstimateRoi>> CalcSavingsEstimate(Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>> historyStats, SavingEssitmateParameters savingEssitmateParameters)
    {
        List<EstimateRoi> list = new List<EstimateRoi>();
        double totalSaving = 0;
        double lastknownInvestment = 0;
        int yearCountFromStart = 1;
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
                YearFromStart = yearCountFromStart,
                AvargePriceSold = item.HistoryStats.FactsProductionSoldAveragePerKwhProfit,
                AvargePrisOwnUse = avargePrisOwnUse,
                ProductionSold = item.HistoryStats.ProductionSold,
                ProductionOwnUse = item.HistoryStats.ProductionOwnUse + item.HistoryStats.BatteryUsed,
                YearSavingsSold = yearSavingsSold,
                YearSavingsOwnUse = yearSavingsOwnUse,
                RemainingOnInvestment = Math.Round(item.HistoryStats.Investment - totalSaving,0),
                ReturnPercentage = Math.Round((savingThisYear/ item.HistoryStats.Investment)*100,1)
            });
            yearCountFromStart++;
             lastknownInvestment = item.HistoryStats.Investment;
        }
        //Future years
        int startYear = historyStats.Item1.Last().FromDate.AddYears(1).Year;
        int endYear = historyStats.Item1.First().FromDate.AddYears(30).Year;

        //double realutvecklingElpris = 1.05;
        //double degrageringPerAr = 0.0025;

        //Räkna upp alla värden

        bool roiYearSet = false;
        for (int i = startYear; i < endYear; i++)
        {
            var newYear = new EstimateRoi
            {
                Year = i,
                YearFromStart = yearCountFromStart,
                AvargePriceSold = Math.Round(list.Last().AvargePriceSold * (1 + savingEssitmateParameters.RealDevelopmentElectricityPrice / 100), 2),
                AvargePrisOwnUse = Math.Round(list.Last().AvargePrisOwnUse * (1 + savingEssitmateParameters.RealDevelopmentElectricityPrice / 100), 2),
                ProductionSold = Math.Round(list.Last().ProductionSold * (1 - savingEssitmateParameters.PanelDegradationPerYear/100), 2),
                ProductionOwnUse = Math.Round(list.Last().ProductionOwnUse * (1 - savingEssitmateParameters.PanelDegradationPerYear/100), 2),
              
            };
            
            newYear.YearSavingsSold = Math.Round( newYear.ProductionSold * newYear.AvargePriceSold,2);
            newYear.YearSavingsOwnUse = Math.Round(newYear.ProductionOwnUse * newYear.AvargePrisOwnUse, 2);
            var savingThisYear = Math.Round(newYear.YearSavingsSold + newYear.YearSavingsOwnUse,2);
            totalSaving = totalSaving + savingThisYear;
            newYear.RemainingOnInvestment = Math.Round(lastknownInvestment - totalSaving, 0);
            if (newYear.RemainingOnInvestment < 0)
            {
                newYear.RemainingOnInvestment = Math.Abs(newYear.RemainingOnInvestment);
                if (!roiYearSet)
                {
                    newYear.IsRoiYear = true;
                    roiYearSet = true;
                }
            }
            newYear.ReturnPercentage = Math.Round((savingThisYear / lastknownInvestment) * 100, 1);

            list.Add(newYear);
            yearCountFromStart++;
        }
        return new Result<List<EstimateRoi>>(list); 
    }
   
}


public static class SnittProductionMonth
{
    public static double GetSnitMonth(int month, double installedKw)
    {
        switch (month)
        {
            case 1:
                return 13.9 * installedKw;
            case 2:
                return 32.3 * installedKw;
            case 3:
                return 85.05 * installedKw;
            case 4:
                return 120.75 * installedKw;
            case 5:
                return 137.95 * installedKw;
            case 6:
                return 136.55 * installedKw;
            case 7:
                return 134.9 * installedKw;
            case 8:
                return 115.65 * installedKw;
            case 9:
                return 89.9 * installedKw;
            case 10:
                return 54.05 * installedKw;
            case 11:
                return 19.6 * installedKw;
            case 12:
                return 9.6 * installedKw;
            default:
                return 0;

        }

    }

}