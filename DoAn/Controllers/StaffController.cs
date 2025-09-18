using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DoAn.Controllers
{
	public class StaffController : Controller
	{
		private readonly DoAnDbContext _context;
		public StaffController(DoAnDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> Index(string keyword, string[] gioiTinhFilters, string[] trangThaiFilters)
		{
			ViewBag.GioiTinhList = new SelectList(new[] {
			new { Value = "Nam", Text = "Nam" },
			new { Value = "Nữ", Text = "Nữ" },
			new { Value = "Khác", Text = "Khác" }
			}, "Value", "Text");

			ViewBag.TrangThaiList = new SelectList(new[] {
			new { Value = "1", Text = "Hoạt động" },
			new { Value = "0", Text = "Không hoạt động" }
			}, "Value", "Text");

			var query = _context.NhanViens.AsQueryable();

			// Kiểm tra nếu từ khóa tìm kiếm không rỗng
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				query = query.Where(q => q.Ten_NhanVien.Contains(keyword) ||
									q.Ma_NhanVien.Contains(keyword) ||
									q.SoDienThoai.Contains(keyword));
			}

			if (gioiTinhFilters != null && gioiTinhFilters.Length > 0)
			{
				query = query.Where(x => gioiTinhFilters.Contains(x.GioiTinh));
			}

			// Lọc theo trạng thái
			if (trangThaiFilters != null && trangThaiFilters.Length > 0)
			{
				// Ép kiểu sang int để so sánh với trường TrangThai
				var intTrangThai = trangThaiFilters.Select(x => Convert.ToInt32(x)).ToList();
				query = query.Where(x => intTrangThai.Contains(x.TrangThai));
			}

			// Sắp xếp theo tên nhân viên
			var danhSachNhanVien = await query.OrderBy(x => x.Ten_NhanVien).ToListAsync();

			ViewBag.Keyword = keyword;
			ViewBag.SelectedGioiTinh = gioiTinhFilters ?? Array.Empty<string>();
			ViewBag.SelectedTrangThai = trangThaiFilters ?? Array.Empty<string>();

			return View(danhSachNhanVien);
		}

		[HttpPost]
		public async Task<IActionResult> LockAccount(Guid id)
		{
			var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(nv => nv.ID_NhanVien == id);
			if (nhanVien != null)
			{
				nhanVien.TrangThai = 0; // Khóa tài khoản
				await _context.SaveChangesAsync();
			}
			return RedirectToAction("Details", new { id = id });
		}

		[HttpGet]
		public async Task<IActionResult> Details(Guid id)
		{
			var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(nv => nv.ID_NhanVien == id);
			if (nhanVien == null)
			{
				return NotFound();
			}
			return View(nhanVien);
		}

		[HttpGet]
		public IActionResult Create()
		{
			ViewBag.GioiTinhList = new SelectList(new[] { "Nam", "Nữ", "Khác" });
			ViewBag.TrangThaiList = new SelectList(new[] {
			new { Value = "1", Text = "Hoạt động" },
			new { Value = "0", Text = "Không hoạt động" }
			}, "Value", "Text");

			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(string username, string maNhanVien, string tenNhanVien, DateTime ngaySinh, string email, string diaChi, string gioiTinh, string soDienThoai)
		{
			ModelState.Clear();
			ViewBag.GioiTinhList = new SelectList(new[] { "Nam", "Nữ", "Khác" });
			ViewBag.TrangThaiList = new SelectList(new[] {
			new { Value = "1", Text = "Hoạt động" },
			new { Value = "0", Text = "Không hoạt động" }
			}, "Value", "Text");

			if (string.IsNullOrWhiteSpace(username))
				ModelState.AddModelError("Uername", "Tên người dùng không được để trống");
			else if (await _context.TaiKhoans.AnyAsync(tk => tk.Uername == username))
				ModelState.AddModelError("Uername", "Tên người dùng đã tồn tại");

			if (string.IsNullOrWhiteSpace(tenNhanVien))
				ModelState.AddModelError("Ten_NhanVien", "Tên nhân viên không được để trống");
			else if (!System.Text.RegularExpressions.Regex.IsMatch(tenNhanVien, @"^[\p{L}\s]+$"))
				ModelState.AddModelError("Ten_NhanVien", "Tên nhân viên chỉ được chứa chữ");

			if (ngaySinh == default)
				ModelState.AddModelError("Ngay_Sinh", "Ngày sinh không được để trống");
			else if (ngaySinh >= DateTime.Now.Date)
				ModelState.AddModelError("Ngay_Sinh", "Ngày sinh phải nhỏ hơn ngày hiện tại");

			if (string.IsNullOrWhiteSpace(email))
				ModelState.AddModelError("Email", "Email không được để trống");
			else if (!new EmailAddressAttribute().IsValid(email) || !email.Contains("."))
				ModelState.AddModelError("Email", "Email không đúng định dạng");
			else if (await _context.NhanViens.AnyAsync(nv => nv.Email == email))
				ModelState.AddModelError("Email", "Email đã tồn tại");

			if (string.IsNullOrWhiteSpace(diaChi))
				ModelState.AddModelError("Dia_Chi", "Địa chỉ không được để trống");

			if (string.IsNullOrWhiteSpace(gioiTinh))
				ModelState.AddModelError("Gioi_Tinh", "Giới tính không được để trống");

			if (string.IsNullOrWhiteSpace(soDienThoai))
				ModelState.AddModelError("So_Dien_Thoai", "Số điện thoại không được để trống");
			else if (!System.Text.RegularExpressions.Regex.IsMatch(soDienThoai, @"^0\d{9}$"))
				ModelState.AddModelError("So_Dien_Thoai", "Số điện thoại phải đủ 10 số và bắt đầu bằng số 0");
			else if (await _context.NhanViens.AnyAsync(nv => nv.SoDienThoai == soDienThoai))
				ModelState.AddModelError("So_Dien_Thoai", "Số điện thoại đã tồn tại");

			// Nếu có lỗi, hiển thị lại thông tin đã nhập
			if (!ModelState.IsValid)
			{
				ViewBag.Username = username;
				ViewBag.MaNhanVien = maNhanVien;
				ViewBag.TenNhanVien = tenNhanVien;
				ViewBag.NgaySinh = ngaySinh == default ? "" : ngaySinh.ToString("yyyy-MM-dd");
				ViewBag.Email = email;
				ViewBag.DiaChi = diaChi;
				ViewBag.GioiTinh = gioiTinh;
				ViewBag.SoDienThoai = soDienThoai;
				return View();
			}

			var taiKhoan = new TaiKhoan
			{
				ID_TaiKhoan = Guid.NewGuid(),
				Uername = username,
				Password = "123456", // Mật khẩu mặc định
				ID_Roles = Guid.Parse("A0000000-0000-0000-0000-000000000002") // Role Nhân viên
			};
			_context.TaiKhoans.Add(taiKhoan);

			var maNV = await TaoMaNhanVien();
			var nhanVien = new NhanVien
			{
				ID_NhanVien = Guid.NewGuid(),
				Ma_NhanVien = maNV,
				Ten_NhanVien = tenNhanVien,
				NgaySinh = ngaySinh,
				Email = email,
				DiaChiLienHe = diaChi,
				GioiTinh = gioiTinh,
				SoDienThoai = soDienThoai,
				NgayThamGia = DateTime.Now,
				TrangThai = 1,
				ID_TaiKhoan = taiKhoan.ID_TaiKhoan
			};
			_context.NhanViens.Add(nhanVien);
			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}

		private async Task<string> TaoMaNhanVien()
		{
			// Tìm mã lớn nhất hiện tại
			var maxMaNhanVien = await _context.NhanViens
				.Where(nv => nv.Ma_NhanVien.StartsWith("NV"))
				.OrderByDescending(nv => nv.Ma_NhanVien)
				.Select(nv => nv.Ma_NhanVien)
				.FirstOrDefaultAsync();

			int soTiepTheo = 1;
			if (!string.IsNullOrEmpty(maxMaNhanVien))
			{
				// Cắt phần số phía sau NV
				var soStr = maxMaNhanVien.Substring(2);
				if (int.TryParse(soStr, out int so))
					soTiepTheo = so + 1;
			}

			// Format: NV + số, tối đa 4 số
			var maNhanVien = "NV" + soTiepTheo.ToString("D4"); // D4: 0001, 0002,...
			return maNhanVien;
		}

	}
}
