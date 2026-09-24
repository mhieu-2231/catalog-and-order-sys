using Microsoft.AspNetCore.Mvc;

namespace Catalog_And_Order_Sys.Controllers;

public class CartController : Controller
{
    // GET: /Cart
    public IActionResult Index()
    {
        // TODO: lấy giỏ hàng từ Session/Cookie
        return View();
    }
}