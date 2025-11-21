using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_3.Models
{
    public class Lecturer
    {
        [Key]
        public int Id { get; set; }

        // Link to Identity User
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Employee ID is required")]
        [Display(Name = "Employee ID")]
        [StringLength(20)]
        public string EmployeeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "ID Number")]
        [StringLength(20)]
        public string? IdNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Faculty")]
        [StringLength(100)]
        public string? Faculty { get; set; }

        [Display(Name = "Position")]
        [StringLength(50)]
        public string? Position { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Display(Name = "Hourly Rate")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000, ErrorMessage = "Hourly rate must be between 0 and 10000")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        // Banking Information
        [Display(Name = "Bank Name")]
        [StringLength(50)]
        public string? BankName { get; set; }

        [Display(Name = "Account Number")]
        [StringLength(20)]
        public string? AccountNumber { get; set; }

        [Display(Name = "Branch Code")]
        [StringLength(10)]
        public string? BranchCode { get; set; }

        [Display(Name = "Account Type")]
        [StringLength(20)]
        public string? AccountType { get; set; }

        // Audit Fields
        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime? ModifiedDate { get; set; }

        // Navigation Property
        public virtual ICollection<Claim>? Claims { get; set; }
    }
}