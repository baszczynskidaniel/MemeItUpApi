using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemeItUpApi.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TextPositions_MemeTemplates_MemeTemplateId",
                table: "TextPositions");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemeTemplateId",
                table: "TextPositions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_TextPositions_MemeTemplates_MemeTemplateId",
                table: "TextPositions",
                column: "MemeTemplateId",
                principalTable: "MemeTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TextPositions_MemeTemplates_MemeTemplateId",
                table: "TextPositions");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemeTemplateId",
                table: "TextPositions",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TextPositions_MemeTemplates_MemeTemplateId",
                table: "TextPositions",
                column: "MemeTemplateId",
                principalTable: "MemeTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
