using AHUWeb.Data;
using AHUWeb.Models;
using AHUWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class UsersController : AdminBaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        // Form "Thêm hội viên" gốc không có ô mật khẩu (chỉ demo phía JS, không đăng nhập thật
        // được). Vì giờ có backend thật, user do admin tạo sẽ có mật khẩu mặc định này —
        // nên đổi ngay sau lần đăng nhập đầu.
        public const string DefaultNewUserPassword = "123456";

        public UsersController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET /Admin/Users — thay cho renderAdminUsers()
        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(u => u.Username.Contains(q) || (u.Email != null && u.Email.Contains(q)));

            ViewBag.CurrentQuery = q ?? "";
            return View(await query.OrderByDescending(u => u.Id).ToListAsync());
        }

        // GET /Admin/Users/Create — thay cho openUserModal() không id
        public IActionResult Create() => View(new UserFormViewModel { Role = "customer" });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (await _db.Users.AnyAsync(u => u.Username == model.Username))
                ModelState.AddModelError(nameof(model.Username), "Tên tài khoản đã tồn tại.");

            if (!ModelState.IsValid) return View(model);

            var user = new User
            {
                Username = model.Username,
                FullName = model.FullName,
                Email = model.Email,
                Role = model.Role,
                Password = BCrypt.Net.BCrypt.HashPassword(DefaultNewUserPassword)
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = $"Đã tạo tài khoản. Mật khẩu mặc định: {DefaultNewUserPassword}";
            return RedirectToAction(nameof(Index));
        }

        // GET /Admin/Users/Edit/5 — thay cho openUserModal(userId)
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var model = new UserFormViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserFormViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Role = model.Role;
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = "Đã cập nhật người dùng";
            return RedirectToAction(nameof(Index));
        }

        // POST /Admin/Users/Delete/5 — thay cho deleteUser()
        // Lab 06: chặn xóa nếu thành viên đã có đơn hàng (tương đương ràng buộc "đã viết bài
        // thì không được xóa" của giáo trình gốc — Order là quan hệ phụ thuộc thật đang có),
        // và dọn file ảnh đại diện vật lý khi xóa thành công.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var hasOrders = await _db.Orders.AnyAsync(o => o.UserId == id);
            if (hasOrders)
            {
                TempData["ToastMessage"] = "Không thể xóa: người dùng này đã có đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                var avatarPath = Path.Combine(_env.WebRootPath, user.AvatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(avatarPath)) System.IO.File.Delete(avatarPath);
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã xóa người dùng";
            return RedirectToAction(nameof(Index));
        }
    }
}
