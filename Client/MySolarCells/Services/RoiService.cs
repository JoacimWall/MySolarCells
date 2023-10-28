

using System;
using Microsoft.EntityFrameworkCore;

namespace MySolarCells.Services;

public interface IRoiService
{

    Task<RoiStats> CalculateTotals(DateTime? start, DateTime? end, RoiSimulate roiSimulate);
    Task<RoiStats> CalculateTotals(DateTime? start, DateTime? end);
    Task<Result<List<ReportRoiStats>>> GenerateTotalPermonthReport();
}

public class RoiService : IRoiService
{
    public RoiService()
    {
    }
    //This function should can prpduce wrong if the cahnge of calulation parameters in the middel of the date span 


    public async Task<Result<List<ReportRoiStats>>> GenerateTotalPermonthReport()
    {
        List<ReportRoiStats> result = new List<ReportRoiStats>();
        var start = DateHelper.GetRelatedDates(MySolarCellsGlobals.SelectedHome.FromDate);
        var dates = DateHelper.GetRelatedDates(DateTime.Today);
        var current = start.ThisMonthStart;
        while (current < dates.ThisMonthEnd)
        {
            var stats = await CalculateTotals(current, current.AddMonths(1), new RoiSimulate());
            result.Add(new ReportRoiStats { FromDate = current, RoiStats = stats });
            current = current.AddMonths(1);
        }

        return new Result<List<ReportRoiStats>>(result);
    }

    public async Task<RoiStats> CalculateTotals(DateTime? start, DateTime? end)
    {

        return await CalculateTotalsInternal(start, end, new RoiSimulate { });
    }

    public async Task<RoiStats> CalculateTotals(DateTime? start, DateTime? end, RoiSimulate roiSimulate)
    {
        List<RoiStats> sumRoi = new List<RoiStats>();
        var difference = end.Value - start.Value;
        var current = start;
        while (current < end)
        {
            var stats = await CalculateTotalsInternal(current, current.Value.AddDays(1), roiSimulate);
            sumRoi.Add(stats);
            current = current.Value.AddDays(1);
        }
        RoiStats returnRoi = new RoiStats
        {
            EnergyCalculationParameter = sumRoi.First().EnergyCalculationParameter,
            Currency = sumRoi.First().Currency,
            Unit = sumRoi.First().Unit,
            TotalProductionSold = Math.Round(sumRoi.Sum(x => x.TotalProductionSold),2),
            TotalProductionNegativeSold = Math.Round(sumRoi.Sum(x => x.TotalProductionNegativeSold), 2),
            TotalProductionOwnUse = Math.Round(sumRoi.Sum(x => x.TotalProductionOwnUse), 2),
            TotalPurchased = Math.Round(sumRoi.Sum(x => x.TotalPurchased), 2),
            TotalBatteryCharge = Math.Round(sumRoi.Sum(x => x.TotalBatteryCharge), 2),
            TotalBatteryUsed = Math.Round(sumRoi.Sum(x => x.TotalBatteryUsed), 2),


            TotalProductionSoldProfit = Math.Round(sumRoi.Sum(x => x.TotalProductionSoldProfit), 2),
            TotalProductionSoldNegativeProfit = Math.Round(sumRoi.Sum(x => x.TotalProductionSoldNegativeProfit), 2),
            TotalProductionOwnUseProfit = Math.Round(sumRoi.Sum(x => x.TotalProductionOwnUseProfit), 2),
            TotalBatteryUsedProfit = Math.Round(sumRoi.Sum(x => x.TotalBatteryUsedProfit), 2),
            //This is only for fun you dont sell this kwh you load the battery  
            TotalBatteryChargeProfitFake = Math.Round(sumRoi.Sum(x => x.TotalBatteryChargeProfitFake), 2),

            TotalPurchasedCost = Math.Round(sumRoi.Sum(x => x.TotalPurchasedCost), 2),

            //calc values
            TotalCompensationForProductionToGrid = Math.Round(sumRoi.Sum(x => x.TotalCompensationForProductionToGrid), 2),
            TotalCompensationForProductionToGridChargeBatteryFake = Math.Round(sumRoi.Sum(x => x.TotalCompensationForProductionToGridChargeBatteryFake), 2),

            TotalSavedTransferFeeProductionOwnUse = Math.Round(sumRoi.Sum(x => x.TotalSavedTransferFeeProductionOwnUse), 2),
            TotalSavedTransferFeeBatteryUse = Math.Round(sumRoi.Sum(x => x.TotalSavedTransferFeeBatteryUse), 2),
            TotalTransferFeePurchased = Math.Round(sumRoi.Sum(x => x.TotalTransferFeePurchased), 2),
            TotalSavedEnergyTaxProductionOwnUse = Math.Round(sumRoi.Sum(x => x.TotalSavedEnergyTaxProductionOwnUse), 2),
            TotalSavedEnergyTaxBatteryUse = Math.Round(sumRoi.Sum(x => x.TotalSavedEnergyTaxBatteryUse), 2),


            TotalTaxPurchased = Math.Round(sumRoi.Sum(x => x.TotalTaxPurchased), 2),
            TotalPurchasedTransferFee = Math.Round(sumRoi.Sum(x => x.TotalPurchasedTransferFee), 2),
            TotalPurchasedTax = Math.Round(sumRoi.Sum(x => x.TotalPurchasedTax), 2),
            //intrest
            TotalInterest = Math.Round(sumRoi.Sum(x => x.TotalInterest), 2),
            TotalInvestment = Math.Round(sumRoi.Sum(x => x.TotalInvestment), 2),

            TotalSavedEnergyTaxReductionProductionToGrid = Math.Round(sumRoi.Sum(x => x.TotalSavedEnergyTaxReductionProductionToGrid), 2),
            TotalSavedEnergyTaxReductionBatteryChargeFakeToGrid = Math.Round(sumRoi.Sum(x => x.TotalSavedEnergyTaxReductionBatteryChargeFakeToGrid), 2),


            //Summ Saved
            SumPurchased = Math.Round(sumRoi.Sum(x => x.SumPurchased), 2),
            SumProductionSold = Math.Round(sumRoi.Sum(x => x.SumProductionSold), 2),
            SumProductionBatteryChargeFakeSold = Math.Round(sumRoi.Sum(x => x.SumProductionBatteryChargeFakeSold), 2),
            SumProductionOwnUseAndBattery = Math.Round(sumRoi.Sum(x => x.SumProductionOwnUseAndBattery), 2),
            SumProductionSoldAndOwnUseAndBattery = Math.Round(sumRoi.Sum(x => x.SumProductionSoldAndOwnUseAndBattery), 2),
            TotalSaved = Math.Round(sumRoi.Sum(x => x.TotalSaved), 2),
            TotalProduction = Math.Round(sumRoi.Sum(x => x.TotalProduction), 2),
            TotalConsumption = Math.Round(sumRoi.Sum(x => x.TotalConsumption), 2),
            //fun Facts
            ProductionIndex = Math.Round(sumRoi.Sum(x => x.ProductionIndex), 2)





        };
        returnRoi.ProductionIndex = Math.Round(returnRoi.ProductionIndex / difference.TotalDays, 2);
        return returnRoi;

    }
    #region Private
    private async Task<RoiStats> CalculateTotalsInternal(DateTime? start, DateTime? end, RoiSimulate roiSimulate)
    {
        using var dbContext = new MscDbContext();


        List<Sqlite.Models.Energy> energy;
        RoiStats roiStats = new RoiStats();
        //roiStats.RoiStatsLines = new List<RoiStatsLine>();

        var calcParams = dbContext.EnergyCalculationParameter.AsNoTracking().OrderBy(o => o.FromDate).Last(x => x.FromDate <= start);

        energy = await dbContext.Energy.AsNoTracking().Where(x => x.Timestamp > start.Value && x.Timestamp <= end.Value).ToListAsync();

        if (roiSimulate.DoSimulate)
        {
            //Simulate battery 
            foreach (var item in energy)
            {
                //Check if battery can be charged
                if (item.ProductionSold > 0 && (roiSimulate.CurrentBatteryPower + item.ProductionSold) < roiSimulate.MaxBatteryPower)
                {
                    //Battery Charged
                    item.BatteryCharge = item.ProductionSold;
                    roiSimulate.CurrentBatteryPower = roiSimulate.CurrentBatteryPower + item.ProductionSold;
                   

                    //Battery Charged
                    item.BatteryCharge = item.ProductionSold;
                    //if (calcParams.UseSpotPrice)
                    //    item.BatteryChargeProfitFake = item.BatteryCharge * item.UnitPriceSold;
                    //else
                    //    item.BatteryChargeProfitFake = item.BatteryCharge * calcParams.FixedPriceKwh;

                    item.ProductionSold = 0;
                    item.ProductionSoldProfit = 0;

                }
                //Check if battery has more charge then Purchased
                if (item.Purchased > 0 && roiSimulate.CurrentBatteryPower > item.Purchased)
                {
                    roiSimulate.CurrentBatteryPower = roiSimulate.CurrentBatteryPower - item.Purchased;
                    item.BatteryUsed = item.Purchased;
                    if (calcParams.UseSpotPrice)
                        item.BatteryUsedProfit = Math.Round(item.BatteryUsed * item.UnitPriceBuy, 2);


                    //Miinska een anvädning profit med batteri
                    item.Purchased = 0;
                    item.PurchasedCost = 0;

                }
            }
        }

        //Base sums
        //Consumed
        roiStats.TotalPurchased = Math.Round(energy.Sum(x => x.Purchased), 2);
        //roiStats.RoiStatsLines.Add(new RoiStatsLine { DisplayName })


        if (calcParams.UseSpotPrice)
            roiStats.TotalPurchasedCost = Math.Round(energy.Sum(x => x.PurchasedCost), 2);
        else
            roiStats.TotalPurchasedCost = Math.Round(roiStats.TotalPurchased * calcParams.FixedPriceKwh, 2);

        //Sold
        roiStats.TotalProductionSold = Math.Round(energy.Sum(x => x.ProductionSold), 2);
        if (calcParams.UseSpotPrice)
            roiStats.TotalProductionSoldProfit = Math.Round(energy.Sum(x => x.ProductionSoldProfit), 2);
        else
            roiStats.TotalProductionSoldProfit = Math.Round(roiStats.TotalProductionSold * calcParams.FixedPriceKwh, 2);

        //Production Own use
        roiStats.TotalProductionOwnUse = Math.Round(energy.Sum(x => x.ProductionOwnUse), 2);
        if (calcParams.UseSpotPrice)
            roiStats.TotalProductionOwnUseProfit = Math.Round(energy.Sum(x => x.ProductionOwnUseProfit), 2);
        else
            roiStats.TotalProductionOwnUseProfit = Math.Round(roiStats.TotalProductionOwnUse * calcParams.FixedPriceKwh, 2);
        //Battery Used
        roiStats.TotalBatteryUsed = Math.Round(energy.Sum(x => x.BatteryUsed), 2);
        if (calcParams.UseSpotPrice)
            roiStats.TotalBatteryUsedProfit = Math.Round(energy.Sum(x => x.BatteryUsedProfit), 2);
        else
            roiStats.TotalBatteryUsedProfit = Math.Round(roiStats.TotalBatteryUsed * calcParams.FixedPriceKwh, 2);
        //Battery Charge
        roiStats.TotalBatteryCharge = Math.Round(energy.Sum(x => x.BatteryCharge), 2);
        //if (calcParams.UseSpotPrice)
        //    roiStats.TotalBatteryChargeProfitFake = Math.Round(energy.Sum(x => x.BatteryChargeProfitFake), 2);
        //else
        //    roiStats.TotalBatteryChargeProfitFake = Math.Round(roiStats.TotalBatteryCharge * calcParams.FixedPriceKwh, 2);

        //Calc
        roiStats.TotalCompensationForProductionToGrid = Math.Round(roiStats.TotalProductionSold * calcParams.ProdCompensationElectricityLowload, 2);
        roiStats.TotalCompensationForProductionToGridChargeBatteryFake = Math.Round(roiStats.TotalBatteryCharge * calcParams.ProdCompensationElectricityLowload, 2);

        roiStats.TotalSavedTransferFeeProductionOwnUse = Math.Round(roiStats.TotalProductionOwnUse * calcParams.TransferFee, 2);
        //roiStats.TotalSavedTransferFeeBatteryChargeFake = Convert.ToSingle(Math.Round(roiStats.TotalBatteryCharge * calcParams.TransferFee, 2));
        roiStats.TotalSavedTransferFeeBatteryUse = Math.Round(roiStats.TotalBatteryUsed * calcParams.TransferFee, 2);
        roiStats.TotalTransferFeePurchased = Math.Round(roiStats.TotalPurchased * calcParams.TransferFee, 2);
        roiStats.TotalSavedEnergyTaxProductionOwnUse = Math.Round(roiStats.TotalProductionOwnUse * calcParams.EnergyTax, 2);
        //roiStats.TotalSavedEnergyTaxBatteryChargeFake = Convert.ToSingle(Math.Round(roiStats.TotalBatteryCharge * calcParams.EnergyTax, 2));
        roiStats.TotalSavedEnergyTaxBatteryUse = Math.Round(roiStats.TotalBatteryUsed * calcParams.EnergyTax, 2);
        roiStats.TotalTaxPurchased = Math.Round(roiStats.TotalPurchased * calcParams.EnergyTax, 2);
        roiStats.TotalSavedEnergyTaxReductionProductionToGrid = Math.Round(roiStats.TotalProductionSold * calcParams.TaxReduction, 2);
        roiStats.TotalSavedEnergyTaxReductionBatteryChargeFakeToGrid = Math.Round(roiStats.TotalBatteryCharge * calcParams.TaxReduction, 2);


        roiStats.TotalPurchasedTransferFee = Math.Round(roiStats.TotalPurchased * calcParams.TransferFee, 2);
        roiStats.TotalPurchasedTax = Math.Round(roiStats.TotalPurchased * calcParams.EnergyTax, 2);


        //Total minus production
        roiStats.TotalProductionNegativeSold = Math.Round(energy.Where(x => x.UnitPriceSold < 0 && x.ProductionSold > 0).Sum(x => x.ProductionSold), 2);
        roiStats.TotalProductionSoldNegativeProfit = Math.Round(energy.Where(x => x.UnitPriceSold < 0 && x.ProductionSold > 0).Sum(x => x.ProductionSoldProfit), 2);

        //summation
        roiStats.SumPurchased = Math.Round(roiStats.TotalPurchasedCost + roiStats.TotalTransferFeePurchased + roiStats.TotalTaxPurchased, 2);
        roiStats.SumProductionSold = Math.Round(roiStats.TotalProductionSoldProfit + roiStats.TotalCompensationForProductionToGrid + roiStats.TotalSavedEnergyTaxReductionProductionToGrid, 2);
        roiStats.SumProductionBatteryChargeFakeSold = Math.Round(roiStats.TotalBatteryChargeProfitFake + roiStats.TotalCompensationForProductionToGridChargeBatteryFake + roiStats.TotalSavedEnergyTaxReductionBatteryChargeFakeToGrid, 2);
        roiStats.SumProductionOwnUseAndBattery = Math.Round(roiStats.TotalProductionOwnUseProfit + roiStats.TotalSavedTransferFeeProductionOwnUse + roiStats.TotalSavedEnergyTaxProductionOwnUse + roiStats.TotalBatteryUsedProfit + roiStats.TotalSavedTransferFeeBatteryUse + roiStats.TotalSavedEnergyTaxBatteryUse, 2);

        roiStats.SumProductionSoldAndOwnUseAndBattery = Math.Round((roiStats.SumProductionSold + roiStats.SumProductionOwnUseAndBattery), 2);

        //Ränta
        var resultRanta = await GetInterest(start, end);
        roiStats.TotalInvestment = resultRanta.Item1;
        roiStats.TotalInterest = Math.Round(resultRanta.Item2, 2);

        roiStats.TotalSaved = Math.Round(roiStats.SumProductionSold + roiStats.SumProductionOwnUseAndBattery - roiStats.TotalInterest, 2);

        roiStats.TotalProduction = Math.Round(roiStats.TotalProductionSold + roiStats.TotalProductionOwnUse + roiStats.TotalBatteryCharge, 2);
        roiStats.TotalConsumption = Math.Round(roiStats.TotalPurchased + roiStats.TotalProductionOwnUse + roiStats.TotalBatteryUsed, 2);


        //Production Index amount of production per installed kWh
        var soloarFirstDate = energy.FirstOrDefault(x => x.ProductionSold > 0 || x.ProductionOwnUse > 0);
        if (soloarFirstDate != null)
        {
            var enddate = end > DateTime.Now ? DateTime.Now : end;
            var difference = enddate - soloarFirstDate.Timestamp;
            roiStats.ProductionIndex = Math.Round((roiStats.TotalProduction / Convert.ToInt32(difference.Value.TotalDays) / calcParams.TotalInstallKwhPanels), 2);
        }
        roiStats.EnergyCalculationParameter = calcParams;
        return roiStats;

    }
    //returns total invest and total Interest
    private async Task<Tuple<int, float>> GetInterest(DateTime? start, DateTime? end)
    {
        int investmentTot = 0;
        float interest = 0;


        using var dbContext = new MscDbContext();
        var result = dbContext.InvestmentAndLon.AsNoTracking().Include(i => i.Interest).Where(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId).ToList();
        foreach (var item in result)
        {
            investmentTot = investmentTot + item.Investment + item.Loan;
        }

        DateTime current = start.Value;
        while (current < end)
        {
            foreach (var item in result)
            {
                if (item.Loan > 0 && item.Interest.Any(x => x.FromDate <= current))
                {
                    var interestCur = item.Interest.Where(x => x.FromDate <= current).OrderBy(o => o.FromDate).First();
                    interest = interest + ((item.Loan * (interestCur.Interest / 100)) / 365);

                }

            }

            current = current.AddDays(1);
        }


        return new Tuple<int, float>(investmentTot, interest);
    }
    #endregion
}
public class RoiSimulate
{
    public bool DoSimulate { get; set; }
    public double MaxBatteryPower { get; set; } = 10;
    public double ChargeLoss { get; set; } = 0.05;
    public double CurrentBatteryPower { get; set; }
}
public class ReportRoiStats
{
    public DateTime FromDate { get; set; }
    public RoiStats RoiStats { get; set; }

}
public class RoiStatsLine
{
    public string DisplayName { get; set; }
    public RoiStatsLineType RoiStatsLineType { get; set; }
    public RoiStatsLineGroup RoiStatsLineGroup { get; set; }
    public float Value { get; set; }
    public bool IsResult { get; set; }
}
public enum RoiStatsLineType
{

}
public enum RoiStatsLineGroup
{

}
public class RoiStats
{
    //public List<RoiStatsLine> RoiStatsLines;


    public double TotalProductionSold { get; set; } = 0;
    public double TotalProductionNegativeSold { get; set; } = 0;
    public double TotalProductionOwnUse { get; set; } = 0;
    public double TotalPurchased { get; set; } = 0;
    public double TotalBatteryCharge { get; set; } = 0;
    public double TotalBatteryUsed { get; set; } = 0;

    public string Unit { get; set; } = "kWh";
    public string Currency { get; set; } = "SEK";
    public double TotalProductionSoldProfit { get; set; } = 0;
    public double TotalProductionSoldNegativeProfit { get; set; } = 0;
    public double TotalProductionOwnUseProfit { get; set; } = 0;
    public double TotalBatteryUsedProfit { get; set; } = 0;
    //This is only for fun you dont sell this kwh you load the battery  
    public double TotalBatteryChargeProfitFake { get; set; } = 0;

    public double TotalPurchasedCost { get; set; } = 0;

    //calc values
    public double TotalCompensationForProductionToGrid { get; set; } = 0;
    public double TotalCompensationForProductionToGridChargeBatteryFake { get; set; } = 0;

    public double TotalSavedTransferFeeProductionOwnUse { get; set; } = 0;
    public double TotalSavedTransferFeeBatteryUse { get; set; } = 0;
    //public float TotalSavedTransferFeeBatteryChargeFake { get; set; } = 0;
    public double TotalTransferFeePurchased { get; set; } = 0;
    public double TotalSavedEnergyTaxProductionOwnUse { get; set; } = 0;
    public double TotalSavedEnergyTaxBatteryUse { get; set; } = 0;
    //public float TotalSavedEnergyTaxBatteryChargeFake { get; set; } = 0;

    public double TotalTaxPurchased { get; set; } = 0;
    public double TotalPurchasedTransferFee { get; set; } = 0;
    public double TotalPurchasedTax { get; set; } = 0;
    //intrest
    public double TotalInterest { get; set; } = 0;
    public double TotalInvestment { get; set; } = 0;

    public double TotalSavedEnergyTaxReductionProductionToGrid { get; set; } = 0;
    public double TotalSavedEnergyTaxReductionBatteryChargeFakeToGrid { get; set; } = 0;


    //Summ Saved
    public double SumPurchased { get; set; } = 0;
    public double SumProductionSold { get; set; } = 0;
    public double SumProductionBatteryChargeFakeSold { get; set; } = 0;
    public double SumProductionOwnUseAndBattery { get; set; } = 0;
    public double SumProductionSoldAndOwnUseAndBattery { get; set; } = 0;
    
    public double TotalSaved { get; set; } = 0;
    public double TotalProduction { get; set; } = 0;
    public double TotalConsumption { get; set; } = 0;

    //fun Facts
    public double ProductionIndex { get; set; } = 0;

    public Sqlite.Models.EnergyCalculationParameter EnergyCalculationParameter { get; set; }

}
