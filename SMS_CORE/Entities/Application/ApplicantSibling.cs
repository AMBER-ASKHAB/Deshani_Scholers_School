using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Application
{
    [Table("applicantsibling")]
    public class ApplicantSibling
    {
        [Key]
        [Column("asg_id")] public long Id { get; set; }
        [Column("asg_schid")] public long SchoolId { get; set; }
        [Column("asg_appid")] public long ApplicantId { get; set; }
        [Column("asg_stuid")] public long StudentId { get; set; }
        [Column("asg_Name")] public string? Name { get; set; }
        [Column("asg_Bform")] public string? Bform { get; set; }
        [Column("asg_class")] public int classNo { get; set; }
        public Applicant Applicant { get; set; } // Navigation

    }
}
