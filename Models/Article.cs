using System.ComponentModel.DataAnnotations;

namespace AHUWeb.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(500)]
        public string? Image { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}
