using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AdmissionChallan
    {
        [Key]
        public long acf_id { get; set; } // PK, Auto-generated

        [ForeignKey("School")]
        public long acf_schid { get; set; } // FK from school table

        public long acf_appid { get; set; } // Applicant ID

        public DateTime acf_dateofissue { get; set; } // Issue date

        public DateTime acf_tobepaidby { get; set; } // To be paid by date
        public DateTime acf_expirydate { get; set; }

        public long acf_preparedby { get; set; } // Staff ID

        public DateTime? acf_paidon { get; set; } // Paid date (nullable)

        [MaxLength(100)]
        public string? acf_paidfilename { get; set; } // Receipt file name

        public long? acf_cancelledby { get; set; } // Staff ID (nullable)

        public DateTime? acf_cancelledon { get; set; } // Cancellation date/time (nullable)

        public long acf_totalamount { get; set; }
        public long acf_fineAfterDueDate { get; set; }

        [MaxLength(100)]
        public string? acf_discountapprovalfilename { get; set; }
    }
}
