using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Migrations
{
    /// <inheritdoc />
    public partial class klkl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_SanPhamChiTiets_SanPhamChiTietID_SanPhamChiTiet",
                table: "HoaDonChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_NhanViens_NhanVienID_NhanVien",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_SanPhamChiTiets_SanPhams_SanPhamID_SanPham",
                table: "SanPhamChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_SanPhamChiTiets_TheTichs_ID_TheTich",
                table: "SanPhamChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_SanPhams_QuocGias_QuocGiaID_QuocGia",
                table: "SanPhams");

            migrationBuilder.DropIndex(
                name: "IX_SanPhams_QuocGiaID_QuocGia",
                table: "SanPhams");

            migrationBuilder.DropIndex(
                name: "IX_SanPhamChiTiets_SanPhamID_SanPham",
                table: "SanPhamChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonChiTiets_SanPhamChiTietID_SanPhamChiTiet",
                table: "HoaDonChiTiets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TheTichs",
                table: "TheTichs");

            migrationBuilder.DropColumn(
                name: "QuocGiaID_QuocGia",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "SanPhamID_SanPham",
                table: "SanPhamChiTiets");

            migrationBuilder.DropColumn(
                name: "SanPhamChiTietID_SanPhamChiTiet",
                table: "HoaDonChiTiets");

            migrationBuilder.RenameTable(
                name: "TheTichs",
                newName: "TheTiches");

            migrationBuilder.RenameColumn(
                name: "NhanVienID_NhanVien",
                table: "HoaDons",
                newName: "ID_NhanVien");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDons_NhanVienID_NhanVien",
                table: "HoaDons",
                newName: "IX_HoaDons_ID_NhanVien");

            migrationBuilder.AlterColumn<string>(
                name: "Ten_Roles",
                table: "Roles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Ma_Roles",
                table: "Roles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Ten_NhanVien",
                table: "NhanViens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayThamGia",
                table: "NhanViens",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Ma_NhanVien",
                table: "NhanViens",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DiaChiLienHe",
                table: "NhanViens",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgaySinh",
                table: "KhachHangs",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Sdt_NguoiNhan",
                table: "HoaDons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "HoaDons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DiaChi",
                table: "HoaDons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "ID_KhachHang",
                table: "HoaDons",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TheTiches",
                table: "TheTiches",
                column: "ID_TheTich");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_ID_QuocGia",
                table: "SanPhams",
                column: "ID_QuocGia");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_ID_SanPham",
                table: "SanPhamChiTiets",
                column: "ID_SanPham");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_ID_KhachHang",
                table: "HoaDons",
                column: "ID_KhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_ID_SanPhamChiTiet",
                table: "HoaDonChiTiets",
                column: "ID_SanPhamChiTiet");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_SanPhamChiTiets_ID_SanPhamChiTiet",
                table: "HoaDonChiTiets",
                column: "ID_SanPhamChiTiet",
                principalTable: "SanPhamChiTiets",
                principalColumn: "ID_SanPhamChiTiet",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_KhachHangs_ID_KhachHang",
                table: "HoaDons",
                column: "ID_KhachHang",
                principalTable: "KhachHangs",
                principalColumn: "ID_KhachHang");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_NhanViens_ID_NhanVien",
                table: "HoaDons",
                column: "ID_NhanVien",
                principalTable: "NhanViens",
                principalColumn: "ID_NhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhamChiTiets_SanPhams_ID_SanPham",
                table: "SanPhamChiTiets",
                column: "ID_SanPham",
                principalTable: "SanPhams",
                principalColumn: "ID_SanPham",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhamChiTiets_TheTiches_ID_TheTich",
                table: "SanPhamChiTiets",
                column: "ID_TheTich",
                principalTable: "TheTiches",
                principalColumn: "ID_TheTich",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhams_QuocGias_ID_QuocGia",
                table: "SanPhams",
                column: "ID_QuocGia",
                principalTable: "QuocGias",
                principalColumn: "ID_QuocGia",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonChiTiets_SanPhamChiTiets_ID_SanPhamChiTiet",
                table: "HoaDonChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_KhachHangs_ID_KhachHang",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_NhanViens_ID_NhanVien",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_SanPhamChiTiets_SanPhams_ID_SanPham",
                table: "SanPhamChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_SanPhamChiTiets_TheTiches_ID_TheTich",
                table: "SanPhamChiTiets");

            migrationBuilder.DropForeignKey(
                name: "FK_SanPhams_QuocGias_ID_QuocGia",
                table: "SanPhams");

            migrationBuilder.DropIndex(
                name: "IX_SanPhams_ID_QuocGia",
                table: "SanPhams");

            migrationBuilder.DropIndex(
                name: "IX_SanPhamChiTiets_ID_SanPham",
                table: "SanPhamChiTiets");

            migrationBuilder.DropIndex(
                name: "IX_HoaDons_ID_KhachHang",
                table: "HoaDons");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonChiTiets_ID_SanPhamChiTiet",
                table: "HoaDonChiTiets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TheTiches",
                table: "TheTiches");

            migrationBuilder.DropColumn(
                name: "ID_KhachHang",
                table: "HoaDons");

            migrationBuilder.RenameTable(
                name: "TheTiches",
                newName: "TheTichs");

            migrationBuilder.RenameColumn(
                name: "ID_NhanVien",
                table: "HoaDons",
                newName: "NhanVienID_NhanVien");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDons_ID_NhanVien",
                table: "HoaDons",
                newName: "IX_HoaDons_NhanVienID_NhanVien");

            migrationBuilder.AddColumn<Guid>(
                name: "QuocGiaID_QuocGia",
                table: "SanPhams",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SanPhamID_SanPham",
                table: "SanPhamChiTiets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Ten_Roles",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Ma_Roles",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Ten_NhanVien",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayThamGia",
                table: "NhanViens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ma_NhanVien",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "DiaChiLienHe",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgaySinh",
                table: "KhachHangs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Sdt_NguoiNhan",
                table: "HoaDons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "HoaDons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiaChi",
                table: "HoaDons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SanPhamChiTietID_SanPhamChiTiet",
                table: "HoaDonChiTiets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TheTichs",
                table: "TheTichs",
                column: "ID_TheTich");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_QuocGiaID_QuocGia",
                table: "SanPhams",
                column: "QuocGiaID_QuocGia");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_SanPhamID_SanPham",
                table: "SanPhamChiTiets",
                column: "SanPhamID_SanPham");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_SanPhamChiTietID_SanPhamChiTiet",
                table: "HoaDonChiTiets",
                column: "SanPhamChiTietID_SanPhamChiTiet");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonChiTiets_SanPhamChiTiets_SanPhamChiTietID_SanPhamChiTiet",
                table: "HoaDonChiTiets",
                column: "SanPhamChiTietID_SanPhamChiTiet",
                principalTable: "SanPhamChiTiets",
                principalColumn: "ID_SanPhamChiTiet",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_NhanViens_NhanVienID_NhanVien",
                table: "HoaDons",
                column: "NhanVienID_NhanVien",
                principalTable: "NhanViens",
                principalColumn: "ID_NhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhamChiTiets_SanPhams_SanPhamID_SanPham",
                table: "SanPhamChiTiets",
                column: "SanPhamID_SanPham",
                principalTable: "SanPhams",
                principalColumn: "ID_SanPham",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhamChiTiets_TheTichs_ID_TheTich",
                table: "SanPhamChiTiets",
                column: "ID_TheTich",
                principalTable: "TheTichs",
                principalColumn: "ID_TheTich",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhams_QuocGias_QuocGiaID_QuocGia",
                table: "SanPhams",
                column: "QuocGiaID_QuocGia",
                principalTable: "QuocGias",
                principalColumn: "ID_QuocGia",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
