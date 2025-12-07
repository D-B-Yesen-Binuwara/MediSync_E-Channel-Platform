using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [AllowAnonymous]
    public class AdminTransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminTransactionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetTransactions([FromQuery] string? search)
        {
            try
            {
                var query = _context.Transactions
                    .Include(t => t.Patient)
                    .Include(t => t.Appointment)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(t => 
                        t.Patient.FullName.Contains(search) ||
                        t.NIC.Contains(search) ||
                        t.PaymentId.Contains(search));
                }

                var transactions = await query
                    .OrderByDescending(t => t.PaymentDate)
                    .Select(t => new {
                        t.TransactionId,
                        t.AppointmentId,
                        PatientName = t.Patient.FullName,
                        t.PaymentId,
                        t.Amount,
                        Status = t.Status == TransactionStatus.pending ? "Pending" : t.Status == TransactionStatus.completed ? "Paid" : "Failed",
                        StatusValue = (int)t.Status,
                        t.PaymentDate,
                        t.PaymentMethod,
                        t.BankName,
                        t.Email,
                        t.NIC
                    })
                    .ToListAsync();

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}