using DoAn.Models;
using DoAn.Service;              // KhuyenMaiService (class)
using DoAn.IService;            // IKhuyenMaiService, IGioHangService, IStatisticsService (interfaces)
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using DoAn.Service.IService;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor runtime compilation + TempData dùng Session
builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation()
    .AddSessionStateTempDataProvider();

// DbContext
builder.Services.AddDbContext<DoAnDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cache + Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".DoAn.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cookie Auth (nếu dùng [Authorize])
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/TaiKhoan/Login";
        o.AccessDeniedPath = "/TaiKhoan/Denied";
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// ===== DI: dùng đúng namespace interface DoAn.IService =====
builder.Services.AddScoped<IGioHangService, GioHangService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IKhuyenMaiService, KhuyenMaiService>();

// (tuỳ nhu cầu) nếu service nào cần HttpContext:
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();          // trước Auth nếu TempData dùng Session
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Shop}/{action=Index}/{id?}");

app.Run();
