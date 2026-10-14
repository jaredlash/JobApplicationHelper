using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplicationHelper.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingExperienceImports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingExperienceImports",
                columns: table => new
                {
                    ExperienceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SerializedExperience = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingExperienceImports", x => x.ExperienceId);
                    table.ForeignKey(
                        name: "FK_PendingExperienceImports_Experiences_ExperienceId",
                        column: x => x.ExperienceId,
                        principalTable: "Experiences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingExperienceImports");
        }
    }
}
