using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class StaffEducation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long sed_id { get; set; }
        public long sed_schid { get; set; }
        public long sed_staffid { get; set; }
        public string sed_degreeName { get; set; }
        public int sed_yearPassing { get; set; }
        public string sed_instituteName { get; set; }
        public string sed_grades { get; set; }
    }
}
