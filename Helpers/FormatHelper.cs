using System.Globalization;

namespace AHUWeb.Helpers
{
    public static class FormatHelper
    {
        // Thay cho formatPrice() trong script.js: làm tròn xuống, chấm phân cách hàng nghìn, hậu tố " VND"
        public static string FormatPrice(decimal price)
        {
            var amount = Math.Floor(price);
            return amount.ToString("#,##0", CultureInfo.InvariantCulture).Replace(",", ".") + " VND";
        }

        // Thay cho getStatusLabel() trong script.js
        public static string StatusLabel(string status) => status switch
        {
            "pending" => "Chờ xác nhận",
            "processing" => "Đã duyệt",
            "shipped" => "Đang giao",
            "delivered" => "Hoàn tất",
            "cancelled" => "Đã hủy",
            _ => status
        };

        // Thay cho getStatusBadgeClass() trong script.js — tên class khớp với style.css sẵn có
        public static string StatusBadgeClass(string status) => status switch
        {
            "pending" => "badge-pending",
            "processing" => "badge-processing",
            "shipped" => "badge-shipped",
            "delivered" => "badge-delivered",
            "cancelled" => "badge-cancelled",
            _ => "badge-pending"
        };

        // ===== Dùng cho trang Tin tức (Article) =====

        // Cắt gọn nội dung bài viết thành đoạn trích ngắn cho danh sách/thẻ bài viết,
        // luôn dừng ở ranh giới từ (không cắt giữa chữ) và không đè lên HTML.
        public static string Excerpt(string? content, int maxLength = 160)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;
            var plain = content.Trim();
            if (plain.Length <= maxLength) return plain;

            var cut = plain.Substring(0, maxLength);
            var lastSpace = cut.LastIndexOf(' ');
            if (lastSpace > 0) cut = cut.Substring(0, lastSpace);
            return cut.TrimEnd('.', ',', ';', ':', ' ') + "…";
        }

        // Ước tính thời gian đọc thật từ số từ trong bài (~200 từ/phút), tối thiểu 1 phút.
        public static int ReadingMinutes(string? content)
        {
            if (string.IsNullOrWhiteSpace(content)) return 1;
            var wordCount = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
            return Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));
        }

        // ===== Dùng cho Diễn đàn (Forum) — tái dùng đúng màu badge trạng thái đơn hàng
        // đã có sẵn (pending=vàng, delivered=xanh, cancelled=đỏ) để không thêm màu mới =====
        public static string ForumStatusLabel(string status) => status switch
        {
            "pending" => "Chờ duyệt",
            "approved" => "Đã duyệt",
            "rejected" => "Từ chối",
            _ => status
        };

        public static string ForumStatusBadgeClass(string status) => status switch
        {
            "pending" => "badge-pending",
            "approved" => "badge-delivered",
            "rejected" => "badge-cancelled",
            _ => "badge-pending"
        };

        // "like" | "love" | "haha" | "sad" | "angry"
        public static string ForumReactionEmoji(string type) => type switch
        {
            "like" => "👍",
            "love" => "❤️",
            "haha" => "😂",
            "sad" => "😢",
            "angry" => "😠",
            _ => "👍"
        };

        public static string ForumReactionLabel(string type) => type switch
        {
            "like" => "Thích",
            "love" => "Yêu thích",
            "haha" => "Haha",
            "sad" => "Buồn",
            "angry" => "Giận",
            _ => "Thích"
        };

        // ===== Lab 13: thuế + phí vận chuyển cho hóa đơn — chỉ tính ở tầng hiển thị,
        // KHÔNG đổi cột Order.Total (đang là doanh thu gốc, Dashboard/Reports dùng trực tiếp) =====
        public const decimal TaxRate = 0.10m;
        public const decimal ShippingFee = 30000m;

        public static decimal CalculateTax(decimal subtotal) => Math.Round(subtotal * TaxRate);

        public static decimal CalculateGrandTotal(decimal subtotal) => subtotal + CalculateTax(subtotal) + ShippingFee;
    }
}
