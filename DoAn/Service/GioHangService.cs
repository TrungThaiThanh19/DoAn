// Services/GioHangService.cs
using DoAn.Models;
using DoAn.ViewModel;
using Microsoft.EntityFrameworkCore;

public class GioHangService : IGioHangService
{
    private readonly DoAnDbContext _db;
    public GioHangService(DoAnDbContext db) => _db = db;

    public async Task<GioHang> GetOrCreateCartAsync(Guid khachHangId)
    {
        var cart = await _db.GioHangs
            .Include(g => g.ChiTietGioHangs)
            .FirstOrDefaultAsync(g => g.ID_KhachHang == khachHangId);

        if (cart == null)
        {
            cart = new GioHang
            {
                ID_GioHang = Guid.NewGuid(),
                ID_KhachHang = khachHangId,
                ChiTietGioHangs = new List<ChiTietGioHang>()
            };
            _db.GioHangs.Add(cart);
            await _db.SaveChangesAsync();
        }
        return cart;
    }

    public async Task AddItemAsync(Guid khachHangId, Guid sanPhamChiTietId, int soLuong)
    {
        if (soLuong <= 0) soLuong = 1;

        var cart = await GetOrCreateCartAsync(khachHangId);

        // Kiểm tra tồn kho
        var spct = await _db.SanPhamChiTiets
            .Include(x => x.SanPham)
            .Include(x => x.TheTich)
            .FirstOrDefaultAsync(x => x.ID_SanPhamChiTiet == sanPhamChiTietId)
            ?? throw new Exception("Không tìm thấy biến thể sản phẩm.");

        if (spct.SoLuong <= 0)
            throw new Exception("Sản phẩm đã hết hàng.");

        var item = await _db.ChiTietGioHangs
            .FirstOrDefaultAsync(i => i.ID_GioHang == cart.ID_GioHang && i.ID_SanPhamChiTiet == sanPhamChiTietId);

        // Số lượng tối đa không vượt tồn kho
        int addQty = Math.Min(soLuong, spct.SoLuong);

        if (item == null)
        {
            item = new ChiTietGioHang
            {
                ID_ChiTietGioHang = Guid.NewGuid(),
                ID_GioHang = cart.ID_GioHang,
                ID_SanPhamChiTiet = sanPhamChiTietId,
                SoLuong = addQty
            };
            _db.ChiTietGioHangs.Add(item);
        }
        else
        {
            item.SoLuong = Math.Min(item.SoLuong + addQty, spct.SoLuong);
            _db.ChiTietGioHangs.Update(item);
        }

        await _db.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(Guid khachHangId, Guid chiTietGioHangId, int soLuong)
    {
        var cart = await GetOrCreateCartAsync(khachHangId);

        var item = await _db.ChiTietGioHangs
            .Include(i => i.SanPhamChiTiet)
            .FirstOrDefaultAsync(i => i.ID_ChiTietGioHang == chiTietGioHangId && i.ID_GioHang == cart.ID_GioHang)
            ?? throw new Exception("Không tìm thấy sản phẩm trong giỏ.");

        if (soLuong <= 0)
        {
            _db.ChiTietGioHangs.Remove(item);
        }
        else
        {
            item.SoLuong = Math.Min(soLuong, item.SanPhamChiTiet.SoLuong);
            _db.ChiTietGioHangs.Update(item);
        }

        await _db.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(Guid khachHangId, Guid chiTietGioHangId)
    {
        var cart = await GetOrCreateCartAsync(khachHangId);

        var item = await _db.ChiTietGioHangs
            .FirstOrDefaultAsync(i => i.ID_ChiTietGioHang == chiTietGioHangId && i.ID_GioHang == cart.ID_GioHang);

        if (item != null)
        {
            _db.ChiTietGioHangs.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    public async Task ClearAsync(Guid khachHangId)
    {
        var cart = await GetOrCreateCartAsync(khachHangId);

        var items = await _db.ChiTietGioHangs
            .Where(i => i.ID_GioHang == cart.ID_GioHang)
            .ToListAsync();

        _db.ChiTietGioHangs.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    public async Task<GioHangVMD> GetCartAsync(Guid khachHangId)
    {
        var cart = await GetOrCreateCartAsync(khachHangId);

        var items = await _db.ChiTietGioHangs
            .Include(i => i.SanPhamChiTiet)
                .ThenInclude(ct => ct.SanPham)
            .Include(i => i.SanPhamChiTiet)
                .ThenInclude(ct => ct.TheTich)
            .Where(i => i.ID_GioHang == cart.ID_GioHang)
            .Select(i => new ChiTietGioHangVMD
            {
                ChiTietGioHangId = i.ID_ChiTietGioHang,
                SanPhamChiTietId = i.ID_SanPhamChiTiet,
                TenSanPham = i.SanPhamChiTiet.SanPham.Ten_SanPham,
                HinhAnh = i.SanPhamChiTiet.SanPham.HinhAnh,
                TheTich = i.SanPhamChiTiet.TheTich != null
                          ? $"{i.SanPhamChiTiet.TheTich.GiaTri} {i.SanPhamChiTiet.TheTich.DonVi}"
                          : string.Empty,
                DonGia = i.SanPhamChiTiet.GiaBan, // giả sử có trường Giá bán
                SoLuong = i.SoLuong
            })
            .ToListAsync();

        return new GioHangVMD
        {
            GioHangId = cart.ID_GioHang,
            Items = items
        };
    }
}
