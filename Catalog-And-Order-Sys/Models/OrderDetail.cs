namespace Catalog_And_Order_Sys.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }  // giá tại thời điểm mua

        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
