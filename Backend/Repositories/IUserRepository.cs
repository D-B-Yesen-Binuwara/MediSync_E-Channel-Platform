using Backend.Models;

namespace Backend.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
    }
}