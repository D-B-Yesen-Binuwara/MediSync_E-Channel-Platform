using Backend.Models;

namespace Backend.Repositories
{
    public interface ITransactionRepository
    {
        Task<Transaction> CreateAsync(Transaction transaction);
        Task<Transaction?> GetByIdAsync(int transactionId);
        Task<List<Transaction>> GetByPatientIdAsync(int patientId);
        Task<Transaction> UpdateAsync(Transaction transaction);
    }
}