using AHUWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Trang chủ: hero banner + 3 thẻ bộ sưu tập (quần áo/phụ kiện/giày dép) — giữ nguyên
        // như cũ. Bổ sung thêm khối "Sản phẩm nổi bật" lấy đúng các sản phẩm Admin đã đánh dấu
        // Featured = true (tối đa 8, mới nhất trước).
        public async Task<IActionResult> Index()
        {
            ViewData["IsHome"] = true;
            var featured = await _db.Products
                .Where(p => p.IsActive && p.Featured)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();
            return View(featured);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
