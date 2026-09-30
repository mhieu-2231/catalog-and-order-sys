using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog_And_Order_Sys.Data;

namespace Catalog_And_Order_Sys.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ShopController(ApplicationDbContext context) => _context = context;

        // GET: /Shop?categoryId=2
        public async Task<IActionResult> Index(int? categoryId)
        {
            // 1. Khởi tạo IQueryable (chưa thực thi xuống DB) - phong cách lab23
            IQueryable<Models.Product> query = _context.Products
                .Include(p => p.Category)
                .AsNoTracking();

            // 2. Lọc theo danh mục nếu có truyền categoryId
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Danh sách danh mục để render sidebar lọc
            ViewBag.Categories = await _context.Categories.AsNoTracking().ToListAsync();
            ViewBag.SelectedCategoryId = categoryId;

            return View(products);
        }
    }
}
