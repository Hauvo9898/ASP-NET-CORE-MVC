using AHUWeb.Models.ViewModels;

namespace AHUWeb.Services
{
    public interface ICartService
    {
        List<CartLine> GetLines(ISession session);
        void AddToCart(ISession session, int productId, int quantity, string? size, string? color);
        void UpdateQuantity(ISession session, int productId, string? size, string? color, int quantity);
        void RemoveLine(ISession session, int productId, string? size, string? color);
        void Clear(ISession session);
        int TotalItems(ISession session);
    }

    // Replaces: State.cart + saveCart() + addToCart()/removeFromCart()/updateCartQuantity()/clearCart()
    // from the original script.js. Cart lives in Session instead of localStorage; it becomes
    // real Order + OrderDetail rows only when the customer completes checkout (OrderController).
    public class CartService : ICartService
    {
        private const string CartKey = "Cart";

        public List<CartLine> GetLines(ISession session)
        {
            return session.GetObjectFromJson<List<CartLine>>(CartKey) ?? new List<CartLine>();
        }

        private void Save(ISession session, List<CartLine> lines)
        {
            session.SetObjectAsJson(CartKey, lines);
        }

        public void AddToCart(ISession session, int productId, int quantity, string? size, string? color)
        {
            if (quantity < 1) quantity = 1;
            var lines = GetLines(session);
            var existing = lines.FirstOrDefault(l => l.ProductId == productId && l.Size == size && l.Color == color);
            if (existing != null)
                existing.Quantity += quantity;
            else
                lines.Add(new CartLine { ProductId = productId, Quantity = quantity, Size = size, Color = color });

            Save(session, lines);
        }

        public void UpdateQuantity(ISession session, int productId, string? size, string? color, int quantity)
        {
            if (quantity < 1) return;
            var lines = GetLines(session);
            var existing = lines.FirstOrDefault(l => l.ProductId == productId && l.Size == size && l.Color == color);
            if (existing != null) existing.Quantity = quantity;
            Save(session, lines);
        }

        public void RemoveLine(ISession session, int productId, string? size, string? color)
        {
            var lines = GetLines(session);
            lines.RemoveAll(l => l.ProductId == productId && l.Size == size && l.Color == color);
            Save(session, lines);
        }

        public void Clear(ISession session)
        {
            session.Remove(CartKey);
        }

        public int TotalItems(ISession session)
        {
            return GetLines(session).Sum(l => l.Quantity);
        }
    }
}
