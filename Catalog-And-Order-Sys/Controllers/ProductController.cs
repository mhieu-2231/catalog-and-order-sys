using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog_And_Order_Sys.Data;
using Catalog_And_Order_Sys.Models;

namespace Catalog_And_Order_Sys.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment env, ILogger<ProductController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        // GET: /Product
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(products);
        }

        // GET: /Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError(nameof(product.ProductName), "Tên sản phẩm không được để trống.");
            }
            if (product.Price <= 0)
            {
                ModelState.AddModelError(nameof(product.Price), "Giá sản phẩm phải lớn hơn 0.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(product);
            }

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    product.ImageUrl = await SaveImageAsync(ImageFile);
                }

                product.CreatedAt = DateTime.Now;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Đã thêm sản phẩm mới: {product.ProductName}");
                TempData["Message"] = "Thêm sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm.");
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu sản phẩm, vui lòng thử lại.");
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(product);
            }
        }

        // GET: /Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        // POST: /Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product input, IFormFile? ImageFile)
        {
            if (id != input.ProductId) return BadRequest();

            if (string.IsNullOrWhiteSpace(input.ProductName))
            {
                ModelState.AddModelError(nameof(input.ProductName), "Tên sản phẩm không được để trống.");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(input);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Xóa ảnh cũ (nếu có) trước khi lưu ảnh mới
                    DeleteImageFile(product.ImageUrl);
                    product.ImageUrl = await SaveImageAsync(ImageFile);
                }

                product.ProductName = input.ProductName;
                product.Description = input.Description;
                product.Price = input.Price;
                product.Quantity = input.Quantity;
                product.CategoryId = input.CategoryId;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Đã cập nhật sản phẩm Id={id}");
                TempData["Message"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm.");
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật, vui lòng thử lại.");
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(input);
            }
        }

        // GET: /Product/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Product/Delete/5  (XÓA MỀM)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsDeleted = true;
            product.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Đã xóa (mềm) sản phẩm Id={id}");
            TempData["Message"] = "Đã xóa sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        // ================== Helper: xử lý file ảnh ==================

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            string uploadFolder = Path.Combine(_env.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            _logger.LogInformation($"Đã lưu ảnh sản phẩm tại: /images/products/{uniqueFileName}");
            return $"/images/products/{uniqueFileName}";
        }

        private void DeleteImageFile(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            string fullPath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        // GET: /Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }
    }
}
