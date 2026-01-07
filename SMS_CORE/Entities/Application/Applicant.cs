using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Application
{
    [Table("applicants")]
    public class Applicant
    {
        [Key]
        [Column("app_id")]
        public long Id { get; set; }
        [Column("app_createdby")]
        public long CreatedBy { get; set; }

        [Column("app_schid")]

        public long SchoolId { get; set; } = 1;

        [Column("app_fullname")]
        public string FullName { get; set; } = string.Empty;

        [Column("app_gender")]
        public string? Gender { get; set; }

        [Column("app_dateofbirth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("app_mothertongue")]
        public string? MotherTongue { get; set; }

        // ✅ BForm details
        [Column("app_BFORM_number")]
        public string? BFormNumber { get; set; }

        [Column("app_BFORM_file")]
        public string? BFormFilePath { get; set; }

        // ✅ Profile picture
        [Column("app_picturefilename")]
        public string? ProfilePicFilePath { get; set; }

        // ✅ Previous school details (inline, not FK)
        [Column("app_prevschool_category")]
        public int classCategory { get; set; }

        [Column("app_prevschool_name")]
        public string? PrevSchool { get; set; }

        [Column("app_prevschool_yearsattended")]
        public string? YearsAttended { get; set; }

        [Column("app_prevschool_grade")]
        public string? Grade { get; set; }  // using string instead of char for safety

        [Column("app_prevschool_percentage")]
        public float? Percentage { get; set; }

        [Column("app_prevschool_certfile")]
        public string? PreviousSchoolCertFilePath { get; set; }
        [Column("app_prevschoolleaving_certfile")]
        public string? PreviousSchoolLeavCertFilePath { get; set; }

        [Column("app_date")]
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        [Column("app_appliedforclass")]
        public long? AppliedForClassId { get; set; }

        [Column("app_status")]
        public string Status { get; set; } = "Incomplete";

        [Column("app_approvedby")]
        public long? ApprovedBy { get; set; }
        [Column("app_contactNo")]
        public string contact { get; set; }= "";
        [Column("app_Email")]
        public string emailAddress { get; set; } = "";

        [Column("app_approvedon")]
        public DateTime? ApprovedOn { get; set; }
        public ICollection<ApplicantGuardian> Guardians { get; set; }
        public ICollection<ApplicantSibling> Siblings{ get; set; }
    }
}
