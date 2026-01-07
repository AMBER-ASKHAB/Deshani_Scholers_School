using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ClassTimeTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ctt_id { get; set; }
        public long ctt_schid { get; set; }
        public long ctt_classid { get; set; }
        public long ctt_sectionid { get; set; }
        public string ctt_timeSlot { get; set; }
        public string ctt_Monday{ get; set; }
        public string ctt_Tuesday { get; set; }
        public string ctt_Wednesday { get; set; }
        public string ctt_Thursday { get; set; }
        public string ctt_Friday { get; set; }
        public string ctt_Saturday { get; set; }
    }
}
