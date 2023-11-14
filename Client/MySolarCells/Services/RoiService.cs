using System;
using CoreVideo;

namespace MySolarCells.Services;
public interface IRoiService
{
    Result<bool> CalcSavingsEstimate(Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>> historyStats);
}

public class RoiService :IRoiService
{
    private readonly MscDbContext mscDbContext;
    public RoiService(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="historyStats"></param>
    /// Input to collectins tuple 1 is for the overview summirized one Year per row
    /// The second tuple is all months summirzed per year 
    /// <returns></returns>
    public Result<bool> CalcSavingsEstimate(Tuple<List<ReportHistoryStats>, List<List<ReportHistoryStats>>> historyStats)
    {
        foreach (var item in historyStats.Item1)
        {

        }
         

        return new Result<bool>(true); 
    }
}
public class EstimateRoi
{
    public int Year { get; set; }
    public double AvargePriceSold { get; set; }
    public double AvargePrisOwnUse { get; set; }
    public double ProductionSold { get; set; }
    public double ProductionOwnUse { get; set; }
    public double AnnualSavingsSold { get; set; }
    public double AnnualSavingsOwnUse { get; set; }
    public double ReturnPercentage { get; set; }
    public double RemainingOnInvestment { get; set; }

}
