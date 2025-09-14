using Microsoft.EntityFrameworkCore;
using MyLibrary.Models;

namespace MyLibrary.DAL
{
    public class ApDbContext : DbContext
    {
        public ApDbContext(DbContextOptions<ApDbContext> options) : base(options)
        {
        }

        public DbSet<User>Users { get; set; }
    }
}
