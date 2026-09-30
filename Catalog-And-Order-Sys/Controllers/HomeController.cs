using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog_And_Order_Sys.Data;

namespace Catalog_And_Order_Sys.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            // Trang chủ chỉ đọc dữ liệu (Read-only) -> dùng AsNoTracking() để tối ưu (lab17)
            var newArrivals = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .ToListAsync();

            return View(newArrivals);
        }

        // Giữ nguyên các action Privacy/Error đã có sẵn từ project mặc định
    }
}
