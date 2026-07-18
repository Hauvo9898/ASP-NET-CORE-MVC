using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHUWeb.Models
{
    public class ForumPost
    {
        [Key]
        public int Id { get; set; }

        // Null nếu user đăng bài sau đó bị xóa — vẫn giữ bài, không mất dữ liệu
        // (giống cách Order/Feedback đang xử lý UserId).
        public int? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        // "pending" | "approved" | "rejected"
        [Required, StringLength(20)]
        public string Status { get; set; } = "pending";

        public bool IsPinned { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ForumImage> Images { get; set; } = new List<ForumImage>();
        public ICollection<ForumReaction> Reactions { get; set; } = new List<ForumReaction>();
        public ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();
    }
}
