using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Application;

namespace Domain.Dto.Application
{
    public  class ClassCategoryViewModel
    {
        public string? Category { get; set; }
        public string DisplayCategory { get; set; }
        public List<Classes>? Classes { get; set; }
        public int MinId { get; set; }
        public Dictionary<int, List<QuickLink>> QuickLinksPerClass { get; set; } = new Dictionary<int, List<QuickLink>>();

    }
}
