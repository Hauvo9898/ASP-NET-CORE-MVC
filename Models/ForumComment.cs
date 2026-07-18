using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHUWeb.Models
{
    public class ForumComment
    {
        [Key]
        public int Id { get; set; }

        public int ForumPostId { get; set; }
        [ForeignKey(nameof(ForumPostId))]
        public ForumPost? ForumPost { get; set; }

        public int? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
