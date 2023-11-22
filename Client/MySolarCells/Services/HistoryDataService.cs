namespace MySolarCells.Services;

public interface IHistoryDataService
{

    Task<HistoryStats> CalculateTotals(DateTime? start, DateTime? end, HistorySimulate historSimulate);
    Task<HistoryStats> CalculateTotals(DateTime? start, DateTime? end);
    Task<Result<Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>>>> GenerateTotalPermonthReport(DateTime? start, DateTime? end);

}

public class HistoryDataService : IHistoryDataService
{
    private readonly MscDbContext mscDbContext;
    public HistoryDataService(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
    }
    //This function should can prpduce wrong if the cahnge of calulation parameters in the middel of the date span 

    /// Return to collectins tuple 1 is for the overview summirized one Year per row
    /// The second tuple is all months summirzed per year 
    public async Task<Result<Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>>>> GenerateTotalPermonthReport(DateTime? start, DateTime? end)
    {
        List<ReportHistoryStats> result = new List<ReportHistoryStats>();
        //var start = DateHelper.GetRelatedDates(MySolarCellsGlobals.SelectedHome.FromDate);
        //var dates = DateHelper.GetRelatedDates(DateTime.Today);
        var startDate = DateHelper.GetRelatedDates(start.Value);
        var endDates = DateHelper.GetRelatedDates(end.Value);
        var current = startDate.ThisMonthStart;

        var resultInvest = this.mscDbContext.InvestmentAndLon.AsNoTracking().Include(i => i.Interest).Where(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId).ToList();
        var firstProductionDay = this.mscDbContext.Energy.AsNoTracking().Where(x => x.ProductionSold > 0 || x.ProductionOwnUse > 0 && x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId).First();

        //Get alla data from start to now per month
        while (current < endDates.ThisMonthEnd)
        {
            var stats = await CalculateTotals(current, current.AddMonths(1), new HistorySimulate());

            //Calulate Intrest currnt month
            var resultInvestcalc = CalculateLonAndInterest(resultInvest, current);
            stats.Investment = resultInvestcalc.Item1;
            stats.InterestCost = Math.Round(resultInvestcalc.Item2, 2);
            //Correct balance with interest cost
            //stats.BalanceProductionProfit_Minus_ConsumptionCost = stats.BalanceProductionProfit_Minus_ConsumptionCost - stats.InterestCost;
            result.Add(new ReportHistoryStats { FromDate = current, HistoryStats = stats, ReportPageTyp = (int)ReportPageTyp.YearDetails, FirstProductionDay = firstProductionDay.Timestamp });
            current = current.AddMonths(1);
        }
        // --- Group all per year and get total ----
        int year = result.First().FromDate.Year;
        List<List<ReportHistoryStats>> historyStatsGrpYearAll = new List<List<ReportHistoryStats>>();
        List<ReportHistoryStats> historyStatsMounthGrpYear = new List<ReportHistoryStats>();
        List<ReportHistoryStats> historStatsOvervView = new List<ReportHistoryStats>();
        // double acumlimatedPodSavedAndMinusInterest = 0;
        // double? previusYearRoi = null;
        for (int i = 0; i < result.Count; i++)
        {
            if (year == result[i].FromDate.Year)
                historyStatsMounthGrpYear.Add(result[i]);

            if (year != result[i].FromDate.Year || i == result.Count - 1)
            {
                TimeSpan timeSpanYear = historyStatsMounthGrpYear.Last().FromDate - historyStatsMounthGrpYear.First().FromDate;
                ReportHistoryStats newYearStats = new ReportHistoryStats
                {
                    ReportPageTyp = (int)ReportPageTyp.YearsOverview,
                    FromDate = historyStatsMounthGrpYear.First().FromDate,
                    HistoryStats = SummerizeToOneRoiStats(historyStatsMounthGrpYear.Select(x => x.HistoryStats).ToList(), timeSpanYear),
                    FirstProductionDay = firstProductionDay.Timestamp
                };

                historStatsOvervView.Add(newYearStats);

                year = result[i].FromDate.Year;
                historyStatsGrpYearAll.Add(historyStatsMounthGrpYear);
                historyStatsMounthGrpYear = new List<ReportHistoryStats>();
                historyStatsMounthGrpYear.Add(result[i]);
            }
        }
        //Check differnt rouls per year calc
        foreach (var item in historStatsOvervView)
        {
            //In sweden you only get Taxraduction for the amount of kwh that you bay
            if (item.HistoryStats.ProductionSold > item.HistoryStats.Purchased)
            {
                var calcParams = this.mscDbContext.EnergyCalculationParameter.AsNoTracking().OrderBy(o => o.FromDate).Last(x => x.FromDate <= item.FromDate);

                var oldvalue = item.HistoryStats.ProductionSoldTaxReductionProfit;
                item.HistoryStats.ProductionSoldTaxReductionProfit = Math.Round(item.HistoryStats.Purchased * calcParams.TaxReduction, 2);
                //item.RoiStats.SumProductionSoldProfit = Math.Round((item.RoiStats.SumProductionSoldProfit - oldvalue) + item.RoiStats.ProductionSoldTaxReductionProfit,2);
                item.HistoryStats.ProductionSoldTaxReductionProfitComment = string.Format(AppResources.Missed_Value_Tax_Reduction_Production_Higher_Than_Consumption, Math.Round(oldvalue - item.HistoryStats.ProductionSoldTaxReductionProfit, 2).ToString());
            }
        }

        return new Result<Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>>>(new Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>>(historStatsOvervView, historyStatsGrpYearAll));
    }

    public async Task<HistoryStats> CalculateTotals(DateTime? start, DateTime? end)
    {

        return await CalculateTotalsInternal(start, end, new HistorySimulate { });
    }
    public async Task<HistoryStats> CalculateTotals(DateTime? start, DateTime? end, HistorySimulate historySimulate)
    {
        List<HistoryStats> sumRoi = new List<HistoryStats>();
        var difference = end.Value - start.Value;
        var current = start;
        while (current < end)
        {
            var stats = await CalculateTotalsInternal(current, current.Value.AddDays(1), historySimulate);
            sumRoi.Add(stats);
            current = current.Value.AddDays(1);
        }

        return SummerizeToOneRoiStats(sumRoi, difference);

    }
    #region Private
    private HistoryStats SummerizeToOneRoiStats(List<HistoryStats> sumHistory, TimeSpan difference)
    {
        HistoryStats returnHistory = new HistoryStats
        {
            EnergyCalculationParameter = sumHistory.First().EnergyCalculationParameter,
            Currency = sumHistory.First().Currency,
            Unit = sumHistory.First().Unit,

            Purchased = Math.Round(sumHistory.Sum(x => x.Purchased), 2),
            PurchasedCost = Math.Round(sumHistory.Sum(x => x.PurchasedCost), 2),

            ProductionSold = Math.Round(sumHistory.Sum(x => x.ProductionSold), 2),
            ProductionSoldProfit = Math.Round(sumHistory.Sum(x => x.ProductionSoldProfit), 2),

            ProductionOwnUse = Math.Round(sumHistory.Sum(x => x.ProductionOwnUse), 2),
            ProductionOwnUseSaved = Math.Round(sumHistory.Sum(x => x.ProductionOwnUseSaved), 2),

            BatteryUsed = Math.Round(sumHistory.Sum(x => x.BatteryUsed), 2),
            BatteryUsedSaved = Math.Round(sumHistory.Sum(x => x.BatteryUsedSaved), 2),

            BatteryCharge = Math.Round(sumHistory.Sum(x => x.BatteryCharge), 2),


            //calc values
            ProductionSoldGridCompensationProfit = Math.Round(sumHistory.Sum(x => x.ProductionSoldGridCompensationProfit), 2),

            ProductionOwnUseTransferFeeSaved = Math.Round(sumHistory.Sum(x => x.ProductionOwnUseTransferFeeSaved), 2),
            BatteryUseTransferFeeSaved = Math.Round(sumHistory.Sum(x => x.BatteryUseTransferFeeSaved), 2),
            PurchasedTransferFeeCost = Math.Round(sumHistory.Sum(x => x.PurchasedTransferFeeCost), 2),
            ProductionOwnUseEnergyTaxSaved = Math.Round(sumHistory.Sum(x => x.ProductionOwnUseEnergyTaxSaved), 2),
            BatteryUseEnergyTaxSaved = Math.Round(sumHistory.Sum(x => x.BatteryUseEnergyTaxSaved), 2),


            PurchasedTaxCost = Math.Round(sumHistory.Sum(x => x.PurchasedTaxCost), 2),

            //intrest
            InterestCost = Math.Round(sumHistory.Sum(x => x.InterestCost), 2),
            Investment = sumHistory.Last().Investment,
            ProductionSoldTaxReductionProfit = Math.Round(sumHistory.Sum(x => x.ProductionSoldTaxReductionProfit), 2)


        };

        //Devided per day
        returnHistory.FactsProductionIndex = Math.Round(returnHistory.SumAllProduction / difference.TotalDays, 2);
        returnHistory.FactsPurchasedCostAveragePerKwhPurchased = returnHistory.Purchased == 0 ? 0 : Math.Round(returnHistory.SumPurchasedCost / returnHistory.Purchased, 2);
        returnHistory.FactsProductionSoldAveragePerKwhProfit = returnHistory.ProductionSold == 0 ? 0 : Math.Round(returnHistory.SumProductionSoldProfit / returnHistory.ProductionSold, 2);
        returnHistory.FactsProductionOwnUseAveragePerKwhSaved = returnHistory.ProductionOwnUse == 0 ? 0 : Math.Round(returnHistory.SumProductionOwnUseSaved / returnHistory.ProductionOwnUse, 2);
        returnHistory.FactsBatteryUsedAveragePerKwhSaved = returnHistory.BatteryUsed == 0 ? 0 : Math.Round(returnHistory.SumBatteryUseSaved / returnHistory.BatteryUsed, 2);
        //Peak
        if (difference.TotalDays < 32)
        {
            int amountPeaks = sumHistory.First().PowerTariffParameters != null ? sumHistory.First().PowerTariffParameters.AmountOfPeaksToUse : 1;
            double peakPricePerKwh = sumHistory.First().PowerTariffParameters != null ? sumHistory.First().PowerTariffParameters.PricePerKwh : 0;


            var topPeakPurchased = sumHistory.OrderByDescending(x => x.PeakPurchased).Take(amountPeaks);
            var topPeakPurchasedOwnuse = sumHistory.OrderByDescending(x => x.PeakPurchasedAndOwnUsage).Take(amountPeaks);
            returnHistory.PeakPurchased = Math.Round(topPeakPurchased.Average(x => x.PeakPurchased), 2);
            returnHistory.PeakPurchasedAndOwnUsage = Math.Round(topPeakPurchasedOwnuse.Average(x => x.PeakPurchasedAndOwnUsage), 2);
            returnHistory.PeakEnergyReduction = returnHistory.PeakPurchased < returnHistory.PeakPurchasedAndOwnUsage ? Math.Round(returnHistory.PeakPurchasedAndOwnUsage - returnHistory.PeakPurchased, 2) : 0;
            returnHistory.PeakEnergyReductionSaved = returnHistory.PeakPurchased < returnHistory.PeakPurchasedAndOwnUsage ? Math.Round(returnHistory.PeakEnergyReduction * peakPricePerKwh, 2) : 0;
            returnHistory.PeakPurchasedCost = peakPricePerKwh > 0 ? Math.Round(returnHistory.PeakPurchased * peakPricePerKwh, 2) : 0;
        }
        else
        {
            //Peak
            returnHistory.PeakPurchased = Math.Round(sumHistory.Max(x => x.PeakPurchased), 2);
            returnHistory.PeakPurchasedAndOwnUsage = Math.Round(sumHistory.Max(x => x.PeakPurchasedAndOwnUsage), 2);
            returnHistory.PeakEnergyReduction = Math.Round(sumHistory.Sum(x => x.PeakEnergyReduction), 2);
            returnHistory.PeakEnergyReductionSaved = Math.Round(sumHistory.Sum(x => x.PeakEnergyReductionSaved), 2);
            returnHistory.PeakPurchasedCost = Math.Round(sumHistory.Sum(x => x.PeakPurchasedCost), 2);
        }
        return returnHistory;
    }
    private async Task<HistoryStats> CalculateTotalsInternal(DateTime? start, DateTime? end, HistorySimulate historySimulate)
    {
        List<Energy> energy;
        HistoryStats historyStats = new HistoryStats();
        var calcParams = this.mscDbContext.EnergyCalculationParameter.AsNoTracking().OrderBy(o => o.FromDate).Last(x => x.FromDate <= start);
        var powerParams = this.mscDbContext.PowerTariffParameters.AsNoTracking().OrderBy(o => o.FromDate).LastOrDefault(x => x.FromDate <= start);
        energy = await this.mscDbContext.Energy.AsNoTracking().Where(x => x.Timestamp > start.Value && x.Timestamp <= end.Value).ToListAsync();

        if (historySimulate.DoSimulate && historySimulate.AddBattery)
        {
            //Simulate battery 
            foreach (var item in energy)
            {
                //Check if battery can be charged
                if (item.ProductionSold > 0 && (historySimulate.CurrentBatteryPower + item.ProductionSold) < historySimulate.MaxBatteryPower)
                {
                    //Battery Charged
                    item.BatteryCharge = item.ProductionSold;
                    historySimulate.CurrentBatteryPower = historySimulate.CurrentBatteryPower + item.ProductionSold;
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
                if (item.Purchased > 0 && historySimulate.CurrentBatteryPower > item.Purchased)
                {
                    historySimulate.CurrentBatteryPower = historySimulate.CurrentBatteryPower - item.Purchased;
                    item.BatteryUsed = item.Purchased;
                    if (calcParams.UseSpotPrice)
                        item.BatteryUsedProfit = Math.Round(item.BatteryUsed * item.UnitPriceBuy, 2);
                    else
                        item.BatteryUsedProfit = Math.Round(item.BatteryUsed * calcParams.FixedPriceKwh, 2);
                    //Minskad anvädning profit med batteri
                    item.Purchased = 0;
                    item.PurchasedCost = 0;
                }
            }
        }
        else if (historySimulate.DoSimulate && historySimulate.RemoveBattery)
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
        historyStats.Purchased = Math.Round(energy.Sum(x => x.Purchased), 2);
        historyStats.PurchasedCost = calcParams.UseSpotPrice ? Math.Round(energy.Sum(x => x.PurchasedCost), 2) : Math.Round(historyStats.Purchased * calcParams.FixedPriceKwh, 2);
        historyStats.PurchasedTransferFeeCost = Math.Round(historyStats.Purchased * calcParams.TransferFee, 2);
        historyStats.PurchasedTaxCost = Math.Round(historyStats.Purchased * calcParams.EnergyTax, 2);
        //--------- ProductionSold -------------------------------
        historyStats.ProductionSold = Math.Round(energy.Sum(x => x.ProductionSold), 2);
        historyStats.ProductionSoldProfit = calcParams.UseSpotPrice ? Math.Round(energy.Sum(x => x.ProductionSoldProfit), 2) : Math.Round(historyStats.ProductionSold * calcParams.FixedPriceKwh, 2);
        historyStats.ProductionSoldGridCompensationProfit = Math.Round(historyStats.ProductionSold * calcParams.ProdCompensationElectricityLowload, 2);
        historyStats.ProductionSoldTaxReductionProfit = Math.Round(historyStats.ProductionSold * calcParams.TaxReduction, 2);

        //--------- Production Own use ---------------------------
        historyStats.ProductionOwnUse = Math.Round(energy.Sum(x => x.ProductionOwnUse), 2);
        historyStats.ProductionOwnUseSaved = calcParams.UseSpotPrice ? Math.Round(energy.Sum(x => x.ProductionOwnUseProfit), 2) : Math.Round(historyStats.ProductionOwnUse * calcParams.FixedPriceKwh, 2);
        historyStats.ProductionOwnUseTransferFeeSaved = Math.Round(historyStats.ProductionOwnUse * calcParams.TransferFee, 2);
        historyStats.ProductionOwnUseEnergyTaxSaved = Math.Round(historyStats.ProductionOwnUse * calcParams.EnergyTax, 2);

        //---------- Battery Used -----------------------------------------
        historyStats.BatteryUsed = Math.Round(energy.Sum(x => x.BatteryUsed), 2);
        historyStats.BatteryUsedSaved = calcParams.UseSpotPrice ? Math.Round(energy.Sum(x => x.BatteryUsedProfit), 2) : Math.Round(historyStats.BatteryUsed * calcParams.FixedPriceKwh, 2);
        historyStats.BatteryUseTransferFeeSaved = Math.Round(historyStats.BatteryUsed * calcParams.TransferFee, 2);
        historyStats.BatteryUseEnergyTaxSaved = Math.Round(historyStats.BatteryUsed * calcParams.EnergyTax, 2);

        //------------ Battery Charge ---------------------------------------
        historyStats.BatteryCharge = Math.Round(energy.Sum(x => x.BatteryCharge), 2);
        historyStats.EnergyCalculationParameter = calcParams;
        historyStats.PowerTariffParameters = powerParams;
        //------------ Peak Reduction ---------------------------------------
        if (energy.Count > 0)
        {
            if (powerParams == null)
            {
                historyStats.PeakPurchased = energy.Max(x => x.Purchased);
                historyStats.PeakPurchasedAndOwnUsage = energy.Max(x => x.Purchased + x.ProductionOwnUse + x.BatteryUsed);
            }
            else
            {
                //Plocka ut månader
                var valid = energy.Where(x => x.Timestamp.Month >= powerParams.PeriodMonthStart && x.Timestamp.Month <= powerParams.PeriodMonthEnd);
                //plocka ut vardagar 
                if (powerParams.Weekday && !powerParams.Weekend)
                    valid = energy.Where(x => x.Timestamp.DayOfWeek == DayOfWeek.Monday || x.Timestamp.DayOfWeek == DayOfWeek.Tuesday || x.Timestamp.DayOfWeek == DayOfWeek.Wednesday || x.Timestamp.DayOfWeek == DayOfWeek.Thursday ||  x.Timestamp.DayOfWeek == DayOfWeek.Friday);
                else if (!powerParams.Weekday && powerParams.Weekend) //Helger
                    valid = energy.Where(x => x.Timestamp.DayOfWeek == DayOfWeek.Saturday || x.Timestamp.DayOfWeek == DayOfWeek.Sunday);

                //plockar ut timmar
                valid = energy.Where(x => x.Timestamp.Hour >= powerParams.DayTimeStart && x.Timestamp.Hour <= powerParams.DayTimeEnd);
                historyStats.PeakPurchased = valid.Max(x => x.Purchased);
                historyStats.PeakPurchasedAndOwnUsage = valid.Max(x => x.Purchased + x.ProductionOwnUse + x.BatteryUsed);
            }
        }

        return historyStats;

    }
    
    //returns total invest, Loanleft and Interestcost
    private Tuple<int, float> CalculateLonAndInterest(List<InvestmentAndLoan> investmentAndLoans, DateTime start)
    {
        int investmentTot = 0;
        //float loanTotLeft = 0;
        float interestTot = 0;

        foreach (var item in investmentAndLoans)
        {
            if (start < item.FromDate)
                continue;
            //Summerize total investment 
            investmentTot = investmentTot + item.Investment + item.Loan;

            if (item.Loan > 0 && item.Interest.Any(x => x.FromDate <= start))
            {
                if (item.LoanLeft is null)
                    item.LoanLeft = item.Loan;

                var interestCur = item.Interest.Where(x => x.FromDate <= start).OrderBy(o => o.FromDate).First();

                var curInterest = ((item.LoanLeft * (interestCur.Interest / 100)) / 12);

                interestTot = interestTot + curInterest.Value;

                item.LoanLeft = item.LoanLeft - interestCur.Amortization;
            }


        }

        return new Tuple<int, float>(investmentTot, interestTot);
    }
    #endregion
}
public class HistorySimulate : ObservableObject
{
    //DOSIMULATE
    public Color ShowSimulateBackgrundColor { get { return doSimulate ? AppColors.SignalBlueColor : AppColors.TransparentColor; } }
    public Color ShowSimulateBorderColor { get { return doSimulate ? AppColors.Gray500Color : AppColors.TransparentColor; } }
    public Color ShowSimulateTextColor { get { return doSimulate ? AppColors.WhiteColor : AppColors.Gray900Color; } }



    private bool doSimulate;
    public bool DoSimulate
    {
        get => doSimulate;
        set
        {
            SetProperty(ref doSimulate, value);
            OnPropertyChanged(nameof(ShowSimulateBackgrundColor));
            OnPropertyChanged(nameof(ShowSimulateBorderColor));
            OnPropertyChanged(nameof(ShowSimulateTextColor));
        }
    }
    //ADD Battery
    public Color AddBatteryBackgrundColor { get { return addBattery ? AppColors.SignalBlueColor : AppColors.TransparentColor; } }
    public Color AddBatteryBorderColor { get { return addBattery ? AppColors.Gray500Color : AppColors.TransparentColor; } }
    public Color AddBatteryTextColor { get { return addBattery ? AppColors.WhiteColor : AppColors.Gray900Color; } }

    private bool addBattery = true;
    public bool AddBattery
    {
        get => addBattery;
        set
        {
            SetProperty(ref addBattery, value);
            removeBattery = !addBattery;
            OnPropertyChanged(nameof(AddBatteryBackgrundColor));
            OnPropertyChanged(nameof(AddBatteryBorderColor));
            OnPropertyChanged(nameof(AddBatteryTextColor));
            OnPropertyChanged(nameof(RemoveBatteryBackgrundColor));
            OnPropertyChanged(nameof(RemoveBatteryBorderColor));
            OnPropertyChanged(nameof(RemoveBatteryTextColor));
        }

    }
    //Remove battery
    public Color RemoveBatteryBackgrundColor { get { return removeBattery ? AppColors.SignalBlueColor : AppColors.TransparentColor; } }
    public Color RemoveBatteryBorderColor { get { return removeBattery ? AppColors.Gray500Color : AppColors.TransparentColor; } }
    public Color RemoveBatteryTextColor { get { return removeBattery ? AppColors.WhiteColor : AppColors.Gray900Color; } }
    private bool removeBattery;
    public bool RemoveBattery
    {
        get => removeBattery;
        set
        {
            SetProperty(ref removeBattery, value);
            addBattery = !removeBattery;
            OnPropertyChanged(nameof(RemoveBatteryBackgrundColor));
            OnPropertyChanged(nameof(RemoveBatteryBorderColor));
            OnPropertyChanged(nameof(RemoveBatteryTextColor));
            OnPropertyChanged(nameof(AddBatteryBackgrundColor));
            OnPropertyChanged(nameof(AddBatteryBorderColor));
            OnPropertyChanged(nameof(AddBatteryTextColor));
        }
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

