using DoAn.Migrations;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
			if (!TryValidateModel(model))
			{
				var allErrors = ModelState
					.Where(kv => kv.Value.Errors.Count > 0)
					.SelectMany(kv => kv.Value.Errors.Select(e => e.ErrorMessage))
					.Distinct()
					.ToList();

				return Json(new { success = false, message = string.Join(" | ", allErrors) });
			}

			// Chỉ chấp nhận 2 giá trị thanh toán có sẵn
			var paymentsMethod = new[] { "Tiền mặt", "Chuyển khoản" };
			if (!paymentsMethod.Contains(model.HinhThucThanhToan))
				return Json(new { success = false, message = "Hình thức thanh toán không hợp lệ." });

			// Chỉ chấp nhận phụ thu lớn hơn 0 (nếu có nhập)
			if (model.PhuThu.HasValue && model.PhuThu.Value <= 0)
				return Json(new { success = false, message = "Phụ thu phải lớn hơn 0 đ" });

			// Lấy danh sách SPCT theo các ID client gửi lên
			var ids = model.HoaDonChiTiets.Select(x => x.ID_SanPhamChiTiet).ToList();
			var spctsCalc = await _context.SanPhamChiTiets
				.Where(x => ids.Contains(x.ID_SanPhamChiTiet))
				.Select(x => new { x.ID_SanPhamChiTiet, x.GiaBan, x.SoLuong, x.TrangThai })
				.ToListAsync();

			if (spctsCalc.Count != ids.Count || spctsCalc.Any(x => x.TrangThai != 1))
				return Json(new { success = false, message = "Có sản phẩm không tồn tại hoặc ngừng bán." });

			// Tính tổng tiền trước giảm và kiểm tra số lượng tồn kho
			decimal tongTruocGiam = 0;
			foreach (var ct in model.HoaDonChiTiets)
			{
				var dbItem = spctsCalc.First(x => x.ID_SanPhamChiTiet == ct.ID_SanPhamChiTiet);

				if (ct.SoLuong > dbItem.SoLuong)
					return Json(new { success = false, message = "Số lượng mua vượt tồn kho." });

				tongTruocGiam += dbItem.GiaBan * ct.SoLuong;
			}

			// Kiểm tra voucher: trạng thái, thời gian hiệu lực, còn lượt, đạt mức tối thiểu
			decimal giamGiaServer = 0m;
			DoAn.Models.Voucher? voucherEntity = null;
			if (model.ID_Voucher.HasValue)
			{
				voucherEntity = await _context.Vouchers.FirstOrDefaultAsync(v => v.ID_Voucher == model.ID_Voucher);
				if (voucherEntity == null || voucherEntity.TrangThai != 1
					|| DateTime.Now < voucherEntity.NgayTao
					|| DateTime.Now > voucherEntity.NgayHetHan
					|| voucherEntity.SoLuong <= 0
					|| tongTruocGiam < voucherEntity.GiaTriToiThieu)
				{
					return Json(new { success = false, message = "Voucher không hợp lệ hoặc không áp dụng được" });
				}

				// Nếu là kiểu giảm giá %, tính số tiền giảm theo % và giới hạn theo giá trị tối đa
				if (voucherEntity.KieuGiamGia == "Phần trăm")
				{
					var tienGiam = tongTruocGiam * (voucherEntity.GiaTriGiam / 100m);
					giamGiaServer = Math.Min(tienGiam, voucherEntity.GiaTriToiDa);
				}
				else // Giảm trực tiếp
				{
					giamGiaServer = voucherEntity.GiaTriGiam;
				}
			}

			// Nếu client không nhập phụ thu thì mặc định là 0
			var phuThuServer = model.PhuThu ?? 0m;
			var tongSauGiamServer = tongTruocGiam - giamGiaServer + phuThuServer;

			// Đối chiếu số tiền client gửi với server (dung sai 0.5đ để tránh vấn đề làm tròn)
			if (Math.Abs((double)(model.TongTienTruocGiam - tongTruocGiam)) > 0.5 ||
				Math.Abs((double)(model.TongTienSauGiam - tongSauGiamServer)) > 0.5)
			{
				return Json(new
				{
					success = false,
					message = "Số tiền không hợp lệ"
				});
			}

			// Sinh mã hoá đơn ngẫu nhiên và duy nhất
			string maHoaDon;
			do { maHoaDon = TaoMaNgauNhien(10); }
			while (await _context.HoaDons.AnyAsync(x => x.Ma_HoaDon == maHoaDon));

			// Trừ số lượng SPCT + trừ số lượng voucher + tạo hoá đơn
			await using var tx = await _context.Database.BeginTransactionAsync();
			try
			{
				var spctsUpdate = await _context.SanPhamChiTiets
					.Where(x => ids.Contains(x.ID_SanPhamChiTiet))
					.ToListAsync();

				foreach (var ct in model.HoaDonChiTiets)
				{
					var ent = spctsUpdate.First(x => x.ID_SanPhamChiTiet == ct.ID_SanPhamChiTiet);

					if (ent.TrangThai != 1)
						return Json(new { success = false, message = $"Sản phẩm đã ngừng bán: {ent.ID_SanPhamChiTiet}" });

					if (ct.SoLuong > ent.SoLuong)
						return Json(new { success = false, message = "Số lượng mua vượt tồn kho (vừa có thay đổi tồn). Vui lòng thử lại." });
				}

				if (voucherEntity != null)
				{
					var voucherTracked = await _context.Vouchers
						.FirstOrDefaultAsync(v => v.ID_Voucher == voucherEntity.ID_Voucher);

					if (voucherTracked == null
						|| voucherTracked.TrangThai != 1
						|| DateTime.Now < voucherTracked.NgayTao
						|| DateTime.Now > voucherTracked.NgayHetHan
						|| voucherTracked.SoLuong <= 0
						|| tongTruocGiam < voucherTracked.GiaTriToiThieu)
					{
						return Json(new { success = false, message = "Voucher không còn hợp lệ. Vui lòng áp dụng lại." });
					}

					voucherTracked.SoLuong -= 1; // trừ 1 lượt dùng voucher
					_context.Vouchers.Update(voucherTracked);
				}

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
					TongTienTruocGiam = tongTruocGiam,
					TongTienSauGiam = tongSauGiamServer,
					PhuThu = phuThuServer,
					LoaiHoaDon = "Offline",
					GhiChu = string.IsNullOrWhiteSpace(model.GhiChu) ? null : model.GhiChu,
					NgayTao = DateTime.Now,
					TrangThai = 1,
					ID_Voucher = model.ID_Voucher
				};

				hoaDon.HoaDonChiTiets = model.HoaDonChiTiets.Select(ct =>
				{
					var dbItem = spctsCalc.First(x => x.ID_SanPhamChiTiet == ct.ID_SanPhamChiTiet);
					return new HoaDonChiTiet
					{
						ID_HoaDonChiTiet = Guid.NewGuid(),
						ID_HoaDon = hoaDon.ID_HoaDon,
						ID_SanPhamChiTiet = ct.ID_SanPhamChiTiet,
						SoLuong = ct.SoLuong,
						DonGia = dbItem.GiaBan
					};
				}).ToList();

				_context.HoaDons.Add(hoaDon);

				// Trừ tồn kho từng SPCT
				foreach (var ct in model.HoaDonChiTiets)
				{
					var ent = spctsUpdate.First(x => x.ID_SanPhamChiTiet == ct.ID_SanPhamChiTiet);
					ent.SoLuong -= ct.SoLuong;
					_context.SanPhamChiTiets.Update(ent);
				}

				await _context.SaveChangesAsync();
				await tx.CommitAsync();

				return Json(new { success = true, message = "Thanh toán thành công!", maHoaDon });
			}
			catch (DbUpdateConcurrencyException)
			{
				await tx.RollbackAsync();
				return Json(new { success = false, message = "Có thay đổi đồng thời" });
			}
			catch (Exception)
			{
				await tx.RollbackAsync();
				return Json(new { success = false, message = "Có lỗi khi tạo hoá đơn" });
			}
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
			[RegularExpression(@"^[\p{L}\s'.-]+$", ErrorMessage = "Tên không hợp lệ")]
			[StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
			public string? HoTen { get; set; }
			[EmailAddress(ErrorMessage = "Email không hợp lệ")]
			public string? Email { get; set; }
			[RegularExpression(@"^0\d{9}$", ErrorMessage = "SĐT không hợp lệ")]
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