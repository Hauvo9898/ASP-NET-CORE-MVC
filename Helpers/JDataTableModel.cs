namespace AHUWeb.Helpers
{
    // Lab 04: tham số chuẩn của JQuery DataTables xử lý phía server (draw/start/length/search/order),
    // được client gửi lên dạng rút gọn (xem hàm `ajax.data` trong Groups/Index.cshtml) thay vì để
    // ASP.NET Core tự bind cấu trúc lồng nhau mặc định của DataTables.
    public class JDataTableRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; } = 10;
        public string? SearchValue { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
    }
}
