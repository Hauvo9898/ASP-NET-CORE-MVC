using AHUWeb.Data;
using AHUWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class ArticlesController : AdminBaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ArticlesController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.Articles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(a => a.Title.Contains(q));

            ViewBag.CurrentQuery = q ?? "";
            return View(await query.OrderByDescending(a => a.Id).ToListAsync());
        }

        public IActionResult Create() => View(new Article());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Article model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(model);
            await ApplyUploadedImage(model, imageFile);
            _db.Articles.Add(model);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã thêm bài viết";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var article = await _db.Articles.FindAsync(id);
            if (article == null) return NotFound();
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Article model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            await ApplyUploadedImage(model, imageFile);
            _db.Update(model);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã cập nhật bài viết";
            return RedirectToAction(nameof(Index));
        }

        // Nếu admin chọn ảnh từ máy (imageFile) thì lưu file và dùng đường dẫn đó,
        // ưu tiên hơn ô URL. Nếu không chọn file, giữ nguyên giá trị ô URL (model.Image)
        // — kể cả khi sửa bài mà không đổi ảnh, ảnh cũ vẫn được giữ nguyên.
        private async Task ApplyUploadedImage(Article model, IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0) return;

            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "articles");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);
            model.Image = $"/images/articles/{fileName}";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _db.Articles.FindAsync(id);
            if (article == null) return NotFound();
            _db.Articles.Remove(article);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã xóa bài viết";
            return RedirectToAction(nameof(Index));
        }
    }
}
