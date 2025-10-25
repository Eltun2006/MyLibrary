using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Models;

namespace MyLibrary.Repository
{
    public class UserRepo
    {
        private readonly ApDbContext _context;

        public UserRepo(ApDbContext context)
        {
            _context = context;
        }

        public List<User> ShowUsers()
        {
            var users = _context.Users
                .OrderBy(u => u.Id)
                .Select((u, index) => new User
                {
                    DisplayId = index + 1,
                    Id = u.Id,
                    Username = u.Username,
                    LastName = u.LastName,
                    Email = u.Email
                })
                .ToList();

            return users;
        }

        public List<User> DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            // PostgreSQL-də IDENTITY reseed avtomatik olur, əl ilə etməyə ehtiyac yoxdur

            return ShowUsers();
        }
    }
}