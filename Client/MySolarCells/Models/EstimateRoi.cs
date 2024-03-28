namespace MySolarCells.Models;

public class EstimateRoi
{
    public int Year { get; set; }
    public int YearFromStart { get; set; }
    public double AveragePriceSold { get; set; }
    public double AveragePrisOwnUse { get; set; }
    public double ProductionSold { get; set; }
    public double ProductionOwnUse { get; set; }
    public double YearSavingsSold { get; set; }
    public double YearSavingsOwnUse { get; set; }
    public double ReturnPercentage { get; set; }
    public double RemainingOnInvestment { get; set; }
    public bool IsRoiYear { get; set; }
    public Color BackgroundColorValueField => IsRoiYear ? AppColors.SignalGreenColor : AppColors.Gray200Color;

    public string YearTitle => $"{YearFromStart.ToString()} ({Year.ToString()})";
}

