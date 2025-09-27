using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Models;
using MyLibrary.Repository;

namespace MyLibrary.Controllers
{
    public class AdminController : Controller
    {
        private UserRepo _repo;
        private readonly ApDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _AdminEmail;
        private readonly string _AdminPassword;
        public readonly AccountController controller;



        public AdminController(ApDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _AdminEmail = _configuration["AppSettings:AdminEmail"];
            _AdminPassword = _configuration["AppSettings:AdminPassword"];
            _repo = new UserRepo(configuration);
            controller = new AccountController(context, configuration);
        }

        public IActionResult Dashboard()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
            {
                // Əgər admin deyilsə login səhifəsinə yönləndir
                return RedirectToAction("Page", "Page");
            }

            return View(); // admin panel view
        }

        public IActionResult ShowUsers()
        {
            var users = _repo.ShowUsers();

            return View(users);
        }

        public IActionResult DeleteUser(int id)
        {
            _repo.DeleteUser(id);           // istifadəçini sil
            var users = _repo.ShowUsers();  // yenidən bütün istifadəçiləri götür
            return View("DeleteUser", users); // ShowUsers view-ə göndər
        }

        public IActionResult AddUserPage()
        {
            var users = _repo.ShowUsers();

            return View(users);
        }

        [HttpGet]
        public IActionResult AddUser() => View();

        [HttpPost]
        public IActionResult AddUser(User model)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu email artıq istifadə olunub.");
            }

            // Password yoxlaması (nadir olur amma sən istədiyin üçün əlavə edirəm)
            var hashedPassword = controller.ComputeHash(model.PasswordHash);

            if (_context.Users.Any(u => u.PasswordHash == hashedPassword))
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
                PasswordHash = controller.ComputeHash(model.PasswordHash)
            });
            _context.SaveChanges();

            return RedirectToAction("ShowUsers");
        }


    }
}
