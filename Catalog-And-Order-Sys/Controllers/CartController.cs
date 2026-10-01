using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog_And_Order_Sys.Data;
using Catalog_And_Order_Sys.Models;
using Catalog_And_Order_Sys.Extensions;

namespace Catalog_And_Order_Sys.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartController> _logger;
        private const string CART_SESSION_KEY = "Cart";

        public CartController(ApplicationDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Cart
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        // POST: /Cart/Add  (gọi từ form ở trang Product/Details)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var product = await _context.Products.AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) return NotFound();

            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                // Sản phẩm đã có trong giỏ -> cộng dồn số lượng
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            SaveCartToSession(cart);
            _logger.LogInformation($"Đã thêm sản phẩm Id={productId} vào giỏ hàng, số lượng={quantity}");

            TempData["Message"] = "Đã thêm vào giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Update  (sửa số lượng từng dòng trong trang giỏ hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int productId, int quantity)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item); // số lượng <= 0 coi như xóa khỏi giỏ
                }
                else
                {
                    item.Quantity = quantity;
                }
                SaveCartToSession(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Remove/5
        public IActionResult Remove(int id)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(i => i.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
                _logger.LogInformation($"Đã xóa sản phẩm Id={id} khỏi giỏ hàng");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Clear
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CART_SESSION_KEY);
            return RedirectToAction(nameof(Index));
        }

        // ================== Helper ==================

        private List<CartItem> GetCartFromSession()
        {
            return HttpContext.Session.GetObject<List<CartItem>>(CART_SESSION_KEY) ?? new List<CartItem>();
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            HttpContext.Session.SetObject(CART_SESSION_KEY, cart);
        }
    }
}
