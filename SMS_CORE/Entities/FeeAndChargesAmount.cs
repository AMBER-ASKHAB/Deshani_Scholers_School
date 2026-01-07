using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class FeeAndChargesAmount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long fcm_id { get; set; }
        public long fcm_FeeHead { get; set; }
        public long fcm_amount { get; set; }
        public long? fcm_clsID { get; set; }
    }
}
