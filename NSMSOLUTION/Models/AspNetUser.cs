using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models
{
    [Table("AspNetUsers")]
    public class AspNetUser
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // Auto-generate GUID

        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } // Optional email

        public bool EmailConfirmed { get; set; } = true;

        [Required(ErrorMessage = "PasswordHash is required.")]
        public string PasswordHash { get; set; } // Must not be null before saving

        public string SecurityStamp { get; set; }

        //[Phone]
        public string PhoneNumber { get; set; }
        public int CustomerID { get; set; }

        public bool PhoneNumberConfirmed { get; set; } = false;

        public bool TwoFactorEnabled { get; set; } = false;

        public DateTime? LockoutEndDateUtc { get; set; }

        public bool LockoutEnabled { get; set; } = false;

        public int AccessFailedCount { get; set; } = 0;

        [Required(ErrorMessage = "UserName is required.")]
        [MaxLength(256)]
        public string UserName { get; set; } // Must not be null before saving

        [MaxLength(50)]
        public string DomainID { get; set; } = "001";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}