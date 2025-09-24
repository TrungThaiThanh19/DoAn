using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Migrations
{
    /// <inheritdoc />
    public partial class khanh10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NhanViens",
                keyColumn: "ID_NhanVien",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000001"),
                columns: new[] { "NgayThamGia", "TrangThai" },
                values: new object[] { new DateTime(2025, 9, 24, 17, 2, 37, 265, DateTimeKind.Utc).AddTicks(7972), 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NhanViens",
                keyColumn: "ID_NhanVien",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000001"),
                columns: new[] { "NgayThamGia", "TrangThai" },
                values: new object[] { new DateTime(2025, 9, 24, 17, 1, 8, 461, DateTimeKind.Utc).AddTicks(9639), 0 });
        }
    }
}
