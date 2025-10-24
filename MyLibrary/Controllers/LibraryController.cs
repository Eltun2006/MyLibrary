using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Models;

namespace MyLibrary.Controllers
{
    public class LibraryController : Controller
    {
        private readonly ApDbContext _context;

        public LibraryController(ApDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Libraries()
        {
            // Session yoxla
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int currentUserId))
                return RedirectToAction("Login", "Account");

            // Current user məlumatını ViewBag-ə əlavə et
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (currentUser != null)
            {
                ViewBag.UserName = $"{currentUser.Username} {currentUser.LastName}";
            }

            // Bütün istifadəçiləri və onların kitablarını çək
            var usersWithBooks = await _context.Users
                .Include(u => u.Books) // User-in kitablarını da gətir
                .Where(u => u.Books.Any()) // Yalnız kitabı olan istifadəçilər
                .Select(u => new UserLibraryViewModel
                {
                    UserId = u.Id,
                    UserName = u.Username,
                    FullName = $"{u.Username} {u.LastName}",
                    BookCount = u.Books.Count,
                    Books = u.Books.OrderByDescending(b => b.Id).ToList() // Ən yeni kitablar əvvəl
                })
                .OrderBy(u => u.UserName) // İstifadəçiləri ada görə sırala
                .ToListAsync();

            return View(usersWithBooks);
        }

        // İstəyə görə - Kitab detaylarını göstərmək üçün
        [HttpGet]
        public async Task<IActionResult> BookDetail(int id)
        {
            var book = await _context.Books
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                TempData["Error"] = "Kitab tapılmadı!";
                return RedirectToAction("Libraries");
            }

            // Current user məlumatı
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int currentUserId))
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                if (currentUser != null)
                {
                    ViewBag.UserName = $"{currentUser.Username} {currentUser.LastName}";
                }
            }

            ViewBag.BookOwner = $"{book.User.Username} {book.User.LastName}";
            return View(book);
        }
    }
}