using AHUWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class OrdersController : AdminBaseController
    {
        private readonly ApplicationDbContext _db;

        public static readonly (string Key, string Label)[] Statuses =
        {
            ("pending", "Chờ xác nhận"),
            ("processing", "Đã duyệt"),
            ("shipped", "Đang giao"),
            ("delivered", "Hoàn tất"),
            ("cancelled", "Đã hủy")
        };

        public OrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Admin/Orders — thay cho renderAdminOrders() (tab "orders")
        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.Orders.Include(o => o.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(o => o.Id.ToString().Contains(q) || o.Name.Contains(q) || o.Phone.Contains(q));

            ViewBag.CurrentQuery = q ?? "";
            var orders = await query.OrderByDescending(o => o.Date).ToListAsync();
            return View(orders);
        }

        // GET /Admin/Orders/Details/5 — thay cho openAdminOrderModal()
        public async Task<IActionResult> Details(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST /Admin/Orders/UpdateStatus — thay cho adminUpdateOrderStatus()
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!Statuses.Any(s => s.Key == status)) return BadRequest();

            var order = await _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            var wasCancelled = order.Status == "cancelled";
            var willBeCancelled = status == "cancelled";
            var message = "Đã cập nhật trạng thái đơn hàng";

            // Tồn kho đã bị trừ lúc đặt hàng (xem OrderController.Checkout), nên:
            // - Hủy đơn  -> cộng trả lại kho
            // - Kích hoạt lại đơn đã hủy -> trừ kho lần nữa, chặn nếu không còn đủ hàng
            if (!wasCancelled && willBeCancelled)
            {
                foreach (var d in order.OrderDetails.Where(d => d.Product != null))
                    d.Product!.Stock += d.Quantity;
                message = "Đã hủy đơn và hoàn lại tồn kho";
            }
            else if (wasCancelled && !willBeCancelled)
            {
                var shortage = order.OrderDetails
                    .Where(d => d.Product != null)
                    .FirstOrDefault(d => d.Product!.Stock < d.Quantity);
                if (shortage != null)
                {
                    TempData["ToastMessage"] =
                        $"Không thể kích hoạt lại: \"{shortage.Product!.Name}\" chỉ còn {shortage.Product.Stock} trong kho (đơn cần {shortage.Quantity}).";
                    return RedirectToAction(nameof(Details), new { id });
                }
                foreach (var d in order.OrderDetails.Where(d => d.Product != null))
                    d.Product!.Stock -= d.Quantity;
                message = "Đã kích hoạt lại đơn và trừ tồn kho";
            }

            order.Status = status;
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
