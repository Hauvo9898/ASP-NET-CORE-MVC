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

## 10. Đối chiếu 17 Lab (thi giữa kỳ lần 2)

Dự án giữ nguyên stack đang chạy ổn định (int Id, BCrypt, Cookie Authentication) thay vì đổi
sang Guid/MD5/Session như giáo trình gốc minh họa — mỗi lab dưới đây được làm **tương đương
tinh thần**, không phá vỡ chức năng hiện có. Xem chi tiết từng lab qua tag Git tương ứng
(`git tag`, hoặc `git show lab-XX`).

| Lab | Nội dung | Bằng chứng |
|---|---|---|
| 01 | Model Code-First, chuẩn hóa `IAuditable`/`IMeta`, danh mục cha-con, nhóm quyền | `Models/IAuditable.cs`, `Models/IMeta.cs`, `Models/Category.cs`, `Models/Group.cs`, `Models/Permission.cs`, `Models/GroupPermission.cs` — tag `lab-01` |
| 02 | Kết nối SQL Server (EF Core + Migration) và Area Admin | `Data/ApplicationDbContext.cs`, `appsettings.json` (`ConnectionStrings:DefaultConnection`), `Migrations/`, `Areas/Admin/`, routing area tại `Program.cs` (`MapControllerRoute("areas", "{area:exists}/...")`) — tương đương `MapAreaControllerRoute` của bản gốc |
| 03 | Layout riêng cho Quản trị viên và Người dùng | `Views/Shared/_Layout.cshtml` (site) và `Areas/Admin/Views/Shared/_AdminLayout.cshtml` (admin) — cùng dùng `@RenderBody()`/`@RenderSectionAsync()`, tương đương `_LayoutSite`/`_LayoutAdmin` của bản gốc. CSS/JS tự viết lại (không dùng template AdminLTE/T-shop có sẵn vì không có file gốc) |
| 04 | Ajax lấy danh sách dữ liệu đổ vào DataTable | `Helpers/JDataTableModel.cs`, `Areas/Admin/Controllers/GroupsController.cs` (`GetDataTable`), `Areas/Admin/Views/Groups/Index.cshtml` — tag `lab-04`/`lab-05` |
| 05 | Ajax Thêm/Sửa/Xóa (Nhóm quyền) | `GroupsController` (`Create`/`Edit`/`GetById`/`Delete`), Modal + Toastr + SweetAlert2 qua CDN trong `Groups/Index.cshtml` — tag `lab-04`/`lab-05` |
| 06 | Upload ảnh Ajax (thành viên) | `Controllers/AccountController.cs` (`UploadAvatar` — FormData + IFormFile), `Models/User.cs` (`AvatarUrl`), ràng buộc xóa (`Areas/Admin/Controllers/UsersController.cs`) — tag `lab-06` |
| 07 | Thiết kế trang Đăng nhập | `Controllers/AccountController.cs` (`Login`/`Logout`), `Program.cs` dòng 26-34 (Cookie Authentication) — tương đương Session+MD5 của giáo trình gốc: dùng BCrypt (an toàn hơn MD5) và Cookie Auth có sẵn của ASP.NET Core (built-in, thay cho custom Session+Action Filter); `[Authorize(Roles="admin")]` chính là một Authorization Filter, đúng bản chất kỹ thuật giáo trình dạy — chỉ khác là dùng cơ chế chuẩn của framework thay vì tự viết |
| 16 | Publish Website lên Hosting | Xem mục 1-7 phía trên (README có sẵn từ đầu dự án) — connection string tách riêng trong `appsettings.json`, tự động `Database.Migrate()` khi khởi động (`Program.cs`), hướng dẫn đổi connection string cho Somee/hosting thật, tài khoản admin mặc định để đăng nhập ngay sau khi publish. Không cần thay đổi gì thêm về code, chỉ cần đổi `ConnectionStrings:DefaultConnection` đúng theo hosting và publish qua Visual Studio hoặc `dotnet publish` |
