using System;
namespace MySolarCellsSQLite.Sqlite.Models
{
	public class SavingEssitmateParameters : ObservableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SavingEssitmateParametersId { get; set; }
        [Required]
        public DateTime FromDate  { get; set; } =  DateTime.Now;
    
        [Required]  //1.05;
        public double RealDevelopmentElectricityPrice { get; set; } = 1.05;
        [Required]  
        public double PanelDegradationPerYear { get; set; } = 0.25;
    }
}

