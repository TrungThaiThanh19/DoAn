using Microsoft.EntityFrameworkCore;

namespace DoAn.Models
{
    public class DoAnDbContext : DbContext
    {
        public DoAnDbContext()
        {

        }
        public DoAnDbContext(DbContextOptions<DoAnDbContext> options) : base(options)
        {
        }
        public DbSet<TheTich> TheTichs { get; set; }
        public DbSet<GioiTinh> GioiTinhs { get; set; }
        public DbSet<SanPhamChiTiet> SanPhamChiTiets { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }
        public DbSet<ChiTietKhuyenMai> ChiTietKhuyenMais { get; set; }
        public DbSet<ChiTietTraHang> ChiTietTraHangs { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<TrangThaiDonHang> TrangThaiDonHangs { get; set; }
        public DbSet<QuanLyTraHang> TraHangs { get; set; }
        public DbSet<ThuongHieu> ThuongHieus { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<HoaDonChiTiet> HoaDonChiTiets { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<DiaChiKhachHang> DiaChiKhachHangs { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }

        public DbSet<QuocGia> QuocGias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ChiTietKhuyenMai
            modelBuilder.Entity<ChiTietKhuyenMai>()
                .HasOne(c => c.SanPhamChiTiet)
                .WithMany(s => s.ChiTietKhuyenMais)
                .HasForeignKey(c => c.ID_SanPhamChiTiet);

            modelBuilder.Entity<ChiTietKhuyenMai>()
                .HasOne(c => c.KhuyenMai)
                .WithMany(k => k.ChiTietKhuyenMais)
                .HasForeignKey(c => c.ID_KhuyenMai);

            // ChiTietGioHang
            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(c => c.SanPhamChiTiet)
                .WithMany()
                .HasForeignKey(c => c.ID_SanPhamChiTiet);

            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(c => c.GioHang)
                .WithMany(g => g.ChiTietGioHangs)
                .HasForeignKey(c => c.ID_GioHang);

            // DiaChiKhachHang
            modelBuilder.Entity<DiaChiKhachHang>()
                .HasOne(d => d.KhachHang)
                .WithMany()
                .HasForeignKey(d => d.ID_KhachHang);

            modelBuilder.Entity<HoaDon>()
                .HasOne(h => h.Voucher)
                .WithMany(v => v.HoaDons)
                .HasForeignKey(h => h.ID_Voucher)
                .OnDelete(DeleteBehavior.Restrict);

            // HoaDonChiTiet
            modelBuilder.Entity<HoaDonChiTiet>()
                .HasOne(h => h.HoaDon)
                .WithMany(hd => hd.HoaDonChiTiets)
                .HasForeignKey(h => h.ID_HoaDon);


            // TraHang
            modelBuilder.Entity<QuanLyTraHang>()
                .HasOne(t => t.HoaDon)
                .WithMany(h => h.TraHangs)
                .HasForeignKey(t => t.ID_HoaDon);

            // TrangThaiDonHang
            modelBuilder.Entity<TrangThaiDonHang>()
                .HasOne(t => t.HoaDon)
                .WithMany(h => h.TrangThaiDonHangs)
                .HasForeignKey(t => t.ID_HoaDon);

            // SanPhamChiTiet
            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.GioiTinh)
                .WithMany(g => g.SanPhams)
                .HasForeignKey(s => s.ID_GioiTinh);

            // SanPhamChiTiet
            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(s => s.TheTich)
                .WithMany(t => t.SanPhamChiTiets)
                .HasForeignKey(s => s.ID_TheTich);

			// 1 SanPham - N SanPhamChiTiet
			modelBuilder.Entity<SanPhamChiTiet>()
				.HasOne(s => s.SanPham)
				.WithMany(t => t.SanPhamChiTiets)
				.HasForeignKey(s => s.ID_SanPham);


			modelBuilder.Entity<SanPham>()
                .HasOne(s => s.ThuongHieu)
                .WithMany(t => t.SanPhams)
                .HasForeignKey(s => s.ID_ThuongHieu);

			// 1 QuocGia - N SanPham
			modelBuilder.Entity<SanPham>()
				.HasOne(s => s.QuocGia)
				.WithMany(t => t.SanPhams)
				.HasForeignKey(s => s.ID_QuocGia);

			// TaiKhoan
			modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.Roles)
                .WithMany(r => r.TaiKhoans)
                .HasForeignKey(t => t.ID_Roles);

            modelBuilder.Entity<NhanVien>()
                .HasOne(n => n.TaiKhoan)
                .WithMany(t => t.NhanViens)
                .HasForeignKey(n => n.ID_TaiKhoan);

            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.TaiKhoan)
                .WithMany(t => t.KhachHangs)
                .HasForeignKey(k => k.ID_TaiKhoan);
            // Seed Roles (phải seed trước Tài khoản vì Tài khoản phụ thuộc Roles)
            var adminRoleId = Guid.Parse("A0000000-0000-0000-0000-000000000003");
            var nhanvienRoleId = Guid.Parse("A0000000-0000-0000-0000-000000000002");
            var khachhangRoleId = Guid.Parse("A0000000-0000-0000-0000-000000000001");

            modelBuilder.Entity<Roles>().HasData(
                new Roles { ID_Roles = khachhangRoleId, Ma_Roles = "KH", Ten_Roles = "khachhang" },
                new Roles { ID_Roles = nhanvienRoleId, Ma_Roles = "NV", Ten_Roles = "nhanvien" },
                new Roles { ID_Roles = adminRoleId, Ma_Roles = "AD", Ten_Roles = "admin" }
            );

            // Seed Admin Account
            var adminAccountId = Guid.Parse("B0000000-0000-0000-0000-000000000001"); // GUID mới cho tài khoản admin

            modelBuilder.Entity<TaiKhoan>().HasData(
                new TaiKhoan
                {
                    ID_TaiKhoan = adminAccountId,
                    Uername = "admin",
                    Password = "admin",
                    ID_Roles = adminRoleId,
                }
            );

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=LAPTOP-8CVDJNS6;Database=DoAn;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }
    }
}
