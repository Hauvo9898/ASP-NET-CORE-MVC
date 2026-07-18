using AHUWeb.Data;
using AHUWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Areas.Admin.Controllers
{
    // Lab 10: quan ly danh muc cha-con (Category, xem Lab 01). Form dang classic (giong
    // Articles/Users) - khong dung Ajax de giu rui ro thap, entity moi hoan toan nen
    // khong dung cham gi den Product/Article dang chay.
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class CategoriesController : AdminBaseController
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Admin/Categories?parentId=5 - danh sach danh muc cha (parentId null) hoac
        // danh muc con cua 1 danh muc cha (bam vao xem con - dung yeu cau Lab 10)
        public async Task<IActionResult> Index(int? parentId)
        {
            var items = await _db.Categories
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.ParentId = parentId;
            ViewBag.Parent = parentId.HasValue ? await _db.Categories.FindAsync(parentId.Value) : null;
            return View(items);
        }

        public IActionResult Create(int? parentId)
        {
            return View(new Category { ParentId = parentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);

            model.CreatedBy = User.Identity?.Name;
            model.CreatedOn = DateTime.Now;
            _db.Categories.Add(model);
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = "Đã thêm danh mục";
            return RedirectToAction(nameof(Index), new { parentId = model.ParentId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.Name = model.Name;
            category.ParentId = model.ParentId;
            category.MetaKeyword = model.MetaKeyword;
            category.MetaDescription = model.MetaDescription;
            category.ModifiedBy = User.Identity?.Name;
            category.ModifiedOn = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = "Đã cập nhật danh mục";
            return RedirectToAction(nameof(Index), new { parentId = category.ParentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var hasChildren = await _db.Categories.AnyAsync(c => c.ParentId == id);
            if (hasChildren)
            {
                TempData["ToastMessage"] = "Không thể xóa: danh mục này đang có danh mục con.";
                return RedirectToAction(nameof(Index), new { parentId = category.ParentId });
            }

            var parentId = category.ParentId;
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = "Đã xóa danh mục";
            return RedirectToAction(nameof(Index), new { parentId });
        }
    }
}
