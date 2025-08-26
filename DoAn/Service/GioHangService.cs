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

            // Bước 1: lấy dữ liệu thô từ DB
            var lines = await _db.ChiTietGioHangs
                .AsNoTracking()
                .Where(x => x.ID_GioHang == cart.ID_GioHang)
                .Include(x => x.SanPhamChiTiet)!.ThenInclude(spct => spct.SanPham)!.ThenInclude(sp => sp.ThuongHieu)
                .Include(x => x.SanPhamChiTiet)!.ThenInclude(spct => spct.TheTich)
                .Include(x => x.SanPhamChiTiet)!.ThenInclude(spct => spct.ChiTietKhuyenMais)!.ThenInclude(ctkm => ctkm.KhuyenMai)
                .ToListAsync();

            // Bước 2: xử lý bằng LINQ to Objects
            var items = lines.Select(x =>
            {
                var spct = x.SanPhamChiTiet!;
                var sp = spct.SanPham!;

                decimal giaGoc = spct.GiaBan;
                decimal giaHienThi = giaGoc;
                int giamPhanTram = 0;

                var now = DateTime.UtcNow.AddHours(7);
                var validKM = spct.ChiTietKhuyenMais?
                    .Where(ctkm => ctkm.KhuyenMai != null
                                && ctkm.KhuyenMai.TrangThai == 1
                                && ctkm.KhuyenMai.NgayBatDau <= now
                                && ctkm.KhuyenMai.NgayHetHan >= now)
                    .ToList();

                if (validKM != null && validKM.Any())
                {
                    foreach (var kmct in validKM)
                    {
                        var km = kmct.KhuyenMai!;
                        decimal discount = 0;
                        var kieu = (km.KieuGiamGia ?? "").Trim().ToLowerInvariant();

                        if (kieu == "percent")
                        {
                            var pct = Math.Clamp(km.GiaTriGiam, 0, 100);
                            discount = giaGoc * (pct / 100m);
                            if (km.GiaTriToiDa > 0 && discount > km.GiaTriToiDa) discount = km.GiaTriToiDa;
                        }
                        else if (kieu == "fixed")
                        {
                            discount = Math.Min(giaGoc, Math.Max(0, km.GiaTriGiam));
                        }

                        var price = Math.Max(0, giaGoc - discount);
                        if (price < giaHienThi) giaHienThi = price;
                    }

                    if (giaHienThi < giaGoc)
                    {
                        giamPhanTram = (int)Math.Clamp(
                            Math.Round((1 - (giaHienThi / giaGoc)) * 100M),
                            0, 100);
                    }
                }

                return new GioHangItemVMD
                {
                    ChiTietGioHangId = x.ID_ChiTietGioHang,
                    SanPhamChiTietId = spct.ID_SanPhamChiTiet,
                    TenSanPham = sp.Ten_SanPham,
                    ThuongHieu = sp.ThuongHieu?.Ten_ThuongHieu,
                    TheTich = spct.TheTich?.DonVi,
                    HinhAnh = string.IsNullOrWhiteSpace(sp.HinhAnh) ? "/images/no-image.png" : sp.HinhAnh,

                    DonGia = giaGoc,
                    SoLuong = x.SoLuong,
                    ThanhTien = giaHienThi * x.SoLuong,
                    TonKho = spct.SoLuong,

                    GiaSauKhuyenMai = giaHienThi,
                    GiamPhanTram = giamPhanTram,
                    ChiTietKhuyenMais = validKM
                };
            }).ToList();

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