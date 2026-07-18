using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHUWeb.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        // --- Optional extensions: the real contact form (index.html #page-contact) already
        // collects a topic, a star rating, an optional phone, and an optional order reference.
        // Kept nullable so they don't break the requested 5-field schema.
        [StringLength(30)]
        public string? Topic { get; set; } // gop-y | ho-tro | hop-tac | khac

        [StringLength(30)]
        public string? Phone { get; set; }

        [StringLength(50)]
        public string? OrderRef { get; set; }

        public int? Rating { get; set; } // 1-5

        // --- Admin reply (mới) ---
        // Ai gửi lúc đang đăng nhập thì lưu lại UserId, để họ xem được phản hồi
        // trong trang "Tin nhắn của tôi". Khách gửi ẩn danh (không đăng nhập) vẫn
        // gửi được như cũ, chỉ là không có nơi để xem lại phản hồi trong web.
        public int? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        // Được thay thế bởi Messages (chat 2 chiều) — giữ lại 2 cột này để không mất
        // dữ liệu các phản hồi cũ đã lưu trước đó; migration mới sẽ tự chuyển nội dung
        // cũ ở đây thành tin nhắn đầu tiên trong Messages. Code mới không đọc/ghi 2 cột
        // này nữa, chỉ còn lưu lại làm lịch sử.
        public string? AdminReply { get; set; }
        public DateTime? RepliedAt { get; set; }

        // Toàn bộ cuộc trò chuyện (tin nhắn gốc của khách + các lượt nhắn qua lại sau đó)
        public ICollection<FeedbackMessage> Messages { get; set; } = new List<FeedbackMessage>();
    }
}
