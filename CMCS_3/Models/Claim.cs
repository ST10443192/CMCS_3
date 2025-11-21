using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_3.Models
{
    public class Claim
    {
        [Key]
        public int Id { get; set; }

        // Claim Reference Number (e.g., CLM-2024-0001)
        [Display(Name = "Claim Reference")]
        [StringLength(20)]
        public string? ClaimReference { get; set; }

        // Foreign Key to Lecturer
        [Required]
        public int LecturerId { get; set; }

        [ForeignKey("LecturerId")]
        public virtual Lecturer? Lecturer { get; set; }

        // Claim Period
        [Required]
        [Display(Name = "Claim Period")]
        [DataType(DataType.Date)]
        public DateTime ClaimPeriod { get; set; }

        // Hours Worked
        [Required]
        [Display(Name = "Total Hours")]
        [Range(0, 500, ErrorMessage = "Hours must be between 0 and 500")]
        public decimal TotalHours { get; set; }

        // Hourly Rate at time of claim
        [Required]
        [Display(Name = "Hourly Rate")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        // Calculated Total Amount
        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Claim Status: Pending, Approved, Rejected, Processing, Paid
        [Required]
        [Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        // Description/Notes
        [Display(Name = "Description")]
        [StringLength(500)]
        public string? Description { get; set; }

        // Supporting Documents
        [Display(Name = "Document Path")]
        [StringLength(255)]
        public string? DocumentPath { get; set; }

        [Display(Name = "Document Name")]
        [StringLength(100)]
        public string? DocumentName { get; set; }

        // Submission Details
        [Display(Name = "Submission Date")]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        // Approval Workflow
        [Display(Name = "Coordinator Approved")]
        public bool? CoordinatorApproved { get; set; }

        [Display(Name = "Coordinator ID")]
        public string? CoordinatorId { get; set; }

        [Display(Name = "Coordinator Comments")]
        [StringLength(500)]
        public string? CoordinatorComments { get; set; }

        [Display(Name = "Coordinator Approval Date")]
        public DateTime? CoordinatorApprovalDate { get; set; }

        [Display(Name = "Manager Approved")]
        public bool? ManagerApproved { get; set; }

        [Display(Name = "Manager ID")]
        public string? ManagerId { get; set; }

        [Display(Name = "Manager Comments")]
        [StringLength(500)]
        public string? ManagerComments { get; set; }

        [Display(Name = "Approved Date")]
        public DateTime? ApprovedDate { get; set; }

        // Rejection Details
        [Display(Name = "Rejection Reason")]
        [StringLength(500)]
        public string? RejectionReason { get; set; }

        [Display(Name = "Rejected Date")]
        public DateTime? RejectedDate { get; set; }

        // Payment Details
        [Display(Name = "Payment Reference")]
        [StringLength(50)]
        public string? PaymentReference { get; set; }

        [Display(Name = "Payment Notes")]
        [StringLength(500)]
        public string? PaymentNotes { get; set; }

        [Display(Name = "Paid Date")]
        public DateTime? PaidDate { get; set; }

        // Audit Fields
        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime? ModifiedDate { get; set; }

        // Helper method to generate claim reference
        public static string GenerateClaimReference()
        {
            return $"CLM-{DateTime.Now:yyyy}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }
    }
}