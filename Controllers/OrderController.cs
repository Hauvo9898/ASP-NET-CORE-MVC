using System.Security.Claims;
using AHUWeb.Data;
using AHUWeb.Models;
using AHUWeb.Models.ViewModels;
using AHUWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Controllers
{
    // Đặt hàng yêu cầu đăng nhập để đơn hàng gắn được với UserId, phục vụ
    // "Xem lịch sử đơn hàng" — khớp yêu cầu "Customer: mua hàng, xem đơn hàng".
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICartService _cart;

        public OrderController(ApplicationDbContext db, ICartService cart)
        {
            _db = db;
            _cart = cart;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET /Order/Checkout — thay cho showCartItems()/handleCartAction() (bước chuyển sang form giao hàng)
        public async Task<IActionResult> Checkout()
        {
            var lines = _cart.GetLines(HttpContext.Session);
            if (lines.Count == 0) return RedirectToAction("Index", "Cart");

            var vm = await BuildCheckoutViewModel();

            var user = await _db.Users.FindAsync(CurrentUserId);
            if (user != null)
            {
                vm.Name = user.FullName ?? user.Username;
            }

            return View(vm);
        }

        // POST /Order/Checkout — tạo Order + OrderDetail, xóa giỏ hàng.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var lines = _cart.GetLines(HttpContext.Session);
            if (lines.Count == 0) return RedirectToAction("Index", "Cart");

            if (!ModelState.IsValid)
            {
                model.Items = (await BuildCheckoutViewModel()).Items;
                return View(model);
            }

            var productIds = lines.Select(l => l.ProductId).Distinct().ToList();
            var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            // Kiểm tra tồn kho trước khi tạo đơn — cùng 1 sản phẩm có thể nằm ở nhiều
            // dòng giỏ hàng (khác size/màu) nên phải cộng dồn số lượng theo ProductId.
            foreach (var group in lines.GroupBy(l => l.ProductId))
            {
                if (!products.TryGetValue(group.Key, out var p)) continue;
                var wanted = group.Sum(l => l.Quantity);
                if (p.Stock < wanted)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Sản phẩm \"{p.Name}\" chỉ còn {p.Stock} trong kho (bạn đang đặt {wanted}). Vui lòng điều chỉnh giỏ hàng.");
                    model.Items = (await BuildCheckoutViewModel()).Items;
                    return View(model);
                }
            }

            var order = new Order
            {
                UserId = CurrentUserId,
                Name = model.Name,
                Phone = model.Phone,
                Address = model.Address,
                Date = DateTime.Now,
                Status = "pending"
            };

            decimal total = 0;
            foreach (var line in lines)
            {
                if (!products.TryGetValue(line.ProductId, out var p)) continue;
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = p.Id,
                    Quantity = line.Quantity,
                    Price = p.Price
                });
                total += p.Price * line.Quantity;
                // Trừ tồn kho ngay khi đặt hàng thành công; đã kiểm tra đủ hàng ở trên.
                // Cùng SaveChanges với Order nên đơn và tồn kho luôn nhất quán.
                p.Stock -= line.Quantity;
            }
            order.Total = total;

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            _cart.Clear(HttpContext.Session);

            return RedirectToAction(nameof(Success), new { id = order.Id });
        }

        // GET /Order/Success/5 — thay cho cart-success-state
        public async Task<IActionResult> Success(int id)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == CurrentUserId);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET /Order/History — thay cho initOrderHistoryPage()/renderOhOrders()
        public async Task<IActionResult> History(string filter = "all", string? q = null)
        {
            var query = _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .Where(o => o.UserId == CurrentUserId);

            if (!string.IsNullOrWhiteSpace(filter) && filter != "all")
                query = query.Where(o => o.Status == filter);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(o => o.Id.ToString().Contains(q) || o.Name.Contains(q) || o.Phone.Contains(q));

            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentQuery = q ?? "";

            var orders = await query.OrderByDescending(o => o.Date).ToListAsync();
            return View(orders);
        }

        // GET /Order/Details/5 — thay cho openOrderDetailModal()
        public async Task<IActionResult> Details(int id)
        {
            var isAdmin = User.IsInRole("admin");
            var order = await _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id && (isAdmin || o.UserId == CurrentUserId));

            if (order == null) return NotFound();
            return View(order);
        }

        // POST /Order/Repurchase/5 — thay cho rePurchaseOrder(): thêm lại toàn bộ sản phẩm vào giỏ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Repurchase(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == CurrentUserId);

            if (order == null) return NotFound();

            foreach (var item in order.OrderDetails)
                _cart.AddToCart(HttpContext.Session, item.ProductId, item.Quantity, null, null);

            return RedirectToAction("Index", "Cart");
        }

        private async Task<CheckoutViewModel> BuildCheckoutViewModel()
        {
            var lines = _cart.GetLines(HttpContext.Session);
            var productIds = lines.Select(l => l.ProductId).Distinct().ToList();
            var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            var vm = new CheckoutViewModel();
            foreach (var line in lines)
            {
                if (!products.TryGetValue(line.ProductId, out var p)) continue;
                vm.Items.Add(new CartItemViewModel
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Image = p.Image,
                    Price = p.Price,
                    Quantity = line.Quantity,
                    Size = line.Size,
                    Color = line.Color
                });
            }
            return vm;
        }
    }
}
