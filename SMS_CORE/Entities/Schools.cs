using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Schools
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long sch_id { get; set; }

        [Required]
        [StringLength(300)]
        public string sch_name { get; set; }

        [StringLength(300)]
        public string sch_address { get; set; }

        [StringLength(30)]
        public string sch_phonenumber { get; set; }

        [StringLength(50)]
        public string sch_mobile { get; set; }

        [StringLength(30)]
        public string sch_city { get; set; }

        [StringLength(100)]
        public string sch_principalname { get; set; }

        [StringLength(30)]
        public string sch_principlemobilenumber { get; set; }

        [StringLength(100)]
        public string sch_voiceprincipalname { get; set; }

        [StringLength(30)]
        public string sch_voiceprincipalmobile { get; set; }

        [StringLength(100)]
        public string sch_othercontactname { get; set; }

        [StringLength(30)]
        public string sch_othercontactmobile { get; set; }

        [StringLength(150)]
        public string sch_website { get; set; }

        [StringLength(150)]
        public string sch_instaid { get; set; }

        [StringLength(150)]
        public string sch_fbid { get; set; }

        [StringLength(150)]
        public string sch_linkedinid { get; set; }

        [StringLength(150)]
        public string sch_twitterid { get; set; }

        public bool sch_governmentapproved { get; set; }

        public bool sch_governmentschool { get; set; }

        public bool sch_active { get; set; }
    }
}
