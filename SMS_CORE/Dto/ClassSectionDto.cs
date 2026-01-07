using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class ClassSectionDto
    {
        public long classId { get; set; }
        public string prefix { get; set; }
        public string description { get; set; }
        public int lastRoll {  get; set; }
        public int year { get; set; }
        public bool requiredAssistant { get; set; }
    }
}
