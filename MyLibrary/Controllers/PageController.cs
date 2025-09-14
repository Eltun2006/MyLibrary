using Microsoft.AspNetCore.Mvc;

namespace MyLibrary.Controllers
{
    public class PageController : Controller
    {
        public IActionResult Page()
        {
            // Session-dan istifadəçi məlumatını oxu
            var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userName))
            {
                // Əgər login olmayıbsa, login səhifəsinə yönləndir
                return RedirectToAction("Login", "Account");
            }

            // Əgər login olubsa, adını view-da göstər
            ViewBag.UserName = userName;
            return View("Page");
        }
    }
}
