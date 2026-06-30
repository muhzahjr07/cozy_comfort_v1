namespace CozyComfort.API.Models.Domain
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int BlanketId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        // Navigation
        public virtual Order Order { get; set; } = null!;
        public virtual Blanket Blanket { get; set; } = null!;
    }
}
