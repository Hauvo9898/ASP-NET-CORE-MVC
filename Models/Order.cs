using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHUWeb.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // Nullable so guest checkout is still possible; FK to User
        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Address { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        // Stored as the English status KEY (matches the original script.js getStatusLabel map),
        // not the Vietnamese label, so display text can change without a data migration:
        // pending | processing | shipped | delivered | cancelled
        [Required, StringLength(30)]
        public string Status { get; set; } = "pending";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
