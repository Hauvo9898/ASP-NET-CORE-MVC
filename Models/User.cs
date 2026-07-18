using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHUWeb.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;

        // Stores a BCrypt hash, never plain text — field name kept as "Password"
        // to match the requested schema, but AccountController always hashes on write
        // and verifies with BCrypt on login.
        [Required]
        public string Password { get; set; } = string.Empty;

        // "admin" | "customer"
        [Required, StringLength(20)]
        public string Role { get; set; } = "customer";

        // --- Optional extension used by the existing admin "Add member" modal (um-email) ---
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? FullName { get; set; }

        // Lab 06: anh dai dien, upload qua Ajax FormData (xem AccountController.UploadAvatar)
        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        // Lab 08: gan them vao 1 "Nhom quyen" (tuy chon) de gioi han quyen chi tiet hon
        // trong khu vuc admin - khong thay the cot Role (admin/customer) dang dung that.
        public int? GroupId { get; set; }
        [ForeignKey(nameof(GroupId))]
        public Group? Group { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
