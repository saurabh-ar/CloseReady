using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloseReady.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientUploadLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientUploadLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollectionCycleId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientUploadLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientUploadLinks_CollectionCycles_CollectionCycleId",
                        column: x => x.CollectionCycleId,
                        principalTable: "CollectionCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientUploadLinks_CollectionCycleId",
                table: "ClientUploadLinks",
                column: "CollectionCycleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientUploadLinks");
        }
    }
}
