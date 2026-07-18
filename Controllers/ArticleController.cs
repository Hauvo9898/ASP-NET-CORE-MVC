using AHUWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Controllers
{
    // GHI CHÚ: "Tin tức" không có trong index.html/script.js gốc (site gốc chỉ có
    // home/auth/products/product-detail/cart/profile/order-history/admin/contact).
    // Đây là tính năng MỚI được thêm vì nằm trong yêu cầu chuyển đổi ban đầu của anh.
    // View sẽ dùng chung ngôn ngữ thiết kế (hero, gold-divider, luxury-button...) với các trang khác.
    public class ArticleController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ArticleController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Article?category=...
        public async Task<IActionResult> Index(string? category)
        {
            var query = _db.Articles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(category) && category != "all")
                query = query.Where(a => a.Category == category);

            ViewBag.CurrentCategory = category ?? "all";
            var articles = await query.OrderByDescending(a => a.Id).ToListAsync();
            return View(articles);
        }

        // GET /Article/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null) return NotFound();

            var related = await _db.Articles
                .Where(a => a.Id != id && a.Category == article.Category)
                .OrderByDescending(a => a.Id)
                .Take(3)
                .ToListAsync();

            // Chưa đủ 3 bài cùng chuyên mục thì lấy thêm bài mới nhất khác để lấp đầy
            if (related.Count < 3)
            {
                var excludeIds = related.Select(a => a.Id).Append(id).ToList();
                var fillers = await _db.Articles
                    .Where(a => !excludeIds.Contains(a.Id))
                    .OrderByDescending(a => a.Id)
                    .Take(3 - related.Count)
                    .ToListAsync();
                related.AddRange(fillers);
            }

            ViewBag.RelatedArticles = related;
            return View(article);
        }
    }
}
