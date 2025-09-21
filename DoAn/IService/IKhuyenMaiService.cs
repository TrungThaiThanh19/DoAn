using DoAn.Models;

namespace DoAn.IService
{
    public interface IKhuyenMaiService
    {
        Task<IEnumerable<KhuyenMai>> GetAllAsync(string? search = null);
        Task<KhuyenMai?> GetByIdAsync(Guid id);
        Task AddAsync(KhuyenMai km, IEnumerable<Guid> spctIds);
        Task UpdateAsync(KhuyenMai km, IEnumerable<Guid> spctIds);
        Task DeleteAsync(Guid id);
        Task ToggleAsync(Guid id);

        // Áp dụng khuyến mãi cho SPCT (tính động)
        (decimal finalPrice, KhuyenMai? applied, decimal discount) ApplyBestDiscount(SanPhamChiTiet spct);
    }
}