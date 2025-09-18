using Microsoft.AspNetCore.Mvc;
using MyLibrary.DAL;
using MyLibrary.Models;
using System.Security.Cryptography;
using System.Text;

namespace MyLibrary.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _AdminEmail;
        private readonly string _AdminPassword;



        public AccountController(ApDbContext context,IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _AdminEmail = _configuration["AppSettings:AdminEmail"];
            _AdminPassword = _configuration["AppSettings:AdminPassword"];
        }




        public IActionResult Account()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Admin yoxlaması
            if (model.Password == _AdminPassword && model.Email == _AdminEmail)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Dashboard", "Admin");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email &&
                                                          u.PasswordHash == ComputeHash(model.Password));

            if (user != null)
            {
                // Normal istifadəçi
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Username);
                HttpContext.Session.SetString("IsAdmin", "false");
                return RedirectToAction("Page", "Page");
            }



            // ❌ Email və ya parol səhvdirsə
            ModelState.AddModelError("Password", "Email və ya şifrə yalnışdır");
            return View(model);
        }




        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User model)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu email artıq istifadə olunub.");
            }

            // Password yoxlaması (nadir olur amma sən istədiyin üçün əlavə edirəm)
            if (_context.Users.Any(u => u.PasswordHash == ComputeHash(model.PasswordHash)))
            {
                ModelState.AddModelError("PasswordHash", "Bu şifrə artıq istifadə olunub. Başqa şifrə seçin.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Users.Add(new User
            {
                Username = model.Username,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = ComputeHash(model.PasswordHash)
            });
            _context.SaveChanges();

            return RedirectToAction("Login");
        }


        private string ComputeHash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(bytes);
            }
        }

    }
}
