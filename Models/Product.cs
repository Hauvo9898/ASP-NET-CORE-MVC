using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHUWeb.Models
{
    public class Product : IAuditable
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // "quần áo" | "phụ kiện" | "giày dép" ...
        [Required, StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Image { get; set; }

        public bool Featured { get; set; } = false;

        // --- Optional extensions kept nullable/defaulted so they don't break the requested schema ---
        // Needed to preserve the size/color variant pickers and description field
        // already present in the current AHU admin UI (index.html: pm-desc, size/color selectors).
        public string? Description { get; set; }

        // The real admin product table already has "Khuyến mãi" (promo), "Tồn kho" (stock)
        // and a "Trạng thái" active/inactive toggle (toggleProductStatus in script.js).
        // Without these, that existing functionality would be lost during the conversion.
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        public int Discount { get; set; } = 0; // percent, 0 = no promo

        public int Stock { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Lab 15: hien thi o khoi "Sap ra mat" tren trang chu (tai dung _ProductCard.cshtml)
        public bool IsComingSoon { get; set; } = false;

        // Stored as comma-separated or JSON text, e.g. "S,M,L,XL"
        public string? SizesJson { get; set; }

        // Stored as JSON text, e.g. [{"name":"Đen","hex":"#000000"}]
        public string? ColorsJson { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // Lab 11: gan tu Session/Claims nguoi dang nhap khi Them/Sua (xem ProductsController)
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
