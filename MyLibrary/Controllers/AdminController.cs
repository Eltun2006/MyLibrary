using Microsoft.AspNetCore.Mvc;
using MyLibrary.Repository;

namespace MyLibrary.Controllers
{
    public class AdminController : Controller
    {
        private UserRepo _repo;

        public AdminController(IConfiguration configuration)
        {
            _repo = new UserRepo(configuration);   
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

    }
}
