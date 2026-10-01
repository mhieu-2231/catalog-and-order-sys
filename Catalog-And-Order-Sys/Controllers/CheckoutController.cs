using Microsoft.AspNetCore.Mvc;
using Catalog_And_Order_Sys.Data;
using Catalog_And_Order_Sys.Models;
using Catalog_And_Order_Sys.Extensions;

namespace Catalog_And_Order_Sys.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CheckoutController> _logger;
        private const string CART_SESSION_KEY = "Cart";

        public CheckoutController(ApplicationDbContext context, ILogger<CheckoutController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Checkout
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_SESSION_KEY) ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống, không thể đặt hàng.";
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.Cart = cart;
            ViewBag.Total = cart.Sum(i => i.TotalPrice);
            return View();
        }

        // POST: /Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Order order)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_SESSION_KEY) ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống, không thể đặt hàng.";
                return RedirectToAction("Index", "Cart");
            }

            if (string.IsNullOrWhiteSpace(order.CustomerName) || string.IsNullOrWhiteSpace(order.PhoneNumber)
                || string.IsNullOrWhiteSpace(order.Address))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ Họ tên, Số điện thoại và Địa chỉ giao hàng.");
                ViewBag.Cart = cart;
                ViewBag.Total = cart.Sum(i => i.TotalPrice);
                return View(order);
            }

            try
            {
                // 1. Lưu thông tin tổng quan đơn hàng
                order.TotalAmount = cart.Sum(i => i.TotalPrice);
                order.Status = "Chờ xử lý";
                order.OrderDate = DateTime.Now;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // SaveChanges lần 1 để sinh OrderId

                // 2. Lưu chi tiết từng sản phẩm trong đơn (chốt giá tại thời điểm mua)
                foreach (var item in cart)
                {
                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    });
                }
                await _context.SaveChangesAsync();

                // 3. Làm sạch giỏ hàng sau khi đặt thành công
                HttpContext.Session.Remove(CART_SESSION_KEY);

                _logger.LogInformation($"Đặt hàng thành công, OrderId={order.OrderId}, Tổng tiền={order.TotalAmount}");

                return RedirectToAction(nameof(Success), new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu đơn hàng.");
                ModelState.AddModelError("", "Có lỗi xảy ra khi đặt hàng, vui lòng thử lại.");
                ViewBag.Cart = cart;
                ViewBag.Total = cart.Sum(i => i.TotalPrice);
                return View(order);
            }
        }

        // GET: /Checkout/Success/5
        public async Task<IActionResult> Success(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
