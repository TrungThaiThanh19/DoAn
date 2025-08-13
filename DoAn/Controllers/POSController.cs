using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
	public class POSController : Controller
	{
		private readonly DoAnDbContext _context;
		public POSController(DoAnDbContext context)
		{
			_context = context;
		}


		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var danhSachSanPham = await _context.SanPhams
				.Include(x => x.ThuongHieu)
				.Include(x => x.QuocGia)
				.Include(x => x.GioiTinh)
				.Include(x => x.SanPhamChiTiets)
				.Where(x => x.SanPhamChiTiets.Any(ct => ct.TrangThai == 1))
				.OrderBy(x => x.Ten_SanPham)
				.ToListAsync();

			ViewBag.SanPhamList = danhSachSanPham;
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> GetVariantsByProductId(Guid idSanPham)
		{
			var danhSachBienThe = await _context.SanPhamChiTiets
				.Include(ct => ct.TheTich)
				.Include(ct => ct.SanPham)
				.Where(ct => ct.ID_SanPham == idSanPham && ct.TrangThai == 1)
				.Select(ct => new {
					id_SanPhamChiTiet = ct.ID_SanPhamChiTiet,
					TenSanPham = ct.SanPham.Ten_SanPham,
					TheTich = ct.TheTich.GiaTri.ToString("0.#") + ct.TheTich.DonVi,
					ct.GiaBan,
					ct.SoLuong
				})
				.ToListAsync();

			return Json(danhSachBienThe);
		}

		[HttpGet]
		public async Task<IActionResult> CheckVoucher(string maGiamGia, decimal tongTienHang)
		{
			if (string.IsNullOrWhiteSpace(maGiamGia))
				return Json(new { success = false, message = "Mã giảm giá không được bỏ trống." });

			var voucher = await _context.Vouchers
				.FirstOrDefaultAsync(v => v.Ma_Voucher == maGiamGia);

			if (voucher == null || voucher.TrangThai != 1)
				return Json(new { success = false, message = "Mã giảm giá không tồn tại hoặc đã hết hạn." });

			if (DateTime.Now < voucher.NgayTao)
				return Json(new { success = false, message = "Mã giảm giá chưa được kích hoạt." });

			if (DateTime.Now > voucher.NgayHetHan)
				return Json(new { success = false, message = "Mã giảm giá đã hết hạn sử dụng." });

			if (voucher.SoLuong <= 0)
				return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng." });

			if (tongTienHang < voucher.GiaTriToiThieu)
				return Json(new { success = false, message = $"Đơn hàng chưa đạt tối thiểu {voucher.GiaTriToiThieu:N0} đ để áp dụng mã." });

			// Tính số tiền giảm
			decimal giamGia = 0;

			if (voucher.KieuGiamGia == "Phần trăm")
			{
				// Tính phần trăm trước, sau đó giới hạn theo Giá trị tối đa
				decimal tienGiam = tongTienHang * (voucher.GiaTriGiam / 100m); // thêm 'm' để ép kiểu decimal
				giamGia = Math.Min(tienGiam, voucher.GiaTriToiDa);
			}
			else
			{
				// Nếu là kiểu "tiền mặt" thì giảm đúng số tiền đó
				giamGia = voucher.GiaTriGiam;
			}

			return Json(new { success = true, giamGia = giamGia, idVoucher = voucher.ID_Voucher });
		}


		[HttpPost]
		public async Task<IActionResult> Pay([FromBody] HoaDonRequest model)
		{
			if (model.HoaDonChiTiets == null || !model.HoaDonChiTiets.Any())
				return Json(new { success = false, message = "Hóa đơn không có sản phẩm" });

			if (string.IsNullOrWhiteSpace(model.HinhThucThanhToan))
				return Json(new { success = false, message = "Vui lòng chọn phương thức thanh toán" });

			// Tự sinh mã hóa đơn duy nhất
			string maHoaDon;
			do
			{
				maHoaDon = TaoMaNgauNhien(10);
			}
			while (await _context.HoaDons.AnyAsync(x => x.Ma_HoaDon == maHoaDon));

			var hoaDon = new HoaDon
			{
				ID_HoaDon = Guid.NewGuid(),
				Ma_HoaDon = maHoaDon,
				HoTen = string.IsNullOrWhiteSpace(model.HoTen) ? null : model.HoTen,
				Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email,
				Sdt_NguoiNhan = string.IsNullOrWhiteSpace(model.Sdt_NguoiNhan) ? null : model.Sdt_NguoiNhan,
				DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi,
				HinhThucThanhToan = model.HinhThucThanhToan,
				PhuongThucNhanHang = "Nhận tại quầy",
				TongTienTruocGiam = model.TongTienTruocGiam,
				TongTienSauGiam = model.TongTienSauGiam,
				PhuThu = model.PhuThu ?? 0,
				LoaiHoaDon = "Offline",
				GhiChu = string.IsNullOrWhiteSpace(model.GhiChu) ? null : model.GhiChu,
				NgayTao = DateTime.Now,
				TrangThai = 1,
				ID_Voucher = model.ID_Voucher
			};

			hoaDon.HoaDonChiTiets = model.HoaDonChiTiets.Select(ct => new HoaDonChiTiet
			{
				ID_HoaDonChiTiet = Guid.NewGuid(),
				ID_HoaDon = hoaDon.ID_HoaDon,
				ID_SanPhamChiTiet = ct.ID_SanPhamChiTiet,
				SoLuong = ct.SoLuong,
				DonGia = ct.DonGia
			}).ToList();

			_context.HoaDons.Add(hoaDon);
			await _context.SaveChangesAsync();

			return Json(new { success = true, message = "Thanh toán thành công!", maHoaDon });
		}

		// Hàm sinh mã ngẫu nhiên
		private string TaoMaNgauNhien(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		}

		// DTO nhận từ frontend
		public class HoaDonRequest
		{
			public string? HoTen { get; set; }
			public string? Email { get; set; }
			public string? Sdt_NguoiNhan { get; set; }
			public string? DiaChi { get; set; }
			public string HinhThucThanhToan { get; set; }
			public decimal TongTienTruocGiam { get; set; }
			public decimal TongTienSauGiam { get; set; }
			public decimal? PhuThu { get; set; }
			public string? GhiChu { get; set; }
			public Guid? ID_Voucher { get; set; }
			public List<HoaDonChiTietRequest> HoaDonChiTiets { get; set; }
		}

		public class HoaDonChiTietRequest
		{
			public Guid ID_SanPhamChiTiet { get; set; }
			public int SoLuong { get; set; }
			public decimal DonGia { get; set; }
		}
	}
}