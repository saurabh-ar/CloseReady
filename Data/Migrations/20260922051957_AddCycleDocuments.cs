using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloseReady.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCycleDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CycleDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollectionCycleId = table.Column<int>(type: "int", nullable: false),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CycleDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CycleDocuments_CollectionCycles_CollectionCycleId",
                        column: x => x.CollectionCycleId,
                        principalTable: "CollectionCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CycleDocuments_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CycleDocuments_CollectionCycleId",
                table: "CycleDocuments",
                column: "CollectionCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_CycleDocuments_DocumentTypeId",
                table: "CycleDocuments",
                column: "DocumentTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CycleDocuments");
        }
    }
}
