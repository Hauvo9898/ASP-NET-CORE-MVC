using AHUWeb.Data;
using AHUWeb.Models.ViewModels;
using AHUWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICartService _cart;

        public CartController(ApplicationDbContext db, ICartService cart)
        {
            _db = db;
            _cart = cart;
        }

        // GET /Cart — thay cho renderCartPage()/renderCartItems()/renderCartSummary()
        public async Task<IActionResult> Index()
        {
            var vm = await BuildCartViewModel();
            return View(vm);
        }

        // POST /Cart/Add — thay cho addToCart(). Dùng chung cho nút "Thêm nhanh" trên
        // danh sách sản phẩm, Quick View, và trang chi tiết sản phẩm.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1, string? size = null, string? color = null)
        {
            var exists = await _db.Products.AnyAsync(p => p.Id == productId && p.IsActive);
            if (!exists) return NotFound();

            _cart.AddToCart(HttpContext.Session, productId, quantity, size, color);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, count = _cart.TotalItems(HttpContext.Session) });

            TempData["ToastMessage"] = "Đã thêm vào giỏ hàng";
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/UpdateQuantity — thay cho updateCartQuantity()
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productId, int quantity, string? size = null, string? color = null)
        {
            _cart.UpdateQuantity(HttpContext.Session, productId, size, color, quantity);
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Remove — thay cho removeFromCart()
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId, string? size = null, string? color = null)
        {
            _cart.RemoveLine(HttpContext.Session, productId, size, color);
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Clear — thay cho clearCart()
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cart.Clear(HttpContext.Session);
            return RedirectToAction(nameof(Index));
        }

        private async Task<CartViewModel> BuildCartViewModel()
        {
            var lines = _cart.GetLines(HttpContext.Session);
            var productIds = lines.Select(l => l.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var vm = new CartViewModel();
            foreach (var line in lines)
            {
                if (!products.TryGetValue(line.ProductId, out var p)) continue; // sp đã bị xóa khỏi kho
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
