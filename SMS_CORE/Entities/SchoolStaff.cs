using Azure.Core.Pipeline;
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
    public class SchoolStaff
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long stf_id { get; set; }
        public long stf_schid { get; set; }
        public long stf_banid { get; set; }
        public string stf_name { get; set; }
        public string stf_gender { get; set; }
        public string stf_designation { get; set; }
        public string stf_staffId { get; set; }
        public string stf_email { get; set; }
        public string stf_nextOfKin { get; set; }
        public string stf_ntnNumber { get; set; }
        public string stf_status { get; set; }
        public DateOnly stf_dob { get; set; }
        public string stf_joiningdate { get; set; }
        public string stf_contact { get; set; }
        public string stf_address { get; set; }
        public string stf_pictureFileName { get; set; }
        public string stf_pictureFileLink { get; set; }
        public string stf_cnicFileName { get; set; }
        public string stf_cnicFileLink { get; set; }
        public long stf_monthlySalary { get; set; }
        public bool stf_teacherorStaff { get; set; }
        public DateOnly? stf_lefton { get; set; }
        public bool stf_isactive { get; set; }


    }
}
