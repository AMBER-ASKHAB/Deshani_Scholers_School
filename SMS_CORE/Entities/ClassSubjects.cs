using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ClassSubjects
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long csb_id { get; set; }
        public long csb_schid { get; set; }
        public long csb_classid { get; set; }
        public long csb_subjectID { get; set; }
        public int csb_year { get; set; }
    }
}
