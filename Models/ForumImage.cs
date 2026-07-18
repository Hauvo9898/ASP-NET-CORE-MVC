using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHUWeb.Models
{
    public class ForumImage
    {
        [Key]
        public int Id { get; set; }

        public int ForumPostId { get; set; }
        [ForeignKey(nameof(ForumPostId))]
        public ForumPost? ForumPost { get; set; }

        [Required, StringLength(500)]
        public string ImagePath { get; set; } = string.Empty;
    }
}
