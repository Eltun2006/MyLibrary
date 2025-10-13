using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Models;

namespace MyLibrary.Controllers
{
    public class PageController : Controller
    {
        private readonly ApDbContext _context;

        public PageController(ApDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Page()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (currentUser != null)
                {
                    var myBooks = await _context.Books
                        .Where(b => b.UserId == currentUser.Id)
                        .ToListAsync();

                    ViewBag.UserName = currentUser.Username;
                    ViewBag.CurrentUserId = currentUser.Id;

                    return View(myBooks);
                }
            }

            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(Book model, IFormFile? ImageFile)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Json(new { error = "User not found" });

            model.UserId = userId;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await ImageFile.CopyToAsync(stream);

                model.ImagePath = "/images/" + fileName;
            }

            _context.Books.Add(model);
            await _context.SaveChangesAsync();

            return Json(new
            {
                id = model.Id,
                title = model.Title,
                notes = model.Notes,
                thoughts = model.Thoughts,
                imagePath = model.ImagePath
            });
        }


        [HttpPost]
        public async Task<IActionResult> RemoveBook(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "İstifadəçi tapılmadı!";
                return RedirectToAction("Login", "Account");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (book == null)
            {
                TempData["Error"] = "Kitab tapılmadı və ya silmək icazəniz yoxdur!";
                return RedirectToAction("Page");
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kitab uğurla silindi!";
            return RedirectToAction("Page");
        }


    }
}
