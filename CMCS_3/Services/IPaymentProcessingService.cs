using CMCS_3.Models;

namespace CMCS_3.Services
{
    public interface IPaymentProcessingService
    {
        Task<PaymentRecord> ProcessPaymentAsync(Claim claim);
        Task<bool> VerifyPaymentAsync(int paymentId);
        Task<PaymentRecord?> GetPaymentStatusAsync(int claimId);
    }

    public class PaymentRecord
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public string? PaymentReference { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? ProcessedDate { get; set; }
        public string? Notes { get; set; }
    }

    public class PaymentProcessingService : IPaymentProcessingService
    {
        public async Task<PaymentRecord> ProcessPaymentAsync(Claim claim)
        {
            await Task.Delay(100); // Simulate processing

            var paymentRecord = new PaymentRecord
            {
                ClaimId = claim.Id,
                PaymentReference = GeneratePaymentReference(),
                Amount = claim.TotalAmount,
                Status = "Processed",
                ProcessedDate = DateTime.Now
            };

            // Update claim status
            claim.Status = "Paid";
            claim.PaidDate = DateTime.Now;
            claim.PaymentReference = paymentRecord.PaymentReference;

            return paymentRecord;
        }

        public async Task<bool> VerifyPaymentAsync(int paymentId)
        {
            await Task.Delay(50); // Simulate verification
            return true;
        }

        public async Task<PaymentRecord?> GetPaymentStatusAsync(int claimId)
        {
            await Task.Delay(50); // Simulate lookup

            // Return null if not found, otherwise return the record
            if (claimId <= 0)
            {
                return null;
            }

            return new PaymentRecord
            {
                ClaimId = claimId,
                Status = "Completed",
                PaymentReference = $"PAY-{claimId}",
               
            };
        }

        private string GeneratePaymentReference()
        {
            return $"PAY-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }
    }
}