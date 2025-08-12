using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueCartPerCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GioHangs_KhachHangs_KhachHangID_KhachHang",
                table: "GioHangs");

            migrationBuilder.DropIndex(
                name: "IX_GioHangs_KhachHangID_KhachHang",
                table: "GioHangs");

            migrationBuilder.DropColumn(
                name: "KhachHangID_KhachHang",
                table: "GioHangs");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_ID_KhachHang",
                table: "GioHangs",
                column: "ID_KhachHang",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangs_KhachHangs_ID_KhachHang",
                table: "GioHangs",
                column: "ID_KhachHang",
                principalTable: "KhachHangs",
                principalColumn: "ID_KhachHang",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GioHangs_KhachHangs_ID_KhachHang",
                table: "GioHangs");

            migrationBuilder.DropIndex(
                name: "IX_GioHangs_ID_KhachHang",
                table: "GioHangs");

            migrationBuilder.AddColumn<Guid>(
                name: "KhachHangID_KhachHang",
                table: "GioHangs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_KhachHangID_KhachHang",
                table: "GioHangs",
                column: "KhachHangID_KhachHang");

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangs_KhachHangs_KhachHangID_KhachHang",
                table: "GioHangs",
                column: "KhachHangID_KhachHang",
                principalTable: "KhachHangs",
                principalColumn: "ID_KhachHang",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
