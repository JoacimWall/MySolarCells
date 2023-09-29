

namespace MySolarCells.Services;

public interface IRoiService
{

    Task<RoiStats> CalculateTotals(DateTime? start, DateTime? end, bool all);
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


        roiStats.TotalSaved = Convert.ToSingle(Math.Round(roiStats.TotalProductionSoldProfit + roiStats.TotalProductionOwnUseProfit + roiStats.TotalCompensationForProductionToGrid
                            + roiStats.TotalSavedTransferFeeProductionOwnUse + roiStats.TotalSavedEnergyTaxProductionOwnUse + roiStats.TotalSavedEnergyTaxReductionProductionToGrid, 2));

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



    public float TotalSavedEnergyTaxReductionProductionToGrid { get; set; } = 0;

    //Total Saved
    public float TotalSaved { get; set; } = 0;
    public float TotalProduction { get; set; } = 0;

    //fun Facts
    public float ProductionIndex { get; set; } = 0;

}
