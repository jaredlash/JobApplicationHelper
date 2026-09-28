using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplicationHelper.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBackgroundJobPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "BackgroundJobs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "BackgroundJobs");
        }
    }
}
