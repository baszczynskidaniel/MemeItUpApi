using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemeItUpApi.Migrations
{
    /// <inheritdoc />
    public partial class Init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemeTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemeTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TextPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Top = table.Column<float>(type: "REAL", nullable: false),
                    Bottom = table.Column<float>(type: "REAL", nullable: false),
                    Left = table.Column<float>(type: "REAL", nullable: false),
                    Right = table.Column<float>(type: "REAL", nullable: false),
                    MemeTemplateId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextPositions_MemeTemplates_MemeTemplateId",
                        column: x => x.MemeTemplateId,
                        principalTable: "MemeTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TextPositions_MemeTemplateId",
                table: "TextPositions",
                column: "MemeTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TextPositions");

            migrationBuilder.DropTable(
                name: "MemeTemplates");
        }
    }
}
