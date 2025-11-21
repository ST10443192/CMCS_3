using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_3.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        // User who performed the action (nullable for system actions)
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // Related Claim (optional)
        public int? ClaimId { get; set; }

        [ForeignKey("ClaimId")]
        public virtual Claim? Claim { get; set; }

        // Action performed
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        // Additional details
        [StringLength(2000)]
        public string? Details { get; set; }

        // Timestamp
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // IP Address of the user
        [StringLength(50)]
        public string? IpAddress { get; set; }
    }
}