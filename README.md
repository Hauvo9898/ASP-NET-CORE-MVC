# AHU Web – ASP.NET Core MVC

Chuyển đổi từ site tĩnh HTML/CSS/JS sang ASP.NET Core MVC 8 + EF Core + SQL Server (Somee).

## 1. Yêu cầu môi trường

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- EF Core CLI tools (nếu chưa có):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## 2. Cấu hình kết nối Database

Mở `appsettings.json`, sửa `ConnectionStrings:DefaultConnection` bằng thông tin Somee thật của bạn:

```json
"DefaultConnection": "Server=TÊN_SERVER.somee.com;Database=sellitemQuanLy;User Id=TÊN_USER;Password=MẬT_KHẨU;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true;"
```

> Lưu ý Somee (gói free): thường chỉ cho **1 kết nối đồng thời** — nếu gặp lỗi timeout/kết nối, thử lại sau vài giây hoặc nâng gói. Bạn lấy đúng connection string trong trang quản lý database trên Somee (mục "Connection strings" → chọn "ADO.NET").

## 3. Khôi phục package

```bash
cd AHUWeb
dotnet restore
```

## 4. Tạo Migration đầu tiên

```bash
dotnet ef migrations add InitialCreate
```

Lệnh này sẽ tạo thư mục `Migrations/` chứa toàn bộ script tạo bảng dựa trên các Model (`Product`, `Article`, `User`, `Order`, `OrderDetail`, `Staff`, `Feedback`).

## 5. Áp dụng Migration lên Database (tạo bảng trên Somee)

```bash
dotnet ef database update
```

`Program.cs` cũng tự động gọi `Database.Migrate()` mỗi khi ứng dụng khởi động, nên nếu bạn quên bước này, lần chạy `dotnet run` đầu tiên vẫn sẽ tự tạo bảng — nhưng nên chạy thủ công trước để chắc chắn thấy lỗi kết nối sớm (nếu có) thay vì lẫn vào log ứng dụng.

## 6. Chạy ứng dụng

```bash
dotnet run
```

Mặc định chạy tại `https://localhost:5001` (hoặc cổng hiển thị trong console).

## 7. Tài khoản mặc định

Ứng dụng tự seed 1 tài khoản quản trị khi khởi động lần đầu (xem `Data/DbInitializer.cs`):

| Tài khoản | Mật khẩu |
|---|---|
| `admin` | `Admin@123` |

**Đổi mật khẩu này ngay sau khi đăng nhập lần đầu trên môi trường thật.**

Vào Cổng quản trị tại: `/Account/Login?mode=admin` hoặc bấm "Cổng quản trị hệ thống" ở footer.

## 8. Những thay đổi/mở rộng so với yêu cầu gốc (đã thông báo trong quá trình làm)

- **Model mở rộng** (nullable, không phá schema gốc bạn yêu cầu):
  - `Product`: + `Description`, `SizesJson`, `ColorsJson`, `OriginalPrice`, `Discount`, `Stock`, `IsActive` — để giữ đúng chức năng khuyến mãi/tồn kho/ẩn-hiện sản phẩm đã có trong admin gốc.
  - `User`: + `Email`, `FullName`.
  - `Feedback`: + `Topic`, `Phone`, `OrderRef`, `Rating` — giữ đúng form liên hệ gốc.
  - `Order.Status` lưu key tiếng Anh (`pending/processing/shipped/delivered/cancelled`) khớp với `getStatusLabel()` gốc.
- **"Tin tức" và "Nhân viên"**: không có sẵn trong `index.html`/`script.js` gốc bạn upload — đây là tính năng **mới**, được thêm vì nằm trong yêu cầu chuyển đổi ban đầu. Giao diện tái sử dụng đúng class CSS có sẵn (không sửa `style.css`).
- **Giỏ hàng**: chuyển từ `localStorage` sang **Session** (`ICartService`), trở thành `Order`/`OrderDetail` thật khi checkout.
- **Tài khoản do Admin tạo** (mục "Quản lý user"): form gốc không có ô mật khẩu (chỉ là demo phía JS, chưa từng đăng nhập được thật) → nay có backend thật nên được gán mật khẩu mặc định `123456` (hiện trong thông báo khi tạo).
- **Trang "Xuất hóa đơn"**: bản gốc xuất file `.txt` phía client bằng JS Blob. Trang Admin/Orders/Details hiện có thể `window.print()` để in vận đơn; nếu bạn cần xuất PDF/Word thật, có thể bổ sung sau bằng iTextSharp/QuestPDF.
- **Trang "Hồ sơ cá nhân" (profile)** trong bản gốc không nằm trong danh sách chức năng bạn liệt kê cho bản MVC này nên chưa được dựng riêng; thông tin cá nhân hiện hiển thị gọn trong trang "Lịch sử đơn hàng". Có thể bổ sung nếu cần.
- Không mang theo 2 tài khoản admin "cứng" (`admin`/`hauvo9898`) từng được hardcode trong `script.js` gốc — đây là lối tắt debug phía client, không an toàn khi có backend thật; phân quyền giờ hoàn toàn dựa trên cột `Role` trong database.

## 9. Cấu trúc thư mục

```
AHUWeb/
├── Controllers/          Home, Product, Cart, Order, Account, Contact, Article
├── Areas/Admin/           Toàn bộ khu vực quản trị (Controllers + Views riêng)
├── Models/                7 model theo yêu cầu + ViewModels/
├── Data/                  ApplicationDbContext, DbInitializer
├── Services/               ICartService (giỏ hàng qua Session)
├── Helpers/                FormatHelper (định dạng giá, trạng thái đơn hàng)
├── ViewComponents/         CartBadge (số lượng giỏ hàng trên navbar)
├── Views/                  Razor views trang khách hàng + Shared/_Layout
├── wwwroot/css/style.css   Giữ nguyên 100% từ bản gốc
├── wwwroot/js/site.js      JS thuần cho UI (menu, search, toast, quick view) — không còn localStorage
└── wwwroot/images/         h1–h4.jpg từ bản gốc
```
