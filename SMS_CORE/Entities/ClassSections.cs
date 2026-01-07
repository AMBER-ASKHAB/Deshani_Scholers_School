using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ClassSections
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long cse_id {  get; set; }
        public long cse_classId { get; set; }
        public long cse_schId { get; set; }
        public string cse_prefix { get; set; }
        public string cse_description { get; set; }
        public int cse_lastrollNo { get; set; }
        public int cse_year { get; set; }
        public bool requiredAssistant { get; set; }
        public bool isActive { get; set; }

       
    }
}
