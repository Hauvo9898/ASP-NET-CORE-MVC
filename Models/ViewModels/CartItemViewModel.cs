namespace AHUWeb.Models.ViewModels
{
    // The old site kept the cart in localStorage. There's no "Cart" table in the
    // requested schema, so the cart is kept server-side in Session (JSON-serialized
    // list of these) until checkout, at which point it becomes Order + OrderDetail rows.
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Image { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
    }
}
