using Backend.Models.DTOs;

namespace Backend.Services
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto request);
        Task ChangePasswordAsync(int userId, ChangePasswordDto request);
        Task DeleteAccountAsync(int userId);
        Task<List<TransactionDto>> GetTransactionsAsync(int userId);
    }
}