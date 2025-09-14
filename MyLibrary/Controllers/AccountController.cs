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
        private const string AdminPasswordHash = "Eltun2006";

        public AccountController(ApDbContext context)
        {
            _context = context;
        }




        public IActionResult Account()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

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

            // Admin yoxlaması
            if (ComputeHash(model.Password) == AdminPasswordHash)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Dashboard", "Admin");
            }

            ModelState.AddModelError("", "Email və ya şifrə səhvdir");
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

        [HttpGet]
        public IActionResult AdminCheck()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AdminCheck(string password)
        {
            if (ComputeHash(password) == AdminPasswordHash)
            {
                // Admin login uğurlu → Admin səhifəsinə yönləndir
                return RedirectToAction("Dashboard");
            }

            // Səhv parol olduqda
            ModelState.AddModelError("", "Yanlış şifrə daxil etmisiniz.");
            return View("AdminCheck");
        }

        public IActionResult Admin()
        {
            return View(); // admin panelin əsas səhifəsi
        }

        private string ComputeHash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                // İstəyə görə ya exception ata bilərsən:
                // throw new ArgumentNullException(nameof(input));

                // Ya da boş string qaytara bilərsən:
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
