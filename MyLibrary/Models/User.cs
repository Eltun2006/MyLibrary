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

    }

    public class Book
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string? ImagePath { get; set; }

        public string Notes { get; set; }

        public string Thoughts { get; set; }

        // İstifadəçi ilə əlaqə
        [ForeignKey("User")]
        public int UserId { get; set; }      // Bu User.Id ilə bağlıdır

        public virtual User User { get; set; } // Navigation property
    }
}
