using DoAn.Models;
using DoAn.Service;
using DoAn.Service.IService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ================== SERVICES ==================

// MVC + Razor Hot Reload (Runtime Compilation)
builder.Services.AddControllersWithViews();
builder.Services.AddMvc().AddRazorRuntimeCompilation();

// DbContext (lấy chuỗi kết nối từ appsettings.json: "DefaultConnection")
builder.Services.AddDbContext<DoAnDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session (cần cache phân tán in-memory)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// (Tuỳ chọn) Cookie Authentication – nếu bạn muốn dùng trong tương lai
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/TaiKhoan/Login";
        o.AccessDeniedPath = "/TaiKhoan/Login";
        // o.SlidingExpiration = true;
        // o.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// DI cho giỏ hàng (service pattern)
builder.Services.AddScoped<IGioHangService, GioHangService>();

var app = builder.Build();

// ================== MIDDLEWARE PIPELINE ==================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session phải trước AuthZ
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ================== ROUTING ==================
// Trang mặc định: TaiKhoan/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TaiKhoan}/{action=Login}/{id?}");

app.Run();
