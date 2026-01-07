using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{

    public class Banda
    {
        [Key]
        [Column("ban_id")]
        public long BanId { get; set; }

        [Column("ban_schid")]// FK from the school table
        public long? BanSchId { get; set; }

        [Required, Column("ban_role"), MaxLength(20)]
        public string BanRole { get; set; }

        [Required, Column("ban_loginname"), MaxLength(150)]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Email must be between 5 and 150 characters.")]
        public string Ban_username { get; set; }

        [Column("ban_password")]
        [StringLength(100)]
        public string BanPassword { get; set; }

        [Column("ban_currentlylogin")] //Useful for sessions, activity tracking, and forced logout features.
        public bool BanCurrentlyLogin { get; set; }

        [Column("ban_lastloggedoffdatetime")]
        public DateTime BanLastLoggedOffDateTime { get; set; } // for auditing and checking suspicious logins.

        [Column("ban_active")]
        public bool BanActive { get; set; }

        [Column("ban_failedattempts")]
        public int BanFailedAttempts { get; set; } = 0; //for security — to detect brute force attacks and lock accounts temporarily.

        [Column("ban_created_at")]
        public DateTime BanCreatedAt { get; set; } = DateTime.UtcNow;
        [Column("ban_ResetToken")]
        public string? BanResetToken { get; set; } = null;
        [Column("ban_ResetTokenExpiresAt")]
        public DateTime? BanResetTokenExpiresAt { get; set; } = null;
    }
}
