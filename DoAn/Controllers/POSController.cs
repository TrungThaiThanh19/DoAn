using DoAn.Migrations;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

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
				.Where(x => x.SanPhamChiTiets.Any(ct => ct.TrangThai == 1 && ct.SoLuong > 0))
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
				.Where(ct => ct.ID_SanPham == idSanPham && ct.TrangThai == 1 && ct.SoLuong > 0)
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

			if (voucher == null)
				return Json(new { success = false, message = "Mã giảm giá không tồn tại" });

			if (voucher.TrangThai != 1)
				return Json(new { success = false, message = "Mã giảm giá chưa được kích hoạt" });

			if (DateTime.Now > voucher.NgayHetHan)
				return Json(new { success = false, message = "Mã giảm giá đã hết hạn sử dụng" });

			if (voucher.SoLuong <= 0)
				return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng" });

			if (tongTienHang < voucher.GiaTriToiThieu)
				return Json(new { success = false, message = $"Đơn hàng chưa đạt tối thiểu {voucher.GiaTriToiThieu:N0} đ để áp dụng mã." });

			// Tính số tiền giảm
			decimal giamGia = 0;

			if (voucher.KieuGiamGia == "Phần trăm")
			{
				// Tính phần trăm trước, sau đó giới hạn theo Giá trị tối đa
				decimal tienGiam = tongTienHang * (voucher.GiaTriGiam / 100m);
				giamGia = Math.Min(tienGiam, voucher.GiaTriToiDa);
			}
			else if (voucher.KieuGiamGia == "Tiền mặt")
			{
				// Nếu là kiểu "tiền mặt" thì giảm đúng số tiền đó
				giamGia = voucher.GiaTriGiam;
			}
			else
			{
				return Json(new { success = false, message = "Kiểu giảm giá không hợp lệ" });
			}

			return Json(new { success = true, giamGia = giamGia, idVoucher = voucher.ID_Voucher });
		}


		// Hàm sinh mã ngẫu nhiên
		private string TaoMaNgauNhien(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		}


		[HttpGet]
		public async Task<IActionResult> DanhSachHoaDonCho()
		{
			var ds = await _context.HoaDons
				.Where(x => x.TrangThai == 0)
				.OrderByDescending(x => x.NgayTao)
				.Select(x => new
				{
					idHoaDon = x.ID_HoaDon,
					maHoaDon = x.Ma_HoaDon,
					phuongThucNhanHang = x.PhuongThucNhanHang,
					loaiHoaDon = x.LoaiHoaDon,
					ngayTao = x.NgayTao,
					trangThai = x.TrangThai == 0 ? "Chưa thanh toán" : "Đã thanh toán"
				})
				.ToListAsync();

			return Json(ds);
		}


		[HttpGet]
		public async Task<IActionResult> LayChiTietHoaDon(Guid idHoaDon)
		{
			var dsChiTiet = await _context.HoaDonChiTiets
				.Include(ct => ct.SanPhamChiTiet).ThenInclude(spct => spct.SanPham)
				.Include(ct => ct.SanPhamChiTiet).ThenInclude(spct => spct.TheTich)
				.Where(ct => ct.ID_HoaDon == idHoaDon)
				.Select(ct => new {
					TenSanPham = ct.SanPhamChiTiet.SanPham.Ten_SanPham,
					TheTich = ct.SanPhamChiTiet.TheTich.GiaTri.ToString("0.#") + ct.SanPhamChiTiet.TheTich.DonVi,
					DonGia = ct.DonGia,
					SoLuong = ct.SoLuong,
					ThanhTien = ct.SoLuong * ct.DonGia,
					ID_SanPhamChiTiet = ct.ID_SanPhamChiTiet,
					ID_HoaDonChiTiet = ct.ID_HoaDonChiTiet
				})
				.ToListAsync();

			return Json(dsChiTiet);
		}



		[HttpPost]
		public async Task<IActionResult> TaoHoaDonCho([FromBody] TaoHoaDonChoRequest model)
		{
			try
			{
				var soHoaDonCho = await _context.HoaDons.CountAsync(x => x.TrangThai == 0);
				if (soHoaDonCho >= 5)
				{
					return Json(new { success = false, message = "Chỉ được phép tạo tối đa 5 hóa đơn chờ! Vui lòng xóa bớt hóa đơn chờ trước khi tạo mới" });
				}

				string maHoaDon;
				do { maHoaDon = TaoMaNgauNhien(10); }
				while (await _context.HoaDons.AnyAsync(x => x.Ma_HoaDon == maHoaDon));

				var idHoaDon = Guid.NewGuid();
				var hoaDon = new HoaDon
				{
					ID_HoaDon = idHoaDon,
					Ma_HoaDon = maHoaDon,
					PhuongThucNhanHang = "Nhận tại quầy",
					LoaiHoaDon = "Offline",
					NgayTao = DateTime.Now,
					TrangThai = 0,
					HinhThucThanhToan = "Chưa xác định"
				};
				var chiTiet = new HoaDonChiTiet
				{
					ID_HoaDonChiTiet = Guid.NewGuid(),
					ID_HoaDon = idHoaDon,
					ID_SanPhamChiTiet = model.ID_SanPhamChiTiet,
					SoLuong = model.SoLuong,
					DonGia = model.DonGia
				};
				hoaDon.HoaDonChiTiets = new List<HoaDonChiTiet> { chiTiet };
				_context.HoaDons.Add(hoaDon);

				await _context.SaveChangesAsync();

				return Json(new { success = true, idHoaDon = hoaDon.ID_HoaDon, maHoaDon = hoaDon.Ma_HoaDon });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message, detail = ex.InnerException?.Message });
			}
		}


		[HttpPost]
		public async Task<IActionResult> TaoHoaDonChoRong()
		{
			try
			{
				// Giới hạn số lượng hóa đơn chờ tối đa (nếu cần)
				var soHoaDonCho = await _context.HoaDons.CountAsync(x => x.TrangThai == 0);
				if (soHoaDonCho >= 5)
				{
					return Json(new { success = false, message = "Chỉ được tạo tối đa 5 hóa đơn chờ. Vui lòng xóa bớt hóa đơn chờ trước khi tạo mới." });
				}

				string maHoaDon;
				do { maHoaDon = TaoMaNgauNhien(10); }
				while (await _context.HoaDons.AnyAsync(x => x.Ma_HoaDon == maHoaDon));

				var idHoaDon = Guid.NewGuid();
				var hoaDon = new HoaDon
				{
					ID_HoaDon = idHoaDon,
					Ma_HoaDon = maHoaDon,
					PhuongThucNhanHang = "Nhận tại quầy",
					LoaiHoaDon = "Offline",
					NgayTao = DateTime.Now,
					TrangThai = 0,
					HinhThucThanhToan = "Chưa xác định"
					// KHÔNG thêm chi tiết nào!
				};

				_context.HoaDons.Add(hoaDon);
				await _context.SaveChangesAsync();

				return Json(new { success = true, idHoaDon = hoaDon.ID_HoaDon, maHoaDon = hoaDon.Ma_HoaDon });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message, detail = ex.InnerException?.Message });
			}
		}



		[HttpPost]
		public async Task<IActionResult> ThemSanPhamVaoHoaDonCho([FromBody] ThemSanPhamVaoHoaDonChoRequest model)
		{
			var hoaDon = await _context.HoaDons
				.Include(hd => hd.HoaDonChiTiets)
				.FirstOrDefaultAsync(hd => hd.ID_HoaDon == model.ID_HoaDon && hd.TrangThai == 0);

			if (hoaDon == null)
				return Json(new { success = false, message = "Không tìm thấy hóa đơn chờ" });

			// Lấy tồn kho thực tế của biến thể
			var spct = await _context.SanPhamChiTiets.FirstOrDefaultAsync(x => x.ID_SanPhamChiTiet == model.ID_SanPhamChiTiet);
			if (spct == null || spct.TrangThai != 1)
				return Json(new { success = false, message = "Biến thể không hợp lệ hoặc đã ngừng kinh doanh" });

			// Kiểm tra tổng số lượng đã có trong hóa đơn
			var chiTiet = hoaDon.HoaDonChiTiets.FirstOrDefault(ct => ct.ID_SanPhamChiTiet == model.ID_SanPhamChiTiet);
			int soLuongHienTai = chiTiet?.SoLuong ?? 0;
			int soLuongMoi = soLuongHienTai + model.SoLuong;

			if (soLuongMoi > spct.SoLuong)
				return Json(new { success = false, message = "Vượt quá số lượng tồn kho" });

			if (chiTiet != null)
			{
				chiTiet.SoLuong = soLuongMoi;
				_context.HoaDonChiTiets.Update(chiTiet);
			}
			else
			{
				chiTiet = new HoaDonChiTiet
				{
					ID_HoaDonChiTiet = Guid.NewGuid(),
					ID_HoaDon = hoaDon.ID_HoaDon,
					ID_SanPhamChiTiet = model.ID_SanPhamChiTiet,
					SoLuong = model.SoLuong,
					DonGia = spct.GiaBan
				};
				_context.HoaDonChiTiets.Add(chiTiet);
			}

			await _context.SaveChangesAsync();
			return Json(new { success = true });
		}


		[HttpPost]
		public async Task<IActionResult> CapNhatSoLuongHoaDonChiTiet([FromBody] CapNhatSoLuongRequest req)
		{
			if (req.SoLuong <= 0)
				return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });

			var chiTiet = await _context.HoaDonChiTiets
				.Include(ct => ct.SanPhamChiTiet)
				.FirstOrDefaultAsync(ct =>
					ct.ID_HoaDonChiTiet == req.ID_HoaDonChiTiet &&
					ct.HoaDon.TrangThai == 0);

			if (chiTiet == null)
				return Json(new { success = false, message = "Không tìm thấy dòng hóa đơn cần cập nhật." });

			// Kiểm tra tồn kho 
			if (req.SoLuong > chiTiet.SanPhamChiTiet.SoLuong)
				return Json(new { success = false, message = "Vượt quá số lượng tồn kho." });

			chiTiet.SoLuong = req.SoLuong;
			_context.HoaDonChiTiets.Update(chiTiet);
			await _context.SaveChangesAsync();

			return Json(new { success = true, message = "Đã cập nhật số lượng." });
		}



		[HttpPost]
		public async Task<IActionResult> XoaHoaDonChiTiet([FromBody] XoaHoaDonChiTietRequest req)
		{
			var chiTiet = await _context.HoaDonChiTiets
				.Include(ct => ct.HoaDon)
				.FirstOrDefaultAsync(ct =>
					ct.ID_HoaDonChiTiet == req.ID_HoaDonChiTiet &&
					ct.HoaDon.TrangThai == 0);

			if (chiTiet == null)
				return Json(new { success = false, message = "Không tìm thấy dòng hóa đơn để xóa." });

			_context.HoaDonChiTiets.Remove(chiTiet);
			await _context.SaveChangesAsync();

			// Option: Nếu hóa đơn không còn dòng nào thì xóa luôn hóa đơn chờ (dọn sạch)
			var hd = chiTiet.HoaDon;
			bool isEmpty = !await _context.HoaDonChiTiets.AnyAsync(x => x.ID_HoaDon == hd.ID_HoaDon);
			if (isEmpty)
			{
				_context.HoaDons.Remove(hd);
				await _context.SaveChangesAsync();
			}

			return Json(new { success = true, message = "Đã xóa sản phẩm khỏi hóa đơn chờ." });
		}


		[HttpPost]
		public async Task<IActionResult> XoaHoaDonCho([FromBody] Guid idHoaDon)
		{
			var hoaDon = await _context.HoaDons
				.Include(x => x.HoaDonChiTiets)
				.FirstOrDefaultAsync(x => x.ID_HoaDon == idHoaDon && x.TrangThai == 0);

			if (hoaDon == null)
				return Json(new { success = false, message = "Không tìm thấy hóa đơn chờ cần xóa!" });

			// Xóa chi tiết trước
			_context.HoaDonChiTiets.RemoveRange(hoaDon.HoaDonChiTiets);
			_context.HoaDons.Remove(hoaDon);

			await _context.SaveChangesAsync();

			return Json(new { success = true, message = "Đã xóa hóa đơn chờ." });
		}



		[HttpPost]
		public async Task<IActionResult> Pay([FromBody] HoaDonRequest model)
		{
			try
			{
				if (model == null || model.HoaDonChiTiets == null || !model.HoaDonChiTiets.Any())
					return Json(new { success = false, message = "Hóa đơn không được để trống!" });

				if (string.IsNullOrWhiteSpace(model.HinhThucThanhToan))
					return Json(new { success = false, message = "Vui lòng chọn phương thức thanh toán!" });

				if (!string.IsNullOrEmpty(model.HoTen) && !System.Text.RegularExpressions.Regex.IsMatch(model.HoTen, @"^[\p{L}\s'.-]+$"))
					return Json(new { success = false, message = "Tên khách hàng không hợp lệ" });

				if (!string.IsNullOrEmpty(model.Sdt_NguoiNhan) && !System.Text.RegularExpressions.Regex.IsMatch(model.Sdt_NguoiNhan, @"^0\d{9}$"))
					return Json(new { success = false, message = "Số điện thoại không hợp lệ" });

				if (!string.IsNullOrEmpty(model.Email) && !new EmailAddressAttribute().IsValid(model.Email))
					return Json(new { success = false, message = "Email không đúng định dạng" });

				if (model.PhuThu.HasValue && model.PhuThu.Value <= 0)
					return Json(new { success = false, message = "Phụ thu phải lớn hơn 0" });

				// Mặc định phương thức nhận hàng là “Nhận tại quầy”
				string phuongThucNhanHang = "Nhận tại quầy";

				// Validate và xử lý Mã giảm giá (cho phép null)
				Voucher? voucher = null;
				decimal giamGia = 0;
				if (model.ID_Voucher.HasValue)
				{
					voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.ID_Voucher == model.ID_Voucher.Value);

					if (voucher == null)
						return Json(new { success = false, message = "Mã giảm giá không tồn tại" });

					if (voucher.TrangThai != 1)
						return Json(new { success = false, message = "Mã giảm giá chưa được kích hoạt" });

					if (DateTime.Now > voucher.NgayHetHan)
						return Json(new { success = false, message = "Mã giảm giá đã hết hạn sử dụng" });

					if (voucher.SoLuong <= 0)
						return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng" });

					if (model.TongTienTruocGiam < voucher.GiaTriToiThieu)
						return Json(new { success = false, message = $"Đơn hàng chưa đạt tối thiểu {voucher.GiaTriToiThieu:N0}đ để áp dụng mã." });

					// Tính số tiền giảm
					if (voucher.KieuGiamGia == "Phần trăm")
					{
						decimal tienGiam = model.TongTienTruocGiam * (voucher.GiaTriGiam / 100m);
						giamGia = Math.Min(tienGiam, voucher.GiaTriToiDa);
					}
					else if (voucher.KieuGiamGia == "Tiền mặt")
					{
						giamGia = voucher.GiaTriGiam;
					}
					else
					{
						return Json(new { success = false, message = "Kiểu giảm giá không hợp lệ" });
					}
				}

				// Kiểm tra tồn kho sản phẩm
				foreach (var item in model.HoaDonChiTiets)
				{
					var spct = await _context.SanPhamChiTiets.FindAsync(item.ID_SanPhamChiTiet);
					if (spct == null || spct.TrangThai == 0)
						return Json(new { success = false, message = "Có sản phẩm trong hóa đơn không đủ số lượng tồn kho!" });
					if (item.SoLuong > spct.SoLuong)
						return Json(new { success = false, message = "Có sản phẩm trong hóa đơn không đủ số lượng tồn kho!" });
				}

				HoaDon hoaDon;
				bool laHoaDonCho = model.ID_HoaDon != Guid.Empty;

				if (laHoaDonCho)
				{
					// Tìm hóa đơn chờ đã có, chỉ update
					hoaDon = await _context.HoaDons
						.Include(hd => hd.HoaDonChiTiets)
						.FirstOrDefaultAsync(hd => hd.ID_HoaDon == model.ID_HoaDon);

					if (hoaDon == null)
						return Json(new { success = false, message = "Không tìm thấy hóa đơn chờ để thanh toán!" });

					// Update các trường thông tin khách hàng
					hoaDon.HoTen = model.HoTen;
					hoaDon.Email = model.Email;
					hoaDon.Sdt_NguoiNhan = model.Sdt_NguoiNhan;
					hoaDon.DiaChi = model.DiaChi;
					hoaDon.HinhThucThanhToan = model.HinhThucThanhToan;
					hoaDon.PhuThu = model.PhuThu;
					hoaDon.GhiChu = model.GhiChu;
					hoaDon.ID_Voucher = voucher?.ID_Voucher;
					hoaDon.TongTienTruocGiam = model.TongTienTruocGiam;
					hoaDon.TongTienSauGiam = model.TongTienTruocGiam - giamGia + (model.PhuThu ?? 0);
					hoaDon.TrangThai = 1; // Đã thanh toán
					hoaDon.NgayCapNhat = DateTime.Now;
					hoaDon.PhuongThucNhanHang = phuongThucNhanHang;

					// Trừ tồn kho sản phẩm
					foreach (var item in model.HoaDonChiTiets)
					{
						var spct = await _context.SanPhamChiTiets.FindAsync(item.ID_SanPhamChiTiet);
						spct.SoLuong -= item.SoLuong;
						if (spct.SoLuong <= 0)
						{
							spct.SoLuong = 0;
							spct.TrangThai = 0;
						}
						_context.SanPhamChiTiets.Update(spct);
					}
				}
				else
				{
					// Tạo hóa đơn mới 
					hoaDon = new HoaDon
					{
						ID_HoaDon = Guid.NewGuid(),
						Ma_HoaDon = TaoMaNgauNhien(10),
						HoTen = model.HoTen,
						Email = model.Email,
						Sdt_NguoiNhan = model.Sdt_NguoiNhan,
						DiaChi = model.DiaChi,
						HinhThucThanhToan = model.HinhThucThanhToan,
						PhuongThucNhanHang = phuongThucNhanHang,
						TongTienTruocGiam = model.TongTienTruocGiam,
						TongTienSauGiam = model.TongTienTruocGiam - giamGia + (model.PhuThu ?? 0),
						PhuThu = model.PhuThu,
						GhiChu = model.GhiChu,
						ID_Voucher = voucher?.ID_Voucher,
						LoaiHoaDon = "Offline",
						NgayTao = DateTime.Now,
						TrangThai = 1 // Đã thanh toán
					};

					// Tạo chi tiết hóa đơn + trừ tồn kho
					hoaDon.HoaDonChiTiets = new List<HoaDonChiTiet>();
					foreach (var item in model.HoaDonChiTiets)
					{
						var spct = await _context.SanPhamChiTiets.FindAsync(item.ID_SanPhamChiTiet);
						spct.SoLuong -= item.SoLuong;
						if (spct.SoLuong <= 0)
						{
							spct.SoLuong = 0;
							spct.TrangThai = 0;
						}
						_context.SanPhamChiTiets.Update(spct);

						hoaDon.HoaDonChiTiets.Add(new HoaDonChiTiet
						{
							ID_HoaDonChiTiet = Guid.NewGuid(),
							ID_HoaDon = hoaDon.ID_HoaDon,
							ID_SanPhamChiTiet = item.ID_SanPhamChiTiet,
							SoLuong = item.SoLuong,
							DonGia = item.DonGia
						});
					}

					_context.HoaDons.Add(hoaDon);
				}

				// Trừ lượt sử dụng của voucher (nếu có)
				if (voucher != null)
				{
					voucher.SoLuong -= 1;
					_context.Vouchers.Update(voucher);
				}

				await _context.SaveChangesAsync();
				var hoaDonDayDu = await _context.HoaDons
					.Include(hd => hd.HoaDonChiTiets)
					.ThenInclude(ct => ct.SanPhamChiTiet)
					.ThenInclude(spct => spct.SanPham)
					.Include(hd => hd.HoaDonChiTiets)
					.ThenInclude(ct => ct.SanPhamChiTiet)
					.ThenInclude(spct => spct.TheTich)
					.FirstOrDefaultAsync(hd => hd.ID_HoaDon == hoaDon.ID_HoaDon);

				var fileName = XuatHoaDonPdf(hoaDonDayDu); // hoaDon là object đã thanh toán
				var fileUrl = Url.Content($"~/hoadon/{fileName}");

				// Trả kết quả thành công kèm mã hóa đơn
				return Json(new
				{
					success = true,
					message = "Thanh toán thành công!",
					maHoaDon = hoaDon.Ma_HoaDon,
					fileName = fileName,
					fileUrl = fileUrl // đường dẫn truy cập từ web
				});

			}
			catch (Exception ex)
			{
				var detail = ex.InnerException?.Message ?? ex.Message;
				return Json(new { success = false, message = "Có lỗi xảy ra khi thanh toán!", detail });
			}
		}


		public string XuatHoaDonPdf(HoaDon hoaDon)
		{
			QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
			var maHoaDon = hoaDon.Ma_HoaDon;
			var fileName = $"{maHoaDon}.pdf";
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/hoadon", fileName);

			// Nếu chưa có thư mục thì tạo
			var dir = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

			// Tính lại tiền ưu đãi đúng logic (đặc biệt khi có phụ thu)
			decimal uuDai = hoaDon.TongTienTruocGiam - hoaDon.TongTienSauGiam + (hoaDon.PhuThu ?? 0);

			// Render PDF
			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Margin(40);
					page.Size(PageSizes.A4);

					page.Header()
						.Text($"HÓA ĐƠN THANH TOÁN\nMã hóa đơn: {maHoaDon}")
						.FontSize(20).Bold().AlignCenter();

					page.Content()
						.PaddingVertical(10)
						.Column(col =>
						{
							col.Item().Text($"Ngày mua: {hoaDon.NgayTao:dd/MM/yyyy HH:mm}");
							col.Item().Text($"Tên khách hàng: {hoaDon.HoTen ?? "Khách lẻ"}");
							col.Item().Text($"Số điện thoại: {hoaDon.Sdt_NguoiNhan}");
							col.Item().Text($"Email: {hoaDon.Email ?? ""}");
							col.Item().Text($"Địa chỉ: {hoaDon.DiaChi}");
							col.Item().Text($"Phương thức thanh toán: {hoaDon.HinhThucThanhToan ?? ""}");
							col.Item().Text($"Phương thức nhận hàng: {hoaDon.PhuongThucNhanHang ?? ""}");
							if (!string.IsNullOrEmpty(hoaDon.GhiChu))
								col.Item().Text($"Ghi chú: {hoaDon.GhiChu}");

							col.Item().LineHorizontal(1);

							// Chi tiết hóa đơn
							col.Item().Table(tbl =>
							{
								tbl.ColumnsDefinition(cols =>
								{
									cols.RelativeColumn();
									cols.RelativeColumn();
									cols.RelativeColumn();
									cols.RelativeColumn();
								});

								tbl.Header(header =>
								{
									header.Cell().Text("Sản phẩm").Bold();
									header.Cell().Text("Thể tích").Bold();
									header.Cell().Text("Số lượng").Bold();
									header.Cell().Text("Thành tiền").AlignRight();
								});

								foreach (var ct in hoaDon.HoaDonChiTiets)
								{
									tbl.Cell().Text(ct.SanPhamChiTiet?.SanPham?.Ten_SanPham ?? "");
									tbl.Cell().Text($"{ct.SanPhamChiTiet?.TheTich?.GiaTri:0.#}{ct.SanPhamChiTiet?.TheTich?.DonVi}");
									tbl.Cell().Text(ct.SoLuong.ToString());
									tbl.Cell().Text($"{(ct.SoLuong * ct.DonGia):n0} đ").AlignRight();
								}
							});

							col.Item().LineHorizontal(1);
							col.Item().AlignRight().Text($"Tổng tiền: {hoaDon.TongTienTruocGiam:n0} đ");
							col.Item().AlignRight().Text($"Ưu đãi: {uuDai:n0} đ");
							col.Item().AlignRight().Text($"Phụ thu: {(hoaDon.PhuThu ?? 0):n0} đ");
							col.Item().AlignRight().Text($"Tổng cộng: {hoaDon.TongTienSauGiam:n0} đ").Bold();
						});

					page.Footer().Text("Cảm ơn quý khách!").AlignCenter();
				});
			});

			document.GeneratePdf(filePath);
			return fileName;
		}



		// DTO nhận từ frontend
		public class HoaDonRequest
		{
			public Guid ID_HoaDon { get; set; }
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


		public class XoaHoaDonChiTietRequest
		{
			public Guid ID_HoaDonChiTiet { get; set; }
		}


		public class CapNhatSoLuongRequest
		{
			public Guid ID_HoaDonChiTiet { get; set; }
			public int SoLuong { get; set; }
		}


		public class TaoHoaDonChoRequest
		{
			public Guid ID_SanPhamChiTiet { get; set; }
			public int SoLuong { get; set; }
			public decimal DonGia { get; set; }
		}


		public class ThemSanPhamVaoHoaDonChoRequest
		{
			public Guid ID_HoaDon { get; set; }
			public Guid ID_SanPhamChiTiet { get; set; }
			public int SoLuong { get; set; }
		}
	}
}