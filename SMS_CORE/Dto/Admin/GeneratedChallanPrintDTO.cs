using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Admin
{
    public class GeneratedChallanPrintDTO
    {
        // School
        public string SchoolName { get; set; }
        public string BankName { get; set; }
        public string BranchLocation { get; set; }
        public string AccountTitle { get; set; }
        public string AccountNumber { get; set; }

        // Applicant
        public string ApplicantName { get; set; }
        public string GuardianName { get; set; }
        public string RegistrationNo { get; set; }
        public string ClassName { get; set; }

        // Challan
        public DateTime DueDate { get; set; }
        public int FineDays { get; set; }
        public long TotalAmount { get; set; }

        public List<ChallanFeeHeadDTO> FeeHeads { get; set; }
    }

    public class ChallanFeeHeadDTO
    {
        public string FeeName { get; set; }
        public int Amount { get; set; }
    }

}
