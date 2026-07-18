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
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = "Đã cập nhật trạng thái đơn hàng";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
