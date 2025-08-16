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
            {
                query = query.Where(nv => nv.Ten_NhanVien.Contains(tenSearch));
            }

            if (!string.IsNullOrEmpty(sdtSearch))
            {
                query = query.Where(nv => nv.SoDienThoai.Contains(sdtSearch));
            }

            if (!string.IsNullOrEmpty(manvSearch))
            {
                query = query.Where(nv => nv.Ma_NhanVien.Contains(manvSearch));
            }

            if (trangThaiFilter.HasValue)
            {
                query = query.Where(nv => nv.TrangThai == trangThaiFilter.Value);
            }

            // Gửi danh sách trạng thái xuống View để tạo dropdown
            ViewBag.TrangThaiList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "Tất cả", Value = "" },
                new SelectListItem { Text = "Hoạt động", Value = "1" },
                new SelectListItem { Text = "Bị khóa", Value = "0" }
            }, "Value", "Text", trangThaiFilter?.ToString());

            return View(query.ToList());
        }

        // GET: NhanVien/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVien nv)
        {

            // 1. Tìm Role nhân viên
            var roleNhanVien = _db.Roles.FirstOrDefault(r => r.Ma_Roles == "NV");

            // 2. Tạo mới tài khoản
            var taiKhoan = new TaiKhoan
            {
                ID_TaiKhoan = Guid.NewGuid(),
                Uername = nv.Email,             // Username là email
                Password = "123456",            // Password mặc định
                Roles = roleNhanVien,
                ID_Roles = roleNhanVien.ID_Roles
            };

            _db.TaiKhoans.Add(taiKhoan);
            _db.SaveChanges(); // Save để lấy ID_TaiKhoan

            // 3. Gán ID_TaiKhoan cho nhân viên và lưu nhân viên
            nv.ID_TaiKhoan = taiKhoan.ID_TaiKhoan;
            _db.NhanViens.Add(nv);
            _db.SaveChanges();

            return RedirectToAction("Index");


        }

        // AJAX: Kiểm tra email
        [HttpGet]
        public JsonResult IsEmailAvailable(string email)
        {
            bool exists = _db.NhanViens.Any(nv => nv.Email == email);
            return Json(!exists); // true = hợp lệ
        }

        // AJAX: Kiểm tra số điện thoại
        [HttpGet]
        public JsonResult IsPhoneAvailable(string soDienThoai)
        {
            bool exists = _db.NhanViens.Any(nv => nv.SoDienThoai == soDienThoai);
            return Json(!exists); // true = hợp lệ
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
                if (!_db.NhanViens.Any(e => e.ID_NhanVien == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));

        }




    }
}