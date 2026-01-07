using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SectionTeachers
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ste_id { get; set; }
        public long ste_schid { get; set; }
        public long ste_teacherID { get; set; }
        public long ste_sectionID { get; set; }
        public bool ste_mainTeacher { get; set; }
        public int ste_year { get; set; }
    }
}
