using Microsoft.AspNetCore.Mvc;

namespace Catalog_And_Order_Sys.Controllers;

public class ProductController : Controller
{
    // GET: /Product/Details/5
    public IActionResult Details(int id)
    {
        // TODO: lấy sản phẩm theo id từ DB, truyền vào View
        return View();
    }
}

