using System.Text.Json;
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
    public class ProductsController : AdminBaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        // Bảng màu cố định — y hệt COLOR_OPTIONS trong script.js gốc, dùng cho bộ chọn màu.
        public static readonly (string Name, string Hex)[] ColorPalette =
        {
            ("Black", "#000000"), ("White", "#FFFFFF"), ("Beige", "#F5F5DC"),
            ("Gold", "#D4AF37"), ("Navy", "#000080"), ("Red", "#FF0000"),
            ("Grey", "#808080"), ("Brown", "#8B4513"), ("Pink", "#FFC0CB")
        };
        public static readonly string[] SizeOptions = { "S", "M", "L", "XL", "XXL" };

        public ProductsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET /Admin/Products — thay cho renderAdminProducts() (tab "products")
        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q));

            ViewBag.CurrentQuery = q ?? "";
            var products = await query.OrderByDescending(p => p.Id).ToListAsync();
            return View(products);
        }

        // GET /Admin/Products/Create — thay cho openProductModal() không có id
        public IActionResult Create()
        {
            ViewBag.ColorPalette = ColorPalette;
            ViewBag.SizeOptions = SizeOptions;
            return View(new ProductFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormViewModel model)
        {
            Console.WriteLine("========== MODEL ==========");
            Console.WriteLine($"Name: [{model.Name}]");
            Console.WriteLine($"Price: [{model.Price}]");
            Console.WriteLine("===========================");
            foreach (var item in ModelState)
            {
                foreach (var error in item.Value.Errors)
                {
                    Console.WriteLine($"{item.Key}: {error.ErrorMessage}");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ColorPalette = ColorPalette;
                ViewBag.SizeOptions = SizeOptions;
                return View(model);
            }
            
            var product = new Product();
            await MapFormToProduct(model, product);

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = "Đã thêm sản phẩm mới";
            return RedirectToAction(nameof(Index));
        }

        // GET /Admin/Products/Edit/5 — thay cho openProductModal(id)
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            var model = new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                Discount = product.Discount,
                Type = product.Type,
                Description = product.Description,
                Stock = product.Stock,
                IsActive = product.IsActive,
                Featured = product.Featured,
                ImageUrl = product.Image,
                SelectedSizes = string.IsNullOrEmpty(product.SizesJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(product.SizesJson) ?? new(),
                SelectedColors = string.IsNullOrEmpty(product.ColorsJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(product.ColorsJson) ?? new()
            };

            ViewBag.ColorPalette = ColorPalette;
            ViewBag.SizeOptions = SizeOptions;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.ColorPalette = ColorPalette;
                ViewBag.SizeOptions = SizeOptions;
                return View(model);
            }

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            await MapFormToProduct(model, product);
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = "Đã cập nhật sản phẩm";
            return RedirectToAction(nameof(Index));
        }

        // POST /Admin/Products/Delete/5 — thay cho confirmDelete()
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã xóa sản phẩm";
            return RedirectToAction(nameof(Index));
        }

        // POST /Admin/Products/ToggleStatus/5 — thay cho toggleProductStatus()
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsActive = !product.IsActive;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST /Admin/Products/ToggleFeatured/5 — đánh dấu/bỏ đánh dấu "Sản phẩm nổi bật"
        // ngay từ danh sách, không cần mở form Sửa.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Featured = !product.Featured;
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = product.Featured ? "Đã đánh dấu sản phẩm nổi bật" : "Đã bỏ đánh dấu sản phẩm nổi bật";
            return RedirectToAction(nameof(Index));
        }

        private async Task MapFormToProduct(ProductFormViewModel model, Product product)
        {
            product.Name = model.Name;
            product.Price = model.Price;
            product.OriginalPrice = model.OriginalPrice;
            product.Discount = model.Discount;
            product.Type = model.Type;
            product.Description = model.Description;
            product.Stock = model.Stock;
            product.IsActive = model.IsActive;
            product.Featured = model.Featured;
            product.SizesJson = JsonSerializer.Serialize(model.SelectedSizes);
            product.ColorsJson = JsonSerializer.Serialize(model.SelectedColors);

            // Ảnh: hỗ trợ cả 2 kiểu như UI gốc (img-type-url-btn / img-type-file-btn)
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);
                product.Image = $"/images/products/{fileName}";
            }
            else if (!string.IsNullOrWhiteSpace(model.ImageUrl))
            {
                product.Image = model.ImageUrl;
            }
        }
    }
}
