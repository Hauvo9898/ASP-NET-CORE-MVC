namespace AHUWeb.Models.ViewModels
{
    // What actually lives in Session (replaces the old localStorage 'cart' array).
    // Kept minimal on purpose — Name/Price/Image are looked up fresh from the DB
    // whenever the cart is displayed, so prices can never go stale.
    public class CartLine
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
    }
}
