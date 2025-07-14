using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class FixTourTemplateFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourDetails_TourCompanies_CreatedById",
                table: "TourDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TourDetails_Users_UpdatedById",
                table: "TourDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TourOperations_TourCompanies_CreatedById",
                table: "TourOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_TourOperations_Users_UpdatedById",
                table: "TourOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_TourTemplates_TourCompanies_CreatedById",
                table: "TourTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_TourTemplates_Users_UpdatedById",
                table: "TourTemplates");

            migrationBuilder.AddForeignKey(
                name: "FK_TourDetails_Users_UpdatedById",
                table: "TourDetails",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TourOperations_Users_UpdatedById",
                table: "TourOperations",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TourTemplates_Users_UpdatedById",
                table: "TourTemplates",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourDetails_Users_UpdatedById",
                table: "TourDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TourOperations_Users_UpdatedById",
                table: "TourOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_TourTemplates_Users_UpdatedById",
                table: "TourTemplates");

            migrationBuilder.AddForeignKey(
                name: "FK_TourDetails_TourCompanies_CreatedById",
                table: "TourDetails",
                column: "CreatedById",
                principalTable: "TourCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TourDetails_Users_UpdatedById",
                table: "TourDetails",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TourOperations_TourCompanies_CreatedById",
                table: "TourOperations",
                column: "CreatedById",
                principalTable: "TourCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TourOperations_Users_UpdatedById",
                table: "TourOperations",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TourTemplates_TourCompanies_CreatedById",
                table: "TourTemplates",
                column: "CreatedById",
                principalTable: "TourCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TourTemplates_Users_UpdatedById",
                table: "TourTemplates",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
