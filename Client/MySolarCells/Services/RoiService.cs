using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
        //var test  = dbContext.Energy.FromSqlRaw("select sum(PurchasedCost) as PurchasedCost_kr from Energy");

        //dbContext.Energy.Where(x => x.Input).GroupBy(x => x.GoodsID).Select(x => x.Sum(y => y.Quantity));
        roiStats.TotalPurchasedCost = Convert.ToInt32( dbContext.Energy.Sum(x => x.PurchasedCost));
        roiStats.TotalPurchased = Convert.ToInt32(dbContext.Energy.Sum(x => x.Purchased));
        roiStats.TotalProductionSoldProfit = Convert.ToInt32(dbContext.Energy.Sum(x => x.ProductionSoldProfit));
        roiStats.TotalProductionSold = Convert.ToInt32(dbContext.Energy.Sum(x => x.ProductionSold));
        roiStats.TotalProductionOwnUseProfit = Convert.ToInt32(dbContext.Energy.Sum(x => x.ProductionOwnUseProfit));
        roiStats.TotalProductionOwnUse = Convert.ToInt32(dbContext.Energy.Sum(x => x.ProductionOwnUse));


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
    
}
