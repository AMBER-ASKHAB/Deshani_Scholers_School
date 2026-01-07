using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class StaffEdu
    {
        public long stfID { get; set; }
        public string degreeName { get; set; }
        public string instituteName { get; set; }
        public string grade { get; set; }
        public int yearpassing {  get; set; }
    }
}
