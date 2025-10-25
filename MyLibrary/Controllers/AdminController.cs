using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Models;
using MyLibrary.Repository;
using MyLibrary.Services;

namespace MyLibrary.Controllers
{
    public class AdminController : Controller
    {
        private UserRepo _repo;
        private readonly ApDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;


        // ✅ IEmailService əlavə et
        public AdminController(ApDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;

            _repo = new UserRepo(configuration);
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

        public IActionResult DeleteUser(int id)
        {
            _repo.DeleteUser(id);
            var users = _repo.ShowUsers();
            return View("DeleteUser", users);
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
            var defaultPassword = "Admin123"; // Default şifrə
            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
            model.IsEmailVerified = true; // Admin tərəfindən əlavə edilən istifadəçi təsdiqlənmiş sayılır

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"İstifadəçi əlavə edildi. Default şifrə: {defaultPassword}";
            return RedirectToAction("ShowUsers");
        }
    }
}