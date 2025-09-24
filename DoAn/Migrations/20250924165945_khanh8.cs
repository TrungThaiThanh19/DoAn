using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Migrations
{
    /// <inheritdoc />
    public partial class khanh8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NhanViens",
                columns: new[] { "ID_NhanVien", "DiaChiLienHe", "Email", "GioiTinh", "ID_TaiKhoan", "Ma_NhanVien", "NgaySinh", "NgayThamGia", "SoDienThoai", "Ten_NhanVien", "TrangThai" },
                values: new object[] { new Guid("c0000000-0000-0000-0000-000000000001"), "Hệ thống", "admin@gamil.com", "Khác", new Guid("b0000000-0000-0000-0000-000000000001"), "NV001", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 9, 24, 16, 59, 42, 466, DateTimeKind.Utc).AddTicks(6479), "0345667892", "Admin", 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NhanViens",
                keyColumn: "ID_NhanVien",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000001"));
        }
    }
}
