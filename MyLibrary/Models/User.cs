using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyLibrary.Models
{

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email daxil edin")]
        [EmailAddress(ErrorMessage = "Düzgün email daxil edin")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifrə daxil edin")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class User
    {
        [Key]
        public int Id { get; set; }
        public int DisplayId { get; set; }
        public string Username { get; set; }
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        // ✅ Yeni əlavələr
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // Navigation property
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }

    public class Book
    {
        [Key]
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? ImagePath { get; set; }

        public string? Notes { get; set; }

        public int Rating { get; set; }

        public string? Thoughts { get; set; }

        // İstifadəçi ilə əlaqə
        [ForeignKey("User")]
        public int UserId { get; set; }      // Bu User.Id ilə bağlıdır

        public virtual User User { get; set; } // Navigation property
    }

    public class UserLibraryViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public int BookCount { get; set; }
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
