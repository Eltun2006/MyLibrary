using Microsoft.AspNetCore.Mvc;

namespace MyLibrary.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "true")
            {
                // Əgər admin deyilsə login səhifəsinə yönləndir
                return RedirectToAction("Login", "Account");
            }

            return View(); // admin panel view
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

    }
}
