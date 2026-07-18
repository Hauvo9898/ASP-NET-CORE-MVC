using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHUWeb.Models
{
    public class ForumReaction
    {
        [Key]
        public int Id { get; set; }

        public int ForumPostId { get; set; }
        [ForeignKey(nameof(ForumPostId))]
        public ForumPost? ForumPost { get; set; }

        // Bắt buộc phải có User (không cho khách ẩn danh react) — nếu user bị xóa
        // thì reaction xóa theo luôn (khác Post/Comment, vì 1 lượt react đứng riêng
        // lẻ không còn ý nghĩa nếu không gắn với ai).
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        // "like" | "love" | "haha" | "sad" | "angry"
        [Required, StringLength(20)]
        public string ReactionType { get; set; } = "like";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
