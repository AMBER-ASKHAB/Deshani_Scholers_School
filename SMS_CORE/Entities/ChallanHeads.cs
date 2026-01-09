using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ChallanHeads
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int chh_id { get; set; }
        public int chh_schid { get; set; }
        public int chh_Challanid { get; set; }
        public int chh_Appid { get; set; }
        public String chh_name { get; set; }
        public int chh_amount { get; set; }
    }
}
