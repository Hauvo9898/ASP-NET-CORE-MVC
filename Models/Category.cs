using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHUWeb.Models
{
    // Lab 01 + Lab 10: danh mục cha-con độc lập với Product.Type (chuỗi phẳng hiện có) —
    // giữ nguyên Product.Type để không phá vỡ bộ lọc/tìm kiếm sản phẩm đang chạy.
    public class Category : IAuditable, IMeta
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public Category? Parent { get; set; }

        public ICollection<Category> Children { get; set; } = new List<Category>();

        public string? MetaKeyword { get; set; }
        public string? MetaDescription { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
