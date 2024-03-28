using MySolarCellsSQLite.Sqlite.Models;

namespace MySolarCells.Models;

public class ReportHistoryStats
{
    public DateTime FirstProductionDay { get; set; }
    public DateTime FromDate { get; set; }
    public HistoryStats HistoryStats { get; set; } = new();
    public int ReportPageTyp { get; set; } = 3;
}

public class HistoryStats
{
    
    public string Unit { get; set; } = "kWh";
    public string Currency { get; set; } = "SEK";

    // ----------- Purchased ---------------------
    public double Purchased { get; set; }
    public double PurchasedCost { get; set; }
    public double PurchasedTransferFeeCost { get; set; }
    public double PurchasedTaxCost { get; set; } 
    public double SumPurchasedCost => Math.Round(PurchasedCost + PurchasedTransferFeeCost + PurchasedTaxCost + PeakPurchasedCost, 2);

    // ----------- ProductionSold ---------------------
    public double ProductionSold { get; set; } 
    public double ProductionSoldProfit { get; set; }
    public double ProductionSoldGridCompensationProfit { get; set; } 
    public double ProductionSoldTaxReductionProfit { get; set; } 
    public string ProductionSoldTaxReductionProfitComment { get; set; } = "";
    public double SumProductionSoldProfit => Math.Round(ProductionSoldProfit + ProductionSoldGridCompensationProfit + ProductionSoldTaxReductionProfit, 2);

    // ----------- ProductionOwnUse ---------------------
    public double ProductionOwnUse { get; set; }
    public double ProductionOwnUseSaved { get; set; }
    public double ProductionOwnUseTransferFeeSaved { get; set; } 
    public double ProductionOwnUseEnergyTaxSaved { get; set; } 
    public double SumProductionOwnUseSaved => Math.Round(ProductionOwnUseSaved + ProductionOwnUseTransferFeeSaved + ProductionOwnUseEnergyTaxSaved, 2);

    // ----------- BatteryUsed ---------------------
    public double BatteryUsed { get; set; } 
    public double BatteryUsedSaved { get; set; } 
    public double BatteryUseTransferFeeSaved { get; set; }
    public double BatteryUseEnergyTaxSaved { get; set; } 
    public double SumBatteryUseSaved => Math.Round(BatteryUsedSaved + BatteryUseTransferFeeSaved + BatteryUseEnergyTaxSaved, 2);

    // ----------- BatteryCharge ---------------------
    public double BatteryCharge { get; set; }
    // ----------- combine sum of many values ---------------------
    public double SumProductionOwnUseAndBatterySaved => Math.Round(SumProductionOwnUseSaved + SumBatteryUseSaved + PeakEnergyReductionSaved, 2);
    public double SumAllProductionSoldAndSaved => Math.Round(SumProductionSoldProfit + SumProductionOwnUseAndBatterySaved, 2);
    public double SumAllProduction => Math.Round(ProductionSold + ProductionOwnUse + BatteryCharge, 2);

    public double SumAllConsumption => Math.Round(Purchased + ProductionOwnUse + BatteryUsed, 2);

    //  ----------- Investment and Loan and interest cost (ony set when calculate totals from start to end )  ------------------------------------ 
    public double InterestCost { get; set; } 
    public double Investment { get; set; }
    //Balance
    public double BalanceProductionMinusConsumption => Math.Round(SumAllProduction - SumAllConsumption, 2);
    public double BalanceProductionProfitMinusConsumptionCost => Math.Round(SumAllProductionSoldAndSaved - SumPurchasedCost, 2);

    //fun Facts
    public double FactsProductionIndex { get; set; }
    public double FactsPurchasedCostAveragePerKwhPurchased { get; set; } 
    public double FactsProductionSoldAveragePerKwhProfit { get; set; } 
    public double FactsProductionOwnUseAveragePerKwhSaved { get; set; } 
    public double FactsBatteryUsedAveragePerKwhSaved { get; set; } 

    // ----------- Peak Production ---------------------
    public double PeakPurchasedAndOwnUsage { get; set; } 
    public double PeakPurchased { get; set; } 

    public double PeakEnergyReduction { get; set; } 
    public double PeakPurchasedCost { get; set; } 
    public double PeakEnergyReductionSaved { get; set; } 
    
    public EnergyCalculationParameter EnergyCalculationParameter { get; set; } = new();
    public PowerTariffParameters PowerTariffParameters { get; set; }= new();

    //This is only for fun you dont sell this kwh you load the battery  
    // public double TotalBatteryChargeProfitFake { get; set; } = 0;
    //public double TotalProductionNegativeSold { get; set; } = 0;
    // public double TotalProductionSoldNegativeProfit { get; set; } = 0;
    // public double TotalCompensationForProductionToGridChargeBatteryFake { get; set; } = 0;
    //public double TotalSavedEnergyTaxReductionBatteryChargeFakeToGrid { get; set; } = 0;
    //public double SumProductionBatteryChargeFakeSold { get; set; } = 0;
}

