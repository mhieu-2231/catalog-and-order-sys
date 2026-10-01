using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Catalog_And_Order_Sys.Data;
using Catalog_And_Order_Sys.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký ApplicationDbContext vào Dependency Injection (DI)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký Identity (theo lab21, KHÔNG thêm AddJwtBearer vì đây là MVC dùng Cookie)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Nới lỏng yêu cầu mật khẩu cho môi trường học tập/demo
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3. Khai báo đường dẫn trang đăng nhập khi bị chặn truy cập ([Authorize])
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache(); // Nơi lưu dữ liệu Session tạm trong RAM server
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Giỏ hàng tự xóa nếu không hoạt động 30 phút
    options.Cookie.HttpOnly = true;                  // Chặn JavaScript đọc Cookie Session -> an toàn hơn
    options.Cookie.IsEssential = true;                // Cho phép hoạt động dù người dùng từ chối Cookie không thiết yếu
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// ================================================================
// 4. Seed 1 tài khoản Admin mặc định khi ứng dụng khởi động lần đầu
//    (thay cho việc gọi API /register như lab21, vì MVC không có
//    sẵn màn hình đăng ký công khai cho Admin)
// ================================================================
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    string adminEmail = "admin@shop.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Quản trị viên"
        };
        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}



app.Run();
