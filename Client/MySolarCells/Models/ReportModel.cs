using System;
namespace MySolarCells.Models;

public class ReportModel
{
    public List<ReportHistoryStats> Stats { get; set; }
    public List<EstimateRoi> EstimatRoi { get; set; }
    public ReportPageTyp ReportPageTyp { get; set; }
    public string ReportTitle { get; set; }

}   
public enum ReportPageTyp
{
    SavingEssitmate = 1,
    YearsOverview = 2,
    YearDetails = 3

}
public class ReportModelTemplatesSelector : DataTemplateSelector
{

    public DataTemplate SavingEssitmate_Template { get; set; }
    public DataTemplate YearsOverview_Template { get; set; }
    //public DataTemplate YearDetails_Template { get; set; }



    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item != null)
        {
            if (item.GetType() != typeof(ReportModel))
                return null;

            var model = (ReportModel)item;


            switch (model.ReportPageTyp)
            {
                case ReportPageTyp.SavingEssitmate:
                    return SavingEssitmate_Template;
                case ReportPageTyp.YearsOverview:
                case ReportPageTyp.YearDetails:
                    return YearsOverview_Template;
               
                default:
                    return new DataTemplate();

            }
        }

        return new DataTemplate();
    }
}
