// Services/KhuyenMaiService.cs
using DoAn.IService;
using DoAn.Models;
using DoAn.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Service
{
    public class KhuyenMaiService : IKhuyenMaiService
    {
        private readonly DoAnDbContext _db;
        public KhuyenMaiService(DoAnDbContext db) => _db = db;

        public async Task<IEnumerable<KhuyenMai>> GetAllAsync(string? search = null)
        {
            var q = _db.KhuyenMais
                       .Include(k => k.ChiTietKhuyenMais)
                       .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => x.Ten_KhuyenMai.Contains(search) || x.Ma_KhuyenMai.Contains(search));
            return await q.ToListAsync();
        }

        public async Task<KhuyenMai?> GetByIdAsync(Guid id) =>
            await _db.KhuyenMais
                .Include(k => k.ChiTietKhuyenMais)
                .FirstOrDefaultAsync(k => k.ID_KhuyenMai == id);

        public async Task AddAsync(KhuyenMai km, IEnumerable<Guid> spctIds)
        {
            km.ID_KhuyenMai = Guid.NewGuid();
            if (spctIds?.Any() == true)
            {
                km.ChiTietKhuyenMais = spctIds.Select(id => new ChiTietKhuyenMai
                {
                    ID_ChiTietKhuyenMai = Guid.NewGuid(),
                    ID_KhuyenMai = km.ID_KhuyenMai,
                    ID_SanPhamChiTiet = id
                }).ToList();
            }
            _db.KhuyenMais.Add(km);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(KhuyenMai km, IEnumerable<Guid> spctIds)
        {
            var exist = await _db.KhuyenMais
                .Include(x => x.ChiTietKhuyenMais)
                .FirstOrDefaultAsync(x => x.ID_KhuyenMai == km.ID_KhuyenMai);

            if (exist == null) return;

            // update props
            exist.Ma_KhuyenMai = km.Ma_KhuyenMai;
            exist.Ten_KhuyenMai = km.Ten_KhuyenMai;
            exist.KieuGiamGia = km.KieuGiamGia;
            exist.GiaTriGiam = km.GiaTriGiam;
            exist.GiaTriToiDa = km.GiaTriToiDa;
            exist.MoTa = km.MoTa;
            exist.NgayBatDau = km.NgayBatDau;
            exist.NgayHetHan = km.NgayHetHan;
            exist.TrangThai = km.TrangThai;

            // update SPCT mapping
            var currentIds = exist.ChiTietKhuyenMais.Select(c => c.ID_SanPhamChiTiet).ToList();
            var incoming = spctIds?.ToList() ?? new List<Guid>();

            // remove
            var toRemove = exist.ChiTietKhuyenMais.Where(c => !incoming.Contains(c.ID_SanPhamChiTiet)).ToList();
            _db.ChiTietKhuyenMais.RemoveRange(toRemove);

            // add
            var toAdd = incoming.Where(id => !currentIds.Contains(id))
                                .Select(id => new ChiTietKhuyenMai
                                {
                                    ID_ChiTietKhuyenMai = Guid.NewGuid(),
                                    ID_KhuyenMai = exist.ID_KhuyenMai,
                                    ID_SanPhamChiTiet = id
                                });
            _db.ChiTietKhuyenMais.AddRange(toAdd);

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var km = await _db.KhuyenMais.Include(k => k.ChiTietKhuyenMais)
                                         .FirstOrDefaultAsync(k => k.ID_KhuyenMai == id);
            if (km == null) return;
            _db.ChiTietKhuyenMais.RemoveRange(km.ChiTietKhuyenMais);
            _db.KhuyenMais.Remove(km);
            await _db.SaveChangesAsync();
        }

        public async Task ToggleAsync(Guid id)
        {
            var km = await _db.KhuyenMais.FindAsync(id);
            if (km == null) return;
            km.TrangThai = km.TrangThai == 1 ? 0 : 1;
            await _db.SaveChangesAsync();
        }

        public (decimal finalPrice, KhuyenMai? applied, decimal discount) ApplyBestDiscount(SanPhamChiTiet spct)
        {
            var now = DateTime.UtcNow.AddHours(7);
            decimal best = spct.GiaBan;
            KhuyenMai? bestKm = null;
            decimal bestDiscount = 0;

            foreach (var ctkm in spct.ChiTietKhuyenMais.Where(c => c.KhuyenMai != null))
            {
                var km = ctkm.KhuyenMai;
                if (km.TrangThai != 1 || now < km.NgayBatDau || now > km.NgayHetHan) continue;

                decimal discount = 0;
                if (km.KieuGiamGia == "percent")
                {
                    discount = spct.GiaBan * (km.GiaTriGiam / 100m);
                    if (km.GiaTriToiDa > 0 && discount > km.GiaTriToiDa)
                        discount = km.GiaTriToiDa;
                }
                else if (km.KieuGiamGia == "fixed")
                {
                    discount = Math.Min(spct.GiaBan, km.GiaTriGiam);
                }

                var final = spct.GiaBan - discount;
                if (final < best)
                {
                    best = final;
                    bestKm = km;
                    bestDiscount = discount;
                }
            }

            return (best, bestKm, bestDiscount);
        }
    }
}