using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Migrations
{
    /// <inheritdoc />
    public partial class khanh1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietTraHangs_TraHangs_TraHangID_TraHang",
                table: "ChiTietTraHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_TraHangs_HoaDons_ID_HoaDon",
                table: "TraHangs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TraHangs",
                table: "TraHangs");

            migrationBuilder.RenameTable(
                name: "TraHangs",
                newName: "QuanLyTraHang");

            migrationBuilder.RenameIndex(
                name: "IX_TraHangs_ID_HoaDon",
                table: "QuanLyTraHang",
                newName: "IX_QuanLyTraHang_ID_HoaDon");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuanLyTraHang",
                table: "QuanLyTraHang",
                column: "ID_TraHang");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietTraHangs_QuanLyTraHang_TraHangID_TraHang",
                table: "ChiTietTraHangs",
                column: "TraHangID_TraHang",
                principalTable: "QuanLyTraHang",
                principalColumn: "ID_TraHang",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuanLyTraHang_HoaDons_ID_HoaDon",
                table: "QuanLyTraHang",
                column: "ID_HoaDon",
                principalTable: "HoaDons",
                principalColumn: "ID_HoaDon",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietTraHangs_QuanLyTraHang_TraHangID_TraHang",
                table: "ChiTietTraHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_QuanLyTraHang_HoaDons_ID_HoaDon",
                table: "QuanLyTraHang");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuanLyTraHang",
                table: "QuanLyTraHang");

            migrationBuilder.RenameTable(
                name: "QuanLyTraHang",
                newName: "TraHangs");

            migrationBuilder.RenameIndex(
                name: "IX_QuanLyTraHang_ID_HoaDon",
                table: "TraHangs",
                newName: "IX_TraHangs_ID_HoaDon");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TraHangs",
                table: "TraHangs",
                column: "ID_TraHang");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietTraHangs_TraHangs_TraHangID_TraHang",
                table: "ChiTietTraHangs",
                column: "TraHangID_TraHang",
                principalTable: "TraHangs",
                principalColumn: "ID_TraHang",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TraHangs_HoaDons_ID_HoaDon",
                table: "TraHangs",
                column: "ID_HoaDon",
                principalTable: "HoaDons",
                principalColumn: "ID_HoaDon",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
