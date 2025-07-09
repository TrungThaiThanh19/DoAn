using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Migrations
{
    /// <inheritdoc />
    public partial class khanh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gioiTinhs",
                columns: table => new
                {
                    IdGioiTinh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenGioTinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gioiTinhs", x => x.IdGioiTinh);
                });

            migrationBuilder.CreateTable(
                name: "muiHuongs",
                columns: table => new
                {
                    IdMuiHuong = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenMH = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_muiHuongs", x => x.IdMuiHuong);
                });

            migrationBuilder.CreateTable(
                name: "PhanQuyens",
                columns: table => new
                {
                    IdPhanQuyen = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenQuyen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanQuyens", x => x.IdPhanQuyen);
                });

            migrationBuilder.CreateTable(
                name: "PhuongThucThanhToans",
                columns: table => new
                {
                    IdPhuongThucThanhToan = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenPT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuongThucThanhToans", x => x.IdPhuongThucThanhToan);
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    IdSp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenSp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonGiaBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonGiaNhap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.IdSp);
                });

            migrationBuilder.CreateTable(
                name: "theTichs",
                columns: table => new
                {
                    IdTheTich = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TheTichs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_theTichs", x => x.IdTheTich);
                });

            migrationBuilder.CreateTable(
                name: "thuongHieus",
                columns: table => new
                {
                    IdThuongHieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenTh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_thuongHieus", x => x.IdThuongHieu);
                });

            migrationBuilder.CreateTable(
                name: "TrangThaiDonHangs",
                columns: table => new
                {
                    IdTrangThaiHd = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenTrangThaiDh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrangThaiDonHangs", x => x.IdTrangThaiHd);
                });

            migrationBuilder.CreateTable(
                name: "VaiTros",
                columns: table => new
                {
                    IdVaiTro = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenVaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTros", x => x.IdVaiTro);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    IdVch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenVoucher = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhanTramGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.IdVch);
                });

            migrationBuilder.CreateTable(
                name: "SanPhamChiTiets",
                columns: table => new
                {
                    IdCtsp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdSp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdThuongHieu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdMuiHuong = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTheTich = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdGioTinh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhamChiTiets", x => x.IdCtsp);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_SanPhams_IdSp",
                        column: x => x.IdSp,
                        principalTable: "SanPhams",
                        principalColumn: "IdSp",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_gioiTinhs_IdGioTinh",
                        column: x => x.IdGioTinh,
                        principalTable: "gioiTinhs",
                        principalColumn: "IdGioiTinh",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_muiHuongs_IdMuiHuong",
                        column: x => x.IdMuiHuong,
                        principalTable: "muiHuongs",
                        principalColumn: "IdMuiHuong",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_theTichs_IdTheTich",
                        column: x => x.IdTheTich,
                        principalTable: "theTichs",
                        principalColumn: "IdTheTich",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SanPhamChiTiets_thuongHieus_IdThuongHieu",
                        column: x => x.IdThuongHieu,
                        principalTable: "thuongHieus",
                        principalColumn: "IdThuongHieu",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    ID_TK = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdVaiTro = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.ID_TK);
                    table.ForeignKey(
                        name: "FK_TaiKhoans_VaiTros_IdVaiTro",
                        column: x => x.IdVaiTro,
                        principalTable: "VaiTros",
                        principalColumn: "IdVaiTro",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VaiTroPhanQuyens",
                columns: table => new
                {
                    IdVtPq = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdVaiTro = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdQuyen = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTroPhanQuyens", x => x.IdVtPq);
                    table.ForeignKey(
                        name: "FK_VaiTroPhanQuyens_PhanQuyens_IdQuyen",
                        column: x => x.IdQuyen,
                        principalTable: "PhanQuyens",
                        principalColumn: "IdPhanQuyen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VaiTroPhanQuyens_VaiTros_IdVaiTro",
                        column: x => x.IdVaiTro,
                        principalTable: "VaiTros",
                        principalColumn: "IdVaiTro",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    IdKh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTk = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenKh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sdt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.IdKh);
                    table.ForeignKey(
                        name: "FK_KhachHangs_TaiKhoans_IdTk",
                        column: x => x.IdTk,
                        principalTable: "TaiKhoans",
                        principalColumn: "ID_TK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nhanViens",
                columns: table => new
                {
                    IdNv = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTk = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenNv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoCccd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nhanViens", x => x.IdNv);
                    table.ForeignKey(
                        name: "FK_nhanViens_TaiKhoans_IdTk",
                        column: x => x.IdTk,
                        principalTable: "TaiKhoans",
                        principalColumn: "ID_TK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonDatHangs",
                columns: table => new
                {
                    ID_DDH = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_KH = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_PTTT = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_TTDH = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_VCH = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiaChiGiaoHang = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonDatHangs", x => x.ID_DDH);
                    table.ForeignKey(
                        name: "FK_DonDatHangs_KhachHangs_ID_KH",
                        column: x => x.ID_KH,
                        principalTable: "KhachHangs",
                        principalColumn: "IdKh",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonDatHangs_PhuongThucThanhToans_ID_PTTT",
                        column: x => x.ID_PTTT,
                        principalTable: "PhuongThucThanhToans",
                        principalColumn: "IdPhuongThucThanhToan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonDatHangs_TrangThaiDonHangs_ID_TTDH",
                        column: x => x.ID_TTDH,
                        principalTable: "TrangThaiDonHangs",
                        principalColumn: "IdTrangThaiHd",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonDatHangs_Vouchers_ID_VCH",
                        column: x => x.ID_VCH,
                        principalTable: "Vouchers",
                        principalColumn: "IdVch",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GioHangs",
                columns: table => new
                {
                    IdGioHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdKh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangs", x => x.IdGioHang);
                    table.ForeignKey(
                        name: "FK_GioHangs_KhachHangs_IdKh",
                        column: x => x.IdKh,
                        principalTable: "KhachHangs",
                        principalColumn: "IdKh",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaDons",
                columns: table => new
                {
                    IdHd = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdNv = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ID_VCH = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SdtKh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenKh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VoucherIdVch = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhuongThucThanhToanIdPhuongThucThanhToan = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDons", x => x.IdHd);
                    table.ForeignKey(
                        name: "FK_HoaDons_PhuongThucThanhToans_PhuongThucThanhToanIdPhuongThucThanhToan",
                        column: x => x.PhuongThucThanhToanIdPhuongThucThanhToan,
                        principalTable: "PhuongThucThanhToans",
                        principalColumn: "IdPhuongThucThanhToan");
                    table.ForeignKey(
                        name: "FK_HoaDons_Vouchers_VoucherIdVch",
                        column: x => x.VoucherIdVch,
                        principalTable: "Vouchers",
                        principalColumn: "IdVch");
                    table.ForeignKey(
                        name: "FK_HoaDons_nhanViens_IdNv",
                        column: x => x.IdNv,
                        principalTable: "nhanViens",
                        principalColumn: "IdNv",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonDatHangs",
                columns: table => new
                {
                    IdDdhct = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdDdh = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCtsp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonDatHangs", x => x.IdDdhct);
                    table.ForeignKey(
                        name: "FK_ChiTietDonDatHangs_DonDatHangs_IdDdh",
                        column: x => x.IdDdh,
                        principalTable: "DonDatHangs",
                        principalColumn: "ID_DDH",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonDatHangs_SanPhamChiTiets_IdCtsp",
                        column: x => x.IdCtsp,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "IdCtsp",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GioHangChiTiets",
                columns: table => new
                {
                    IdGioHangCt = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdGioHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCtsp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SanPhamIdSp = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangChiTiets", x => x.IdGioHangCt);
                    table.ForeignKey(
                        name: "FK_GioHangChiTiets_GioHangs_IdGioHang",
                        column: x => x.IdGioHang,
                        principalTable: "GioHangs",
                        principalColumn: "IdGioHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GioHangChiTiets_SanPhamChiTiets_IdCtsp",
                        column: x => x.IdCtsp,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "IdCtsp",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GioHangChiTiets_SanPhams_SanPhamIdSp",
                        column: x => x.SanPhamIdSp,
                        principalTable: "SanPhams",
                        principalColumn: "IdSp");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHoaDons",
                columns: table => new
                {
                    IdHdct = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdHd = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCtsp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietHoaDons", x => x.IdHdct);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDons_HoaDons_IdHd",
                        column: x => x.IdHd,
                        principalTable: "HoaDons",
                        principalColumn: "IdHd",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDons_SanPhamChiTiets_IdCtsp",
                        column: x => x.IdCtsp,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "IdCtsp",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonDatHangs_IdCtsp",
                table: "ChiTietDonDatHangs",
                column: "IdCtsp");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonDatHangs_IdDdh",
                table: "ChiTietDonDatHangs",
                column: "IdDdh");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDons_IdCtsp",
                table: "ChiTietHoaDons",
                column: "IdCtsp");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDons_IdHd",
                table: "ChiTietHoaDons",
                column: "IdHd");

            migrationBuilder.CreateIndex(
                name: "IX_DonDatHangs_ID_KH",
                table: "DonDatHangs",
                column: "ID_KH");

            migrationBuilder.CreateIndex(
                name: "IX_DonDatHangs_ID_PTTT",
                table: "DonDatHangs",
                column: "ID_PTTT");

            migrationBuilder.CreateIndex(
                name: "IX_DonDatHangs_ID_TTDH",
                table: "DonDatHangs",
                column: "ID_TTDH");

            migrationBuilder.CreateIndex(
                name: "IX_DonDatHangs_ID_VCH",
                table: "DonDatHangs",
                column: "ID_VCH");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangChiTiets_IdCtsp",
                table: "GioHangChiTiets",
                column: "IdCtsp");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangChiTiets_IdGioHang",
                table: "GioHangChiTiets",
                column: "IdGioHang");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangChiTiets_SanPhamIdSp",
                table: "GioHangChiTiets",
                column: "SanPhamIdSp");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_IdKh",
                table: "GioHangs",
                column: "IdKh");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_IdNv",
                table: "HoaDons",
                column: "IdNv");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_PhuongThucThanhToanIdPhuongThucThanhToan",
                table: "HoaDons",
                column: "PhuongThucThanhToanIdPhuongThucThanhToan");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_VoucherIdVch",
                table: "HoaDons",
                column: "VoucherIdVch");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHangs_IdTk",
                table: "KhachHangs",
                column: "IdTk",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nhanViens_IdTk",
                table: "nhanViens",
                column: "IdTk",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_IdGioTinh",
                table: "SanPhamChiTiets",
                column: "IdGioTinh");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_IdMuiHuong",
                table: "SanPhamChiTiets",
                column: "IdMuiHuong");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_IdSp",
                table: "SanPhamChiTiets",
                column: "IdSp");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_IdTheTich",
                table: "SanPhamChiTiets",
                column: "IdTheTich");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_IdThuongHieu",
                table: "SanPhamChiTiets",
                column: "IdThuongHieu");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_IdVaiTro",
                table: "TaiKhoans",
                column: "IdVaiTro");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTroPhanQuyens_IdQuyen",
                table: "VaiTroPhanQuyens",
                column: "IdQuyen");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTroPhanQuyens_IdVaiTro",
                table: "VaiTroPhanQuyens",
                column: "IdVaiTro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonDatHangs");

            migrationBuilder.DropTable(
                name: "ChiTietHoaDons");

            migrationBuilder.DropTable(
                name: "GioHangChiTiets");

            migrationBuilder.DropTable(
                name: "VaiTroPhanQuyens");

            migrationBuilder.DropTable(
                name: "DonDatHangs");

            migrationBuilder.DropTable(
                name: "HoaDons");

            migrationBuilder.DropTable(
                name: "GioHangs");

            migrationBuilder.DropTable(
                name: "SanPhamChiTiets");

            migrationBuilder.DropTable(
                name: "PhanQuyens");

            migrationBuilder.DropTable(
                name: "TrangThaiDonHangs");

            migrationBuilder.DropTable(
                name: "PhuongThucThanhToans");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropTable(
                name: "nhanViens");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "gioiTinhs");

            migrationBuilder.DropTable(
                name: "muiHuongs");

            migrationBuilder.DropTable(
                name: "theTichs");

            migrationBuilder.DropTable(
                name: "thuongHieus");

            migrationBuilder.DropTable(
                name: "TaiKhoans");

            migrationBuilder.DropTable(
                name: "VaiTros");
        }
    }
}
