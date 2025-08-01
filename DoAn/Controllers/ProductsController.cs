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

		public async Task<IActionResult> Index(string[] thuongHieuFilters, string[] quocGiaFilters, string[] gioiTinhFilters)
		{
			var query = _context.SanPhams
				.Include(sp => sp.ThuongHieu)
				.Include(sp => sp.QuocGia)
				.Include(sp => sp.GioiTinh)
				.AsQueryable();

			// Lọc theo Thương hiệu
			if (thuongHieuFilters != null && thuongHieuFilters.Any())
			{
				query = query.Where(sp => thuongHieuFilters.Contains(sp.ID_ThuongHieu.ToString()));
			}

			// Lọc theo Quốc gia
			if (quocGiaFilters != null && quocGiaFilters.Any())
			{
				query = query.Where(sp => quocGiaFilters.Contains(sp.ID_QuocGia.ToString()));
			}

			// Lọc theo Giới tính
			if (gioiTinhFilters != null && gioiTinhFilters.Any())
			{
				query = query.Where(sp => gioiTinhFilters.Contains(sp.ID_GioiTinh.ToString()));
			}

			// Chỉ select các trường cần thiết để tiết kiệm bộ nhớ
			var danhSachSanPham = await query
				.Select(sp => new SanPham
				{
					ID_SanPham = sp.ID_SanPham,
					Ma_SanPham = sp.Ma_SanPham,
					Ten_SanPham = sp.Ten_SanPham,
					ThoiGianLuuHuong = sp.ThoiGianLuuHuong,
					HinhAnh = sp.HinhAnh,
					ThuongHieu = sp.ThuongHieu,
					QuocGia = sp.QuocGia,
					GioiTinh = sp.GioiTinh
				})
				.ToListAsync();

			// Truyền danh sách các bộ lọc vào ViewBag
			ViewBag.ThuongHieuList = new SelectList(await _context.ThuongHieus.ToListAsync(), "ID_ThuongHieu", "Ten_ThuongHieu");
			ViewBag.QuocGiaList = new SelectList(await _context.QuocGias.ToListAsync(), "ID_QuocGia", "Ten_QuocGia");
			ViewBag.GioiTinhList = new SelectList(await _context.GioiTinhs.ToListAsync(), "ID_GioiTinh", "Ten_GioiTinh");

			// Truyền lại các giá trị đã chọn để giữ trạng thái checkbox
			ViewBag.SelectedThuongHieu = thuongHieuFilters;
			ViewBag.SelectedQuocGia = quocGiaFilters;
			ViewBag.SelectedGioiTinh = gioiTinhFilters;

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
				.Where(sp => sp.Ten_SanPham.ToLower().Contains(keyword) ||
							 sp.Ma_SanPham.ToLower().Contains(keyword) ||
							 sp.ThuongHieu.Ten_ThuongHieu.ToLower().Contains(keyword) ||
							 sp.QuocGia.Ten_QuocGia.ToLower().Contains(keyword) ||
							 sp.GioiTinh.Ten_GioiTinh.ToLower().Contains(keyword)
				)
				.ToListAsync();

			// Truyền ViewBag cho các bộ lọc
			ViewBag.ThuongHieuList = new SelectList(await _context.ThuongHieus.ToListAsync(), "ID_ThuongHieu", "Ten_ThuongHieu");
			ViewBag.QuocGiaList = new SelectList(await _context.QuocGias.ToListAsync(), "ID_QuocGia", "Ten_QuocGia");
			ViewBag.GioiTinhList = new SelectList(await _context.GioiTinhs.ToListAsync(), "ID_GioiTinh", "Ten_GioiTinh");

			// Clear bộ lọc để tránh gây hiểu nhầm khi tìm kiếm
			ViewBag.SelectedThuongHieu = Array.Empty<string>();
			ViewBag.SelectedQuocGia = Array.Empty<string>();
			ViewBag.SelectedGioiTinh = Array.Empty<string>();

			// Truyền từ khóa tìm kiếm để hiển thị lại
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
			ViewBag.ThuongHieuList = new SelectList(
				await _context.ThuongHieus
				.Where(th => th.TrangThai == 1)
				.OrderBy(th => th.Ten_ThuongHieu)
				.ToListAsync(),
				"ID_ThuongHieu", "Ten_ThuongHieu", sanPham.ID_ThuongHieu
				);
			ViewBag.QuocGiaList = new SelectList(_context.QuocGias, "ID_QuocGia", "Ten_QuocGia", sanPham.ID_QuocGia);
			ViewBag.GioiTinhList = new SelectList(_context.GioiTinhs, "ID_GioiTinh", "Ten_GioiTinh", sanPham.ID_GioiTinh);
			ViewBag.TheTichList = new SelectList(
				_context.TheTichs.Select(t => new
				{
					t.ID_TheTich,
					HienThi = t.GiaTri.ToString("0.#") + t.DonVi  // Ghép giá trị + đơn vị, ví dụ: 50ml
				}), "ID_TheTich", "HienThi"
			);
			return View(sanPham);
		}
		[HttpPost]
		public async Task<IActionResult> Update(Guid idSanPham, string tenSanPham, string thoiGianLuuHuong, string moTa,
			string huongDau, string huongGiua, string huongCuoi, Guid idThuongHieu, Guid idQuocGia, Guid idGioiTinh, IFormFile hinhAnh)
		{
			ClearModelErrors("TenSanPham", "MoTa", "ThoiGianLuuHuong", "SoLuong", "HuongDau", "HuongGiua", "HuongCuoi", "GiaBan", "GiaNhap", "Trạng thái");

			var sanPham = await _context.SanPhams
				.Include(sp => sp.ThuongHieu)
				.Include(sp => sp.QuocGia)
				.Include(sp => sp.GioiTinh)
				.Include(sp => sp.SanPhamChiTiets)
				.ThenInclude(ct => ct.TheTich) // Lấy thông tin thể tích của từng biến thể sản phẩm
				.FirstOrDefaultAsync(sp => sp.ID_SanPham == idSanPham);

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
				ViewBag.ThuongHieuList = new SelectList(
					await _context.ThuongHieus
					.Where(th => th.TrangThai == 1)
					.OrderBy(th => th.Ten_ThuongHieu)
					.ToListAsync(),
					"ID_ThuongHieu", "Ten_ThuongHieu", idThuongHieu
					);

				ViewBag.QuocGiaList = new SelectList(_context.QuocGias, "ID_QuocGia", "Ten_QuocGia", idQuocGia);
				ViewBag.GioiTinhList = new SelectList(_context.GioiTinhs, "ID_GioiTinh", "Ten_GioiTinh", idGioiTinh);

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

			sanPham.Ten_SanPham = tenSanPham;
			sanPham.ThoiGianLuuHuong = thoiGianLuuHuongParse;
			sanPham.MoTa = moTa;
			sanPham.HuongDau = huongDau;
			sanPham.HuongGiua = huongGiua;
			sanPham.HuongCuoi = huongCuoi;
			sanPham.ID_ThuongHieu = idThuongHieu;
			sanPham.ID_QuocGia = idQuocGia;
			sanPham.ID_GioiTinh = idGioiTinh;
			sanPham.NgayCapNhat = DateTime.Now;

			_context.SanPhams.Update(sanPham);
			await _context.SaveChangesAsync();

			return RedirectToAction("Details", new { idSanPham = sanPham.ID_SanPham });
		}

		[HttpGet]
		public async Task<IActionResult> UpdateDetails(Guid id)
		{
			var bienThe = await _context.SanPhamChiTiets
				.Include(ct => ct.SanPham)
				.Include(ct => ct.TheTich)
				.FirstOrDefaultAsync(ct => ct.ID_SanPhamChiTiet == id);

			if (bienThe == null)
				return NotFound();

			ViewBag.TheTichList = new SelectList(
					await _context.TheTichs
					.Where(t => t.TrangThai == 1)
					.OrderBy(t => t.GiaTri)
					.Select(t => new
					{
						t.ID_TheTich,
						HienThi = t.GiaTri.ToString("0.#") + t.DonVi
					})
					.ToListAsync(),
					"ID_TheTich", "HienThi", bienThe.ID_TheTich
					);

			// Danh sách dropdown trạng thái
			ViewBag.TrangThaiList = new SelectList(new[]
			{
				new { Value = 2, Text = "Tạm ngừng kinh doanh" },
				new { Value = 1, Text = "Còn hàng" }
			}, "Value", "Text", bienThe.TrangThai);

			return View(bienThe);
		}
		[HttpPost]
		public async Task<IActionResult> UpdateDetails(Guid id, string soLuong, string giaNhap, string giaBan, int trangThai, Guid idTheTich)
		{
			var bienThe = await _context.SanPhamChiTiets
				.Include(ct => ct.SanPham)
				.FirstOrDefaultAsync(ct => ct.ID_SanPhamChiTiet == id);

			if (bienThe == null)
				return NotFound();

			ViewBag.TrangThaiList = new SelectList(new[]
			{
				// Tạm thời ngừng bán để chờ dịp sale
				new { Value = 2, Text = "Tạm ngừng kinh doanh" },
				new { Value = 1, Text = "Còn hàng" }
			}, "Value", "Text", trangThai);

			// Parse dữ liệu
			int soLuongParse = 0;
			decimal giaNhapParse = 0, giaBanParse = 0;

			ClearModelErrors("SoLuong", "GiaNhap", "GiaBan", "ID_TheTich");

			if (string.IsNullOrWhiteSpace(soLuong))
			{
				ModelState.AddModelError("SoLuong", "Số lượng không được để trống");
			}
			else if (!int.TryParse(soLuong, out soLuongParse))
			{
				ModelState.AddModelError("SoLuong", "Số lượng phải là số nguyên dương");
			}
			else if (soLuongParse < 1)
			{
				ModelState.AddModelError("SoLuong", "Số lượng phải ≥ 1");
			}

			if (string.IsNullOrWhiteSpace(giaNhap))
			{
				ModelState.AddModelError("GiaNhap", "Giá nhập không được để trống");
			}
			else if (!decimal.TryParse(giaNhap, out giaNhapParse))
			{
				ModelState.AddModelError("GiaNhap", "Giá nhập phải là số");
			}
			else if (giaNhapParse <= 0)
			{
				ModelState.AddModelError("GiaNhap", "Giá nhập phải > 0");
			}

			if (giaNhap.Contains('.'))
				ModelState.AddModelError("GiaNhap", "Giá nhập phải sử dụng dấu phẩy (,) thay vì dấu chấm (.)");
			if (giaBan.Contains('.'))
				ModelState.AddModelError("GiaBan", "Giá bán phải sử dụng dấu phẩy (,) thay vì dấu chấm (.)");

			if (string.IsNullOrWhiteSpace(giaBan))
			{
				ModelState.AddModelError("GiaBan", "Giá bán không được để trống");
			}
			else if (!decimal.TryParse(giaBan, out giaBanParse))
			{
				ModelState.AddModelError("GiaBan", "Giá bán phải là số");
			}
			else if (giaBanParse <= 0)
			{
				ModelState.AddModelError("GiaBan", "Giá bán phải > 0");
			}
			else if (giaNhapParse > 0 && giaBanParse <= giaNhapParse)
			{
				ModelState.AddModelError("GiaBan", "Giá bán phải lớn hơn giá nhập");
			}

			// Kiểm tra trùng thể tích
			bool daCo = await _context.SanPhamChiTiets.AnyAsync(ct =>
				ct.ID_SanPham == bienThe.ID_SanPham &&
				ct.ID_TheTich == idTheTich &&
				ct.ID_SanPhamChiTiet != bienThe.ID_SanPhamChiTiet);

			if (daCo)
				ModelState.AddModelError("ID_TheTich", "Sản phẩm đã có biến thể với thể tích này");

			if (!ModelState.IsValid)
			{
				ViewBag.TheTichList = new SelectList(
					await _context.TheTichs
					.Where(t => t.TrangThai == 1)
					.OrderBy(t => t.GiaTri)
					.Select(t => new
					{
						t.ID_TheTich,
						HienThi = t.GiaTri.ToString("0.#") + t.DonVi
					})
					.ToListAsync(),
					"ID_TheTich", "HienThi"
					);

				ViewBag.TrangThaiList = new SelectList(new[]
				{
					new { Value = 2, Text = "Tạm ngừng kinh doanh" },
					new { Value = 1, Text = "Còn hàng" }
				}, "Value", "Text", trangThai);

				return View(bienThe);
			}

			bienThe.SoLuong = soLuongParse;
			bienThe.GiaNhap = giaNhapParse;
			bienThe.GiaBan = giaBanParse;
			bienThe.TrangThai = trangThai;
			bienThe.ID_TheTich = idTheTich;
			bienThe.NgayCapNhat = DateTime.Now;

			_context.SanPhamChiTiets.Update(bienThe);
			await _context.SaveChangesAsync();

			return RedirectToAction("Update", new { idSanPham = bienThe.ID_SanPham });
		}


		public async Task<IActionResult> Create()
		{
			ViewBag.ThuongHieuList = new SelectList(
				await _context.ThuongHieus
				.Where(th => th.TrangThai == 1)
				.OrderBy(th => th.Ten_ThuongHieu)
				.ToListAsync(),
				"ID_ThuongHieu", "Ten_ThuongHieu"
			);
			ViewBag.QuocGiaList = new SelectList(_context.QuocGias, "ID_QuocGia", "Ten_QuocGia");
			ViewBag.GioiTinhList = new SelectList(_context.GioiTinhs, "ID_GioiTinh", "Ten_GioiTinh");
			ViewBag.TheTichList = new SelectList(
				await _context.TheTichs
				.Where(t => t.TrangThai == 1)
				.OrderBy(t => t.GiaTri)
				.Select(t => new
				{
					t.ID_TheTich,
					HienThi = t.GiaTri.ToString("0.#") + t.DonVi
				})
				.ToListAsync(),
				"ID_TheTich", "HienThi"
				);
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Create(Guid idSanPham, Guid idSanPhamChiTiet, string tenSanPham, string maSanPham, string moTa, string thoiGianLuuHuong,
			string huongDau, string huongGiua, string huongCuoi, string soLuong, int trangThai, string giaNhap, string giaBan, IFormFile hinhAnh,
			Guid idTheTich, Guid idThuongHieu, Guid idQuocGia, Guid idGioiTinh)
		{
			int thoiGianLuuHuongParse = 0, soLuongParse = 0;
			decimal giaNhapParse = 0, giaBanParse = 0;
			ClearModelErrors("TenSanPham", "MaSanPham", "MoTa", "ThoiGianLuuHuong", "SoLuong", "HuongDau", "HuongGiua", "HuongCuoi", "GiaBan", "GiaNhap", "HinhAnh");
			
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
				ModelState.AddModelError("SoLuong", "Số lượng phải là số nguyên dương");
			}
			else if (soLuongParse < 1)
			{
				ModelState.AddModelError("SoLuong", "Số lượng phải lớn hơn 0");
			}

			if (string.IsNullOrWhiteSpace(giaNhap))
			{
				ModelState.AddModelError("GiaNhap", "Vui lòng nhập giá nhập");
			}
			else if (giaNhap.Contains("."))
			{
				ModelState.AddModelError("GiaNhap", "Giá nhập phải sử dụng dấu phẩy (,) thay vì dấu chấm (.)");
			}
			else if (!decimal.TryParse(giaNhap, out giaNhapParse))
			{
				ModelState.AddModelError("GiaNhap", "Giá nhập phải là số");
			}
			else if (giaNhapParse <= 0)
			{
				ModelState.AddModelError("GiaNhap", "Giá nhập phải > 0");
			}


			if (string.IsNullOrWhiteSpace(giaBan))
			{
				ModelState.AddModelError("GiaBan", "Vui lòng nhập giá bán");
			}
			else if (giaBan.Contains("."))
			{
				ModelState.AddModelError("GiaBan", "Giá bán phải sử dụng dấu phẩy (,) thay vì dấu chấm (.)");
			}
			else if (!decimal.TryParse(giaBan, out giaBanParse))
			{
				ModelState.AddModelError("GiaBan", "Giá bán phải là số");
			}
			else if (giaBanParse <= 0)
			{
				ModelState.AddModelError("GiaBan", "Giá bán phải > 0");
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
				ModelState.AddModelError("HuongDau", "Hương đầu không được chứa số");
			
			if (string.IsNullOrWhiteSpace(huongGiua) || huongGiua.Length > 100)
				ModelState.AddModelError("HuongGiua", "Hương giữa không được để trống và không được quá 100 ký tự");
			else if (Regex.IsMatch(huongGiua, @"\d"))
				ModelState.AddModelError("HuongGiua", "Hương giữa không được chứa số");
			
			if (string.IsNullOrWhiteSpace(huongCuoi) || huongCuoi.Length > 100)
				ModelState.AddModelError("HuongCuoi", "Hương cuối không được để trống và không được quá 100 ký tự");
			else if (Regex.IsMatch(huongCuoi, @"\d"))
				ModelState.AddModelError("HuongCuoi", "Hương cuối không được chứa số");

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

			if (string.IsNullOrWhiteSpace(maSanPham) || maSanPham.Length > 50)
			{
				ModelState.AddModelError("MaSanPham", "Mã sản phẩm không được để trống và không được quá 50 ký tự");
			}
			else if (sanPham == null) // chỉ kiểm tra mã trùng nếu là sản phẩm mới
			{
				var maTrung = await _context.SanPhams
					.AnyAsync(sp => sp.Ma_SanPham == maSanPham);

				if (maTrung)
					ModelState.AddModelError("MaSanPham", "Mã sản phẩm này đã tồn tại.");
			}

			// Nếu đã tồn tại, kiểm tra các thông tin chung, nếu khác thì báo lỗi
			if (sanPham != null)
			{
				if (sanPham.Ma_SanPham != maSanPham)
					ModelState.AddModelError("MaSanPham", $"Sản phẩm đã tồn tại với mã khác: {sanPham.Ma_SanPham}");
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
				ViewBag.ThuongHieuList = new SelectList(
					_context.ThuongHieus
					.Where(th => th.TrangThai == 1)
					.OrderBy(th => th.Ten_ThuongHieu)
					.ToList(),
					"ID_ThuongHieu", "Ten_ThuongHieu", idThuongHieu
);
				ViewBag.QuocGiaList = new SelectList(_context.QuocGias, "ID_QuocGia", "Ten_QuocGia", idQuocGia);
				ViewBag.GioiTinhList = new SelectList(_context.GioiTinhs, "ID_GioiTinh", "Ten_GioiTinh", idGioiTinh);
				ViewBag.TheTichList = new SelectList(
					await _context.TheTichs
					.Where(t => t.TrangThai == 1)
					.OrderBy(t => t.GiaTri)
					.Select(t => new
					{
						t.ID_TheTich,
						HienThi = t.GiaTri.ToString("0.#") + t.DonVi
					})
					.ToListAsync(),
					"ID_TheTich", "HienThi"
					);

				ViewBag.TenSanPham = tenSanPham;
				ViewBag.MaSanPham = maSanPham;
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
					Ma_SanPham = maSanPham,
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
