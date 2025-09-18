using Microsoft.AspNetCore.Mvc;

namespace MyLibrary.Controllers
{
    public class PageController : Controller
    {
        public IActionResult Page()
        {
            var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Account");
            }

            
            ViewBag.UserName = userName;
            return View("Page");
        }
    }
}
