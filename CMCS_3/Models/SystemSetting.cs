using System;
using System.ComponentModel.DataAnnotations;

namespace CMCS_3.Models
{
    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string SettingValue { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}