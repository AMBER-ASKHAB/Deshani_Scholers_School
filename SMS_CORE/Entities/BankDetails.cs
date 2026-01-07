using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BankDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long bnk_id { get; set; } // PK
        public long bnk_schid { get; set; } // FK from School table
        public string bnk_name { get; set; }
        public string bnk_branchLocation { get; set; }
        public string bnk_accounttitle { get; set; }
        public string bnk_accountnumber { get; set; }
        public string bnk_ibannumber { get; set; }
        public bool bnk_active { get; set; }
       
    }
}
