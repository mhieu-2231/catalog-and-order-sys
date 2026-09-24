using Microsoft.AspNetCore.Mvc;

namespace Catalog_And_Order_Sys.Controllers;

public class ShopController : Controller
{
    // GET: /Shop
    public IActionResult Index()
    {
        // TODO: lấy danh sách sản phẩm từ DB, truyền vào View qua model
        return View();
    }
}
