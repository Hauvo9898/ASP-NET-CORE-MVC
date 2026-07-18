using AHUWeb.Data;
using AHUWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class StaffsController : AdminBaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public StaffsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.Staffs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(s => s.Name.Contains(q));

            ViewBag.CurrentQuery = q ?? "";
            return View(await query.OrderByDescending(s => s.Id).ToListAsync());
        }

        public IActionResult Create() => View(new Staff());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Staff model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(model);
            await ApplyUploadedImage(model, imageFile);
            _db.Staffs.Add(model);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã thêm nhân viên";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _db.Staffs.FindAsync(id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Staff model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            await ApplyUploadedImage(model, imageFile);
            _db.Update(model);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã cập nhật nhân viên";
            return RedirectToAction(nameof(Index));
        }

        // Nếu chọn ảnh mới từ máy thì lưu file và cập nhật đường dẫn; nếu không
        // chọn thì giữ nguyên ảnh cũ (model.Image giữ giá trị hidden field cũ).
        private async Task ApplyUploadedImage(Staff model, IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0) return;

            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "staff");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);
            model.Image = $"/images/staff/{fileName}";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var staff = await _db.Staffs.FindAsync(id);
            if (staff == null) return NotFound();
            _db.Staffs.Remove(staff);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã xóa nhân viên";
            return RedirectToAction(nameof(Index));
        }
    }
}
