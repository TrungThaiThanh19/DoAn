using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly DoAnDbContext _db;
        public NhanVienController(DoAnDbContext db)
        {
            _db = db;
        }

        // GET: NhanVien
        public async Task<IActionResult> Index(string tenSearch, string sdtSearch, string manvSearch, int? trangThaiFilter)
        {
            var query = _db.NhanViens.AsQueryable();

            if (!string.IsNullOrEmpty(tenSearch))
                query = query.Where(nv => nv.Ten_NhanVien.Contains(tenSearch));

            if (!string.IsNullOrEmpty(sdtSearch))
                query = query.Where(nv => nv.SoDienThoai.Contains(sdtSearch));

            if (!string.IsNullOrEmpty(manvSearch))
                query = query.Where(nv => nv.Ma_NhanVien.Contains(manvSearch));

            if (trangThaiFilter.HasValue)
                query = query.Where(nv => nv.TrangThai == trangThaiFilter.Value);

            // Dropdown trạng thái
            ViewBag.TrangThaiList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "Tất cả", Value = "" },
                new SelectListItem { Text = "Hoạt động", Value = "1" },
                new SelectListItem { Text = "Bị khóa", Value = "0" }
            }, "Value", "Text", trangThaiFilter?.ToString());

            return View(await query.ToListAsync());
        }

        // GET: NhanVien/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVien nv)
        {
            // 1. Tìm role nhân viên
            var roleNhanVien = _db.Roles.FirstOrDefault(r => r.Ma_Roles == "NV");
            if (roleNhanVien == null)
            {
                ModelState.AddModelError("", "Không tìm thấy role nhân viên.");
                return View(nv);
            }

            // 2. Tạo tài khoản
            var taiKhoan = new TaiKhoan
            {
                ID_TaiKhoan = Guid.NewGuid(),
                Uername = nv.Email,
                Password = "123456", // mật khẩu mặc định
                Roles = roleNhanVien,
                ID_Roles = roleNhanVien.ID_Roles
            };

            _db.TaiKhoans.Add(taiKhoan);
            await _db.SaveChangesAsync();

            // 3. Gán tài khoản cho nhân viên
            nv.ID_TaiKhoan = taiKhoan.ID_TaiKhoan;
            _db.NhanViens.Add(nv);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Kiểm tra email
        [HttpGet]
        public JsonResult IsEmailAvailable(string email)
        {
            bool exists = _db.NhanViens.Any(nv => nv.Email == email);
            return Json(!exists);
        }

        // AJAX: Kiểm tra số điện thoại
        [HttpGet]
        public JsonResult IsPhoneAvailable(string soDienThoai)
        {
            bool exists = _db.NhanViens.Any(nv => nv.SoDienThoai == soDienThoai);
            return Json(!exists);
        }

        // GET: NhanVien/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var nv = await _db.NhanViens
                .Include(n => n.TaiKhoan)
                .FirstOrDefaultAsync(m => m.ID_NhanVien == id);
            if (nv == null) return NotFound();
            return View(nv);
        }

        // GET: NhanVien/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var nv = await _db.NhanViens.FindAsync(id);
            if (nv == null) return NotFound();
            return View(nv);
        }

        // POST: NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, NhanVien nv)
        {
            if (id != nv.ID_NhanVien) return NotFound();



            try
            {
                _db.Update(nv);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.NhanViens.Any(e => e.ID_NhanVien == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
