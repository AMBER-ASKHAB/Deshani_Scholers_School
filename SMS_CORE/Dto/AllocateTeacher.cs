using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class AllocateTeacher
    {
        public long teacherId { get; set; }
        public long classId { get; set; }
        public long sectionId { get; set; }
        public long subjectId { get; set; }
    }
}
