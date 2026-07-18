using System.Security.Claims;
using AHUWeb.Data;
using AHUWeb.Models;
using AHUWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Account/Login — trang có 2 tab: "Dành cho Khách hàng" / "Cổng Quản trị"
        // thay cho page-auth + switchAuthMode() trong index.html/script.js
        [HttpGet]
        public IActionResult Login(string mode = "customer", string? returnUrl = null)
        {
            return View(new LoginViewModel { Mode = mode == "admin" ? "admin" : "customer", ReturnUrl = returnUrl });
        }

        // POST /Account/Login — dùng chung cho cả 2 tab; Mode quyết định điều kiện Role.
        // Thay cho handleCustomerAuth() / handleAdminAuth() / loginLocal()
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError(string.Empty, "Tên tài khoản hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (model.Mode == "admin" && user.Role != "admin")
            {
                ModelState.AddModelError(string.Empty, "Tài khoản này không có quyền quản trị.");
                return View(model);
            }

            await SignInUser(user);

            if (user.Role == "admin")
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        // GET /Account/Register — thay cho toggleAuthMode() (form đăng ký cùng trang Auth)
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST /Account/Register — thay cho registerLocal()
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _db.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "Tên tài khoản đã tồn tại.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                FullName = model.FullName,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "customer"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            await SignInUser(user);
            return RedirectToAction("Index", "Home");
        }

        // GET /Account/Profile — đích đến của mục "Thông tin tài khoản" trong dropdown navbar.
        // Trang chỉ xem (read-only); trước đây menu này chưa tồn tại nên chưa có action nào trỏ tới đây.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST /Account/Logout — thay cho handleLogout()
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role),
                new("FullName", user.FullName ?? user.Username)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}
