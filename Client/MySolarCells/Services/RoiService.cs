using MySolarCells.Services.Sqlite.Models;

namespace MySolarCells.Services;

public interface IRoiService
{

    Task<RoiStats> CalculateTotals(DateTime? start, DateTime? end, RoiSimulate roiSimulate);
    Task<RoiStats> CalculateTotals(DateTime? start, DateTime? end);
    Task<Result<List<ReportRoiStats>>> GenerateTotalPermonthReport();
}

public class RoiService : IRoiService
{
    private readonly MscDbContext mscDbContext;
    public RoiService(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
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

            Purchased = Math.Round(sumRoi.Sum(x => x.Purchased), 2),
            PurchasedCost = Math.Round(sumRoi.Sum(x => x.PurchasedCost), 2),

            ProductionSold = Math.Round(sumRoi.Sum(x => x.ProductionSold), 2),
            ProductionSoldProfit = Math.Round(sumRoi.Sum(x => x.ProductionSoldProfit), 2),

            ProductionOwnUse = Math.Round(sumRoi.Sum(x => x.ProductionOwnUse), 2),
            ProductionOwnUseSaved = Math.Round(sumRoi.Sum(x => x.ProductionOwnUseSaved), 2),

            BatteryUsed = Math.Round(sumRoi.Sum(x => x.BatteryUsed), 2),
            BatteryUsedSaved = Math.Round(sumRoi.Sum(x => x.BatteryUsedSaved), 2),

            BatteryCharge = Math.Round(sumRoi.Sum(x => x.BatteryCharge), 2),


            //calc values
            ProductionSoldGridCompensationProfit = Math.Round(sumRoi.Sum(x => x.ProductionSoldGridCompensationProfit), 2),



            ProductionOwnUseTransferFeeSaved = Math.Round(sumRoi.Sum(x => x.ProductionOwnUseTransferFeeSaved), 2),
            BatteryUseTransferFeeSaved = Math.Round(sumRoi.Sum(x => x.BatteryUseTransferFeeSaved), 2),
            PurchasedTransferFeeCost = Math.Round(sumRoi.Sum(x => x.PurchasedTransferFeeCost), 2),
            ProductionOwnUseEnergyTaxSaved = Math.Round(sumRoi.Sum(x => x.ProductionOwnUseEnergyTaxSaved), 2),
            BatteryUseEnergyTaxSaved = Math.Round(sumRoi.Sum(x => x.BatteryUseEnergyTaxSaved), 2),


            PurchasedTaxCost = Math.Round(sumRoi.Sum(x => x.PurchasedTaxCost), 2),

            //intrest
            InterestCost = Math.Round(sumRoi.Sum(x => x.InterestCost), 2),
            Investment = Math.Round(sumRoi.Sum(x => x.Investment), 2),

            ProductionSoldTaxReductionProfit = Math.Round(sumRoi.Sum(x => x.ProductionSoldTaxReductionProfit), 2),


            //Summ Saved
            SumPurchasedCost = Math.Round(sumRoi.Sum(x => x.SumPurchasedCost), 2),
            SumProductionSoldProfit = Math.Round(sumRoi.Sum(x => x.SumProductionSoldProfit), 2),
            SumProductionOwnUseSaved = Math.Round(sumRoi.Sum(x => x.SumProductionOwnUseSaved), 2),
            SumBatteryUseSaved = Math.Round(sumRoi.Sum(x => x.SumBatteryUseSaved), 2),
            SumProductionOwnUseAndBatterySaved = Math.Round(sumRoi.Sum(x => x.SumProductionOwnUseAndBatterySaved), 2),
            SumAllProductionSoldAndSaved = Math.Round(sumRoi.Sum(x => x.SumAllProductionSoldAndSaved), 2),
            BalanceProfitAndSaved_Minus_InterestCost = Math.Round(sumRoi.Sum(x => x.BalanceProfitAndSaved_Minus_InterestCost), 2),
            SumAllProduction = Math.Round(sumRoi.Sum(x => x.SumAllProduction), 2),
            SumAllConsumption = Math.Round(sumRoi.Sum(x => x.SumAllConsumption), 2),

            BalanceProduction_Minus_Consumption = Math.Round(sumRoi.Sum(x => x.BalanceProduction_Minus_Consumption), 2),
            BalanceProductionProfit_Minus_ConsumptionCost = Math.Round(sumRoi.Sum(x => x.BalanceProductionProfit_Minus_ConsumptionCost), 2),


        };

        //var soloarFirstDate = await mscDbContext.Energy.FirstOrDefaultAsync(x => x.ProductionSold > 0 || x.ProductionOwnUse > 0 && x.Timestamp > start);
        //if (soloarFirstDate != null)
        //{
        //    var enddate = end > DateTime.Now ? DateTime.Now : end;
        //    var differenceProductionIndex = enddate - soloarFirstDate.Timestamp;
        //    returnRoi.FactsProductionIndex = Math.Round(returnRoi.FactsProductionIndex / Convert.ToInt32(differenceProductionIndex.Value.TotalDays), 2);
        //}
        //Devided per day
        returnRoi.FactsProductionIndex = Math.Round(returnRoi.SumAllProduction / difference.TotalDays, 2);
        returnRoi.FactsPurchasedCostAveragePerKwhPurchased = Math.Round(returnRoi.SumPurchasedCost / returnRoi.Purchased, 2);
        returnRoi.FactsProductionSoldAveragePerKwhProfit = Math.Round(returnRoi.SumProductionSoldProfit / returnRoi.ProductionSold, 2);
        returnRoi.FactsProductionOwnUseAveragePerKwhSaved = Math.Round(returnRoi.SumProductionOwnUseSaved / returnRoi.ProductionOwnUse, 2);
        returnRoi.FactsBatteryUsedAveragePerKwhSaved = Math.Round(returnRoi.SumBatteryUseSaved / returnRoi.BatteryUsed, 2);



        //FactsPurchasedCostAveragePerKwhPurchased = Math.Round(sumRoi.Sum(x => x.FactsPurchasedCostAveragePerKwhPurchased), 2),
        //    FactsProductionSoldAveragePerKwhProfit = Math.Round(sumRoi.Sum(x => x.FactsProductionSoldAveragePerKwhProfit), 2),
        //    FactsProductionOwnUseAveragePerKwhSaved = Math.Round(sumRoi.Sum(x => x.FactsProductionOwnUseAveragePerKwhSaved), 2),
        //    FactsBatteryUsedAveragePerKwhSaved = Math.Round(sumRoi.Sum(x => x.FactsBatteryUsedAveragePerKwhSaved), 2)

        return returnRoi;

    }
    #region Private
    private async Task<RoiStats> CalculateTotalsInternal(DateTime? start, DateTime? end, RoiSimulate roiSimulate)
    {

        List<Sqlite.Models.Energy> energy;
        RoiStats roiStats = new RoiStats();
        //roiStats.RoiStatsLines = new List<RoiStatsLine>();

        var calcParams = this.mscDbContext.EnergyCalculationParameter.AsNoTracking().OrderBy(o => o.FromDate).Last(x => x.FromDate <= start);

        energy = await this.mscDbContext.Energy.AsNoTracking().Where(x => x.Timestamp > start.Value && x.Timestamp <= end.Value).ToListAsync();

        if (roiSimulate.DoSimulate && roiSimulate.AddBattery)
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
                    else
                        item.BatteryUsedProfit = Math.Round(item.BatteryUsed * calcParams.FixedPriceKwh, 2);


                    //Miinska een anvädning profit med batteri
                    item.Purchased = 0;
                    item.PurchasedCost = 0;

                }
            }
        }
        else if (roiSimulate.DoSimulate && roiSimulate.RemoveBattery)
        {
            //Simulate battery 
            foreach (var item in energy)
            {
                //Check if battery is charged
                if (item.BatteryCharge > 0)
                {
                    //Battery Charged
                    item.ProductionSold = item.ProductionSold + item.BatteryCharge;
                    if (calcParams.UseSpotPrice)
                        item.ProductionSoldProfit = item.ProductionSold * item.UnitPriceSold;
                    else
                        item.ProductionSoldProfit = item.ProductionSold * calcParams.FixedPriceKwh;

                    item.BatteryCharge = 0;
                }
                //Check if battery has more charge then Purchased
                if (item.BatteryUsed > 0)
                {
                    item.Purchased = item.Purchased + item.BatteryUsed;
                    if (calcParams.UseSpotPrice)
                        item.PurchasedCost = Math.Round(item.Purchased * item.UnitPriceBuy, 2);
                    else
                        item.PurchasedCost = Math.Round(item.Purchased * calcParams.FixedPriceKwh, 2);

                    item.BatteryUsed = 0;
                    item.BatteryUsedProfit = 0;

                }


            }


        }


        //------------Purchased --------------------------------
        roiStats.Purchased = Math.Round(energy.Sum(x => x.Purchased), 2);
        roiStats.PurchasedCost = calcParams.UseSpotPrice ? Math.Round(energy.Sum(x => x.PurchasedCost), 2) : Math.Round(roiStats.Purchased * calcParams.FixedPriceKwh, 2);
        roiStats.PurchasedTransferFeeCost = Math.Round(roiStats.Purchased * calcParams.TransferFee, 2);
        roiStats.PurchasedTaxCost = Math.Round(roiStats.Purchased * calcParams.EnergyTax, 2);
        roiStats.SumPurchasedCost = Math.Round(roiStats.PurchasedCost + roiStats.PurchasedTransferFeeCost + roiStats.PurchasedTaxCost, 2);
        //roiStats.FactsPurchasedCostAveragePerKwhPurchased = Math.Round(roiStats.SumPurchasedCost / roiStats.Purchased, 2);
        //--------- ProductionSold -------------------------------
        roiStats.ProductionSold = Math.Round(energy.Sum(x => x.ProductionSold), 2);
        roiStats.ProductionSoldProfit = calcParams.UseSpotPrice ? Math.Round(energy.Sum(x => x.ProductionSoldProfit), 2) : Math.Round(roiStats.ProductionSold * calcParams.FixedPriceKwh, 2);
        roiStats.ProductionSoldGridCompensationProfit = Math.Round(roiStats.ProductionSold * calcParams.ProdCompensationElectricityLowload, 2);
        roiStats.ProductionSoldTaxReductionProfit = Math.Round(roiStats.ProductionSold * calcParams.TaxReduction, 2);
        roiStats.SumProductionSoldProfit = Math.Round(roiStats.ProductionSoldProfit + roiStats.ProductionSoldGridCompensationProfit + roiStats.ProductionSoldTaxReductionProfit, 2);
        //roiStats.FactsProductionSoldAveragePerKwhProfit = Math.Round(roiStats.SumProductionSoldProfit / roiStats.ProductionSold, 2);
       
        //--------- Production Own use ---------------------------
        roiStats.ProductionOwnUse = Math.Round(energy.Sum(x => x.ProductionOwnUse), 2);
        roiStats.ProductionOwnUseSaved = calcParams.UseSpotPrice ? Math.Round(energy.Sum(x => x.ProductionOwnUseProfit), 2) : Math.Round(roiStats.ProductionOwnUse * calcParams.FixedPriceKwh, 2);
        roiStats.ProductionOwnUseTransferFeeSaved = Math.Round(roiStats.ProductionOwnUse * calcParams.TransferFee, 2);
        roiStats.ProductionOwnUseEnergyTaxSaved = Math.Round(roiStats.ProductionOwnUse * calcParams.EnergyTax, 2);
        roiStats.SumProductionOwnUseSaved = Math.Round(roiStats.ProductionOwnUseSaved + roiStats.ProductionOwnUseTransferFeeSaved + roiStats.ProductionOwnUseEnergyTaxSaved, 2);
        //roiStats.FactsProductionOwnUseAveragePerKwhSaved = Math.Round(roiStats.SumProductionOwnUseSaved / roiStats.ProductionOwnUse, 2);
     
        //---------- Battery Used -----------------------------------------
        roiStats.BatteryUsed = Math.Round(energy.Sum(x => x.BatteryUsed), 2);
        roiStats.BatteryUsedSaved = calcParams.UseSpotPrice ? Math.Round(energy.Sum(x => x.BatteryUsedProfit), 2) : Math.Round(roiStats.BatteryUsed * calcParams.FixedPriceKwh, 2);
        roiStats.BatteryUseTransferFeeSaved = Math.Round(roiStats.BatteryUsed * calcParams.TransferFee, 2);
        roiStats.BatteryUseEnergyTaxSaved = Math.Round(roiStats.BatteryUsed * calcParams.EnergyTax, 2);
        roiStats.SumBatteryUseSaved = Math.Round(roiStats.BatteryUsedSaved + roiStats.BatteryUseTransferFeeSaved + roiStats.BatteryUseEnergyTaxSaved, 2);
        //roiStats.FactsBatteryUsedAveragePerKwhSaved = Math.Round(roiStats.SumBatteryUseSaved/ roiStats.BatteryUsed, 2);

        //------------ Battery Charge ---------------------------------------
        roiStats.BatteryCharge = Math.Round(energy.Sum(x => x.BatteryCharge), 2);


        // ---------------- sum of combinded valus ----------------------------------------
        roiStats.SumProductionOwnUseAndBatterySaved = Math.Round(roiStats.SumProductionOwnUseSaved + roiStats.SumBatteryUseSaved, 2);
        roiStats.SumAllProductionSoldAndSaved = Math.Round((roiStats.SumProductionSoldProfit + roiStats.SumProductionOwnUseAndBatterySaved), 2);
        roiStats.SumAllProduction = Math.Round(roiStats.ProductionSold + roiStats.ProductionOwnUse + roiStats.BatteryCharge, 2);
        roiStats.SumAllConsumption = Math.Round(roiStats.Purchased + roiStats.ProductionOwnUse + roiStats.BatteryUsed, 2);

        //--------------------- Interest  -----------------------------------------
        var resultRanta = await GetInterest(start, end);
        roiStats.Investment = resultRanta.Item1;
        roiStats.InterestCost = Math.Round(resultRanta.Item2, 2);

        // ---------------- Balance's ----------------------------------------
        roiStats.BalanceProfitAndSaved_Minus_InterestCost = Math.Round(roiStats.SumAllProductionSoldAndSaved - roiStats.InterestCost, 2);
        roiStats.BalanceProduction_Minus_Consumption = Math.Round(roiStats.SumAllProduction - roiStats.SumAllConsumption, 2);
        roiStats.BalanceProductionProfit_Minus_ConsumptionCost = Math.Round(roiStats.SumAllProductionSoldAndSaved - roiStats.SumPurchasedCost, 2);


        //Production Index amount of production per installed kWh
        //.FactsProductionIndex = roiStats.SumAllProduction / calcParams.TotalInstallKwhPanels;

        roiStats.EnergyCalculationParameter = calcParams;
        return roiStats;

    }
    //returns total invest and total Interest
    private async Task<Tuple<int, float>> GetInterest(DateTime? start, DateTime? end)
    {
        int investmentTot = 0;
        float interest = 0;



        var result = this.mscDbContext.InvestmentAndLon.AsNoTracking().Include(i => i.Interest).Where(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId).ToList();
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
public class RoiSimulate : ObservableObject
{
    public Color ShowSimulateBackgrundColor { get { return DoSimulate ? AppColors.Gray200Color : AppColors.TransparentColor; } }

    private bool doSimulate;
    public bool DoSimulate
    {
        get => doSimulate;
        set
        {
            SetProperty(ref doSimulate, value);
            OnPropertyChanged(nameof(ShowSimulateBackgrundColor));
        }
    }

    private bool addBattery;
    public bool AddBattery
    {
        get => addBattery;
        set { SetProperty(ref addBattery, value); }
    }
    private bool removeBattery;
    public bool RemoveBattery
    {
        get => removeBattery;
        set { SetProperty(ref removeBattery, value); }
    }
    private double maxBatteryPower = 10;
    public double MaxBatteryPower
    {
        get => maxBatteryPower;
        set
        {
            SetProperty(ref maxBatteryPower, value);

        }
    }
    private double chargeLoss = 0.1; //10%
    public double ChargeLoss
    {
        get => chargeLoss;
        set { SetProperty(ref chargeLoss, value); }
    }
    private double currentBatteryPower = 0; //10%
    public double CurrentBatteryPower
    {
        get => currentBatteryPower;
        set { SetProperty(ref currentBatteryPower, value); }
    }

}
public class ReportRoiStats
{
    public DateTime FromDate { get; set; }
    public RoiStats RoiStats { get; set; }

}

public class RoiStats
{
    public string Unit { get; set; } = "kWh";
    public string Currency { get; set; } = "SEK";

    // ----------- Purchased ---------------------
    public double Purchased { get; set; } = 0;
    public double PurchasedCost { get; set; } = 0;
    public double PurchasedTransferFeeCost { get; set; } = 0;
    public double PurchasedTaxCost { get; set; } = 0;
    public double SumPurchasedCost { get; set; } = 0;
    // ----------- ProductionSold ---------------------
    public double ProductionSold { get; set; } = 0;
    public double ProductionSoldProfit { get; set; } = 0;
    public double ProductionSoldGridCompensationProfit { get; set; } = 0;
    public double ProductionSoldTaxReductionProfit { get; set; } = 0;
    public double SumProductionSoldProfit { get; set; } = 0;
    // ----------- ProductionOwnUse ---------------------
    public double ProductionOwnUse { get; set; } = 0;
    public double ProductionOwnUseSaved { get; set; } = 0;
    public double ProductionOwnUseTransferFeeSaved { get; set; } = 0;
    public double ProductionOwnUseEnergyTaxSaved { get; set; } = 0;
    public double SumProductionOwnUseSaved { get; set; } = 0;
    // ----------- BatteryUsed ---------------------
    public double BatteryUsed { get; set; } = 0;
    public double BatteryUsedSaved { get; set; } = 0;
    public double BatteryUseTransferFeeSaved { get; set; } = 0;
    public double BatteryUseEnergyTaxSaved { get; set; } = 0;
    public double SumBatteryUseSaved { get; set; } = 0;

    // ----------- BatteryCharge ---------------------
    public double BatteryCharge { get; set; } = 0;
    // ----------- combinde sum of many values ---------------------
    public double SumProductionOwnUseAndBatterySaved { get; set; } = 0;
    public double SumAllProductionSoldAndSaved { get; set; } = 0;
    public double SumAllProduction { get; set; } = 0;
    public double SumAllConsumption { get; set; } = 0;
    //  ----------- intrest  ------------------------------------ 
    public double InterestCost { get; set; } = 0;
    public double Investment { get; set; } = 0;

    //Balance
    public double BalanceProfitAndSaved_Minus_InterestCost { get; set; } = 0;
    public double BalanceProduction_Minus_Consumption { get; set; } = 0;
    public double BalanceProductionProfit_Minus_ConsumptionCost { get; set; } = 0;

    //fun Facts
    public double FactsProductionIndex { get; set; } = 0;
    public double FactsPurchasedCostAveragePerKwhPurchased { get; set; } = 0;
    public double FactsProductionSoldAveragePerKwhProfit { get; set; } = 0;
    public double FactsProductionOwnUseAveragePerKwhSaved { get; set; } = 0;
    public double FactsBatteryUsedAveragePerKwhSaved { get; set; } = 0;

    public Sqlite.Models.EnergyCalculationParameter EnergyCalculationParameter { get; set; }


    //This is only for fun you dont sell this kwh you load the battery  
    // public double TotalBatteryChargeProfitFake { get; set; } = 0;
    //public double TotalProductionNegativeSold { get; set; } = 0;
    // public double TotalProductionSoldNegativeProfit { get; set; } = 0;
    // public double TotalCompensationForProductionToGridChargeBatteryFake { get; set; } = 0;
    //public double TotalSavedEnergyTaxReductionBatteryChargeFakeToGrid { get; set; } = 0;
    //public double SumProductionBatteryChargeFakeSold { get; set; } = 0;
}
