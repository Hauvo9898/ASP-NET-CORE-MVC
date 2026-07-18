using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHUWeb.Models
{
    // Mỗi dòng là 1 tin nhắn trong 1 cuộc hội thoại (Feedback = cuộc hội thoại,
    // FeedbackMessage = từng tin nhắn qua lại giữa khách hàng và admin).
    public class FeedbackMessage
    {
        [Key]
        public int Id { get; set; }

        public int FeedbackId { get; set; }
        [ForeignKey(nameof(FeedbackId))]
        public Feedback? Feedback { get; set; }

        // "customer" | "admin"
        [Required, StringLength(20)]
        public string SenderRole { get; set; } = "customer";

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
