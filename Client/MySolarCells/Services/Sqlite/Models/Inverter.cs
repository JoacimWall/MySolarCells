using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySolarCells.Services.Sqlite.Models
{
    public class Inverter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InverterId { get; set; }
        [Required]
        public string Name { get; set; } =string.Empty;
        [Required]
        public DateTime FromDate { get; set; }
        [Required]
        public string SubSystemEntityId { get; set; }
        [Required]
        public int InverterTyp { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        //FK's
        [Required]
        public int HomeId { get; set; }
    }
}

