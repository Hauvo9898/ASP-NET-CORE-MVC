using System.Security.Claims;
using AHUWeb.Data;
using AHUWeb.Helpers;
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
        private readonly IWebHostEnvironment _env;

        public AccountController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
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

        // POST /Account/UploadAvatar — Lab 06: upload anh dai dien qua Ajax FormData + IFormFile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn ảnh." });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "avatars");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            // Xoa anh cu (neu co) de khong tich rac file trong wwwroot
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                var oldPath = Path.Combine(_env.WebRootPath, user.AvatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            user.AvatarUrl = $"/images/avatars/{fileName}";
            await _db.SaveChangesAsync();

            return Json(new { success = true, avatarUrl = user.AvatarUrl, message = "Đã cập nhật ảnh đại diện." });
        }

        // POST /Account/Logout — thay cho handleLogout()
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove(RequirePermissionAttribute.PermissionsSessionKey);
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

            // Lab 08: neu tai khoan da duoc gan vao 1 Nhom quyen, nap danh sach ma quyen
            // cua nhom do vao Session - song song voi Cookie Auth, khong thay the. Luon
            // xoa/ghi de ro rang (khong chi set khi co GroupId) de tranh ma quyen cua lan
            // dang nhap truoc (tai khoan khac, cung trinh duyet) con sot lai trong Session.
            if (user.GroupId.HasValue)
            {
                var codes = await _db.GroupPermissions
                    .Where(gp => gp.GroupId == user.GroupId.Value)
                    .Select(gp => gp.Permission!.Code)
                    .ToListAsync();
                HttpContext.Session.SetString(RequirePermissionAttribute.PermissionsSessionKey, string.Join(",", codes));
            }
            else
            {
                HttpContext.Session.Remove(RequirePermissionAttribute.PermissionsSessionKey);
            }
        }
    }
}
