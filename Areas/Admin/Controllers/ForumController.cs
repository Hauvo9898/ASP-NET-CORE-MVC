using AHUWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class ForumController : AdminBaseController
    {
        private readonly ApplicationDbContext _db;

        public ForumController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Admin/Forum — hàng chờ duyệt (mặc định lọc "pending" vì đây là việc cần làm nhất)
        public async Task<IActionResult> Index(string filter = "pending")
        {
            var query = _db.ForumPosts
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter) && filter != "all")
                query = query.Where(p => p.Status == filter);

            ViewBag.CurrentFilter = filter;
            ViewBag.PendingCount = await _db.ForumPosts.CountAsync(p => p.Status == "pending");

            var posts = await query
                .OrderByDescending(p => p.IsPinned)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string filter = "pending")
        {
            var post = await _db.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();
            post.Status = "approved";
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã duyệt bài viết";
            return RedirectToAction(nameof(Index), new { filter });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string filter = "pending")
        {
            var post = await _db.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();
            post.Status = "rejected";
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã từ chối bài viết";
            return RedirectToAction(nameof(Index), new { filter });
        }

        // Ghim/bỏ ghim — chỉ nên ghim bài đã duyệt, nhưng không chặn cứng ở đây để đơn giản;
        // trang chủ diễn đàn (Pha 3) chỉ hiện bài "approved" nên ghim bài pending cũng vô hại,
        // chỉ là chưa hiện ra ngoài cho tới khi được duyệt.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePin(int id, string filter = "pending")
        {
            var post = await _db.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();
            post.IsPinned = !post.IsPinned;
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = post.IsPinned ? "Đã ghim bài viết" : "Đã bỏ ghim bài viết";
            return RedirectToAction(nameof(Index), new { filter });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string filter = "pending")
        {
            var post = await _db.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();
            _db.ForumPosts.Remove(post);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã xóa bài viết";
            return RedirectToAction(nameof(Index), new { filter });
        }
    }
}
