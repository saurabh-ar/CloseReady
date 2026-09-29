using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloseReady.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCycleDocumentFileInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "CycleDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                table: "CycleDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "CycleDocuments");

            migrationBuilder.DropColumn(
                name: "StoredFileName",
                table: "CycleDocuments");
        }
    }
}
