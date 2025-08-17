using DoAn.Models;
using DoAn.Service.IService;
using DoAn.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DoAnDbContext _db;
        private readonly IGioHangService _cart;

        public CheckoutController(DoAnDbContext db, IGioHangService cart)
        {
            _db = db;
            _cart = cart;
        }

        private async Task<Guid> GetKhachHangIdAsync()
        {
            var tk = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(tk)) throw new Exception("Bạn cần đăng nhập.");
            var taiKhoanId = Guid.Parse(tk);

            var kh = await _db.KhachHangs.FirstOrDefaultAsync(x => x.ID_TaiKhoan == taiKhoanId);
            if (kh == null)
            {
                kh = new KhachHang
                {
                    ID_KhachHang = Guid.NewGuid(),
                    ID_TaiKhoan = taiKhoanId,
                    Ma_KhachHang = "KH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    Ten_KhachHang = HttpContext.Session.GetString("Username") ?? "Khách hàng",
                    GioiTinh = "Khác",
                    SoDienThoai = "",
                    Email = "",
                    TrangThai = 1,
                    NgayTao = DateTime.Now
                };
                _db.KhachHangs.Add(kh);
                await _db.SaveChangesAsync();
            }
            return kh.ID_KhachHang;
        }

        // ===== B1: CHỌN ĐỊA CHỈ =====
        public async Task<IActionResult> Address()
        {
            var khId = await GetKhachHangIdAsync();
            var kh = await _db.KhachHangs.FirstAsync(x => x.ID_KhachHang == khId);

            var list = await _db.DiaChiKhachHangs
                .Where(x => x.ID_KhachHang == khId)
                .OrderByDescending(x => x.DiaChiMacDinh)
                .ToListAsync();

            var vm = new CheckoutAddressVM
            {
                Addresses = list.Select(a => new CheckoutAddressVM.AddressVM
                {
                    ID_DiaChiKhachHang = a.ID_DiaChiKhachHang,
                    SoNha = a.SoNha,
                    Xa_Phuong = a.Xa_Phuong,
                    Quan_Huyen = a.Quan_Huyen,
                    Tinh_ThanhPho = a.Tinh_ThanhPho,
                    DiaChiMacDinh = a.DiaChiMacDinh,
                    HoTen = string.IsNullOrWhiteSpace(a.HoTen) ? kh.Ten_KhachHang : a.HoTen,             // ưu tiên địa chỉ
                    SoDienThoai = string.IsNullOrWhiteSpace(a.SoDienThoai) ? kh.SoDienThoai : a.SoDienThoai
                }).ToList(),
                SelectedAddressId = list.FirstOrDefault(x => x.DiaChiMacDinh)?.ID_DiaChiKhachHang
                                    ?? list.FirstOrDefault()?.ID_DiaChiKhachHang
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UseAddress(Guid addressId)
            => RedirectToAction(nameof(Review), new { addressId });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(DiaChiKhachHang model, bool MakeDefault)
        {
            var khId = await GetKhachHangIdAsync();
            var kh = await _db.KhachHangs.FirstAsync(x => x.ID_KhachHang == khId);

            model.ID_DiaChiKhachHang = Guid.NewGuid();
            model.ID_KhachHang = khId;

            // fallback nếu người dùng không nhập
            if (string.IsNullOrWhiteSpace(model.HoTen)) model.HoTen = kh.Ten_KhachHang;
            if (string.IsNullOrWhiteSpace(model.SoDienThoai)) model.SoDienThoai = kh.SoDienThoai;

            if (MakeDefault)
            {
                var olds = _db.DiaChiKhachHangs.Where(x => x.ID_KhachHang == khId && x.DiaChiMacDinh);
                await olds.ForEachAsync(x => x.DiaChiMacDinh = false);
                model.DiaChiMacDinh = true;
            }

            _db.DiaChiKhachHangs.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Review), new { addressId = model.ID_DiaChiKhachHang });
        }

        // ===== B2: REVIEW =====
        public async Task<IActionResult> Review(Guid addressId)
        {
            var khId = await GetKhachHangIdAsync();
            var kh = await _db.KhachHangs.FirstAsync(x => x.ID_KhachHang == khId);
            var addr = await _db.DiaChiKhachHangs
                .FirstOrDefaultAsync(a => a.ID_DiaChiKhachHang == addressId && a.ID_KhachHang == khId);
            if (addr == null) return RedirectToAction(nameof(Address));

            var cart = await _cart.GetCartAsync(khId);

            var vm = new CheckoutReviewVM
            {
                AddressId = addressId,
                FullAddress = $"{addr.SoNha}, {addr.Xa_Phuong}, {addr.Quan_Huyen}, {addr.Tinh_ThanhPho}",
                ReceiverName = string.IsNullOrWhiteSpace(addr.HoTen) ? kh.Ten_KhachHang : addr.HoTen,
                Phone = string.IsNullOrWhiteSpace(addr.SoDienThoai) ? kh.SoDienThoai : addr.SoDienThoai,
                Items = cart.Items,
                ShippingFee = 1000,
                PaymentMethod = "COD"
            };
            return View(vm);
        }

        // ===== B3: ĐẶT HÀNG =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(PlaceOrderPost dto)
        {
            var khId = await GetKhachHangIdAsync();
            var addr = await _db.DiaChiKhachHangs
                .FirstOrDefaultAsync(a => a.ID_DiaChiKhachHang == dto.AddressId && a.ID_KhachHang == khId);
            if (addr == null) return RedirectToAction(nameof(Address));

            var cart = await _cart.GetCartAsync(khId);

            var hd = new HoaDon
            {
                ID_HoaDon = Guid.NewGuid(),
                Ma_HoaDon = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                ID_KhachHang = khId,
                HoTen = dto.ReceiverName,
                Sdt_NguoiNhan = dto.Phone,
                DiaChi = $"{addr.SoNha}, {addr.Xa_Phuong}, {addr.Quan_Huyen}, {addr.Tinh_ThanhPho}",
                HinhThucThanhToan = dto.PaymentMethod,
                PhuongThucNhanHang = "Giao hàng",
                TongTienTruocGiam = cart.Subtotal,
                TongTienSauGiam = cart.Subtotal + dto.ShippingFee,
                PhuThu = dto.ShippingFee,
                LoaiHoaDon = "Online",
                TrangThai = 0,
                NgayTao = DateTime.Now
            };
            _db.HoaDons.Add(hd);

            foreach (var i in cart.Items)
            {
                _db.HoaDonChiTiets.Add(new HoaDonChiTiet
                {
                    ID_HoaDonChiTiet = Guid.NewGuid(),
                    ID_HoaDon = hd.ID_HoaDon,
                    ID_SanPhamChiTiet = i.SanPhamChiTietId,
                    SoLuong = i.SoLuong,
                    DonGia = i.DonGia
                });

                var variant = await _db.SanPhamChiTiets.FindAsync(i.SanPhamChiTietId);
                if (variant != null) variant.SoLuong -= i.SoLuong;
            }

            await _db.SaveChangesAsync();
            await _cart.ClearAsync(khId);

            return RedirectToAction(nameof(Success), new { id = hd.ID_HoaDon });
        }

        public async Task<IActionResult> Success(Guid id)
        {
            var hd = await _db.HoaDons
                .Include(h => h.HoaDonChiTiets)
                .ThenInclude(ct => ct.SanPhamChiTiet)
                .FirstOrDefaultAsync(h => h.ID_HoaDon == id);

            if (hd == null) return RedirectToAction("Index", "GioHang");
            return View(hd);
        }
    }
}