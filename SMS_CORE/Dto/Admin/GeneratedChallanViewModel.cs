using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Admin
{
    public class GeneratedChallanViewModel
    {
        public int app_id { get; set; }
        public DateTime dueDate { get; set; }
        public int fineDate { get; set; }
        public string challanType { get; set; }

        public List<FeeHeadVM> FeeHeads { get; set; }
    }

    public class FeeHeadVM
    {
        public string Key { get; set; }
        public int Value { get; set; }
        public int FacId { get; set; }
        public int FcmId { get; set; }
    }
}
