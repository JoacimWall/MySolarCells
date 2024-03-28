namespace MySolarCells.Models;
public class ReportModel
{
    public List<ReportHistoryStats> Stats { get; set; } = new();
    public List<EstimateRoi> EstimateRoi { get; set; }= new();
    public ReportPageTyp ReportPageTyp { get; set; }
    public string ReportTitle { get; set; } = "";

}   
public enum ReportPageTyp
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


            switch (model.ReportPageTyp)
            {
                case ReportPageTyp.SavingEstimate:
                    return SavingEstimateTemplate;
                case ReportPageTyp.YearsOverview:
                case ReportPageTyp.YearDetails:
                    return YearsOverviewTemplate;
                default:
                    return new DataTemplate();
            }
    }
}
