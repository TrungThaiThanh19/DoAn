using Microsoft.EntityFrameworkCore;

namespace DoAn.Models
{
    public class DuAnDbContext : DbContext
    {
        public DuAnDbContext()
        {
            
        }

        public DuAnDbContext(DbContextOptions<DuAnDbContext> options) : base(options)
        {
        }

        public DbSet<DonDatHang> DonDatHangs { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<PhuongThucThanhToan> PhuongThucThanhToans { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<TrangThaiDonHang> TrangThaiDonHangs { get; set; }
        public DbSet<ChiTietDonDatHang> ChiTietDonDatHangs { get; set; }
        public DbSet<GioHangChiTiet> GioHangChiTiets { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<SanPhamChiTiet> SanPhamChiTiets { get; set; }
        public DbSet<GioiTinh> gioiTinhs { get; set; }
        public DbSet<MuiHuong> muiHuongs { get; set; }
        public DbSet<TheTich> theTichs { get; set; }
        public DbSet<ThuongHieu> thuongHieus { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public DbSet<NhanVien> nhanViens { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<VaiTro_PhanQuyen> VaiTroPhanQuyens { get; set; }
        public DbSet<PhanQuyen> PhanQuyens { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Gọi lại phương thức của lớp cơ sở trước
            base.OnModelCreating(modelBuilder);

            // Mối quan hệ 1-N: VaiTro -> TaiKhoan
            // Một VaiTro có thể có nhiều TaiKhoan.
            // IdVaiTro trong TaiKhoan là khóa ngoại.
            // Khi VaiTro bị xóa, không cho phép xóa nếu có TaiKhoan liên quan (Restrict).
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(tk => tk.VaiTro)
                .WithMany(vt => vt.TaiKhoans)
                .HasForeignKey(tk => tk.IdVaiTro)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ N-N: VaiTro <-> PhanQuyen thông qua VaiTro_PhanQuyen
            // Một VaiTro có thể có nhiều PhanQuyen, và một PhanQuyen có thể thuộc nhiều VaiTro.
            // IdVaiTro và IdQuyen trong VaiTro_PhanQuyen là khóa ngoại.
            // Không cho phép xóa VaiTro hoặc PhanQuyen nếu có liên kết trong VaiTro_PhanQuyen (Restrict).
            modelBuilder.Entity<VaiTro_PhanQuyen>()
                .HasOne(vtpq => vtpq.VaiTro)
                .WithMany(vt => vt.VaiTroPhanQuyens)
                .HasForeignKey(vtpq => vtpq.IdVaiTro)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VaiTro_PhanQuyen>()
                .HasOne(vtpq => vtpq.PhanQuyen)
                .WithMany(pq => pq.VaiTroPhanQuyens)
                .HasForeignKey(vtpq => vtpq.IdQuyen)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ 1-1: TaiKhoan <-> NhanVien
            // Mỗi TaiKhoan có thể liên kết với một NhanVien duy nhất.
            // IdTk trong NhanVien là khóa ngoại.
            // Khi TaiKhoan bị xóa, NhanVien tương ứng cũng bị xóa (Cascade).
            modelBuilder.Entity<NhanVien>()
                .HasOne(nv => nv.TaiKhoan)
                .WithOne(tk => tk.NhanVien)
                .HasForeignKey<NhanVien>(nv => nv.IdTk)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Mối quan hệ 1-1: TaiKhoan <-> KhachHang
            // Mỗi TaiKhoan có thể liên kết với một KhachHang duy nhất.
            // IdTk trong KhachHang là khóa ngoại.
            // Khi TaiKhoan bị xóa, KhachHang tương ứng cũng bị xóa (Cascade).
            modelBuilder.Entity<KhachHang>()
                .HasOne(kh => kh.TaiKhoan)
                .WithOne(tk => tk.KhachHang)
                .HasForeignKey<KhachHang>(kh => kh.IdTk)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Mối quan hệ 1-N: SanPham -> SanPhamChiTiet
            // Một SanPham có nhiều SanPhamChiTiet.
            // IdSp trong SanPhamChiTiet là khóa ngoại.
            // Khi SanPham bị xóa, các SanPhamChiTiet tương ứng cũng bị xóa (Cascade).
            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(ctsp => ctsp.SanPham)
                .WithMany(sp => sp.SanPhamChiTiets)
                .HasForeignKey(ctsp => ctsp.IdSp)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Mối quan hệ 1-N: ThuongHieu -> SanPhamChiTiet
            // Một ThuongHieu có nhiều SanPhamChiTiet.
            // IdThuongHieu trong SanPhamChiTiet là khóa ngoại.
            // Không cho phép xóa ThuongHieu nếu có SanPhamChiTiet liên quan (Restrict).
            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(ctsp => ctsp.ThuongHieu)
                .WithMany(th => th.SanPhamChiTiets)
                .HasForeignKey(ctsp => ctsp.IdThuongHieu)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ 1-N: MuiHuong -> SanPhamChiTiet
            // Một MuiHuong có nhiều SanPhamChiTiet.
            // IdMuiHuong trong SanPhamChiTiet là khóa ngoại.
            // Không cho phép xóa MuiHuong nếu có SanPhamChiTiet liên quan (Restrict).
            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(ctsp => ctsp.MuiHuong)
                .WithMany(mh => mh.SanPhamChiTiets)
                .HasForeignKey(ctsp => ctsp.IdMuiHuong)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ 1-N: TheTich -> SanPhamChiTiet
            // Một TheTich có nhiều SanPhamChiTiet.
            // IdTheTich trong SanPhamChiTiet là khóa ngoại.
            // Không cho phép xóa TheTich nếu có SanPhamChiTiet liên quan (Restrict).
            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(ctsp => ctsp.TheTich)
                .WithMany(tt => tt.SanPhamChiTiets)
                .HasForeignKey(ctsp => ctsp.IdTheTich)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ 1-N: GioTinh -> SanPhamChiTiet
            // Một GioTinh có nhiều SanPhamChiTiet.
            // IdGioTinh trong SanPhamChiTiet là khóa ngoại.
            // Không cho phép xóa GioTinh nếu có SanPhamChiTiet liên quan (Restrict).
            modelBuilder.Entity<SanPhamChiTiet>()
                .HasOne(ctsp => ctsp.GioTinh)
                .WithMany(gt => gt.SanPhamChiTiets)
                .HasForeignKey(ctsp => ctsp.IdGioTinh)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ 1-N: NhanVien -> HoaDon (Tại Quầy)
            // Một NhanVien có thể tạo nhiều HoaDon.
            // IdNv trong HoaDon là khóa ngoại.
            // Khi NhanVien bị xóa, không cho phép xóa nếu có HoaDon liên quan (Restrict).
            modelBuilder.Entity<HoaDon>()
                .HasOne(hd => hd.NhanVien)
                .WithMany(nv => nv.HoaDons)
                .HasForeignKey(hd => hd.IdNv)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // Mối quan hệ 1-N: HoaDon -> ChiTietHoaDon
            // Một HoaDon có nhiều ChiTietHoaDon.
            // IdHd trong ChiTietHoaDon là khóa ngoại.
            // Khi HoaDon bị xóa, các ChiTietHoaDon tương ứng cũng bị xóa (Cascade).
            modelBuilder.Entity<ChiTietHoaDon>()
                .HasOne(hdct => hdct.HoaDon)
                .WithMany(hd => hd.ChiTietHoaDons)
                .HasForeignKey(hdct => hdct.IdHd)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Mối quan hệ 1-N: SanPhamChiTiet -> ChiTietHoaDon
            // Một SanPhamChiTiet có thể xuất hiện trong nhiều ChiTietHoaDon.
            // IdCtsp trong ChiTietHoaDon là khóa ngoại.
            // Không cho phép xóa SanPhamChiTiet nếu có ChiTietHoaDon liên quan (Restrict).
            modelBuilder.Entity<ChiTietHoaDon>()
                .HasOne(hdct => hdct.SanPhamChiTiet)
                .WithMany(spct => spct.ChiTietHoaDons)
                .HasForeignKey(hdct => hdct.IdCtsp)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ 1-N: KhachHang -> DonDatHang (Online)
            // Một KhachHang có thể tạo nhiều DonDatHang.
            // IdKh trong DonDatHang là khóa ngoại.
            // Khi KhachHang bị xóa, không cho phép xóa nếu có DonDatHang liên quan (Restrict).
            modelBuilder.Entity<DonDatHang>()
                .HasOne(ddh => ddh.KhachHang)
                .WithMany(kh => kh.DonDatHangs)
                .HasForeignKey(ddh => ddh.ID_KH) // Đã sửa từ ID_KH
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ 1-N: TrangThaiDonHang -> DonDatHang
            // Một TrangThaiDonHang có thể được dùng cho nhiều DonDatHang.
            // IdTrangThaiDdh trong DonDatHang là khóa ngoại.
            // Không cho phép xóa TrangThaiDonHang nếu có DonDatHang liên quan (Restrict).
            modelBuilder.Entity<DonDatHang>()
                .HasOne(ddh => ddh.TrangThaiDonHang)
                .WithMany(tthd => tthd.DonDatHangs)
                .HasForeignKey(ddh => ddh.ID_TTDH) // Đã sửa từ ID_TTDH
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ 1-N: PhuongThucThanhToan -> DonDatHang
            // Một PhuongThucThanhToan có thể được dùng cho nhiều DonDatHang.
            // IdPhuongThucThanhToan trong DonDatHang là khóa ngoại.
            // Không cho phép xóa PhuongThucThanhToan nếu có DonDatHang liên quan (Restrict).
            modelBuilder.Entity<DonDatHang>()
                .HasOne(ddh => ddh.PhuongThucThanhToan)
                .WithMany(pttt => pttt.DonDatHangs)
                .HasForeignKey(ddh => ddh.ID_PTTT) // Đã sửa từ ID_PTTT
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ 1-N: Voucher -> DonDatHang
            // Một Voucher có thể được áp dụng cho nhiều DonDatHang.
            // IdVch trong DonDatHang là khóa ngoại.
            // IsRequired(false) nghĩa là khóa ngoại này có thể NULL (một đơn hàng không nhất thiết phải có Voucher).
            // OnDelete(DeleteBehavior.Restrict) ngăn chặn việc xóa Voucher nếu có đơn hàng đang sử dụng nó.
            modelBuilder.Entity<DonDatHang>()
                .HasOne(ddh => ddh.Voucher)
                .WithMany(vch => vch.DonDatHangs)
                .HasForeignKey(ddh => ddh.ID_VCH) // Đã sửa từ ID_VCH
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ 1-N: DonDatHang -> ChiTietDonDatHang
            // Một DonDatHang có nhiều ChiTietDonDatHang.
            // IdDdh trong ChiTietDonDatHang là khóa ngoại.
            // Khi DonDatHang bị xóa, các ChiTietDonDatHang tương ứng cũng bị xóa (Cascade).
            modelBuilder.Entity<ChiTietDonDatHang>()
                .HasOne(ddhct => ddhct.DonDatHang)
                .WithMany(ddh => ddh.ChiTietDonDatHangs)
                .HasForeignKey(ddhct => ddhct.IdDdh)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Mối quan hệ 1-N: SanPhamChiTiet -> ChiTietDonDatHang
            // Một SanPhamChiTiet có thể xuất hiện trong nhiều ChiTietDonDatHang.
            // IdCtsp trong ChiTietDonDatHang là khóa ngoại.
            // Không cho phép xóa SanPhamChiTiet nếu có ChiTietDonDatHang liên quan (Restrict).
            modelBuilder.Entity<ChiTietDonDatHang>()
                .HasOne(ddhct => ddhct.SanPhamChiTiet)
                .WithMany(spct => spct.ChiTietDonDatHangs)
                .HasForeignKey(ddhct => ddhct.IdCtsp)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ 1-N: KhachHang -> GioHang
            // Một KhachHang có một GioHang (mặc dù trong code là WithMany, nhưng thường thì mỗi KH chỉ có 1 GH).
            // IdKh trong GioHang là khóa ngoại.
            // Khi KhachHang bị xóa, GioHang tương ứng cũng bị xóa (Cascade).
            modelBuilder.Entity<GioHang>()
                .HasOne(gh => gh.KhachHang)
                .WithMany(kh => kh.GioHangs) // Có thể là WithOne nếu bạn muốn mỗi KH chỉ có một GH
                .HasForeignKey(gh => gh.IdKh)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Mối quan hệ 1-N: GioHang -> GioHangChiTiet
            // Một GioHang có nhiều GioHangChiTiet.
            // IdGioHang trong GioHangChiTiet là khóa ngoại.
            // Khi GioHang bị xóa, các GioHangChiTiet tương ứng cũng bị xóa (Cascade).
            modelBuilder.Entity<GioHangChiTiet>()
                .HasOne(ghct => ghct.GioHang)
                .WithMany(gh => gh.GioHangChiTiets)
                .HasForeignKey(ghct => ghct.IdGioHang)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Mối quan hệ 1-N: SanPhamChiTiet -> GioHangChiTiet
            // Một SanPhamChiTiet có thể xuất hiện trong nhiều GioHangChiTiet.
            // IdCtsp trong GioHangChiTiet là khóa ngoại.
            // Không cho phép xóa SanPhamChiTiet nếu có GioHangChiTiet liên quan (Restrict).
            modelBuilder.Entity<GioHangChiTiet>()
                .HasOne(ghct => ghct.SanPhamChiTiet)
                .WithMany(spct => spct.GioHangChiTiets)
                .HasForeignKey(ghct => ghct.IdCtsp)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
