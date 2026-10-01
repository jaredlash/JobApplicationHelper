using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplicationHelper.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppIdAndPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobApplicationId",
                table: "BackgroundJobs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Payload",
                table: "BackgroundJobs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_JobApplicationId",
                table: "BackgroundJobs",
                column: "JobApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_BackgroundJobs_JobApplications_JobApplicationId",
                table: "BackgroundJobs",
                column: "JobApplicationId",
                principalTable: "JobApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BackgroundJobs_JobApplications_JobApplicationId",
                table: "BackgroundJobs");

            migrationBuilder.DropIndex(
                name: "IX_BackgroundJobs_JobApplicationId",
                table: "BackgroundJobs");

            migrationBuilder.DropColumn(
                name: "JobApplicationId",
                table: "BackgroundJobs");

            migrationBuilder.DropColumn(
                name: "Payload",
                table: "BackgroundJobs");
        }
    }
}
