using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Application
{
    public class QuickLink
    {
        public int Id { get; set; }
        public long ClassId { get; set; }   // FK to Class
        public string Title { get; set; }  // e.g., "Fee Structure"
        public string Url { get; set; }    // e.g., "/files/fee.pdf"
    }
}
