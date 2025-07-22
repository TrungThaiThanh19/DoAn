using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DoAn.Controllers
{
	public class ProductsController : Controller
	{
		private readonly DoAnDbContext _context;
		public ProductsController(DoAnDbContext context)
		{
			_context = context;
		}
		public async Task<IActionResult> Index()
		{
			var danhSachSanPham = await _context.SanPhams
				.Include(sp => sp.ThuongHieu)
				.Include(sp => sp.GioiTinh)
				.Include(sp => sp.QuocGia)
				.ToListAsync();
			return View(danhSachSanPham);
		}

		[HttpGet]
		public async Task<IActionResult> Details(Guid idSanPham)
		{
			var sanPham = await _context.SanPhams
				.Include(sp => sp.ThuongHieu)
				.Include(sp => sp.QuocGia)
				.Include(sp => sp.GioiTinh)
				.Include(sp => sp.SanPhamChiTiets)
				.ThenInclude(ct => ct.TheTich)  // Lấy thông tin thể tích của từng biến thể sản phẩm
				.FirstOrDefaultAsync(sp => sp.ID_SanPham == idSanPham);

			if (sanPham == null)
			{
				return NotFound();
			}

			return View(sanPham);
		}

		public async Task<IActionResult> Create()
		{
			ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus, "ID_ThuongHieu", "TenThuongHieu");
			ViewBag.QuocGiaList = new SelectList(_context.QuocGias, "ID_QuocGia", "TenQuocGia");
			ViewBag.GioiTinhList = new SelectList(_context.GioiTinhs, "ID_GioiTinh", "TenGioiTinh");
			ViewBag.TheTichList = new SelectList(_context.TheTichs, "ID_TheTich", "TenTheTich");
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(Guid idSanPham, Guid idSanPhamChiTiet, string tenSanPham, string moTa, string thoiGianLuuHuong,
			string huongDau, string huongGiua, string huongCuoi, string soLuong, string giaNhap, string giaBan, IFormFile hinhAnh,
			Guid idTheTich, Guid idThuongHieu, Guid idQuocGia, Guid idGioiTinh)
		{
			int thoiGianLuuHuongParse = 0, soLuongParse = 0, giaNhapParse = 0, giaBanParse = 0;
			ClearModelErrors("TenSanPham", "MoTa", "ThoiGianLuuHuong", "SoLuong", "HuongDau", "HuongGiua", "HuongCuoi", "GiaBan", "GiaNhap", "HinhAnh");
			
			// Nếu thời gian lưu hương bỏ trống hoặc nhập toàn khoảng trắng thì báo lỗi
			if (string.IsNullOrWhiteSpace(thoiGianLuuHuong))
			{
				ModelState.AddModelError("ThoiGianLuuHuong", "Vui lòng nhập thời gian lưu hương");
			}
			// Chuyển đổi giá trị từ form sang int, nếu không chuyển được thì báo lỗi
			else if (!int.TryParse(thoiGianLuuHuong, out thoiGianLuuHuongParse))
			{
				ModelState.AddModelError("ThoiGianLuuHuong", "Thời gian lưu hương phải là số nguyên");
			}
			else if (thoiGianLuuHuongParse < 1)
			{
				ModelState.AddModelError("ThoiGianLuuHuong", "Thời gian lưu hương phải lớn hơn hoặc = 1");
			}


			if (string.IsNullOrWhiteSpace(soLuong))
			{
				ModelState.AddModelError("SoLuong", "Vui lòng nhập số lượng");
			}
			else if (!int.TryParse(soLuong, out soLuongParse))
			{
				ModelState.AddModelError("SoLuong", "Số lượng phải là số nguyên");
			}
			else if (soLuongParse < 1)
			{
				ModelState.AddModelError("SoLuong", "Số lượng phải lớn hơn 0");
			}

			if (string.IsNullOrWhiteSpace(giaNhap))
			{
				ModelState.AddModelError("GiaNhap", "Vui lòng nhập giá nhập");
			}
			else if (!int.TryParse(giaNhap, out giaNhapParse))
			{
				ModelState.AddModelError("GiaNhap", "Giá nhập phải là số nguyên");
			}
			else if (giaNhapParse < 1)
			{
				ModelState.AddModelError("GiaNhap", "Giá nhập phải lớn hơn 0");
			}


			if (string.IsNullOrWhiteSpace(giaBan))
			{
				ModelState.AddModelError("GiaBan", "Vui lòng nhập giá bán");
			}
			else if (!int.TryParse(giaBan, out giaBanParse))
			{
				ModelState.AddModelError("GiaBan", "Giá bán phải là số nguyên");
			}
			else if (giaBanParse < 1)
			{
				ModelState.AddModelError("GiaBan", "Giá bán phải lớn hơn 0");
			}
			else if (giaBanParse <= giaNhapParse)
			{
				ModelState.AddModelError("GiaBan", "Giá bán phải lớn hơn giá nhập");
			}

			if (string.IsNullOrEmpty(tenSanPham) || tenSanPham.Length > 100)
				ModelState.AddModelError("TenSanPham", "Tên sản phẩm không được để trống và không được quá 100 ký tự");
			if (string.IsNullOrWhiteSpace(moTa) || moTa.Length > 1000)
				ModelState.AddModelError("MoTa", "Mô tả không được để trống và không được quá 1000 ký tự");
			
			if (string.IsNullOrWhiteSpace(huongDau) || huongDau.Length > 100)
				ModelState.AddModelError("HuongDau", "Hương đầu không được để trống và không được quá 100 ký tự");
			// Kiểm tra xem hương đầu có chứa số hay không
			else if (Regex.IsMatch(huongDau, @"\d"))
			{
				ModelState.AddModelError("HuongDau", "Hương đầu không được chứa số");
			}
			
			if (string.IsNullOrWhiteSpace(huongGiua) || huongGiua.Length > 100)
				ModelState.AddModelError("HuongGiua", "Hương giữa không được để trống và không được quá 100 ký tự");
			else if (Regex.IsMatch(huongGiua, @"\d"))
			{
				ModelState.AddModelError("HuongGiua", "Hương giữa không được chứa số");
			}
			
			if (string.IsNullOrWhiteSpace(huongCuoi) || huongCuoi.Length > 100)
				ModelState.AddModelError("HuongCuoi", "Hương cuối không được để trống và không được quá 100 ký tự");
			else if (Regex.IsMatch(huongCuoi, @"\d"))
			{
				ModelState.AddModelError("HuongCuoi", "Hương cuối không được chứa số");
			}

			if (idTheTich == Guid.Empty)
				ModelState.AddModelError("ID_TheTich", "Vui lòng chọn thể tích");
			if (idThuongHieu == Guid.Empty)
				ModelState.AddModelError("ID_ThuongHieu", "Vui lòng chọn thương hiệu");
			if (idQuocGia == Guid.Empty)
				ModelState.AddModelError("ID_QuocGia", "Vui lòng chọn quốc gia");
			if (idGioiTinh == Guid.Empty)
				ModelState.AddModelError("ID_GioiTinh", "Vui lòng chọn giới tính");
			if (hinhAnh == null || hinhAnh.Length == 0)
				ModelState.AddModelError("HinhAnh", "Vui lòng chọn ảnh cho sản phẩm");
			if (!_context.TheTichs.Any(t => t.ID_TheTich == idTheTich))
				ModelState.AddModelError("ID_TheTich", "Vui lòng chọn thể tích hợp lệ");
			if (!_context.ThuongHieus.Any(th => th.ID_ThuongHieu == idThuongHieu))
				ModelState.AddModelError("ID_ThuongHieu", "Vui lòng chọn thương hiệu hợp lệ");
			if (!_context.QuocGias.Any(qg => qg.ID_QuocGia == idQuocGia))
				ModelState.AddModelError("ID_QuocGia", "Vui lòng chọn quốc gia hợp lệ");
			if (!_context.GioiTinhs.Any(gt => gt.ID_GioiTinh == idGioiTinh))
				ModelState.AddModelError("ID_GioiTinh", "Vui lòng chọn giới tính hợp lệ");

			// Kiểm tra tên sp đã tồn tại hay chưa
			var sanPham = await _context.SanPhams
				.Include(sp => sp.ThuongHieu)
				.Include(sp => sp.QuocGia)
				.Include(sp => sp.GioiTinh)
				.FirstOrDefaultAsync(sp => sp.Ten_SanPham == tenSanPham);
			// Nếu đã tồn tại, kiểm tra các thông tin chung, nếu khác thì báo lỗi
			if (sanPham != null)
			{
				if (sanPham.ID_ThuongHieu != idThuongHieu)
					ModelState.AddModelError("ID_ThuongHieu", $"Sản phẩm đã tồn tại với thông tin thương hiệu khác:{sanPham.ThuongHieu.Ten_ThuongHieu}");

				if (sanPham.ID_QuocGia != idQuocGia)
					ModelState.AddModelError("ID_QuocGia", $"Sản phẩm đã tồn tại với thông tin quốc gia khác: {sanPham.QuocGia.Ten_QuocGia}");

				if (sanPham.ID_GioiTinh != idGioiTinh)
					ModelState.AddModelError("ID_GioiTinh", $"Sản phẩm đã tồn tại với thông tin giới tính khác: {sanPham.GioiTinh.Ten_GioiTinh}");

				if (sanPham.ThoiGianLuuHuong != thoiGianLuuHuongParse)
					ModelState.AddModelError("ThoiGianLuuHuong", $"Sản phẩm đã tồn tại với thời gian lưu hương khác: {sanPham.ThoiGianLuuHuong}");

				if (sanPham.MoTa != moTa)
					ModelState.AddModelError("MoTa", $"Sản phẩm đã tồn tại với mô tả khác: {sanPham.MoTa}");

				if (sanPham.HuongDau != huongDau)
					ModelState.AddModelError("HuongDau", $"Sản phẩm đã tồn tại với hương đầu khác: {sanPham.HuongDau}");

				if (sanPham.HuongGiua != huongGiua)
					ModelState.AddModelError("HuongGiua", $"Sản phẩm đã tồn tại với hương giữa khác: {sanPham.HuongGiua}");

				if (sanPham.HuongCuoi != huongCuoi)
					ModelState.AddModelError("HuongCuoi", $"Sản phẩm đã tồn tại với hương cuối khác: {sanPham.HuongCuoi}");

				// Kiểm tra xem sản phẩm đã có thể tích này chưa
				bool daCoTheTich = await _context.SanPhamChiTiets
					.AnyAsync(ct => ct.ID_SanPham == sanPham.ID_SanPham && ct.ID_TheTich == idTheTich);

				if (daCoTheTich)
				{
					ModelState.AddModelError("ID_TheTich", "Sản phẩm này đã tồn tại với thể tích này rồi");
				}
			}
			// Nếu có lỗi thì trả về View với các thông tin đã nhập
			if (!ModelState.IsValid)
			{
				ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus, "ID_ThuongHieu", "TenThuongHieu", idThuongHieu);
				ViewBag.QuocGiaList = new SelectList(_context.QuocGias, "ID_QuocGia", "TenQuocGia", idQuocGia);
				ViewBag.GioiTinhList = new SelectList(_context.GioiTinhs, "ID_GioiTinh", "TenGioiTinh", idGioiTinh);
				ViewBag.TheTichList = new SelectList(_context.TheTichs, "ID_TheTich", "TenTheTich", idTheTich);

				ViewBag.TenSanPham = tenSanPham;
				ViewBag.ThoiGianLuuHuong = thoiGianLuuHuong;
				ViewBag.MoTa = moTa;
				ViewBag.HuongDau = huongDau;
				ViewBag.HuongGiua = huongGiua;
				ViewBag.HuongCuoi = huongCuoi;
				ViewBag.SoLuong = soLuong;
				ViewBag.GiaNhap = giaNhap;
				ViewBag.GiaBan = giaBan;
				return View();
			}
			// Xử lý ảnh
			string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(hinhAnh.FileName);
			var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
			if (!Directory.Exists(uploadsFolder))
				Directory.CreateDirectory(uploadsFolder);

			var filePath = Path.Combine(uploadsFolder, uniqueFileName);
			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await hinhAnh.CopyToAsync(stream);
			}

			if (sanPham == null)
			{
				sanPham = new SanPham()
				{
					ID_SanPham = Guid.NewGuid(),
					Ten_SanPham = tenSanPham,
					ThoiGianLuuHuong = thoiGianLuuHuongParse,
					MoTa = moTa,
					HuongDau = huongDau,
					HuongGiua = huongGiua,
					HuongCuoi = huongCuoi,
					HinhAnh = "/images/" + uniqueFileName,
					ID_ThuongHieu = idThuongHieu,
					ID_QuocGia = idQuocGia,
					ID_GioiTinh = idGioiTinh
				};
				_context.SanPhams.Add(sanPham);
				await _context.SaveChangesAsync();
			}
			var sanPhamChiTiet = new SanPhamChiTiet()
			{
				ID_SanPhamChiTiet = Guid.NewGuid(),
				SoLuong = soLuongParse,
				GiaBan = giaBanParse,
				GiaNhap = giaNhapParse,
				NgayTao = DateTime.Now,
				ID_SanPham = sanPham.ID_SanPham,
				ID_TheTich = idTheTich
			};
			_context.SanPhamChiTiets.Add(sanPhamChiTiet);
			await _context.SaveChangesAsync();
			return RedirectToAction("Index");
		}
		private void ClearModelErrors(params string[] keys) // Hàm xóa lỗi mặc định
		{
			foreach (var key in keys)
			{
				if (ModelState.ContainsKey(key))
					ModelState[key].Errors.Clear();
			}
		}
	}
}
