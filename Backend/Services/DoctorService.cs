using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class DoctorService
    {
        private readonly DoctorRepository _repo;
        public DoctorService(DoctorRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Doctor>> GetDoctorsAsync(string? name, string? specialization, DateTime? date)
        {
            return await _repo.GetDoctorsAsync(name, specialization, date);
        }
    }
}
