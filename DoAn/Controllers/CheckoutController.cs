using DoAn.Models;
using DoAn.Service.IService;
using DoAn.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using DoAn.IService;
using DoAn.Service;

namespace DoAn.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DoAnDbContext _db;
        private readonly IGioHangService _cart;
        private readonly IHoaDonService _hoaDonService;

        private const int TrangThaiConBan = 1;
        private const int TrangThaiHetHang = 0;

        public CheckoutController(DoAnDbContext db, IGioHangService cart, IHoaDonService hoaDonService)
        {
            _db = db;
            _hoaDonService = hoaDonService;
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

        // Hàm tính phí ship
        private int TinhPhiShip(decimal subtotal, string tinhThanh)
        {
            if (subtotal >= 500000) return 0; // free ship nếu >= 500k
            if (!string.IsNullOrEmpty(tinhThanh) && tinhThanh.Contains("Hà Nội"))
                return 10000;
            if (!string.IsNullOrEmpty(tinhThanh) && tinhThanh.Contains("Hồ Chí Minh"))
                return 15000;
            return 20000; // mặc định
        }
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

                    // fallback: nếu chưa nhập thì dùng tên/sđt KH
                    HoTen = string.IsNullOrWhiteSpace(a.HoTen) ? kh.Ten_KhachHang : a.HoTen,
                    SoDienThoai = string.IsNullOrWhiteSpace(a.SoDienThoai) ? kh.SoDienThoai : a.SoDienThoai
                }).ToList(),
                SelectedAddressId = list.FirstOrDefault(x => x.DiaChiMacDinh)?.ID_DiaChiKhachHang
                                    ?? list.FirstOrDefault()?.ID_DiaChiKhachHang
            };

            ViewBag.Lines = lines;
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

            // Nếu có lỗi validation → quay lại view
            if (!ModelState.IsValid)
            {
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

                ViewBag.Lines = lines;
                return View("Address", vm);
            }

            // Gán ID và FK
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
        public async Task<IActionResult> Review(Guid addressId, string? lines)
        {
            var khId = await GetKhachHangIdAsync();
            var kh = await _db.KhachHangs.FirstAsync(x => x.ID_KhachHang == khId);
            var addr = await _db.DiaChiKhachHangs
                .FirstOrDefaultAsync(a => a.ID_DiaChiKhachHang == addressId && a.ID_KhachHang == khId);
            if (addr == null) return RedirectToAction(nameof(Address), new { lines });

            var cart = await _cart.GetCartAsync(khId);

            HashSet<Guid>? selectedIds = null;
            if (!string.IsNullOrWhiteSpace(lines))
            {
                selectedIds = new HashSet<Guid>(
                    lines.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                         .Where(g => g != Guid.Empty)
                );
            }
            var rawItems = (selectedIds == null || selectedIds.Count == 0)
                ? cart.Items
                : cart.Items.Where(x => selectedIds.Contains(x.ChiTietGioHangId)).ToList();

            if (!rawItems.Any())
            {
                TempData["OrderError"] = "Không có sản phẩm nào được chọn.";
                return RedirectToAction("Index", "GioHang");
            }

            // 🔥 Map lại để lấy MaSanPham từ bảng SanPham
            var items = new List<GioHangItemVMD>();
            foreach (var i in rawItems)
            {
                var spct = await _db.SanPhamChiTiets
                    .Include(ct => ct.SanPham)
                    .Include(ct => ct.TheTich)
                    .FirstOrDefaultAsync(ct => ct.ID_SanPhamChiTiet == i.SanPhamChiTietId);

                if (spct == null) continue;

                items.Add(new GioHangItemVMD
                {
                    ChiTietGioHangId = i.ChiTietGioHangId,
                    SanPhamChiTietId = i.SanPhamChiTietId,
                    MaSanPham = spct.SanPham.Ma_SanPham,   // 👈 lấy mã sản phẩm
                    TenSanPham = spct.SanPham.Ten_SanPham,
                    TheTich = spct.TheTich != null
    ? $"{spct.TheTich.GiaTri} {spct.TheTich.DonVi}"
    : null,

                    HinhAnh = spct.SanPham.HinhAnh,
                    DonGia = i.DonGia,
                    SoLuong = i.SoLuong,
                    ThanhTien = i.ThanhTien,
                    TonKho = spct.SoLuong
                });
            }

            var subtotal = items.Sum(x => x.ThanhTien);
            var shipping = TinhPhiShip(subtotal, addr.Tinh_ThanhPho);

            var vm = new CheckoutReviewVM
            {
                AddressId = addressId,
                FullAddress = $"{addr.SoNha}, {addr.Xa_Phuong}, {addr.Quan_Huyen}, {addr.Tinh_ThanhPho}",
                ReceiverName = string.IsNullOrWhiteSpace(addr.HoTen) ? kh.Ten_KhachHang : addr.HoTen,
                Phone = string.IsNullOrWhiteSpace(addr.SoDienThoai) ? kh.SoDienThoai : addr.SoDienThoai,
                Items = items,
                ShippingFee = shipping,
                PaymentMethod = "COD"
            };
            ViewBag.Lines = lines;
            return View(vm);
        }


        // ===== B3: ĐẶT HÀNG =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(PlaceOrderPost dto, string? lines)
        {
            var khId = await GetKhachHangIdAsync();

            // --- kiểm tra địa chỉ ---
            var addr = await _db.DiaChiKhachHangs
                .FirstOrDefaultAsync(a => a.ID_DiaChiKhachHang == dto.AddressId && a.ID_KhachHang == khId);
            if (addr == null) return RedirectToAction(nameof(Address), new { lines });

            // --- kiểm tra giỏ hàng ---
            var cart = await _cart.GetCartAsync(khId);
            if (cart == null || !cart.Items.Any())
            {
                TempData["OrderError"] = "Giỏ hàng trống.";
                return RedirectToAction(nameof(Review), new { addressId = dto.AddressId, lines });
            }

            // --- lọc sản phẩm được chọn ---
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

            // === TÍNH TOÁN ===
            decimal preDiscountTotal = 0m;
            decimal afterDiscountTotal = 0m;
            decimal totalDiscount = 0m;

            foreach (var item in items)
            {
                var spct = await _db.SanPhamChiTiets
                    .Include(x => x.ChiTietKhuyenMais)
                        .ThenInclude(ctkm => ctkm.KhuyenMai)
                    .FirstOrDefaultAsync(x => x.ID_SanPhamChiTiet == item.SanPhamChiTietId);

                if (spct == null) continue;

                var lineSubtotal = item.SoLuong * item.DonGia;
                decimal lineTotalAfterDiscount = lineSubtotal;

                // Tìm khuyến mãi còn hiệu lực
                var km = spct.ChiTietKhuyenMais
                    .Where(ctkm => ctkm.KhuyenMai.NgayBatDau <= DateTime.Now
                                && ctkm.KhuyenMai.NgayHetHan >= DateTime.Now
                                && ctkm.KhuyenMai.TrangThai == 1)
                    .Select(ctkm => ctkm.KhuyenMai)
                    .FirstOrDefault();

                if (km != null)
                {
                    decimal giam = 0m;

                    if (km.KieuGiamGia == "percent")
                        giam = lineSubtotal * (km.GiaTriGiam / 100m);
                    else if (km.KieuGiamGia == "amount")
                        giam = km.GiaTriGiam * item.SoLuong;

                    // Giới hạn giảm giá
                    if (km.GiaTriToiDa > 0 && giam > km.GiaTriToiDa)
                        giam = km.GiaTriToiDa;

                    lineTotalAfterDiscount -= giam;
                    totalDiscount += giam;
                }

                if (lineTotalAfterDiscount < 0) lineTotalAfterDiscount = 0;

                preDiscountTotal += lineSubtotal;
                afterDiscountTotal += lineTotalAfterDiscount;
            }

            var shipping = TinhPhiShip(afterDiscountTotal, addr.Tinh_ThanhPho);

            // === TẠO HOÁ ĐƠN ===
            var hd = new HoaDon
            {
                ID_HoaDon = Guid.NewGuid(),
                Ma_HoaDon = _hoaDonService.GenerateMaHoaDon(),
                ID_KhachHang = khId,
                HoTen = dto.ReceiverName,
                Sdt_NguoiNhan = dto.Phone,
                DiaChi = $"{addr.SoNha}, {addr.Xa_Phuong}, {addr.Quan_Huyen}, {addr.Tinh_ThanhPho}",
                HinhThucThanhToan = dto.PaymentMethod,
                PhuongThucNhanHang = "Giao hàng",
                TongTienTruocGiam = preDiscountTotal,
                TongTienGiam = totalDiscount, // 👈 dùng đúng tổng đã cộng
                TongTienSauGiam = afterDiscountTotal + shipping,
                PhuThu = shipping,
                LoaiHoaDon = "Online",
                TrangThai = 0,
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

            // Xoá sản phẩm khỏi giỏ
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
                        .ThenInclude(spct => spct.SanPham) // 👈 thêm Include SanPham
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(spct => spct.TheTich) // 👈 thêm Include Thể tích
                .FirstOrDefaultAsync(h => h.ID_HoaDon == id);

            if (hd == null) return RedirectToAction("Index", "GioHang");
            return View(hd);
        }
    }
}