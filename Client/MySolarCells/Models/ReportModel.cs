namespace MySolarCells.Models;
public class ReportModel
{
    public List<ReportHistoryStats> Stats { get; set; } = new();
    public List<EstimateRoi> EstimateRoi { get; set; }= new();
    public ReportPageType ReportPageType { get; set; }
    public string ReportTitle { get; set; } = "";

}   
public enum ReportPageType
{
    SavingEstimate = 1,
    YearsOverview = 2,
    YearDetails = 3

}
public class ReportModelTemplatesSelector : DataTemplateSelector
{

    public DataTemplate SavingEstimateTemplate { get; set; } = new();
    public DataTemplate YearsOverviewTemplate { get; set; }= new();
  
    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        
            if (item.GetType() != typeof(ReportModel))
                return null;

            var model = (ReportModel)item;


            switch (model.ReportPageType)
            {
                case ReportPageType.SavingEstimate:
                    return SavingEstimateTemplate;
                case ReportPageType.YearsOverview:
                case ReportPageType.YearDetails:
                    return YearsOverviewTemplate;
                default:
                    return new DataTemplate();
            }
    }
}
