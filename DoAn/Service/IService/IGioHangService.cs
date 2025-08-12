using DoAn.Models;
using DoAn.ViewModel;

public interface IGioHangService
{
    Task<GioHang> GetOrCreateCartAsync(Guid khachHangId);
    Task AddItemAsync(Guid khachHangId, Guid sanPhamChiTietId, int soLuong);
    Task UpdateItemAsync(Guid khachHangId, Guid chiTietGioHangId, int soLuong);
    Task RemoveItemAsync(Guid khachHangId, Guid chiTietGioHangId);
    Task ClearAsync(Guid khachHangId);

    Task<GioHangVMD> GetCartAsync(Guid khachHangId);
}