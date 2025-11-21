using CMCS_3.Models;

namespace CMCS_3.Services
{
    public interface IClaimVerificationService
    {
        Task<ClaimVerificationResult> VerifyClaimAsync(Claim claim);
        Task<bool> ValidateHoursAsync(decimal totalHours, int lecturerId);
        Task<bool> ValidateDocumentsAsync(string? documentPath);
    }

    public class ClaimVerificationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class ClaimVerificationService : IClaimVerificationService
    {
        private const decimal MAX_HOURS_PER_MONTH = 160;
        private const decimal MIN_HOURS_PER_CLAIM = 1;

        public async Task<ClaimVerificationResult> VerifyClaimAsync(Claim claim)
        {
            var result = new ClaimVerificationResult { IsValid = true };

            // Validate hours
            if (claim.TotalHours < MIN_HOURS_PER_CLAIM)
            {
                result.IsValid = false;
                result.Errors.Add($"Total hours must be at least {MIN_HOURS_PER_CLAIM} hour.");
            }

            if (claim.TotalHours > MAX_HOURS_PER_MONTH)
            {
                result.IsValid = false;
                result.Errors.Add($"Total hours cannot exceed {MAX_HOURS_PER_MONTH} hours per month.");
            }

            // Validate hourly rate
            if (claim.HourlyRate <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("Hourly rate must be greater than zero.");
            }

            // Validate total amount calculation
            var expectedAmount = claim.TotalHours * claim.HourlyRate;
            if (claim.TotalAmount != expectedAmount)
            {
                result.Warnings.Add("Total amount has been recalculated.");
                claim.TotalAmount = expectedAmount;
            }

            // Validate claim period
            if (claim.ClaimPeriod > DateTime.Now)
            {
                result.IsValid = false;
                result.Errors.Add("Claim period cannot be in the future.");
            }

            await Task.CompletedTask;
            return result;
        }

        public async Task<bool> ValidateHoursAsync(decimal totalHours, int lecturerId)
        {
            // Add business logic to validate hours against lecturer's contract
            await Task.CompletedTask;
            return totalHours >= MIN_HOURS_PER_CLAIM && totalHours <= MAX_HOURS_PER_MONTH;
        }

        public async Task<bool> ValidateDocumentsAsync(string? documentPath)
        {
            if (string.IsNullOrEmpty(documentPath))
            {
                return true; // Documents are optional
            }

            // Validate document exists and is valid format
            var validExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".png" };
            var extension = Path.GetExtension(documentPath).ToLower();
            
            await Task.CompletedTask;
            return validExtensions.Contains(extension);
        }
    }
}