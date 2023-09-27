namespace MySolarCells.Services;

public interface IEnergyChartService
{

    Task<RoiStats> CalculateTotals();
}

public class EnergyChartService : IEnergyChartService
{
	public EnergyChartService()
	{
	}
    public async Task<Microcharts.ChartSerie> GetChartData()
    {

        return new Microcharts.ChartSerie();
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

