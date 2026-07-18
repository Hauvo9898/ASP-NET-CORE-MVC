using AHUWeb.Data;
using AHUWeb.Models;
using AHUWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Controllers
{
    public class ProductController : Controller
    {
        private const int PageSize = 12;

        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Product?type=quần áo&q=áo&sort=price_asc&page=2
        // Thay cho getFilteredProducts() + renderProductsGrid() trong script.js.
        // Mặc định (type=all, không tìm/sắp xếp) vẫn giữ nguyên kiểu nhóm theo danh mục
        // như trước giờ — chỉ khi chọn 1 danh mục cụ thể, tìm kiếm, hoặc chọn sắp xếp thì
        // mới chuyển sang lưới phẳng có sắp xếp + phân trang (hợp lý hơn vì nhóm nhiều
        // danh mục cùng lúc mà phân trang sẽ rất rối).
        public async Task<IActionResult> Index(string? type, string? q, string? sort, int page = 1)
        {
            var normalizedType = string.IsNullOrWhiteSpace(type) ? "all" : type;
            bool grouped = normalizedType == "all" && string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(sort);

            if (grouped)
            {
                var all = await _db.Products.Where(p => p.IsActive).OrderByDescending(p => p.Id).ToListAsync();
                return View(new ProductListViewModel
                {
                    Products = all,
                    Grouped = true,
                    CurrentType = normalizedType,
                    CurrentQuery = q ?? "",
                    ActionName = nameof(Index)
                });
            }

            var query = BuildBaseQuery(type: normalizedType, q: q, featuredOnly: false);
            var vm = await BuildListViewModel(query, normalizedType, q, sort, page, nameof(Index));
            return View(vm);
        }

        // GET /Product/Featured?type=...&q=...&sort=...&page=...
        // Chỉ sản phẩm có Featured = true — tận dụng đúng field Admin đã đánh dấu sẵn,
        // không tạo cột/bảng nào mới. Luôn hiển thị dạng lưới phẳng có sắp xếp + phân trang.
        public async Task<IActionResult> Featured(string? type, string? q, string? sort, int page = 1)
        {
            var normalizedType = string.IsNullOrWhiteSpace(type) ? "all" : type;
            var query = BuildBaseQuery(type: normalizedType, q: q, featuredOnly: true);
            var vm = await BuildListViewModel(query, normalizedType, q, sort, page, nameof(Featured));
            vm.EmptyMessage = "Hiện chưa có sản phẩm nổi bật nào được đánh dấu.";
            return View(vm);
        }

        // GET /Product/Details/5 — thay cho loadProductDetail()
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET /Product/QuickView/5 — trả JSON để site.js dựng modal "Xem nhanh" không cần tải lại trang
        [HttpGet]
        public async Task<IActionResult> QuickView(int id)
        {
            var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
            if (p == null) return NotFound();

            var sizes = string.IsNullOrEmpty(p.SizesJson)
                ? new List<string>()
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.SizesJson);
            var colors = string.IsNullOrEmpty(p.ColorsJson)
                ? new List<string>()
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.ColorsJson);

            return Json(new
            {
                p.Id,
                p.Name,
                p.Price,
                p.OriginalPrice,
                p.Discount,
                p.Image,
                p.Type,
                p.Description,
                sizes,
                colors
            });
        }

        // GET /Product/SearchSuggestions?q=... — dùng cho ô tìm kiếm nổi (search-overlay),
        // trả JSON để site.js render gợi ý theo thời gian thực (thay cho handleSearchInput cũ
        // vốn lọc mảng State.products đã nạp sẵn trong bộ nhớ trình duyệt).
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 1)
                return Json(new List<object>());

            var results = await _db.Products
                .Where(p => p.IsActive && p.Name.Contains(q))
                .OrderByDescending(p => p.Id)
                .Take(6)
                .Select(p => new { p.Id, p.Name, p.Price, p.OriginalPrice, p.Discount, p.Image, p.Type })
                .ToListAsync();

            return Json(results);
        }

        private IQueryable<Product> BuildBaseQuery(string? type, string? q, bool featuredOnly)
        {
            var query = _db.Products.Where(p => p.IsActive).AsQueryable();

            if (featuredOnly)
                query = query.Where(p => p.Featured);

            if (!string.IsNullOrWhiteSpace(type) && type != "all")
                query = query.Where(p => p.Type == type);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q));

            return query;
        }

        private static IQueryable<Product> ApplySort(IQueryable<Product> query, string sort) => sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.Id) // "newest" (mặc định)
        };

        private async Task<ProductListViewModel> BuildListViewModel(
            IQueryable<Product> query, string type, string? q, string? sort, int page, string actionName)
        {
            var normalizedSort = string.IsNullOrWhiteSpace(sort) ? "newest" : sort;
            query = ApplySort(query, normalizedSort);

            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            var safePage = Math.Clamp(page, 1, totalPages);

            var items = await query.Skip((safePage - 1) * PageSize).Take(PageSize).ToListAsync();

            return new ProductListViewModel
            {
                Products = items,
                CurrentPage = safePage,
                TotalPages = totalPages,
                TotalCount = totalCount,
                CurrentSort = normalizedSort,
                CurrentQuery = q ?? "",
                CurrentType = type,
                ActionName = actionName
            };
        }
    }
}
