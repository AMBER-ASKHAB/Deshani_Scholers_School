using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class AddSubject
    {
        public long classID {  get; set; }
        public long subjectID { get; set; }
        public int year { get; set; }
    }
}
