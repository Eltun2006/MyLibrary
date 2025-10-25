using MyLibrary.Models;

namespace MyLibrary.Repository
{
    public interface IUserRepo
    {
        List<User> ShowUsers();
        Task<bool> DeleteUserAsync(int id);  // ✅ bool qaytarır
    }
}
