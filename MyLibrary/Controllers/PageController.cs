using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary.DAL;
using MyLibrary.Models;

namespace MyLibrary.Controllers
{
    public class PageController : Controller
    {
        private readonly ApDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PageController(IWebHostEnvironment webHostEnvironment, ApDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
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

        // ✅ GET Metodu - Düzəldilmiş
        [HttpGet]
        public async Task<IActionResult> AddBook(int? id)  // ✅ int? (nullable) olmalıdır
        {
            if (!id.HasValue || id.Value == 0)
            {
                // Yeni kitab əlavə et
                return View(new Book());
            }

            // Mövcud kitabı tap və edit et
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Account");

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id.Value && b.UserId == userId);

            if (book == null)
            {
                TempData["Error"] = "Kitab tapılmadı!";
                return RedirectToAction("Page");
            }

            return View(book);
        }

        // ✅ POST Metodu
        [HttpPost]
        public async Task<IActionResult> AddBook(Book model, IFormFile? ImageFile)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Json(new { error = "User not found" });

            model.UserId = userId;

            // 🔹 Mövcud kitabı tapırıq (update üçün)
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == model.Id && b.UserId == userId);

            // 🔹 Şəkil varsa, serverə yükləyirik
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // 🔹 Köhnə şəkili fiziki olaraq silmək
                if (existingBook != null && !string.IsNullOrEmpty(existingBook.ImagePath))
                {
                    // Query string-i təmizlə (?v=... hissəsini sil)
                    var oldImagePath = existingBook.ImagePath.Split('?')[0];
                    var oldImageFullPath = Path.Combine(_webHostEnvironment.WebRootPath, oldImagePath.TrimStart('/'));

                    if (System.IO.File.Exists(oldImageFullPath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldImageFullPath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error deleting old image: {ex.Message}");
                        }
                    }
                }

                // 🔹 Yeni şəkili yüklə
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                // Hər dəfə unikal fayl adı
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                // Cache-buster əlavə et
                var timestamp = DateTime.Now.Ticks;
                var newImagePath = $"/images/{fileName}?v={timestamp}";

                // 🔹 ImagePath-i set et
                if (existingBook != null)
                {
                    existingBook.ImagePath = newImagePath;
                    existingBook.Title = model.Title;
                    existingBook.Notes = model.Notes;
                    existingBook.Thoughts = model.Thoughts;
                    existingBook.Rating = model.Rating;

                    _context.Books.Update(existingBook);
                }
                else
                {
                    model.ImagePath = newImagePath;
                    _context.Books.Add(model);
                }
            }
            else
            {
                // Şəkil yüklənməyib, yalnız mətn məlumatlarını update et
                if (existingBook != null)
                {
                    existingBook.Title = model.Title;
                    existingBook.Notes = model.Notes;
                    existingBook.Thoughts = model.Thoughts;
                    existingBook.Rating = model.Rating;
                    // ImagePath toxunma, köhnəsi qalsın

                    _context.Books.Update(existingBook);
                }
                else
                {
                    // Yeni kitab, şəkil yoxdur
                    _context.Books.Add(model);
                }
            }

            await _context.SaveChangesAsync();

            // 🔹 Redirect edərkən cache-buster əlavə et
            return RedirectToAction("Page", new { t = DateTime.Now.Ticks });
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

        [HttpGet]
        public async Task<IActionResult> ViewBook(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                TempData["Error"] = "Kitab tapılmadı!";
                return RedirectToAction("Page");
            }

            return View(book);
        }


    }
}
