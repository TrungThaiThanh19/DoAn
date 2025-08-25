using DoAn.ViewModel;

namespace DoAn.Service.IService
{
    public interface IGioHangService
    {
        Task<GioHangVMD> GetCartAsync(Guid khachHangId);
        Task AddItemAsync(Guid khachHangId, Guid sanPhamChiTietId, int soLuong);
        Task UpdateItemAsync(Guid khachHangId, Guid chiTietGioHangId, int soLuong);
        Task RemoveItemAsync(Guid khachHangId, Guid chiTietGioHangId);
        Task ClearAsync(Guid khachHangId);
    }
}