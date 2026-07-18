using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AHUWeb.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalProductsSold { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    }

    public class AdminReportsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalProductsSold { get; set; }
        public int TotalUsers { get; set; }
        // Day label -> revenue that day, last 14 days, used for the simple bar chart
        public List<(string Label, decimal Revenue)> RevenueByDay { get; set; } = new();

        // Lab 14: doanh thu theo tung thang cua 1 nam, ve bang Chart.js
        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = new();
        public List<(string MonthLabel, decimal Revenue)> RevenueByMonth { get; set; } = new();
    }

    public class UserFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [StringLength(200)]
        public string? FullName { get; set; }

        [StringLength(200)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        public string Role { get; set; } = "customer";

        // Lab 08/09: gan tai khoan vao 1 Nhom quyen (null = khong gioi han quyen chi tiet)
        public int? GroupId { get; set; }
    }

    public class GroupFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhóm quyền")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Giá bán không hợp lệ")]
        public decimal Price { get; set; }

        public decimal? OriginalPrice { get; set; }

        [Range(0, 100, ErrorMessage = "Khuyến mãi từ 0-100%")]
        public int Discount { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
        public string Type { get; set; } = "quần áo";

        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;
        public bool Featured { get; set; }
        public bool IsComingSoon { get; set; }

        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

        public List<string> SelectedSizes { get; set; } = new();
        public List<string> SelectedColors { get; set; } = new();
    }
}
