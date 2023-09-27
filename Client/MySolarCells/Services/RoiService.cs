namespace MySolarCells.Services;

public interface IRoiService
{

    Task<RoiStats> CalculateTotals();
}

public class RoiService : IRoiService
{
	public RoiService()
	{
	}
	public async Task<RoiStats> CalculateTotals()
	{
		RoiStats roiStats = new RoiStats();
        using var dbContext = new MscDbContext();
        var calcparms = await dbContext.EnergyCalculationParameter.FirstAsync(); 
        //Base sums
        roiStats.TotalPurchasedCost = Convert.ToInt32( dbContext.Energy.Sum(x => x.PurchasedCost));
        roiStats.TotalPurchased = Convert.ToInt32(dbContext.Energy.Sum(x => x.Purchased));
        roiStats.TotalProductionSoldProfit = Convert.ToInt32(dbContext.Energy.Sum(x => x.ProductionSoldProfit));
        roiStats.TotalProductionSold = Convert.ToInt32(dbContext.Energy.Sum(x => x.ProductionSold));
        roiStats.TotalProductionOwnUseProfit = Convert.ToInt32(dbContext.Energy.Sum(x => x.ProductionOwnUseProfit));
        roiStats.TotalProductionOwnUse = Convert.ToInt32(dbContext.Energy.Sum(x => x.ProductionOwnUse));
        //Calc
        roiStats.TotalCompensationForProductionToGrid = Convert.ToInt32(roiStats.TotalProductionSold * calcparms.ProdCompensationElectricityLowload);
        roiStats.TotalSavedTransferFeeProductionOwnUse = Convert.ToInt32(roiStats.TotalProductionOwnUse * calcparms.TransferFee);
        roiStats.TotalSavedEnergyTaxProductionOwnUse = Convert.ToInt32(roiStats.TotalProductionOwnUse * calcparms.EnergyTax);
        roiStats.TotalSavedEnergyTaxReductionProductionToGrid = Convert.ToInt32(roiStats.TotalProductionSold * calcparms.TaxReduction);


        roiStats.TotalSaved = Convert.ToInt32(roiStats.TotalProductionSoldProfit + roiStats.TotalProductionOwnUseProfit + roiStats.TotalCompensationForProductionToGrid
                            + roiStats.TotalSavedTransferFeeProductionOwnUse + roiStats.TotalSavedEnergyTaxProductionOwnUse + roiStats.TotalSavedEnergyTaxReductionProductionToGrid);
        return roiStats;
	}
}
public class RoiStats
{
    public int TotalProductionSold { get; set; } = 0;
    public int TotalProductionOwnUse { get; set; } = 0;
    public int TotalPurchased { get; set; } = 0;

    public string Unit { get; set; } = "kWh";
    public string Currency { get; set; } = "SEK";
    public int TotalProductionSoldProfit { get; set; } = 0;
    public int TotalProductionOwnUseProfit { get; set; } = 0;
    public int TotalPurchasedCost { get; set; } = 0;

    //calc values
    public int TotalCompensationForProductionToGrid { get; set; } = 0;
    public int TotalSavedTransferFeeProductionOwnUse { get; set; } = 0;
    public int TotalSavedEnergyTaxProductionOwnUse { get; set; } = 0;
    public int TotalSavedEnergyTaxReductionProductionToGrid { get; set; } = 0;

    //Total Saved
    public int TotalSaved { get; set; } = 0;

}
