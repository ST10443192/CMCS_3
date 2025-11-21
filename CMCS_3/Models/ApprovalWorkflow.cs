using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_3.Models
{
    public class ApprovalWorkflow
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key to Claim
        [Required]
        public int ClaimId { get; set; }

        [ForeignKey("ClaimId")]
        public virtual Claim? Claim { get; set; }

        // Approval Step (1 = Coordinator, 2 = Manager)
        [Required]
        public int Step { get; set; }

        // Approver Information
        [Required]
        [StringLength(450)]
        public string ApproverId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ApproverName { get; set; }

        [StringLength(50)]
        public string? ApproverRole { get; set; }

        // Approval Status: Pending, Approved, Rejected
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        // Comments
        [StringLength(500)]
        public string? Comments { get; set; }

        // Timestamps
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? ActionDate { get; set; }
    }
}