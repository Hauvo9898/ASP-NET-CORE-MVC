using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHUWeb.Models
{
    public class Staff
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Phone { get; set; }

        [StringLength(50)]
        public string? Shift { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        // Ảnh nhân viên — thêm theo yêu cầu chọn ảnh từ máy. Field mới nên
        // cần chạy migration trước khi dùng (xem hướng dẫn kèm theo).
        [StringLength(500)]
        public string? Image { get; set; }
    }
}
