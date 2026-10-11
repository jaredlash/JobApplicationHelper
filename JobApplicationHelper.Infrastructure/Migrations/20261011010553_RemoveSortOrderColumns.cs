using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplicationHelper.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSortOrderColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExperienceLinks_ExperienceId_SortOrder",
                table: "ExperienceLinks");

            migrationBuilder.DropIndex(
                name: "IX_ExperienceEvidence_ExperienceId_SortOrder",
                table: "ExperienceEvidence");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "ExperienceLinks");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "ExperienceEvidence");

            migrationBuilder.CreateIndex(
                name: "IX_ExperienceLinks_ExperienceId",
                table: "ExperienceLinks",
                column: "ExperienceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExperienceEvidence_ExperienceId",
                table: "ExperienceEvidence",
                column: "ExperienceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExperienceLinks_ExperienceId",
                table: "ExperienceLinks");

            migrationBuilder.DropIndex(
                name: "IX_ExperienceEvidence_ExperienceId",
                table: "ExperienceEvidence");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ExperienceLinks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ExperienceEvidence",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExperienceLinks_ExperienceId_SortOrder",
                table: "ExperienceLinks",
                columns: new[] { "ExperienceId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ExperienceEvidence_ExperienceId_SortOrder",
                table: "ExperienceEvidence",
                columns: new[] { "ExperienceId", "SortOrder" });
        }
    }
}
