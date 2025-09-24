using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Migrations
{
    /// <inheritdoc />
    public partial class khanh9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NhanViens",
                keyColumn: "ID_NhanVien",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000001"),
                column: "NgayThamGia",
                value: new DateTime(2025, 9, 24, 17, 1, 8, 461, DateTimeKind.Utc).AddTicks(9639));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NhanViens",
                keyColumn: "ID_NhanVien",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000001"),
                column: "NgayThamGia",
                value: new DateTime(2025, 9, 24, 16, 59, 42, 466, DateTimeKind.Utc).AddTicks(6479));
        }
    }
}
