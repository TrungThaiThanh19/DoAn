using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Migrations
{
    /// <inheritdoc />
    public partial class khanh5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                newName: "QuanLyTraHangs");

            migrationBuilder.RenameIndex(
                name: "IX_QuanLyTraHang_ID_HoaDon",
                table: "QuanLyTraHangs",
                newName: "IX_QuanLyTraHangs_ID_HoaDon");

            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "SanPhamChiTiets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MaSanPhamChiTiet",
                table: "SanPhamChiTiets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MaQuocGia",
                table: "QuocGias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TrangThai",
                table: "QuocGias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MaGioiTinh",
                table: "GioiTinhs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TrangThai",
                table: "GioiTinhs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuanLyTraHangs",
                table: "QuanLyTraHangs",
                column: "ID_TraHang");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietTraHangs_QuanLyTraHangs_TraHangID_TraHang",
                table: "ChiTietTraHangs",
                column: "TraHangID_TraHang",
                principalTable: "QuanLyTraHangs",
                principalColumn: "ID_TraHang",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuanLyTraHangs_HoaDons_ID_HoaDon",
                table: "QuanLyTraHangs",
                column: "ID_HoaDon",
                principalTable: "HoaDons",
                principalColumn: "ID_HoaDon",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietTraHangs_QuanLyTraHangs_TraHangID_TraHang",
                table: "ChiTietTraHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_QuanLyTraHangs_HoaDons_ID_HoaDon",
                table: "QuanLyTraHangs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuanLyTraHangs",
                table: "QuanLyTraHangs");

            migrationBuilder.DropColumn(
                name: "HinhAnh",
                table: "SanPhamChiTiets");

            migrationBuilder.DropColumn(
                name: "MaSanPhamChiTiet",
                table: "SanPhamChiTiets");

            migrationBuilder.DropColumn(
                name: "MaQuocGia",
                table: "QuocGias");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "QuocGias");

            migrationBuilder.DropColumn(
                name: "MaGioiTinh",
                table: "GioiTinhs");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "GioiTinhs");

            migrationBuilder.RenameTable(
                name: "QuanLyTraHangs",
                newName: "QuanLyTraHang");

            migrationBuilder.RenameIndex(
                name: "IX_QuanLyTraHangs_ID_HoaDon",
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
    }
}
