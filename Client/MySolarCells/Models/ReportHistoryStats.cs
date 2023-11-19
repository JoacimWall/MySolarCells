using System;
using MySolarCells.Services.Sqlite.Models;

namespace MySolarCells.Models;

public class ReportHistoryStats
{
    public DateTime FirstProductionDay { get; set; }
    public DateTime FromDate { get; set; }
    public HistoryStats HistoryStats { get; set; }
    public int ReportPageTyp { get; set; } = 3;
}

public class HistoryStats
{
    
    public string Unit { get; set; } = "kWh";
    public string Currency { get; set; } = "SEK";

    // ----------- Purchased ---------------------
    public double Purchased { get; set; } = 0;
    public double PurchasedCost { get; set; } = 0;
    public double PurchasedTransferFeeCost { get; set; } = 0;
    public double PurchasedTaxCost { get; set; } = 0;
    public double SumPurchasedCost
    {
        get { return Math.Round(PurchasedCost + PurchasedTransferFeeCost + PurchasedTaxCost, 2); }
    }
    // ----------- ProductionSold ---------------------
    public double ProductionSold { get; set; } = 0;
    public double ProductionSoldProfit { get; set; } = 0;
    public double ProductionSoldGridCompensationProfit { get; set; } = 0;
    public double ProductionSoldTaxReductionProfit { get; set; } = 0;
    public string ProductionSoldTaxReductionProfitComment { get; set; } = "";
    public double SumProductionSoldProfit
    {
        get { return Math.Round(ProductionSoldProfit + ProductionSoldGridCompensationProfit + ProductionSoldTaxReductionProfit, 2); }
    }
    // ----------- ProductionOwnUse ---------------------
    public double ProductionOwnUse { get; set; } = 0;
    public double ProductionOwnUseSaved { get; set; } = 0;
    public double ProductionOwnUseTransferFeeSaved { get; set; } = 0;
    public double ProductionOwnUseEnergyTaxSaved { get; set; } = 0;
    public double SumProductionOwnUseSaved { get { return Math.Round(ProductionOwnUseSaved + ProductionOwnUseTransferFeeSaved + ProductionOwnUseEnergyTaxSaved, 2); } }
    // ----------- BatteryUsed ---------------------
    public double BatteryUsed { get; set; } = 0;
    public double BatteryUsedSaved { get; set; } = 0;
    public double BatteryUseTransferFeeSaved { get; set; } = 0;
    public double BatteryUseEnergyTaxSaved { get; set; } = 0;
    public double SumBatteryUseSaved { get { return Math.Round(BatteryUsedSaved + BatteryUseTransferFeeSaved + BatteryUseEnergyTaxSaved, 2); } }

    // ----------- BatteryCharge ---------------------
    public double BatteryCharge { get; set; } = 0;
    // ----------- combinde sum of many values ---------------------
    public double SumProductionOwnUseAndBatterySaved { get { return Math.Round(SumProductionOwnUseSaved + SumBatteryUseSaved + PeakEnergyReductionSaved, 2); } }
    public double SumAllProductionSoldAndSaved { get { return Math.Round(SumProductionSoldProfit + SumProductionOwnUseAndBatterySaved, 2); } }
    public double SumAllProduction { get { return Math.Round(ProductionSold + ProductionOwnUse + BatteryCharge, 2); } }
    public double SumAllConsumption { get { return Math.Round(Purchased + ProductionOwnUse + BatteryUsed, 2); } }
    //  ----------- Investment and Loan and intrest cost (ony set when calcualte totals from start to end )  ------------------------------------ 
    public double InterestCost { get; set; } = 0;
    public double Investment { get; set; } = 0;
    //Balance
    public double BalanceProduction_Minus_Consumption { get { return Math.Round(SumAllProduction - SumAllConsumption, 2); } }
    public double BalanceProductionProfit_Minus_ConsumptionCost { get { return Math.Round(SumAllProductionSoldAndSaved - SumPurchasedCost, 2); } }

    //fun Facts
    public double FactsProductionIndex { get; set; } = 0;
    public double FactsPurchasedCostAveragePerKwhPurchased { get; set; } = 0;
    public double FactsProductionSoldAveragePerKwhProfit { get; set; } = 0;
    public double FactsProductionOwnUseAveragePerKwhSaved { get; set; } = 0;
    public double FactsBatteryUsedAveragePerKwhSaved { get; set; } = 0;

    // ----------- Peak Peduction ---------------------
    public double PeakPurchasedAndOwnUsage { get; set; } = 0;
    public double PeakPurchased { get; set; } = 0;
    public double PeakEnergyReduction { get; set; } = 0;
    //TODO:Ändra 35 kr till database egennskap
    public double PeakEnergyReductionSaved { get; set; } = 0;
    //public int NoOfPeaksUsedForPeakDetermination { get; set; } = 0;
    //public List<double> PeakPurchasedAndOwnUsage { get; set; } = new List<double>();
    //public List<double> PeakPurchased { get; set; } = new List<double>();
    //public double AveragePeakPurchasedAndOwnUsage { get; set; } = 0;
    // public double AveragePeakPurchased { get; set; } = 0;


    public EnergyCalculationParameter EnergyCalculationParameter { get; set; }


    //This is only for fun you dont sell this kwh you load the battery  
    // public double TotalBatteryChargeProfitFake { get; set; } = 0;
    //public double TotalProductionNegativeSold { get; set; } = 0;
    // public double TotalProductionSoldNegativeProfit { get; set; } = 0;
    // public double TotalCompensationForProductionToGridChargeBatteryFake { get; set; } = 0;
    //public double TotalSavedEnergyTaxReductionBatteryChargeFakeToGrid { get; set; } = 0;
    //public double SumProductionBatteryChargeFakeSold { get; set; } = 0;
}

