namespace Catalog_And_Order_Sys.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Note { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Chờ xử lý"; // Chờ xử lý -> Đang giao -> Hoàn thành / Đã hủy
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
