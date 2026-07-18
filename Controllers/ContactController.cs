using System.Security.Claims;
using AHUWeb.Data;
using AHUWeb.Models;
using AHUWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ContactController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Contact — thay cho page-contact (form liên hệ). Việc "Xem phản hồi khách hàng"
        // của admin được đặt riêng tại /Admin/Feedbacks theo đúng mục Admin trong yêu cầu.
        [HttpGet]
        public IActionResult Index()
        {
            return View(new ContactFormViewModel());
        }

        // POST /Contact — thay cho submitContact(). Tin nhắn gốc khách gửi giờ cũng
        // là tin nhắn đầu tiên trong cuộc hội thoại (Messages), để có thể nhắn qua lại tiếp.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var feedback = new Feedback
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Topic = model.Topic,
                OrderRef = model.OrderRef,
                Rating = model.Rating,
                Content = model.Content,
                Date = DateTime.Now
            };

            // Nếu đang đăng nhập lúc gửi thì gắn UserId, để họ xem/nhắn tiếp được
            // trong trang "Tin nhắn của tôi" sau này.
            if (User.Identity?.IsAuthenticated == true)
            {
                feedback.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            }

            feedback.Messages.Add(new FeedbackMessage
            {
                SenderRole = "customer",
                Content = model.Content,
                SentAt = feedback.Date
            });

            _db.Feedbacks.Add(feedback);
            await _db.SaveChangesAsync();

            ViewBag.SubmitSuccess = true;
            return View(new ContactFormViewModel());
        }

        // GET /Contact/MyMessages — danh sách các cuộc hội thoại đã gửi (hộp thư).
        // Chỉ áp dụng cho tin nhắn gửi lúc đã đăng nhập (khách gửi ẩn danh sẽ không thấy ở đây).
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyMessages()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var threads = await _db.Feedbacks
                .Include(f => f.Messages)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.Messages.Max(m => (DateTime?)m.SentAt) ?? f.Date)
                .ToListAsync();
            return View(threads);
        }

        // GET /Contact/Thread/5 — mở 1 cuộc hội thoại dạng chat, chỉ chủ tin nhắn xem được.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Thread(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var thread = await _db.Feedbacks
                .Include(f => f.Messages)
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (thread == null) return NotFound();
            return View(thread);
        }

        // POST /Contact/SendMessage — khách nhắn tiếp vào đúng cuộc hội thoại của mình.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int id, string content)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var thread = await _db.Feedbacks.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
            if (thread == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(content))
            {
                _db.FeedbackMessages.Add(new FeedbackMessage
                {
                    FeedbackId = id,
                    SenderRole = "customer",
                    Content = content.Trim(),
                    SentAt = DateTime.Now
                });
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Thread), new { id });
        }
    }
}
