using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> CreateAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction?> GetByIdAsync(int transactionId)
        {
            return await _context.Transactions
                .Include(t => t.Appointment)
                .Include(t => t.Patient)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        public async Task<List<Transaction>> GetByPatientIdAsync(int patientId)
        {
            return await _context.Transactions
                .Include(t => t.Appointment)
                .Where(t => t.PatientId == patientId)
                .OrderByDescending(t => t.PaymentDate)
                .ToListAsync();
        }

        public async Task<Transaction> UpdateAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }
    }
}