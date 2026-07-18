using AHUWeb.Data;
using AHUWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class FeedbacksController : AdminBaseController
    {
        private readonly ApplicationDbContext _db;

        public FeedbacksController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Admin/Feedbacks — thay cho tab "contacts" (acmFilter/acmSearch)
        public async Task<IActionResult> Index(string filter = "all", string? q = null)
        {
            var query = _db.Feedbacks.Include(f => f.Messages).AsQueryable();

            if (filter == "unreplied")
                query = query.Where(f => !f.Messages.Any(m => m.SenderRole == "admin"));
            else if (!string.IsNullOrWhiteSpace(filter) && filter != "all")
                query = query.Where(f => f.Topic == filter);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(f => f.Name.Contains(q) || f.Email.Contains(q) || f.Content.Contains(q));

            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentQuery = q ?? "";

            return View(await query.OrderByDescending(f => f.Date).ToListAsync());
        }

        // GET /Admin/Feedbacks/Thread/5 — mở cuộc hội thoại dạng chat để admin trả lời.
        public async Task<IActionResult> Thread(int id)
        {
            var thread = await _db.Feedbacks
                .Include(f => f.Messages)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (thread == null) return NotFound();
            return View(thread);
        }

        // POST /Admin/Feedbacks/SendMessage — admin nhắn tiếp vào cuộc hội thoại.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int id, string content)
        {
            var thread = await _db.Feedbacks.FirstOrDefaultAsync(f => f.Id == id);
            if (thread == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(content))
            {
                _db.FeedbackMessages.Add(new FeedbackMessage
                {
                    FeedbackId = id,
                    SenderRole = "admin",
                    Content = content.Trim(),
                    SentAt = DateTime.Now
                });
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Thread), new { id });
        }

        // POST /Admin/Feedbacks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var fb = await _db.Feedbacks.FindAsync(id);
            if (fb == null) return NotFound();
            _db.Feedbacks.Remove(fb);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã xóa cuộc hội thoại";
            return RedirectToAction(nameof(Index));
        }

        // POST /Admin/Feedbacks/ClearAll — thay cho clearAllContacts()
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            _db.Feedbacks.RemoveRange(_db.Feedbacks);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Đã xóa tất cả tin nhắn";
            return RedirectToAction(nameof(Index));
        }
    }
}
