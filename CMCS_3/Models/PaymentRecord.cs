using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_3.Models
{
    public class PaymentRecord
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key to Claim
        [Required]
        public int ClaimId { get; set; }

        [ForeignKey("ClaimId")]
        public virtual Claim? Claim { get; set; }

        // Payment Reference Number
        [Required]
        [StringLength(100)]
        public string PaymentReference { get; set; } = string.Empty;

        // Payment Amount
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Payment Date
        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        // Payment Method (EFT, Cash, Cheque, etc.)
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        // Additional Notes
        [StringLength(500)]
        public string? Notes { get; set; }

        // User who processed the payment
        public string? ProcessedBy { get; set; }

        [ForeignKey("ProcessedBy")]
        public virtual ApplicationUser? Processor { get; set; }
    }
}