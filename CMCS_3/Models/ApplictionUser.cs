using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CMCS_3.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        [StringLength(20)]
        [Display(Name = "Employee ID")]
        public string? EmployeeId { get; set; }

        [StringLength(100)]
        [Display(Name = "Department")]
        public string? Department { get; set; }

        [StringLength(100)]
        [Display(Name = "Faculty")]
        public string? Faculty { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ProfilePicture { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        [Display(Name = "Last Login")]
        public DateTime? LastLogin { get; set; }
    }
}