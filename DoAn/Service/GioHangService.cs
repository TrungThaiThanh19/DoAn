using DoAn.Models;
using DoAn.Service.IService;
using DoAn.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Service
{
    public class GioHangService : IGioHangService
    {
        private readonly DoAnDbContext _db;
        public GioHangService(DoAnDbContext db) => _db = db;

        private async Task<GioHang> GetOrCreateCartAsync(Guid khachHangId)
        {
            var cart = await _db.GioHangs.FirstOrDefaultAsync(g => g.ID_KhachHang == khachHangId);
            if (cart == null)
            {
                cart = new GioHang
                {
                    ID_GioHang = Guid.NewGuid(),
                    ID_KhachHang = khachHangId
                };
                _db.GioHangs.Add(cart);
                await _db.SaveChangesAsync();
            }
            return cart;
        }

        public async Task<GioHangVMD> GetCartAsync(Guid khachHangId)
        {
            var cart = await GetOrCreateCartAsync(khachHangId);

            var items = await _db.ChiTietGioHangs
                .AsNoTracking()
                .Where(x => x.ID_GioHang == cart.ID_GioHang)
                .Include(x => x.SanPhamChiTiet)!.ThenInclude(spct => spct.SanPham)!.ThenInclude(sp => sp.ThuongHieu)
                .Include(x => x.SanPhamChiTiet)!.ThenInclude(spct => spct.TheTich)
                .Select(x => new GioHangItemVMD
                {
                    ChiTietGioHangId = x.ID_ChiTietGioHang,
                    SanPhamChiTietId = x.ID_SanPhamChiTiet,

                    TenSanPham = x.SanPhamChiTiet!.SanPham!.Ten_SanPham,
                    ThuongHieu = x.SanPhamChiTiet!.SanPham!.ThuongHieu != null
                                ? x.SanPhamChiTiet!.SanPham!.ThuongHieu.Ten_ThuongHieu : null,
                    TheTich = x.SanPhamChiTiet!.TheTich != null ? x.SanPhamChiTiet!.TheTich.DonVi : null,
                    HinhAnh = string.IsNullOrWhiteSpace(x.SanPhamChiTiet!.SanPham!.HinhAnh)
                                ? "/images/no-image.png"
                                : x.SanPhamChiTiet!.SanPham!.HinhAnh,

                    DonGia = x.SanPhamChiTiet!.GiaBan,
                    SoLuong = x.SoLuong,
                    ThanhTien = x.SoLuong * x.SanPhamChiTiet!.GiaBan,
                    TonKho = x.SanPhamChiTiet!.SoLuong
                })
                .ToListAsync();

            return new GioHangVMD { Items = items };
        }

        public async Task AddItemAsync(Guid khachHangId, Guid sanPhamChiTietId, int soLuong)
        {
            var cart = await GetOrCreateCartAsync(khachHangId);
            var variant = await _db.SanPhamChiTiets.FirstOrDefaultAsync(x => x.ID_SanPhamChiTiet == sanPhamChiTietId);
            if (variant == null) return;

            var line = await _db.ChiTietGioHangs
                .FirstOrDefaultAsync(x => x.ID_GioHang == cart.ID_GioHang && x.ID_SanPhamChiTiet == sanPhamChiTietId);

            int stock = Math.Max(0, variant.SoLuong);
            if (stock == 0) return;

            if (line == null)
            {
                var qty = Math.Clamp(soLuong, 1, stock);
                line = new ChiTietGioHang
                {
                    ID_ChiTietGioHang = Guid.NewGuid(),
                    ID_GioHang = cart.ID_GioHang,
                    ID_SanPhamChiTiet = sanPhamChiTietId,
                    SoLuong = qty
                };
                _db.ChiTietGioHangs.Add(line);
            }
            else
            {
                line.SoLuong = Math.Clamp(line.SoLuong + soLuong, 1, stock);
                _db.ChiTietGioHangs.Update(line);
            }
            await _db.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(Guid khachHangId, Guid chiTietGioHangId, int soLuong)
        {
            var cart = await GetOrCreateCartAsync(khachHangId);
            var line = await _db.ChiTietGioHangs
                .Include(l => l.SanPhamChiTiet)
                .FirstOrDefaultAsync(x => x.ID_ChiTietGioHang == chiTietGioHangId && x.ID_GioHang == cart.ID_GioHang);

            if (line == null) return;

            int stock = Math.Max(0, line.SanPhamChiTiet?.SoLuong ?? 0);
            var clamped = Math.Clamp(soLuong, 1, Math.Max(1, stock));  // không cho 0, không vượt tồn
            line.SoLuong = clamped;

            _db.ChiTietGioHangs.Update(line);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(Guid khachHangId, Guid chiTietGioHangId)
        {
            var cart = await GetOrCreateCartAsync(khachHangId);
            var line = await _db.ChiTietGioHangs
                .FirstOrDefaultAsync(x => x.ID_ChiTietGioHang == chiTietGioHangId && x.ID_GioHang == cart.ID_GioHang);
            if (line != null)
            {
                _db.ChiTietGioHangs.Remove(line);
                await _db.SaveChangesAsync();
            }
        }

        public async Task ClearAsync(Guid khachHangId)
        {
            var cart = await GetOrCreateCartAsync(khachHangId);
            var lines = _db.ChiTietGioHangs.Where(x => x.ID_GioHang == cart.ID_GioHang);
            _db.ChiTietGioHangs.RemoveRange(lines);
            await _db.SaveChangesAsync();
        }
    }
}