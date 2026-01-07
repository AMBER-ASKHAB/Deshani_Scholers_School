using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class AddStaffDto
    {
        public string Name { get; set; }
        public string gender { get; set; }
        public string stfNo { get; set; }
        public string designation { get; set; }
        public string contactNo { get; set; }
        public string address { get; set; }
        public string joiningdate { get; set; }
        public DateOnly dob { get; set; }
        public string email { get; set; }
        public string nextOFKin { get; set; }
        public long salary { get; set; }
        public string ntnNo { get; set; }
        public string pfppicname { get; set; }
        public string cnicpicname { get; set; }
        public string Status { get; set; }
        public bool isTeacher { get; set; }
        public DateOnly lefton { get; set; }

    }
}
