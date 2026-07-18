namespace AHUWeb.Models.ViewModels
{
    // Dùng chung cho phần lưới sản phẩm có sắp xếp + phân trang — tái sử dụng ở cả
    // /Product/Index (khi lọc/tìm/sắp xếp cụ thể, không còn ở chế độ nhóm theo danh mục)
    // và /Product/Featured (luôn hiển thị kiểu này).
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new();

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }

        // "newest" | "price_asc" | "price_desc" | "name_asc"
        public string CurrentSort { get; set; } = "newest";

        public string? CurrentQuery { get; set; }
        public string CurrentType { get; set; } = "all";

        // Điều hướng phân trang/sắp xếp cần biết đang ở action nào ("Index" hay "Featured")
        // để sinh đúng link, vì 2 trang dùng chung 1 partial nhưng khác action.
        public string ActionName { get; set; } = "Index";

        public string EmptyMessage { get; set; } = "Không tìm thấy sản phẩm nào trong bộ sưu tập này.";

        // true = trang Shop đang ở chế độ mặc định (nhóm theo danh mục, không phân trang) —
        // chỉ dùng cho /Product/Index, /Product/Featured luôn để false.
        public bool Grouped { get; set; }
    }
}
