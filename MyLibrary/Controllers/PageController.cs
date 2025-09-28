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

        [HttpPost]

        [HttpPost]
        public async Task<IActionResult> AddBook(Book model, IFormFile ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await ImageFile.CopyToAsync(ms);
                model.ImageData = ms.ToArray();
            }
            else
            {
                model.ImageData = null; // şəkil yoxdursa null saxla
            }

            _context.Books.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyLibrary");
        }



        public IActionResult Page()
        {
            var currentUser = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);

            if (currentUser != null)
            {
                var myBooks = _context.Books
                    .Where(b => b.UserId == currentUser.Id)
                    .ToList();

                return View(myBooks);
            }

            var userName = HttpContext.Session.GetString("UserName");

            ViewBag.UserName = userName;
            return View(new List<Book>());
        }


    }
}
