using Backend.Models;

namespace Backend.Repositories
{
    public interface IFavoriteRepository
    {
        Task<IEnumerable<Favorite>> GetUserFavoritesAsync(int userId);
        Task<Favorite?> GetFavoriteAsync(int userId, int doctorId);
        Task<Favorite> AddFavoriteAsync(int userId, int doctorId);
        Task RemoveFavoriteAsync(int userId, int doctorId);
        Task<bool> IsFavoriteAsync(int userId, int doctorId);
    }
}