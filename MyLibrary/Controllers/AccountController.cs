using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLibrary.Models;
using MyLibrary.DAL;
using MyLibrary.Services;
using System.Security.Cryptography;

namespace MyLibrary.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApDbContext _context;
        private readonly IEmailService _emailService;

        // ✅ Constructor - EmailService inject et
        public AccountController(ApDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Account()
        {
            return View();
        }



        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Email-in real Gmail olub olmadığını yoxla
            if (!await _emailService.IsValidEmailAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Zəhmət olmasa real Gmail ünvanı daxil edin!");
                return View(model);
            }

            // Email artıq qeydiyyatdan keçibmi?
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Bu email artıq qeydiyyatdan keçib!");
                return View(model);
            }

            // Şifrəni hash-lə
            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);

            // Email verification token yarat
            model.EmailVerificationToken = GenerateToken();
            model.EmailVerificationTokenExpiry = DateTime.Now.AddHours(24);
            model.IsEmailVerified = false;

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            // Verification email göndər
            var verificationLink = Url.Action(
                "VerifyEmail",
                "Account",
                new { token = model.EmailVerificationToken },
                Request.Scheme
            );

            var emailBody = $@"
                <h2>Salam {model.Username} {model.LastName}!</h2>
                <p>MyLibrary-ə xoş gəlmisiniz! 📚</p>
                <p>Email ünvanınızı təsdiqləmək üçün aşağıdakı linkə klikləyin:</p>
                <a href='{verificationLink}' style='padding: 10px 20px; background: #4CAF50; color: white; text-decoration: none; border-radius: 5px;'>
                    Email-i Təsdiqlə
                </a>
                <p>Link 24 saat etibarlıdır.</p>
                <p>Əgər siz bu qeydiyyatı etməmisinizsə, bu emaili ignore edin.</p>
            ";

            try
            {
                await _emailService.SendEmailAsync(model.Email, "Email Təsdiqi - MyLibrary", emailBody);
                TempData["Success"] = "Qeydiyyat uğurlu! Email ünvanınıza təsdiq linki göndərildi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Email göndərilmədi: {ex.Message}";
            }

            return RedirectToAction("Login");
        }

        // ========== EMAIL VERIFICATION ==========
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
            {
                TempData["Error"] = "Yanlış və ya köhnə təsdiq linki!";
                return RedirectToAction("Login");
            }

            if (user.EmailVerificationTokenExpiry < DateTime.Now)
            {
                TempData["Error"] = "Təsdiq linkinin müddəti bitib! Yenidən qeydiyyatdan keçin.";
                return RedirectToAction("Register");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Email təsdiqləndi! İndi daxil ola bilərsiniz.";
            return RedirectToAction("Login");
        }

        // ========== LOGIN ==========
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Email və ya şifrə yanlışdır!");
                return View(model);
            }

            // Email təsdiqlənibmi?
            if (!user.IsEmailVerified)
            {
                TempData["Error"] = "Email ünvanınızı təsdiqləməlisiniz! Email-inizi yoxlayın.";
                return View(model);
            }

            // Şifrəni yoxla
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Email və ya şifrə yanlışdır!");
                return View(model);
            }

            // Session yarat
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", $"{user.Username} {user.LastName}");

            return RedirectToAction("Page", "Page");
        }

        // ========== FORGOT PASSWORD ==========
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Email daxil edin!";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                TempData["Success"] = "Əgər email sistemdə varsa, şifrə sıfırlama linki göndəriləcək.";
                return RedirectToAction("Login");
            }

            // Token yarat
            user.PasswordResetToken = GenerateToken();
            user.PasswordResetTokenExpiry = DateTime.Now.AddHours(1);

            await _context.SaveChangesAsync();

            // Email göndər
            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { token = user.PasswordResetToken },
                Request.Scheme
            );

            var emailBody = $@"
                <h2>Salam {user.Username}!</h2>
                <p>Şifrənizi sıfırlamaq üçün aşağıdakı linkə klikləyin:</p>
                <a href='{resetLink}' style='padding: 10px 20px; background: #f44336; color: white; text-decoration: none; border-radius: 5px;'>
                    Şifrəni Sıfırla
                </a>
                <p><strong>Diqqət:</strong> Link yalnız 1 saat etibarlıdır.</p>
                <p>Əgər siz bu sorğunu etməmisinizsə, bu emaili ignore edin.</p>
            ";

            try
            {
                await _emailService.SendEmailAsync(user.Email, "Şifrə Sıfırlama - MyLibrary", emailBody);
                TempData["Success"] = "Şifrə sıfırlama linki email ünvanınıza göndərildi!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Email göndərilmədi: {ex.Message}";
            }

            return RedirectToAction("Login");
        }

        // ========== RESET PASSWORD ==========
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token);

            if (user == null || user.PasswordResetTokenExpiry < DateTime.Now)
            {
                TempData["Error"] = "Yanlış və ya müddəti bitmiş link!";
                return RedirectToAction("Login");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Şifrələr uyğun gəlmir!");
                ViewBag.Token = token;
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token);

            if (user == null || user.PasswordResetTokenExpiry < DateTime.Now)
            {
                TempData["Error"] = "Yanlış və ya müddəti bitmiş link!";
                return RedirectToAction("Login");
            }

            // Yeni şifrəni set et
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Şifrə uğurla dəyişdirildi! İndi daxil ola bilərsiniz.";
            return RedirectToAction("Login");
        }

        // ========== HELPER METHOD ==========
        private string GenerateToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}