using System.Security.Claims;
using AHUWeb.Data;
using AHUWeb.Models;
using AHUWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Controllers
{
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ForumController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET /Forum — trang chủ diễn đàn, feed công khai: chỉ bài "approved", bài ghim lên đầu.
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var posts = await _db.ForumPosts
                .Include(p => p.Images)
                .Include(p => p.User)
                .Include(p => p.Reactions)
                .Include(p => p.Comments)
                .Where(p => p.Status == "approved")
                .OrderByDescending(p => p.IsPinned)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(posts);
        }

        // GET /Forum/Details/5 — chi tiết 1 bài (chỉ xem được nếu đã duyệt, trừ khi là chủ bài).
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _db.ForumPosts
                .Include(p => p.Images)
                .Include(p => p.User)
                .Include(p => p.Reactions)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();
            if (!CanView(post)) return NotFound();

            if (User.Identity?.IsAuthenticated == true)
            {
                ViewBag.MyReaction = post.Reactions
                    .FirstOrDefault(r => r.UserId == CurrentUserId)?.ReactionType;
            }

            return View(post);
        }

        // POST /Forum/React — thả/đổi/bỏ reaction. Mỗi user chỉ 1 reaction/bài (unique index
        // ở DB); bấm lại đúng loại đang chọn thì coi như bỏ reaction (giống Facebook).
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> React(int postId, string reactionType)
        {
            var post = await _db.ForumPosts.FindAsync(postId);
            if (post == null || !CanView(post)) return NotFound();

            var existing = await _db.ForumReactions
                .FirstOrDefaultAsync(r => r.ForumPostId == postId && r.UserId == CurrentUserId);

            string? myReaction = reactionType;

            if (existing == null)
            {
                _db.ForumReactions.Add(new ForumReaction
                {
                    ForumPostId = postId,
                    UserId = CurrentUserId,
                    ReactionType = reactionType,
                    CreatedAt = DateTime.Now
                });
            }
            else if (existing.ReactionType == reactionType)
            {
                _db.ForumReactions.Remove(existing);
                myReaction = null;
            }
            else
            {
                existing.ReactionType = reactionType;
                existing.CreatedAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var total = await _db.ForumReactions.CountAsync(r => r.ForumPostId == postId);
                var topTypes = await _db.ForumReactions
                    .Where(r => r.ForumPostId == postId)
                    .GroupBy(r => r.ReactionType)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .Take(3)
                    .ToListAsync();

                return Json(new { success = true, myReaction, total, topTypes });
            }

            return RedirectToAction(nameof(Details), new { id = postId });
        }

        // POST /Forum/Comment — bình luận phẳng, không lồng nhau. Admin bình luận cũng dùng
        // chung action này (vai trò "AHU" suy ra từ User.Role lúc hiển thị, xem Pha 5).
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(int postId, string content)
        {
            var post = await _db.ForumPosts.FindAsync(postId);
            if (post == null || !CanView(post)) return NotFound();

            if (!string.IsNullOrWhiteSpace(content))
            {
                _db.ForumComments.Add(new ForumComment
                {
                    ForumPostId = postId,
                    UserId = CurrentUserId,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now
                });
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = postId });
        }

        // Chỉ xem/react/bình luận được nếu bài đã duyệt, hoặc mình là chủ bài, hoặc là admin.
        private bool CanView(ForumPost post)
        {
            if (post.Status == "approved") return true;
            if (User.IsInRole("admin")) return true;
            return User.Identity?.IsAuthenticated == true
                && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)
                && uid == post.UserId;
        }

        // GET /Forum/Create — form đăng bài (nút "Đăng bài" nằm trên trang chủ diễn đàn)
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ForumPostFormViewModel());
        }

        // POST /Forum/Create — bài luôn tạo ở trạng thái "pending", chỉ Admin duyệt xong
        // (Pha 5) mới hiện công khai ở feed (Pha 3).
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumPostFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var post = new ForumPost
            {
                UserId = CurrentUserId,
                Content = model.Content.Trim(),
                Status = "pending",
                CreatedAt = DateTime.Now
            };

            if (model.ImageFiles != null && model.ImageFiles.Count > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "images", "forum");
                Directory.CreateDirectory(uploadsDir);

                foreach (var file in model.ImageFiles)
                {
                    if (file.Length == 0) continue;
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    post.Images.Add(new ForumImage { ImagePath = $"/images/forum/{fileName}" });
                }
            }

            _db.ForumPosts.Add(post);
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = "Đã gửi bài viết — đang chờ Admin duyệt";
            return RedirectToAction(nameof(MyPosts));
        }

        // GET /Forum/MyPosts — "trang chờ duyệt": khách tự theo dõi trạng thái bài mình đã đăng
        // (pending/approved/rejected). Bài "pending"/"rejected" chưa hiện công khai nên đây là
        // chỗ duy nhất khách thấy được bài của chính mình trước khi được duyệt.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyPosts()
        {
            var posts = await _db.ForumPosts
                .Include(p => p.Images)
                .Where(p => p.UserId == CurrentUserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(posts);
        }
    }
}
