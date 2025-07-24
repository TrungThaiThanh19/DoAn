using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DoAn.Migrations
{
    /// <inheritdoc />
    public partial class hi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GioiTinhs",
                columns: table => new
                {
                    ID_GioiTinh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten_GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioiTinhs", x => x.ID_GioiTinh);
                });

            migrationBuilder.CreateTable(
                name: "KhuyenMais",
                columns: table => new
                {
                    ID_KhuyenMai = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ma_KhuyenMai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ten_KhuyenMai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayHetHan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KieuGiamGia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaTriGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaTriToiDa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMais", x => x.ID_KhuyenMai);
                });

            migrationBuilder.CreateTable(
                name: "QuocGias",
                columns: table => new
                {
                    ID_QuocGia = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten_QuocGia = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuocGias", x => x.ID_QuocGia);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    ID_Roles = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ma_Roles = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ten_Roles = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.ID_Roles);
                });

            migrationBuilder.CreateTable(
                name: "TheTichs",
                columns: table => new
                {
                    ID_TheTich = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ma_TheTich = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaTri = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonVi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheTichs", x => x.ID_TheTich);
                });

            migrationBuilder.CreateTable(
                name: "ThuongHieus",
                columns: table => new
                {
                    ID_ThuongHieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ma_ThuongHieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ten_ThuongHieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThuongHieus", x => x.ID_ThuongHieu);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    ID_TaiKhoan = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Uername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ID_Roles = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.ID_TaiKhoan);
                    table.ForeignKey(
                        name: "FK_TaiKhoans_Roles_ID_Roles",
                        column: x => x.ID_Roles,
                        principalTable: "Roles",
                        principalColumn: "ID_Roles",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    ID_SanPham = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ma_SanPham = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ten_SanPham = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HuongDau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HuongGiua = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HuongCuoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianLuuHuong = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ID_ThuongHieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_GioiTinh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_QuocGia = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.ID_SanPham);
                    table.ForeignKey(
                        name: "FK_SanPhams_GioiTinhs_ID_GioiTinh",
                        column: x => x.ID_GioiTinh,
                        principalTable: "GioiTinhs",
                        principalColumn: "ID_GioiTinh",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhams_QuocGias_ID_QuocGia",
                        column: x => x.ID_QuocGia,
                        principalTable: "QuocGias",
                        principalColumn: "ID_QuocGia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhams_ThuongHieus_ID_ThuongHieu",
                        column: x => x.ID_ThuongHieu,
                        principalTable: "ThuongHieus",
                        principalColumn: "ID_ThuongHieu",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    ID_KhachHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ma_KhachHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ten_KhachHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    ID_TaiKhoan = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.ID_KhachHang);
                    table.ForeignKey(
                        name: "FK_KhachHangs_TaiKhoans_ID_TaiKhoan",
                        column: x => x.ID_TaiKhoan,
                        principalTable: "TaiKhoans",
                        principalColumn: "ID_TaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhanViens",
                columns: table => new
                {
                    ID_NhanVien = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ma_NhanVien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ten_NhanVien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChiLienHe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayThamGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    ID_TaiKhoan = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanViens", x => x.ID_NhanVien);
                    table.ForeignKey(
                        name: "FK_NhanViens_TaiKhoans_ID_TaiKhoan",
                        column: x => x.ID_TaiKhoan,
                        principalTable: "TaiKhoans",
                        principalColumn: "ID_TaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    ID_Voucher = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ma_Voucher = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ten_Voucher = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayHetHan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KieuGiamGia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaTriGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaTriToiThieu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaTriToiDa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ID_TaiKhoan = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiKhoanID_TaiKhoan = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.ID_Voucher);
                    table.ForeignKey(
                        name: "FK_Vouchers_TaiKhoans_TaiKhoanID_TaiKhoan",
                        column: x => x.TaiKhoanID_TaiKhoan,
                        principalTable: "TaiKhoans",
                        principalColumn: "ID_TaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SanPhamChiTiets",
                columns: table => new
                {
                    ID_SanPhamChiTiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaNhap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ID_TheTich = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_SanPham = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhamChiTiets", x => x.ID_SanPhamChiTiet);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_SanPhams_ID_SanPham",
                        column: x => x.ID_SanPham,
                        principalTable: "SanPhams",
                        principalColumn: "ID_SanPham",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_TheTichs_ID_TheTich",
                        column: x => x.ID_TheTich,
                        principalTable: "TheTichs",
                        principalColumn: "ID_TheTich",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaChiKhachHangs",
                columns: table => new
                {
                    ID_DiaChiKhachHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoNha = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Xa_Phuong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quan_Huyen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tinh_ThanhPho = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChiMacDinh = table.Column<bool>(type: "bit", nullable: false),
                    ID_KhachHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaChiKhachHangs", x => x.ID_DiaChiKhachHang);
                    table.ForeignKey(
                        name: "FK_DiaChiKhachHangs_KhachHangs_ID_KhachHang",
                        column: x => x.ID_KhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "ID_KhachHang",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GioHangs",
                columns: table => new
                {
                    ID_GioHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_KhachHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KhachHangID_KhachHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangs", x => x.ID_GioHang);
                    table.ForeignKey(
                        name: "FK_GioHangs_KhachHangs_KhachHangID_KhachHang",
                        column: x => x.KhachHangID_KhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "ID_KhachHang",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaDons",
                columns: table => new
                {
                    ID_HoaDon = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ma_HoaDon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sdt_NguoiNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhuongThucNhanHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TongTienTruocGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongTienSauGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhuThu = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LoaiHoaDon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    ID_Voucher = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhanVienID_NhanVien = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDons", x => x.ID_HoaDon);
                    table.ForeignKey(
                        name: "FK_HoaDons_NhanViens_NhanVienID_NhanVien",
                        column: x => x.NhanVienID_NhanVien,
                        principalTable: "NhanViens",
                        principalColumn: "ID_NhanVien");
                    table.ForeignKey(
                        name: "FK_HoaDons_Vouchers_ID_Voucher",
                        column: x => x.ID_Voucher,
                        principalTable: "Vouchers",
                        principalColumn: "ID_Voucher",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietKhuyenMais",
                columns: table => new
                {
                    ID_ChiTietKhuyenMai = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaSauGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ID_SanPhamChiTiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_KhuyenMai = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietKhuyenMais", x => x.ID_ChiTietKhuyenMai);
                    table.ForeignKey(
                        name: "FK_ChiTietKhuyenMais_KhuyenMais_ID_KhuyenMai",
                        column: x => x.ID_KhuyenMai,
                        principalTable: "KhuyenMais",
                        principalColumn: "ID_KhuyenMai",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietKhuyenMais_SanPhamChiTiets_ID_SanPhamChiTiet",
                        column: x => x.ID_SanPhamChiTiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "ID_SanPhamChiTiet",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietGioHangs",
                columns: table => new
                {
                    ID_ChiTietGioHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    ID_SanPhamChiTiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_GioHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietGioHangs", x => x.ID_ChiTietGioHang);
                    table.ForeignKey(
                        name: "FK_ChiTietGioHangs_GioHangs_ID_GioHang",
                        column: x => x.ID_GioHang,
                        principalTable: "GioHangs",
                        principalColumn: "ID_GioHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietGioHangs_SanPhamChiTiets_ID_SanPhamChiTiet",
                        column: x => x.ID_SanPhamChiTiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "ID_SanPhamChiTiet",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonChiTiets",
                columns: table => new
                {
                    ID_HoaDonChiTiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ID_HoaDon = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_SanPhamChiTiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamChiTietID_SanPhamChiTiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonChiTiets", x => x.ID_HoaDonChiTiet);
                    table.ForeignKey(
                        name: "FK_HoaDonChiTiets_HoaDons_ID_HoaDon",
                        column: x => x.ID_HoaDon,
                        principalTable: "HoaDons",
                        principalColumn: "ID_HoaDon",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaDonChiTiets_SanPhamChiTiets_SanPhamChiTietID_SanPhamChiTiet",
                        column: x => x.SanPhamChiTietID_SanPhamChiTiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "ID_SanPhamChiTiet",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TraHangs",
                columns: table => new
                {
                    ID_TraHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NhanVienXuLy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    TongTienHoan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ID_HoaDon = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraHangs", x => x.ID_TraHang);
                    table.ForeignKey(
                        name: "FK_TraHangs_HoaDons_ID_HoaDon",
                        column: x => x.ID_HoaDon,
                        principalTable: "HoaDons",
                        principalColumn: "ID_HoaDon",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrangThaiDonHangs",
                columns: table => new
                {
                    ID_TrangThaiDonHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayChuyen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NhanVienDoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDungDoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ID_HoaDon = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrangThaiDonHangs", x => x.ID_TrangThaiDonHang);
                    table.ForeignKey(
                        name: "FK_TrangThaiDonHangs_HoaDons_ID_HoaDon",
                        column: x => x.ID_HoaDon,
                        principalTable: "HoaDons",
                        principalColumn: "ID_HoaDon",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietTraHangs",
                columns: table => new
                {
                    ID_ChiTietTraHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    TienHoan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ID_ChiTietSanPham = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamChiTietID_SanPhamChiTiet = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_TraHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TraHangID_TraHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietTraHangs", x => x.ID_ChiTietTraHang);
                    table.ForeignKey(
                        name: "FK_ChiTietTraHangs_SanPhamChiTiets_SanPhamChiTietID_SanPhamChiTiet",
                        column: x => x.SanPhamChiTietID_SanPhamChiTiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "ID_SanPhamChiTiet",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietTraHangs_TraHangs_TraHangID_TraHang",
                        column: x => x.TraHangID_TraHang,
                        principalTable: "TraHangs",
                        principalColumn: "ID_TraHang",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "ID_Roles", "Ma_Roles", "Ten_Roles" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), "KH", "khachhang" },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), "NV", "nhanvien" },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), "AD", "admin" }
                });

            migrationBuilder.InsertData(
                table: "TaiKhoans",
                columns: new[] { "ID_TaiKhoan", "ID_Roles", "Password", "Uername" },
                values: new object[] { new Guid("b0000000-0000-0000-0000-000000000001"), new Guid("a0000000-0000-0000-0000-000000000003"), "admin", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHangs_ID_GioHang",
                table: "ChiTietGioHangs",
                column: "ID_GioHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHangs_ID_SanPhamChiTiet",
                table: "ChiTietGioHangs",
                column: "ID_SanPhamChiTiet");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKhuyenMais_ID_KhuyenMai",
                table: "ChiTietKhuyenMais",
                column: "ID_KhuyenMai");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKhuyenMais_ID_SanPhamChiTiet",
                table: "ChiTietKhuyenMais",
                column: "ID_SanPhamChiTiet");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietTraHangs_SanPhamChiTietID_SanPhamChiTiet",
                table: "ChiTietTraHangs",
                column: "SanPhamChiTietID_SanPhamChiTiet");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietTraHangs_TraHangID_TraHang",
                table: "ChiTietTraHangs",
                column: "TraHangID_TraHang");

            migrationBuilder.CreateIndex(
                name: "IX_DiaChiKhachHangs_ID_KhachHang",
                table: "DiaChiKhachHangs",
                column: "ID_KhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_KhachHangID_KhachHang",
                table: "GioHangs",
                column: "KhachHangID_KhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_ID_HoaDon",
                table: "HoaDonChiTiets",
                column: "ID_HoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_SanPhamChiTietID_SanPhamChiTiet",
                table: "HoaDonChiTiets",
                column: "SanPhamChiTietID_SanPhamChiTiet");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_ID_Voucher",
                table: "HoaDons",
                column: "ID_Voucher");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_NhanVienID_NhanVien",
                table: "HoaDons",
                column: "NhanVienID_NhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHangs_ID_TaiKhoan",
                table: "KhachHangs",
                column: "ID_TaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_ID_TaiKhoan",
                table: "NhanViens",
                column: "ID_TaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_ID_SanPham",
                table: "SanPhamChiTiets",
                column: "ID_SanPham");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_ID_TheTich",
                table: "SanPhamChiTiets",
                column: "ID_TheTich");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_ID_GioiTinh",
                table: "SanPhams",
                column: "ID_GioiTinh");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_ID_QuocGia",
                table: "SanPhams",
                column: "ID_QuocGia");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_ID_ThuongHieu",
                table: "SanPhams",
                column: "ID_ThuongHieu");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_ID_Roles",
                table: "TaiKhoans",
                column: "ID_Roles");

            migrationBuilder.CreateIndex(
                name: "IX_TraHangs_ID_HoaDon",
                table: "TraHangs",
                column: "ID_HoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_TrangThaiDonHangs_ID_HoaDon",
                table: "TrangThaiDonHangs",
                column: "ID_HoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_TaiKhoanID_TaiKhoan",
                table: "Vouchers",
                column: "TaiKhoanID_TaiKhoan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietGioHangs");

            migrationBuilder.DropTable(
                name: "ChiTietKhuyenMais");

            migrationBuilder.DropTable(
                name: "ChiTietTraHangs");

            migrationBuilder.DropTable(
                name: "DiaChiKhachHangs");

            migrationBuilder.DropTable(
                name: "HoaDonChiTiets");

            migrationBuilder.DropTable(
                name: "TrangThaiDonHangs");

            migrationBuilder.DropTable(
                name: "GioHangs");

            migrationBuilder.DropTable(
                name: "KhuyenMais");

            migrationBuilder.DropTable(
                name: "TraHangs");

            migrationBuilder.DropTable(
                name: "SanPhamChiTiets");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "HoaDons");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "TheTichs");

            migrationBuilder.DropTable(
                name: "NhanViens");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropTable(
                name: "GioiTinhs");

            migrationBuilder.DropTable(
                name: "QuocGias");

            migrationBuilder.DropTable(
                name: "ThuongHieus");

            migrationBuilder.DropTable(
                name: "TaiKhoans");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
