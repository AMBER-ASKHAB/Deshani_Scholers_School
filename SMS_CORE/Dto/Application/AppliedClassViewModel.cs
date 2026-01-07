using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Application
{
    public class AppliedClassViewModel
    {
        public long ApplicantId { get; set; }
        public long? AppliedForClassId { get; set; } = 0;
        public string classDesc { get; set; } = "";
    }
}
