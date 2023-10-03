

namespace MySolarCells.Services;

public interface IRoiService
{

    Task<RoiStats> CalculateTotals(DateTime? start, DateTime? end, bool all);
    Task<Result<List<ReportRoiStats>>> GenerateTotalPermonthReport();
}

public class RoiService : IRoiService
{
    public RoiService()
    {
    }
    public async Task<RoiStats> CalculateTotals(DateTime? start, DateTime? end, bool all)
    {
        List<Sqlite.Models.Energy> energy;
        RoiStats roiStats = new RoiStats();
        using var dbContext = new MscDbContext();
        var calcParams = dbContext.EnergyCalculationParameter.First();
        var calcparms = await dbContext.EnergyCalculationParameter.FirstAsync();
        if (all)
            energy = await dbContext.Energy.ToListAsync();
        else
            energy = await dbContext.Energy.Where(x => x.Timestamp > start.Value && x.Timestamp <= end.Value).ToListAsync();

        //Base sums
        roiStats.TotalPurchasedCost =  Convert.ToSingle(Math.Round(energy.Sum(x => x.PurchasedCost),2));
        roiStats.TotalPurchased = Convert.ToSingle(Math.Round(energy.Sum(x => x.Purchased), 2));
        roiStats.TotalProductionSoldProfit = Convert.ToSingle(Math.Round(energy.Sum(x => x.ProductionSoldProfit), 2));
        roiStats.TotalProductionSold = Convert.ToSingle(Math.Round(energy.Sum(x => x.ProductionSold), 2));
        roiStats.TotalProductionOwnUseProfit = Convert.ToSingle(Math.Round(energy.Sum(x => x.ProductionOwnUseProfit), 2));
        roiStats.TotalProductionOwnUse = Convert.ToSingle(Math.Round(energy.Sum(x => x.ProductionOwnUse), 2));

        //Calc
        roiStats.TotalCompensationForProductionToGrid = Convert.ToSingle(Math.Round(roiStats.TotalProductionSold * calcParams.ProdCompensationElectricityLowload, 2));
        roiStats.TotalSavedTransferFeeProductionOwnUse = Convert.ToSingle(Math.Round(roiStats.TotalProductionOwnUse * calcParams.TransferFee, 2));
        roiStats.TotalTransferFeePurchased = Convert.ToSingle(Math.Round(roiStats.TotalPurchased * calcParams.TransferFee, 2));
        roiStats.TotalSavedEnergyTaxProductionOwnUse = Convert.ToSingle(Math.Round(roiStats.TotalProductionOwnUse * calcParams.EnergyTax, 2));
        roiStats.TotalTaxPurchased = Convert.ToSingle(Math.Round(roiStats.TotalPurchased * calcParams.EnergyTax, 2));
        roiStats.TotalSavedEnergyTaxReductionProductionToGrid = Convert.ToSingle(Math.Round(roiStats.TotalProductionSold * calcParams.TaxReduction, 2));


        roiStats.TotalPurchasedTransferFee = Convert.ToSingle(Math.Round(roiStats.TotalPurchased * calcparms.TransferFee, 2));
        roiStats.TotalPurchasedTax = Convert.ToSingle(Math.Round(roiStats.TotalPurchased * calcparms.EnergyTax, 2));


        //Total minus production
        roiStats.TotalProductionNegativeSold = Convert.ToSingle(Math.Round(energy.Where(x => x.UnitPriceSold < 0 && x.ProductionSold > 0).Sum(x => x.ProductionSold), 2));
        roiStats.TotalProductionSoldNegativeProfit = Convert.ToSingle(Math.Round(energy.Where(x => x.UnitPriceSold < 0 && x.ProductionSold > 0).Sum(x => x.ProductionSoldProfit), 2));

        //summation
        roiStats.SumPurchased = Convert.ToSingle(Math.Round(roiStats.TotalPurchasedCost + roiStats.TotalTransferFeePurchased + roiStats.TotalTaxPurchased,2));
        roiStats.SumProductionSold = Convert.ToSingle(Math.Round(roiStats.TotalProductionSoldProfit + roiStats.TotalCompensationForProductionToGrid + roiStats.TotalSavedEnergyTaxReductionProductionToGrid, 2));
        roiStats.SumProductionOwnUse = Convert.ToSingle(Math.Round(roiStats.TotalProductionOwnUseProfit + roiStats.TotalSavedTransferFeeProductionOwnUse + roiStats.TotalSavedEnergyTaxProductionOwnUse, 2));


        //Ränta
        var resultRanta =await  GetInterest(start, end);
        roiStats.TotalInvestment = resultRanta.Item1;
        roiStats.TotalInterest = Convert.ToSingle(Math.Round(resultRanta.Item2,2));

        roiStats.TotalSaved = Convert.ToSingle(Math.Round(roiStats.SumProductionSold + roiStats.SumProductionOwnUse - roiStats.TotalInterest, 2));

        roiStats.TotalProduction = Convert.ToSingle(Math.Round(roiStats.TotalProductionSold + roiStats.TotalProductionOwnUse, 2));
        //Production Index amount of production per installed kWh
        var soloarFirstDate = energy.FirstOrDefault(x => x.ProductionSold > 0 || x.ProductionOwnUse > 0);
        if (soloarFirstDate != null)
        {
            var difference = DateTime.Now - soloarFirstDate.Timestamp;
            roiStats.ProductionIndex = Convert.ToSingle(Math.Round((roiStats.TotalProduction / Convert.ToInt32(difference.TotalDays) / calcParams.TotalInstallKwhPanels), 2));
        }
        return roiStats;

    }

    public async Task<Result<List<ReportRoiStats>>> GenerateTotalPermonthReport()
    {
        List<ReportRoiStats> result = new List<ReportRoiStats>();
        using var dbContext = new MscDbContext();
        var  start = Helpers.DateHelper.GetRelatedDates(MySolarCellsGlobals.SelectedHome.FromDate);
        var dates = Helpers.DateHelper.GetRelatedDates(DateTime.Today);
        var current = start.ThisMonthStart;
        while (current < dates.ThisMonthEnd)
        {
            var stats = await CalculateTotals(current, current.AddMonths(1),false);
            result.Add(new ReportRoiStats { FromDate = current, RoiStats = stats });
            current = current.AddMonths(1);
        }

        return new Result<List<ReportRoiStats>>(result);
    }

    //returns total invest and total Interest
    private async Task<Tuple<int,float>> GetInterest(DateTime? start, DateTime? end)
    {
        int investmentTot = 0;
        float interest = 0;
        

        using var dbContext = new MscDbContext();
        var result = dbContext.InvestmentAndLon.Include(i => i.Interest).Where(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId).ToList();
        foreach (var item in result)
        {
            investmentTot = investmentTot + item.Investment + item.Lon;
        }
            
        DateTime current = start.Value;
        while (current < end)
        {
            foreach (var item in result)
            {
                if (item.Lon > 0 && item.Interest.Any(x => x.FromDate <= current))
                {
                    var interestCur = item.Interest.Where(x => x.FromDate <= current).OrderBy(o => o.FromDate).First();
                    interest = interest + ((item.Lon * (interestCur.Interest / 100)) / 365);

                }

            }         
  
            current = current.AddDays(1);
        }
           
        
        return new Tuple<int, float>(investmentTot, interest);
    }
}
public class ReportRoiStats
{
    public DateTime FromDate { get; set; }
    public RoiStats RoiStats { get; set; }

}
public class RoiStats
{
    public float TotalProductionSold { get; set; } = 0;
    public float TotalProductionNegativeSold { get; set; } = 0;
    public float TotalProductionOwnUse { get; set; } = 0;
    public float TotalPurchased { get; set; } = 0;



    public string Unit { get; set; } = "kWh";
    public string Currency { get; set; } = "SEK";
    public float TotalProductionSoldProfit { get; set; } = 0;
    public float TotalProductionSoldNegativeProfit { get; set; } = 0;
    public float TotalProductionOwnUseProfit { get; set; } = 0;
    public float TotalPurchasedCost { get; set; } = 0;

    //calc values
    public float TotalCompensationForProductionToGrid { get; set; } = 0;
    public float TotalSavedTransferFeeProductionOwnUse { get; set; } = 0;
    public float TotalTransferFeePurchased { get; set; } = 0;
    public float TotalSavedEnergyTaxProductionOwnUse { get; set; } = 0;
    public float TotalTaxPurchased { get; set; } = 0;
    public float TotalPurchasedTransferFee { get; set; } = 0;
    public float TotalPurchasedTax { get; set; } = 0;
    //intrest
    public float TotalInterest { get; set; } = 0;
    public float TotalInvestment { get; set; } = 0;

    public float TotalSavedEnergyTaxReductionProductionToGrid { get; set; } = 0;

    //Summ Saved
    public float SumPurchased { get; set; } = 0;
    public float SumProductionSold { get; set; } = 0;
    public float SumProductionOwnUse { get; set; } = 0;

    public float TotalSaved { get; set; } = 0;
    public float TotalProduction { get; set; } = 0;
    
    //fun Facts
    public float ProductionIndex { get; set; } = 0;

}
