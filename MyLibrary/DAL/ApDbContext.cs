using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyLibrary.Models;

namespace MyLibrary.DAL
{
    // ✅ BURA BAX: IDataProtectionKeyContext əlavə etməlisən
    public class ApDbContext : DbContext, IDataProtectionKeyContext
    {
        public ApDbContext(DbContextOptions<ApDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}