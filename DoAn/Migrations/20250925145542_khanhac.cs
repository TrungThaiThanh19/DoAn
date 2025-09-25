using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Migrations
{
    /// <inheritdoc />
    public partial class khanhac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NhanViens",
                keyColumn: "ID_NhanVien",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000001"));

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "DiaChiKhachHangs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "DiaChiKhachHangs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "DiaChiKhachHangs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "DiaChiKhachHangs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.InsertData(
                table: "NhanViens",
                columns: new[] { "ID_NhanVien", "DiaChiLienHe", "Email", "GioiTinh", "ID_TaiKhoan", "Ma_NhanVien", "NgaySinh", "NgayThamGia", "SoDienThoai", "Ten_NhanVien", "TrangThai" },
                values: new object[] { new Guid("c0000000-0000-0000-0000-000000000001"), "Hệ thống", "admin@gamil.com", "Khác", new Guid("b0000000-0000-0000-0000-000000000001"), "NV001", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 9, 24, 17, 2, 37, 265, DateTimeKind.Utc).AddTicks(7972), "0345667892", "Admin", 1 });
        }
    }
}
