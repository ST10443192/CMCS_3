using System;

namespace ContractMonthlyClaimSystem2.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class Users
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }  // stored hashed
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; } = true;
        public string CreatedDate { get; set; }
        public string LastLogin { get; set; }
    }

    
}
