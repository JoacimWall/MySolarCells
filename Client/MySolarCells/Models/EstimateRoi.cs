namespace MySolarCells.Models;

public class EstimateRoi
{
    public int Year { get; set; }
    public int YearFromStart { get; set; }
    public double AvargePriceSold { get; set; }
    public double AvargePrisOwnUse { get; set; }
    public double ProductionSold { get; set; }
    public double ProductionOwnUse { get; set; }
    public double YearSavingsSold { get; set; }
    public double YearSavingsOwnUse { get; set; }
    public double ReturnPercentage { get; set; }
    public double RemainingOnInvestment { get; set; }
    public bool IsRoiYear { get; set; }
    public Color BackgroundColorValueField
    { get { return IsRoiYear ? AppColors.SignalGreenColor : AppColors.Gray200Color; } }
    public string YearTitle
    { get { return string.Format("{0} ({1})",YearFromStart.ToString(), Year.ToString()); } }
}

