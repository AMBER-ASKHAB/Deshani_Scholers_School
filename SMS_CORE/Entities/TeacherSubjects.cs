using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TeacherSubjects
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long sts_id { get; set; }
        public long sts_schid { get; set; }
        public long sts_teacherID { get; set; }
        public long sts_sectionID { get; set; }
        public long sts_subjectID { get; set; }
        public int sts_year { get; set; }
    }
}
