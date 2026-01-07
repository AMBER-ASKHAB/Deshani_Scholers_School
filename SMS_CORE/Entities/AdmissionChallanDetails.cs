using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AdmissionChallanDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long acd_id { get; set; }

        [Required]
        public long acd_schid { get; set; }

        [Required]
        public long acd_acfid { get; set; }

        [Required]
        public long acd_detialid { get; set; }

        [Required]
        [MaxLength(20)]
        public string acd_chargestype { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal acd_amount { get; set; }
    }
}
