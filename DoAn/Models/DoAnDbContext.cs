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
        public DbSet<TraHang> TraHangs { get; set; }
        public DbSet<HinhAnh> HinhAnhs { get; set; }
        public DbSet<MuiHuong> MuiHuongs { get; set; }
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

            // HinhAnh
            modelBuilder.Entity<HinhAnh>()
                .HasOne(h => h.SanPhamChiTiet)
                .WithMany(s => s.HinhAnhs)
                .HasForeignKey(h => h.ID_ChiTietSanPham);

            // HoaDon


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

            modelBuilder.Entity<HoaDonChiTiet>()
                .HasOne(h => h.SanPhamChiTiet)
                .WithMany()
                .HasForeignKey(h => h.ID_ChiTietSanPham);

            // TraHang
            modelBuilder.Entity<TraHang>()
                .HasOne(t => t.HoaDon)
                .WithMany(h => h.TraHangs)
                .HasForeignKey(t => t.ID_HoaDon);

            // TrangThaiDonHang
            modelBuilder.Entity<TrangThaiDonHang>()
                .HasOne(t => t.HoaDon)
                .WithMany(h => h.TrangThaiDonHangs)
                .HasForeignKey(t => t.ID_HoaDon);

            // SanPhamChiTiet
            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(s => s.GioiTinh)
                .WithMany(g => g.SanPhamChiTiets)
                .HasForeignKey(s => s.ID_GioiTinh);

            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(s => s.TheTich)
                .WithMany(t => t.SanPhamChiTiets)
                .HasForeignKey(s => s.ID_TheTich);

            // SanPham
            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.MuiHuong)
                .WithMany(m => m.SanPhams)
                .HasForeignKey(s => s.ID_MuiHuong);

            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.ThuongHieu)
                .WithMany(t => t.SanPhams)
                .HasForeignKey(s => s.ID_ThuongHieu);

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

            modelBuilder.Entity<Voucher>()
                .HasOne(v => v.TaiKhoan)
                .WithMany(t => t.Vouchers)
                .HasForeignKey(v => v.ID_TaiKhoan);
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
