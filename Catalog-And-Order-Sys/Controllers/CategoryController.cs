using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog_And_Order_Sys.Data;
using Catalog_And_Order_Sys.Models;
using Microsoft.AspNetCore.Authorization;

namespace Catalog_And_Order_Sys.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Category
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            // HasQueryFilter trong DbContext đã tự động lọc IsDeleted = false
            var categories = await _context.Categories
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(categories);
        }

        // GET: /Category/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Category/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError(nameof(category.CategoryName), "Tên danh mục không được để trống.");
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            category.CreatedAt = DateTime.Now;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Thêm danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Category/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: /Category/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category input)
        {
            if (id != input.CategoryId) return BadRequest();

            if (string.IsNullOrWhiteSpace(input.CategoryName))
            {
                ModelState.AddModelError(nameof(input.CategoryName), "Tên danh mục không được để trống.");
            }
            if (!ModelState.IsValid) return View(input);

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.CategoryName = input.CategoryName;
            category.Slug = input.Slug;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Cập nhật danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Category/Delete/5  (trang xác nhận)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: /Category/Delete/5  (XÓA MỀM — không gọi _context.Categories.Remove())
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            bool hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["Error"] = "Không thể xóa: vẫn còn sản phẩm thuộc danh mục này.";
                return RedirectToAction(nameof(Index));
            }

            category.IsDeleted = true;
            category.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đã xóa danh mục.";
            return RedirectToAction(nameof(Index));
        }
    }
}
