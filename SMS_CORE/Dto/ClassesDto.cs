using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class ClassesDto
    {
        public string className { get; set; }
        public string prefix { get; set; }
        public string category { get; set; }
        public bool subjectWiseTeaching { get; set; }
    }
}
