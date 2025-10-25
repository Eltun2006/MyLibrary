using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Models;
using MyLibrary.Repository;
using MyLibrary.Services;
using Org.BouncyCastle.Crypto.Generators;

namespace MyLibrary.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserRepo _repo;
        private readonly ApDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        // ✅ UserRepo-ya ApDbContext göndər
        public AdminController(ApDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _repo = new UserRepo(context); // ← BURASI DƏYİŞDİ (IConfiguration yox, ApDbContext)
        }

        public IActionResult Dashboard()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
            {
                return RedirectToAction("Page", "Page");
            }
            return View();
        }

        public IActionResult ShowUsers()
        {
            var users = _repo.ShowUsers();
            return View(users);
        }

        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _repo.DeleteUserAsync(id);

            if (deleted)
            {
                TempData["Success"] = "User silindi!";
            }
            else
            {
                TempData["Error"] = "User tapılmadı!";
            }

            // ✅ Eyni səhifəyə redirect et (GET DeleteUser action-ına)
            return RedirectToAction("DeleteUser");
        }

        [HttpGet]
        public IActionResult DeleteUser()
        {
            var users = _repo.ShowUsers();
            return View(users);
        }

        public IActionResult AddUserPage()
        {
            var users = _repo.ShowUsers();
            return View(users);
        }

        [HttpGet]
        public IActionResult AddUser() => View();

        [HttpPost]
        public async Task<IActionResult> AddUser(User model)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu email artıq istifadə olunub.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ✅ Şifrə hash-lə
            var defaultPassword = "Admin123";
            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
            model.IsEmailVerified = true;

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"İstifadəçi əlavə edildi. Default şifrə: {defaultPassword}";
            return RedirectToAction("ShowUsers");
        }
    }
}