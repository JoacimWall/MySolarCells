using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySolarCells.Services.Sqlite.Models
{
    public class EnergyCalculationParameter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnergyCalculationParameterId { get; set; }
        [Required]
        public DateTime FromDate { get; set; } = DateTime.Today;
        [Required]  //Nätnytta 0.078 kr/kWh
        public decimal ProdCompensationElectricityLowload { get; set; } = 0.078M;
        [Required]  //Eventuell överföringsavgift som du kostar vi köp av eller (sparar vi egen användning) Ellevio 0.3 kr
        public decimal TransferFee { get; set; } = 0.3M;
        [Required] //0.60/kWh såld el Max 18 0000 kr och inte för fler kWh än huset köper in
        public decimal TaxReduction { get; set; } = 0.6M;
        [Required] //0.49/kWh såld (sparar vi egen användning)
        public decimal EnergyTax { get; set; } = 0.49M;
        //FK's
        [Required]
        public int HomeId { get; set; }
    }
}

