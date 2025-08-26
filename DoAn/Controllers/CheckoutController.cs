using DoAn.Models;
using DoAn.Service.IService;
using DoAn.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace DoAn.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DoAnDbContext _db;
        private readonly IGioHangService _cart;

        // Trạng thái biến thể
        private const int TrangThaiConBan = 1;
        private const int TrangThaiHetHang = 0;

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
        // NHẬN THÊM lines để mang theo qua các bước, KHÔNG đổi logic khác
        public async Task<IActionResult> Address(string? lines)
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
                    HoTen = string.IsNullOrWhiteSpace(a.HoTen) ? kh.Ten_KhachHang : a.HoTen,
                    SoDienThoai = string.IsNullOrWhiteSpace(a.SoDienThoai) ? kh.SoDienThoai : a.SoDienThoai
                }).ToList(),
                SelectedAddressId = list.FirstOrDefault(x => x.DiaChiMacDinh)?.ID_DiaChiKhachHang
                                    ?? list.FirstOrDefault()?.ID_DiaChiKhachHang
            };

            ViewBag.Lines = lines; // mang theo
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UseAddress(Guid addressId, string? lines)
            => RedirectToAction(nameof(Review), new { addressId, lines });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(DiaChiKhachHang model, bool MakeDefault, string? lines)
        {
            var khId = await GetKhachHangIdAsync();
            var kh = await _db.KhachHangs.FirstAsync(x => x.ID_KhachHang == khId);

            model.ID_DiaChiKhachHang = Guid.NewGuid();
            model.ID_KhachHang = khId;

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

            return RedirectToAction(nameof(Review), new { addressId = model.ID_DiaChiKhachHang, lines });
        }

        // ===== B2: REVIEW =====
        // THÊM tham số lines để hiển thị đúng các dòng đã tick (nếu có)
        public async Task<IActionResult> Review(Guid addressId, string? lines)
        {
            var khId = await GetKhachHangIdAsync();
            var kh = await _db.KhachHangs.FirstAsync(x => x.ID_KhachHang == khId);
            var addr = await _db.DiaChiKhachHangs
                .FirstOrDefaultAsync(a => a.ID_DiaChiKhachHang == addressId && a.ID_KhachHang == khId);
            if (addr == null) return RedirectToAction(nameof(Address), new { lines });

            var cart = await _cart.GetCartAsync(khId);

            // --- LỌC items theo lines (nếu có). Không có lines => giữ nguyên như cũ ---
            HashSet<Guid>? selectedIds = null;
            if (!string.IsNullOrWhiteSpace(lines))
            {
                selectedIds = new HashSet<Guid>(
                    lines.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                         .Where(g => g != Guid.Empty)
                );
            }
            var items = (selectedIds == null || selectedIds.Count == 0)
                ? cart.Items
                : cart.Items.Where(x => selectedIds.Contains(x.ChiTietGioHangId)).ToList();

            if (!items.Any())
            {
                TempData["OrderError"] = "Không có sản phẩm nào được chọn.";
                return RedirectToAction("Index", "GioHang");
            }

            var vm = new CheckoutReviewVM
            {
                AddressId = addressId,
                FullAddress = $"{addr.SoNha}, {addr.Xa_Phuong}, {addr.Quan_Huyen}, {addr.Tinh_ThanhPho}",
                ReceiverName = string.IsNullOrWhiteSpace(addr.HoTen) ? kh.Ten_KhachHang : addr.HoTen,
                Phone = string.IsNullOrWhiteSpace(addr.SoDienThoai) ? kh.SoDienThoai : addr.SoDienThoai,
                Items = items,
                ShippingFee = 1000,
                PaymentMethod = "COD"
            };
            ViewBag.Lines = lines; // mang theo tiếp
            return View(vm);
        }

        // ===== B3: ĐẶT HÀNG — KHÔNG TRỪ KHO  =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(PlaceOrderPost dto, string? lines)
        {
            var khId = await GetKhachHangIdAsync();

            var addr = await _db.DiaChiKhachHangs
                .FirstOrDefaultAsync(a => a.ID_DiaChiKhachHang == dto.AddressId && a.ID_KhachHang == khId);
            if (addr == null) return RedirectToAction(nameof(Address), new { lines });

            var cart = await _cart.GetCartAsync(khId);
            if (cart == null || !cart.Items.Any())
            {
                TempData["OrderError"] = "Giỏ hàng trống.";
                return RedirectToAction(nameof(Review), new { addressId = dto.AddressId, lines });
            }

            // --- LỌC items theo lines (nếu có). Không có lines => giữ nguyên như cũ ---
            HashSet<Guid>? selectedIds = null;
            if (!string.IsNullOrWhiteSpace(lines))
            {
                selectedIds = new HashSet<Guid>(
                    lines.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                         .Where(g => g != Guid.Empty)
                );
            }
            var items = (selectedIds == null || selectedIds.Count == 0)
                ? cart.Items
                : cart.Items.Where(x => selectedIds.Contains(x.ChiTietGioHangId)).ToList();

            if (!items.Any())
            {
                TempData["OrderError"] = "Không có sản phẩm nào được chọn.";
                return RedirectToAction(nameof(Review), new { addressId = dto.AddressId, lines });
            }

            // 👉 Chỉ tạo hóa đơn + chi tiết, KHÔNG trừ kho
            var subtotal = items.Sum(x => x.ThanhTien);
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
                TongTienTruocGiam = subtotal,
                TongTienSauGiam = subtotal + dto.ShippingFee,
                PhuThu = dto.ShippingFee,
                LoaiHoaDon = "Online",
                TrangThai = 0, // ⏳ Chờ xác nhận
                NgayTao = DateTime.Now
            };
            _db.HoaDons.Add(hd);

            foreach (var i in items)
            {
                _db.HoaDonChiTiets.Add(new HoaDonChiTiet
                {
                    ID_HoaDonChiTiet = Guid.NewGuid(),
                    ID_HoaDon = hd.ID_HoaDon,
                    ID_SanPhamChiTiet = i.SanPhamChiTietId,
                    SoLuong = i.SoLuong,
                    DonGia = i.DonGia
                });
            }

            await _db.SaveChangesAsync();

            // 4) Dọn giỏ: nếu có lines -> chỉ xóa các dòng đã mua; nếu không -> xóa toàn bộ như cũ
            if (selectedIds != null && selectedIds.Count > 0)
            {
                foreach (var lineId in selectedIds)
                    await _cart.RemoveItemAsync(khId, lineId);
            }
            else
            {
                await _cart.ClearAsync(khId);
            }

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
