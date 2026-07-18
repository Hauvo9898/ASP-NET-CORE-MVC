using AHUWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AHUWeb.ViewComponents
{
    // Thay cho updateCartBadge() trong script.js — hiển thị số lượng trong icon giỏ hàng
    // trên navbar. Được gọi từ _Layout.cshtml nên có mặt trên mọi trang.
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly ICartService _cart;

        public CartBadgeViewComponent(ICartService cart)
        {
            _cart = cart;
        }

        public IViewComponentResult Invoke()
        {
            var count = _cart.TotalItems(HttpContext.Session);
            return View(count);
        }
    }
}
