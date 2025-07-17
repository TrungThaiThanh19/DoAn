using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
				.Include(sp => sp.QuocGia)
				.Include(sp => sp.GioiTinh)
				.ToListAsync();
			return View(danhSachSanPham);
		}

		[HttpGet]
		public async Task<IActionResult> Search(string keyword)
		{
			if (string.IsNullOrWhiteSpace(keyword))
				return RedirectToAction("Index");

			keyword = keyword.ToLower();

			var ketQua = await _context.SanPhams
				.Include(sp => sp.ThuongHieu)
				.Include(sp => sp.QuocGia)
				.Include(sp => sp.GioiTinh)
				.Where(sp =>
					sp.TenSanPham.ToLower().Contains(keyword) ||
					sp.ThuongHieu.TenThuongHieu.ToLower().Contains(keyword) ||
					sp.QuocGia.TenQuocGia.ToLower().Contains(keyword) ||
					sp.GioiTinh.TenGioiTinh.ToLower().Contains(keyword)
				)
				.ToListAsync();

			ViewBag.TuKhoa = keyword;
			return View("Index", ketQua);
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

		[HttpGet]
		public async Task<IActionResult> Update(Guid idSanPham)
		{
			var sanPham = await _context.SanPhams
				.Include(sp => sp.ThuongHieu)
				.Include(sp => sp.QuocGia)
				.Include(sp => sp.GioiTinh)
				.Include(sp => sp.SanPhamChiTiets)
				.ThenInclude(ct => ct.TheTich) // Lấy thông tin thể tích của từng biến thể sản phẩm
				.FirstOrDefaultAsync(sp => sp.ID_SanPham == idSanPham);
			if (sanPham == null)
			{
				return NotFound();
			}
			ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus, "ID_ThuongHieu", "TenThuongHieu", sanPham.ID_ThuongHieu);
			ViewBag.QuocGiaList = new SelectList(_context.QuocGias, "ID_QuocGia", "TenQuocGia", sanPham.ID_QuocGia);
			ViewBag.GioiTinhList = new SelectList(_context.GioiTinhs, "ID_GioiTinh", "TenGioiTinh", sanPham.ID_GioiTinh);
			ViewBag.TheTichList = new SelectList(_context.TheTichs, "ID_TheTich", "TenTheTich");
			return View(sanPham);
		}
		[HttpPost]
		public async Task<IActionResult> Update(Guid idSanPham, string tenSanPham, string thoiGianLuuHuong, string moTa,
			string huongDau, string huongGiua, string huongCuoi, Guid idThuongHieu, Guid idQuocGia, Guid idGioiTinh, IFormFile hinhAnh)
		{
			ClearModelErrors("TenSanPham", "MoTa", "ThoiGianLuuHuong", "SoLuong", "HuongDau", "HuongGiua", "HuongCuoi", "GiaBan", "GiaNhap");

			var sanPham = await _context.SanPhams.FindAsync(idSanPham);
			if (sanPham == null)
				return NotFound();
			int thoiGianLuuHuongParse = 0;

			if (string.IsNullOrWhiteSpace(tenSanPham) || tenSanPham.Length > 100)
				ModelState.AddModelError("TenSanPham", "Tên sản phẩm không được để trống");

			if (string.IsNullOrWhiteSpace(moTa) || moTa.Length > 1000)
				ModelState.AddModelError("MoTa", "Mô tả không được để trống");

			if (string.IsNullOrWhiteSpace(huongDau) || huongDau.Length > 100)
				ModelState.AddModelError("HuongDau", "Hương đầu không được để trống");

			if (string.IsNullOrWhiteSpace(huongGiua) || huongGiua.Length > 100)
				ModelState.AddModelError("HuongGiua", "Hương giữa không được để trống");

			if (string.IsNullOrWhiteSpace(huongCuoi) || huongCuoi.Length > 100)
				ModelState.AddModelError("HuongCuoi", "Hương cuối không được để trống");

			if (string.IsNullOrWhiteSpace(thoiGianLuuHuong))
			{
				ModelState.AddModelError("ThoiGianLuuHuong", "Vui lòng nhập thời gian lưu hương");
			}
			else if (!int.TryParse(thoiGianLuuHuong, out thoiGianLuuHuongParse))
			{
				ModelState.AddModelError("ThoiGianLuuHuong", "Thời gian lưu hương phải là số nguyên");
			}
			else if (thoiGianLuuHuongParse < 1)
			{
				ModelState.AddModelError("ThoiGianLuuHuong", "Thời gian lưu hương phải lớn hoặc = 1");
			}

			if (idThuongHieu == Guid.Empty)
				ModelState.AddModelError("ID_ThuongHieu", "Vui lòng chọn thương hiệu");

			if (idQuocGia == Guid.Empty)
				ModelState.AddModelError("ID_QuocGia", "Vui lòng chọn quốc gia");

			if (idGioiTinh == Guid.Empty)
				ModelState.AddModelError("ID_GioiTinh", "Vui lòng chọn giới tính");

			if (hinhAnh == null || hinhAnh.Length == 0)
			{
				ModelState.Remove("HinhAnh");
			}

			if (!ModelState.IsValid)
			{
				ViewBag.ThuongHieuList = new SelectList(_context.ThuongHieus, "ID_ThuongHieu", "TenThuongHieu", idThuongHieu);
				ViewBag.QuocGiaList = new SelectList(_context.QuocGias, "ID_QuocGia", "TenQuocGia", idQuocGia);
				ViewBag.GioiTinhList = new SelectList(_context.GioiTinhs, "ID_GioiTinh", "TenGioiTinh", idGioiTinh);

				return View(sanPham);
			}

			// Nếu có ảnh mới thì thay thế
			if (hinhAnh != null && hinhAnh.Length > 0)
			{
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
				var extension = Path.GetExtension(hinhAnh.FileName).ToLower();

				if (!allowedExtensions.Contains(extension))
				{
					ModelState.AddModelError("HinhAnh", "Chỉ chấp nhận các định dạng ảnh: JPG, JPEG, PNG, WEBP.");
				}
				else
				{
					// Lưu ảnh mới
					var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(hinhAnh.FileName);
					var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
					if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

					var filePath = Path.Combine(uploadsFolder, uniqueFileName);
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await hinhAnh.CopyToAsync(stream);
					}

					// Cập nhật lại đường dẫn hình ảnh mới
					sanPham.HinhAnh = "/images/" + uniqueFileName;
				}
			}

			sanPham.TenSanPham = tenSanPham;
			sanPham.ThoiGianLuuHuong = thoiGianLuuHuongParse;
			sanPham.MoTa = moTa;
			sanPham.HuongDau = huongDau;
			sanPham.HuongGiua = huongGiua;
			sanPham.HuongCuoi = huongCuoi;
			sanPham.ID_ThuongHieu = idThuongHieu;
			sanPham.ID_QuocGia = idQuocGia;
			sanPham.ID_GioiTinh = idGioiTinh;

			_context.SanPhams.Update(sanPham);
			await _context.SaveChangesAsync();

			return RedirectToAction("Details", new { idSanPham = sanPham.ID_SanPham });
		}

		[HttpGet]
		public async Task<IActionResult> UpdateDetails(Guid id)
		{
			var chiTiet = await _context.SanPhamChiTiets
				.Include(ct => ct.SanPham)
				.Include(ct => ct.TheTich)
				.FirstOrDefaultAsync(ct => ct.ID_SanPhamChiTiet == id);

			if (chiTiet == null)
				return NotFound();

			ViewBag.TheTichList = new SelectList(_context.TheTichs, "ID_TheTich", "TenTheTich", chiTiet.ID_TheTich);
			return View(chiTiet);
		}

		[HttpPost]
		public async Task<IActionResult> UpdateDetails(Guid id, Guid idTheTich, string soLuong, string giaNhap, string giaBan)
		{
			ClearModelErrors("ID_TheTich", "SoLuong", "GiaBan", "GiaNhap");
			var chiTiet = await _context.SanPhamChiTiets
				.Include(ct => ct.SanPham)
				.FirstOrDefaultAsync(ct => ct.ID_SanPhamChiTiet == id);

			if (chiTiet == null)
				return NotFound();

			int soLuongParse = 0, giaNhapParse = 0, giaBanParse = 0;

			if (idTheTich == Guid.Empty)
				ModelState.AddModelError("ID_TheTich", "Vui lòng chọn thể tích");

			if (string.IsNullOrWhiteSpace(soLuong) || !int.TryParse(soLuong, out soLuongParse))
				ModelState.AddModelError("SoLuong", "Số lượng phải là số nguyên");
			else if (soLuongParse < 0)
				ModelState.AddModelError("SoLuong", "Số lượng phải lớn hoặc = 0");

			if (string.IsNullOrWhiteSpace(giaNhap) || !int.TryParse(giaNhap, out giaNhapParse))
				ModelState.AddModelError("GiaNhap", "Giá nhập phải là số nguyên");
			else if (giaNhapParse <= 0)
				ModelState.AddModelError("GiaNhap", "Giá nhập phải lớn hoặc = 1");

			if (string.IsNullOrWhiteSpace(giaBan) || !int.TryParse(giaBan, out giaBanParse))
				ModelState.AddModelError("GiaBan", "Giá bán phải là số nguyên");
			else if (giaBanParse <= 0)
				ModelState.AddModelError("GiaBan", "Giá bán phải lớn hoặc = 1");
			else if (giaBanParse <= giaNhapParse)
				ModelState.AddModelError("GiaBan", "Giá bán phải lớn hơn giá nhập");

			// Kiểm tra thể tích đã tồn tại trong sản phẩm cha
			bool trungTheTich = await _context.SanPhamChiTiets.AnyAsync(ct =>
				ct.ID_SanPham == chiTiet.ID_SanPham &&
				ct.ID_TheTich == idTheTich &&
				ct.ID_SanPhamChiTiet != chiTiet.ID_SanPhamChiTiet);

			if (trungTheTich)
				ModelState.AddModelError("ID_TheTich", "Sản phẩm này đã có biến thể với thể tích này");

			if (!ModelState.IsValid)
			{
				ViewBag.TheTichList = new SelectList(_context.TheTichs, "ID_TheTich", "TenTheTich", idTheTich);
				return View(chiTiet);
			}

			chiTiet.ID_TheTich = idTheTich;
			chiTiet.SoLuong = soLuongParse;
			chiTiet.GiaNhap = giaNhapParse;
			chiTiet.GiaBan = giaBanParse;

			_context.SanPhamChiTiets.Update(chiTiet);
			await _context.SaveChangesAsync();

			return RedirectToAction("Update", new { idSanPham = chiTiet.ID_SanPham });
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
			string huongDau, string huongGiua, string huongCuoi, string soLuong, string giaNhap, string giaBan, int trangThai, IFormFile hinhAnh,
			Guid idTheTich, Guid idThuongHieu, Guid idQuocGia, Guid idGioiTinh)
		{
			int thoiGianLuuHuongParse = 0, soLuongParse = 0, giaNhapParse = 0, giaBanParse = 0;
			ClearModelErrors("TenSanPham", "MoTa", "ThoiGianLuuHuong", "SoLuong", "HuongDau", "HuongGiua", "HuongCuoi", "GiaBan", "GiaNhap", "HinhAnh");
			if (string.IsNullOrWhiteSpace(tenSanPham) || tenSanPham.Length > 100)
				ModelState.AddModelError("TenSanPham", "Tên sản phẩm không được để trống");
			if (string.IsNullOrWhiteSpace(moTa) || moTa.Length > 1000)
				ModelState.AddModelError("MoTa", "Mô tả không được để trống");

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

			if (thoiGianLuuHuongParse < 1)
				ModelState.AddModelError("ThoiGianLuuHuong", "Thời gian lưu hương phải lớn hơn 0");
			if (soLuongParse < 1)
				ModelState.AddModelError("SoLuong", "Số lượng phải lớn hơn 0");
			if (giaNhapParse < 1)
				ModelState.AddModelError("GiaNhap", "Giá nhập phải lớn hơn 0");
			if (giaBanParse < 1 || giaBanParse <= giaNhapParse)
				ModelState.AddModelError("GiaBan", "Giá bán phải lớn hơn giá nhập");

			if (string.IsNullOrWhiteSpace(huongDau) || huongDau.Length > 100)
				ModelState.AddModelError("HuongDau", "Hương đầu không được để trống");
			if (string.IsNullOrWhiteSpace(huongGiua) || huongGiua.Length > 100)
				ModelState.AddModelError("HuongGiua", "Hương giữa không được để trống");
			if (string.IsNullOrWhiteSpace(huongCuoi) || huongCuoi.Length > 100)
				ModelState.AddModelError("HuongCuoi", "Hương cuối không được để trống");
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
				ModelState.AddModelError("ID_TheTich", "Vui lòng chọn thể tích hợp l");
			if (!_context.ThuongHieus.Any(th => th.ID_ThuongHieu == idThuongHieu))
				ModelState.AddModelError("ID_ThuongHieu", "Vui lòng chọn thương hiệu hợp lệ");
			if (!_context.QuocGias.Any(qg => qg.ID_QuocGia == idQuocGia))
				ModelState.AddModelError("ID_QuocGia", "Vui lòng chọn quốc gia hợp lệ");
			if (!_context.GioiTinhs.Any(gt => gt.ID_GioiTinh == idGioiTinh))
				ModelState.AddModelError("ID_GioiTinh", "Vui lòng chọn giới tính hợp lệ");

			var sanPham = await _context.SanPhams
				.Include(sp => sp.ThuongHieu)
				.Include(sp => sp.QuocGia)
				.Include(sp => sp.GioiTinh)
				.FirstOrDefaultAsync(sp => sp.TenSanPham == tenSanPham);
			if (sanPham != null)
			{
				if (sanPham.ID_ThuongHieu != idThuongHieu)
					ModelState.AddModelError("ID_ThuongHieu", $"Sản phẩm đã tồn tại với thông tin thương hiệu khác:{sanPham.ThuongHieu.TenThuongHieu}");

				if (sanPham.ID_QuocGia != idQuocGia)
					ModelState.AddModelError("ID_QuocGia", $"Sản phẩm đã tồn tại với thông tin quốc gia khác: {sanPham.QuocGia.TenQuocGia}");

				if (sanPham.ID_GioiTinh != idGioiTinh)
					ModelState.AddModelError("ID_GioiTinh", $"Sản phẩm đã tồn tại với thông tin giới tính khác: {sanPham.GioiTinh.TenGioiTinh}");

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

				bool daCoTheTich = await _context.SanPhamChiTiets
					.AnyAsync(ct => ct.ID_SanPham == sanPham.ID_SanPham && ct.ID_TheTich == idTheTich);

				if (daCoTheTich)
				{
					ModelState.AddModelError("ID_TheTich", "Sản phẩm này đã tồn tại với thể tích này rồi");
				}
			}
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
					TenSanPham = tenSanPham,
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
				TrangThai = 1,
				ID_SanPham = sanPham.ID_SanPham,
				ID_TheTich = idTheTich
			};
			_context.SanPhamChiTiets.Add(sanPhamChiTiet);
			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}

		private void ClearModelErrors(params string[] keys)
		{
			foreach (var key in keys)
			{
				if (ModelState.ContainsKey(key))
					ModelState[key].Errors.Clear();
			}
		}
	}
}
