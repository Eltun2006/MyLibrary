using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Models;

namespace MyLibrary.Repository
{
    public class UserRepo : IUserRepo
    {
        private readonly ApDbContext _context;

        public UserRepo(ApDbContext context)
        {
            _context = context;
        }

        public List<User> ShowUsers()
        {
            // ✅ Əvvəl ToList() et, sonra DisplayId əlavə et
            var users = _context.Users
                .OrderBy(u => u.Id)
                .ToList();

            // Memory-də DisplayId təyin et
            for (int i = 0; i < users.Count; i++)
            {
                users[i].DisplayId = i + 1;
            }

            return users;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;  // ✅ ShowUsers() çağırma, sadəcə true/false qaytar
            }

            return false;
        }
    }
}