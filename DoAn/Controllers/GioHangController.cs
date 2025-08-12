using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class GioHangController : Controller
{
    private readonly IGioHangService _cart;
    private readonly DoAnDbContext _db;
    public GioHangController(IGioHangService cart, DoAnDbContext db)
    {
        _cart = cart;
        _db = db;
    }

    // Lấy KhachHangId từ claim TaiKhoanId
    private async Task<Guid> GetKhachHangIdAsync()
    {
        // 1) Lấy ID_TaiKhoan từ Session do bạn set ở action Login
        var taiKhoanIdStr = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(taiKhoanIdStr))
            throw new Exception("Bạn cần đăng nhập.");

        var taiKhoanId = Guid.Parse(taiKhoanIdStr);

        // 2) Tìm KhachHang theo ID_TaiKhoan
        var kh = await _db.KhachHangs
            .FirstOrDefaultAsync(k => k.ID_TaiKhoan == taiKhoanId);

        // 3) Nếu chưa có KhachHang -> tạo mới (để giỏ hàng dùng được cho mọi TK)
        if (kh == null)
        {
            kh = new KhachHang
            {
                ID_KhachHang = Guid.NewGuid(),
                ID_TaiKhoan = taiKhoanId,
                Ten_KhachHang = HttpContext.Session.GetString("Username") ?? "Khách hàng",
                NgayTao = DateTime.Now
            };
            _db.KhachHangs.Add(kh);
            await _db.SaveChangesAsync();
        }

        return kh.ID_KhachHang;
    }

    // GET: /GioHang
    public async Task<IActionResult> Index()
    {
        var khId = await GetKhachHangIdAsync();
        var vm = await _cart.GetCartAsync(khId);
        return View(vm);
    }

    // POST: /GioHang/Add
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid sanPhamChiTietId, int soLuong = 1, string? returnUrl = null)
    {
        var khId = await GetKhachHangIdAsync();
        await _cart.AddItemAsync(khId, sanPhamChiTietId, soLuong);

        if (!string.IsNullOrWhiteSpace(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    // POST: /GioHang/Update
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid chiTietGioHangId, int soLuong)
    {
        var khId = await GetKhachHangIdAsync();
        await _cart.UpdateItemAsync(khId, chiTietGioHangId, soLuong);
        return RedirectToAction(nameof(Index));
    }

    // POST: /GioHang/Remove
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid chiTietGioHangId)
    {
        var khId = await GetKhachHangIdAsync();
        await _cart.RemoveItemAsync(khId, chiTietGioHangId);
        return RedirectToAction(nameof(Index));
    }

    // POST: /GioHang/Clear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var khId = await GetKhachHangIdAsync();
        await _cart.ClearAsync(khId);
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAjax(Guid chiTietGioHangId, int soLuong)
    {
        var khId = await GetKhachHangIdAsync();
        await _cart.UpdateItemAsync(khId, chiTietGioHangId, soLuong);

        var cart = await _cart.GetCartAsync(khId);
        var item = cart.Items.FirstOrDefault(x => x.ChiTietGioHangId == chiTietGioHangId);

        return Json(new
        {
            ok = true,
            lineTotal = item?.ThanhTien ?? 0m,
            subtotal = cart.Subtotal
        });
    }
}