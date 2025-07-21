using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Migrations
{
    /// <inheritdoc />
    public partial class DoAn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SanPhams_QuocGia_ID_QuocGia",
                table: "SanPhams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuocGia",
                table: "QuocGia");

            migrationBuilder.RenameTable(
                name: "QuocGia",
                newName: "QuocGias");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuocGias",
                table: "QuocGias",
                column: "ID_QuocGia");

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
                name: "FK_SanPhams_QuocGias_ID_QuocGia",
                table: "SanPhams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuocGias",
                table: "QuocGias");

            migrationBuilder.RenameTable(
                name: "QuocGias",
                newName: "QuocGia");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuocGia",
                table: "QuocGia",
                column: "ID_QuocGia");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhams_QuocGia_ID_QuocGia",
                table: "SanPhams",
                column: "ID_QuocGia",
                principalTable: "QuocGia",
                principalColumn: "ID_QuocGia",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
