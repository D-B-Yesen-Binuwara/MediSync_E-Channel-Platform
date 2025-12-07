using Backend.Models.DTOs;

namespace Backend.Services
{
    public interface IPaymentService
    {
        Task<string> ProcessPaymentAsync(PaymentDetailsDto paymentDetails);
        Task<bool> ValidatePaymentAsync(string paymentId);
    }
}