using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Application
{
    public class ApplicantDocuments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("adc_id")]
        public long Id { get; set; }
        [Column("adc_schid")]

        public long SchoolId { get; set; } = 1;
        [Column("adc_appid")]
        public long appId { get; set; }

        [Column("adc_BFORM_name")]
        public string? BFormFileName { get; set; }

        [Column("adc_BFORM_file")]
        public string? BFormFilePath { get; set; }

        [Column("adc_picturefileName")]
        public string? ProfilePicFileName { get; set; }

        [Column("adc_picturefilePath")]
        public string? ProfilePicFilePath { get; set; }

        [Column("adc_prevschoolleavingcertfileName")]
        public string? PreviousSchoolLeavCertFileName { get; set; }

        [Column("adc_prevschoolleaving_certfile")]
        public string? PreviousSchoolLeavCertFilePath { get; set; }

        [Column("adc_prevMarkSheetfileName")]
        public string? MarkSheetfileName { get; set; }

        [Column("adc_prevMarkSheetfile")]
        public string? prevMarkSheetFilePath { get; set; }
        [Column("adc_GuardCnicfileNameF")]
        public string? GuardCnicfileNameF { get; set; }

        [Column("adc_GuardCnicfilePathF")]
        public string? GuardCnicfilePathF { get; set; }

        [Column("adc_GuardCnicfileNameB")]
        public string? GuardCnicfileNameB { get; set; }

        [Column("adc_GuardCnicfilePathB")]
        public string? GuardCnicfilePathB { get; set; }

        [Column("adc_ChallanReceiptName")]
        public string? ChallanReceiptName { get; set; }

        [Column("adc_ChallanReceiptPath")]
        public string? ChallanReceiptPath { get; set; }


    }
}