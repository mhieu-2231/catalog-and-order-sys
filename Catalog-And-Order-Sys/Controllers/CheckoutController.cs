using Microsoft.AspNetCore.Mvc;

namespace Catalog_And_Order_Sys.Controllers;

public class CheckoutController : Controller
{
    // GET: /Checkout
    public IActionResult Index()
    {
        return View();
    }

    // POST: /Checkout  (xử lý khi bấm Đặt hàng)
    //[HttpPost]
    //public IActionResult Index(/* OrderViewModel model */)
    //{
    //    // TODO: lưu đơn hàng vào DB
    //    return RedirectToAction("Success");
    //}
}
