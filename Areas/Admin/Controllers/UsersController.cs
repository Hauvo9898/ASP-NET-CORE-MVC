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

        // Form "Thêm hội viên" gốc không có ô mật khẩu (chỉ demo phía JS, không đăng nhập thật
        // được). Vì giờ có backend thật, user do admin tạo sẽ có mật khẩu mặc định này —
        // nên đổi ngay sau lần đăng nhập đầu.
        public const string DefaultNewUserPassword = "123456";

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã xóa người dùng";
            return RedirectToAction(nameof(Index));
        }
    }
}
