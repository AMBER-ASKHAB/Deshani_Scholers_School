using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SchoolParams
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long spa_id { get; set; }
        public long spa_schid { get; set; }
        public string spa_paramName { get; set; }
        public string spa_paramValue { get; set; }
        public string spa_description { get; set; }
    }
}
