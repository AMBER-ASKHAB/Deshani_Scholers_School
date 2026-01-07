using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Classes
    {
        public long Id { get; set; }          // cls_id
        public long SchoolId { get; set; }    // cls_schid
        public string Prefix { get; set; }    // cls_prefix
        public string Category { get; set; }  // cls_category  (preprimary, primary, middle, high)
        public string Description { get; set; } // cls_description (Nursery, KG-1, 1, 2, 3…)
        public bool SubjectWiseTeaching { get; set; }
        public bool Active { get; set; }      // cls_active
    }
}
