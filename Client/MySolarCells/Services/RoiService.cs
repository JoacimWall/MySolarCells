

using System;
using Microsoft.EntityFrameworkCore;

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
    //This function should can prpduce wrong if the cahnge of calulation parameters in the middel of the date span 
    public async Task<RoiStats> CalculateTotals(DateTime? start, DateTime? end, bool all)
    {
        using var dbContext = new MscDbContext();
        

        List<Sqlite.Models.Energy> energy;
        RoiStats roiStats = new RoiStats();
        
        var calcParams = dbContext.EnergyCalculationParameter.AsNoTracking().OrderBy(o => o.FromDate).Last(x => x.FromDate <= start);
        //var calcparms = await dbContext.EnergyCalculationParameter.FirstAsync();
        if (all)
            energy = await dbContext.Energy.AsNoTracking().ToListAsync();
        else
            energy = await dbContext.Energy.AsNoTracking().Where(x => x.Timestamp > start.Value && x.Timestamp <= end.Value).ToListAsync();

        //Base sums
        //Consumed
        roiStats.TotalPurchased = Convert.ToSingle(Math.Round(energy.Sum(x => x.Purchased), 2));
        if (calcParams.UseSpotPrice)
            roiStats.TotalPurchasedCost =  Convert.ToSingle(Math.Round(energy.Sum(x => x.PurchasedCost),2));
        else
            roiStats.TotalPurchasedCost = Convert.ToSingle(Math.Round(roiStats.TotalPurchased * calcParams.FixedPriceKwh, 2));

        //Sold
        roiStats.TotalProductionSold = Convert.ToSingle(Math.Round(energy.Sum(x => x.ProductionSold), 2));
        if (calcParams.UseSpotPrice)
            roiStats.TotalProductionSoldProfit = Convert.ToSingle(Math.Round(energy.Sum(x => x.ProductionSoldProfit), 2));
        else
            roiStats.TotalProductionSoldProfit = Convert.ToSingle(Math.Round(roiStats.TotalProductionSold * calcParams.FixedPriceKwh, 2));

        //Production Own use
        roiStats.TotalProductionOwnUse = Convert.ToSingle(Math.Round(energy.Sum(x => x.ProductionOwnUse), 2));
        if (calcParams.UseSpotPrice)
            roiStats.TotalProductionOwnUseProfit = Convert.ToSingle(Math.Round(energy.Sum(x => x.ProductionOwnUseProfit), 2));
        else
            roiStats.TotalProductionOwnUseProfit = Convert.ToSingle(Math.Round(roiStats.TotalProductionOwnUse * calcParams.FixedPriceKwh, 2));
        //Battery Used
        roiStats.TotalBatteryUsed = Convert.ToSingle(Math.Round(energy.Sum(x => x.BatteryUsed), 2));
        if (calcParams.UseSpotPrice)
            roiStats.TotalBatteryUsedProfit = Convert.ToSingle(Math.Round(energy.Sum(x => x.BatteryUsedProfit), 2));
        else
            roiStats.TotalBatteryUsedProfit = Convert.ToSingle(Math.Round(roiStats.TotalBatteryUsed * calcParams.FixedPriceKwh, 2));
        //Battery Used
        roiStats.TotalBatteryCharge = Convert.ToSingle(Math.Round(energy.Sum(x => x.BatteryCharge), 2));
        if (calcParams.UseSpotPrice)
            roiStats.TotalBatteryChargeProfitFake = Convert.ToSingle(Math.Round(energy.Sum(x => x.BatteryChargeProfitFake), 2));
        else
            roiStats.TotalBatteryChargeProfitFake = Convert.ToSingle(Math.Round(roiStats.TotalBatteryCharge * calcParams.FixedPriceKwh, 2));

        //Calc
        roiStats.TotalCompensationForProductionToGrid = Convert.ToSingle(Math.Round(roiStats.TotalProductionSold * calcParams.ProdCompensationElectricityLowload, 2));
        roiStats.TotalSavedTransferFeeProductionOwnUse = Convert.ToSingle(Math.Round(roiStats.TotalProductionOwnUse * calcParams.TransferFee, 2));
        roiStats.TotalSavedTransferFeeBatteryChargeFake = Convert.ToSingle(Math.Round(roiStats.TotalBatteryCharge * calcParams.TransferFee, 2));
        roiStats.TotalSavedTransferFeeBatteryUse = Convert.ToSingle(Math.Round(roiStats.TotalBatteryUsed * calcParams.TransferFee, 2));
        roiStats.TotalTransferFeePurchased = Convert.ToSingle(Math.Round(roiStats.TotalPurchased * calcParams.TransferFee, 2));
        roiStats.TotalSavedEnergyTaxProductionOwnUse = Convert.ToSingle(Math.Round(roiStats.TotalProductionOwnUse * calcParams.EnergyTax, 2));
        roiStats.TotalSavedEnergyTaxBatteryChargeFake = Convert.ToSingle(Math.Round(roiStats.TotalBatteryCharge * calcParams.EnergyTax, 2));
        roiStats.TotalSavedEnergyTaxBatteryUse = Convert.ToSingle(Math.Round(roiStats.TotalBatteryUsed * calcParams.EnergyTax, 2));
        roiStats.TotalTaxPurchased = Convert.ToSingle(Math.Round(roiStats.TotalPurchased * calcParams.EnergyTax, 2));
        roiStats.TotalSavedEnergyTaxReductionProductionToGrid = Convert.ToSingle(Math.Round(roiStats.TotalProductionSold * calcParams.TaxReduction, 2));


        roiStats.TotalPurchasedTransferFee = Convert.ToSingle(Math.Round(roiStats.TotalPurchased * calcParams.TransferFee, 2));
        roiStats.TotalPurchasedTax = Convert.ToSingle(Math.Round(roiStats.TotalPurchased * calcParams.EnergyTax, 2));


        //Total minus production
        roiStats.TotalProductionNegativeSold = Convert.ToSingle(Math.Round(energy.Where(x => x.UnitPriceSold < 0 && x.ProductionSold > 0).Sum(x => x.ProductionSold), 2));
        roiStats.TotalProductionSoldNegativeProfit = Convert.ToSingle(Math.Round(energy.Where(x => x.UnitPriceSold < 0 && x.ProductionSold > 0).Sum(x => x.ProductionSoldProfit), 2));

        //summation
        roiStats.SumPurchased = Convert.ToSingle(Math.Round(roiStats.TotalPurchasedCost + roiStats.TotalTransferFeePurchased + roiStats.TotalTaxPurchased,2));
        roiStats.SumProductionSold = Convert.ToSingle(Math.Round(roiStats.TotalProductionSoldProfit + roiStats.TotalCompensationForProductionToGrid + roiStats.TotalSavedEnergyTaxReductionProductionToGrid, 2));
        roiStats.SumProductionOwnUseAndBattery = Convert.ToSingle(Math.Round(roiStats.TotalProductionOwnUseProfit + roiStats.TotalSavedTransferFeeProductionOwnUse + roiStats.TotalSavedEnergyTaxProductionOwnUse + roiStats.TotalBatteryUsedProfit + roiStats.TotalSavedTransferFeeBatteryUse + roiStats.TotalSavedEnergyTaxBatteryUse, 2));


        //Ränta
        var resultRanta =await  GetInterest(start, end);
        roiStats.TotalInvestment = resultRanta.Item1;
        roiStats.TotalInterest = Convert.ToSingle(Math.Round(resultRanta.Item2,2));

        roiStats.TotalSaved = Convert.ToSingle(Math.Round(roiStats.SumProductionSold + roiStats.SumProductionOwnUseAndBattery - roiStats.TotalInterest, 2));

        roiStats.TotalProduction = Convert.ToSingle(Math.Round(roiStats.TotalProductionSold + roiStats.TotalProductionOwnUse, 2));
        //Production Index amount of production per installed kWh
        var soloarFirstDate = energy.FirstOrDefault(x => x.ProductionSold > 0 || x.ProductionOwnUse > 0);
        if (soloarFirstDate != null)
        {
            var difference = DateTime.Now - soloarFirstDate.Timestamp;
            roiStats.ProductionIndex = Convert.ToSingle(Math.Round((roiStats.TotalProduction / Convert.ToInt32(difference.TotalDays) / calcParams.TotalInstallKwhPanels), 2));
        }
        roiStats.EnergyCalculationParameter = calcParams;
        return roiStats;

    }

    public async Task<Result<List<ReportRoiStats>>> GenerateTotalPermonthReport()
    {
        List<ReportRoiStats> result = new List<ReportRoiStats>();
        var  start = DateHelper.GetRelatedDates(MySolarCellsGlobals.SelectedHome.FromDate);
        var dates = DateHelper.GetRelatedDates(DateTime.Today);
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
        var result = dbContext.InvestmentAndLon.AsNoTracking().Include(i => i.Interest).Where(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId).ToList();
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
    public float TotalBatteryCharge { get; set; } = 0;
    public float TotalBatteryUsed { get; set; } = 0;

    public string Unit { get; set; } = "kWh";
    public string Currency { get; set; } = "SEK";
    public float TotalProductionSoldProfit { get; set; } = 0;
    public float TotalProductionSoldNegativeProfit { get; set; } = 0;
    public float TotalProductionOwnUseProfit { get; set; } = 0;
    public float TotalBatteryUsedProfit { get; set; } = 0;
    //This is only for fun you dont sell this kwh you load the battery  
    public float TotalBatteryChargeProfitFake { get; set; } = 0;
    
    public float TotalPurchasedCost { get; set; } = 0;

    //calc values
    public float TotalCompensationForProductionToGrid { get; set; } = 0;
    public float TotalSavedTransferFeeProductionOwnUse { get; set; } = 0;
    public float TotalSavedTransferFeeBatteryUse { get; set; } = 0;
    public float TotalSavedTransferFeeBatteryChargeFake { get; set; } = 0;
    public float TotalTransferFeePurchased { get; set; } = 0;
    public float TotalSavedEnergyTaxProductionOwnUse { get; set; } = 0;
    public float TotalSavedEnergyTaxBatteryUse { get; set; } = 0;
    public float TotalSavedEnergyTaxBatteryChargeFake { get; set; } = 0;

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
    public float SumProductionOwnUseAndBattery { get; set; } = 0;

    public float TotalSaved { get; set; } = 0;
    public float TotalProduction { get; set; } = 0;
    
    //fun Facts
    public float ProductionIndex { get; set; } = 0;

    public Sqlite.Models.EnergyCalculationParameter EnergyCalculationParameter { get; set; }

}
