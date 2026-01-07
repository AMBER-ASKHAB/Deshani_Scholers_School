using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Application
{
    [Table("applicantguardians")]
    public class ApplicantGuardian
    {
        [Key]
        [Column("agu_id")] public long Id { get; set; }
        [Column("agu_schid")] public long SchoolId { get; set; }//fk
        [Column("agu_appid")] public long ApplicantId { get; set; } //fk
        public Applicant Applicant { get; set; } // Navigation
        [Column("agu_fullname")] public string FullName { get; set; } = string.Empty;
        [Column("agu_gender")] public string? Gender { get; set; }
        [Column("agu_relationwithapplicant")] public string? RelationWithApplicant { get; set; } // father/mother/...
        [Column("agu_address")] public string? Address { get; set; }
        [Column("agu_contactnumber")] public string? ContactNumber { get; set; }
        [Column("agu_email")] public string? Email { get; set; }
        [Column("agu_cnicfilename")] public string? CnicFileName { get; set; }
        [Column("agu_cnicNo")] public string? Cnic { get; set; }
        [Column("agu_qualifications")] public string? Qualifications { get; set; }
        [Column("agu_occupation")] public string? Occupation { get; set; }

    }
}
