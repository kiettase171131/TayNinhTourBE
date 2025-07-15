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
            // Drop existing foreign keys that point to TourCompanies
            migrationBuilder.DropForeignKey(
                name: "FK_TourDetails_TourCompanies_CreatedById",
                table: "TourDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TourDetails_TourCompanies_UpdatedById",
                table: "TourDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TourOperations_TourCompanies_CreatedById",
                table: "TourOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_TourOperations_TourCompanies_UpdatedById",
                table: "TourOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_TourTemplates_TourCompanies_CreatedById",
                table: "TourTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_TourTemplates_TourCompanies_UpdatedById",
                table: "TourTemplates");

            // Convert TourCompany IDs to User IDs for all affected tables
            // Update TourDetails
            migrationBuilder.Sql(@"
                UPDATE TourDetails
                SET CreatedById = (
                    SELECT tc.UserId
                    FROM TourCompanies tc
                    WHERE tc.Id = TourDetails.CreatedById
                )
                WHERE EXISTS (
                    SELECT 1 FROM TourCompanies tc
                    WHERE tc.Id = TourDetails.CreatedById
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE TourDetails
                SET UpdatedById = (
                    SELECT tc.UserId
                    FROM TourCompanies tc
                    WHERE tc.Id = TourDetails.UpdatedById
                )
                WHERE UpdatedById IS NOT NULL
                AND EXISTS (
                    SELECT 1 FROM TourCompanies tc
                    WHERE tc.Id = TourDetails.UpdatedById
                );
            ");

            // Update TourOperations
            migrationBuilder.Sql(@"
                UPDATE TourOperations
                SET CreatedById = (
                    SELECT tc.UserId
                    FROM TourCompanies tc
                    WHERE tc.Id = TourOperations.CreatedById
                )
                WHERE EXISTS (
                    SELECT 1 FROM TourCompanies tc
                    WHERE tc.Id = TourOperations.CreatedById
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE TourOperations
                SET UpdatedById = (
                    SELECT tc.UserId
                    FROM TourCompanies tc
                    WHERE tc.Id = TourOperations.UpdatedById
                )
                WHERE UpdatedById IS NOT NULL
                AND EXISTS (
                    SELECT 1 FROM TourCompanies tc
                    WHERE tc.Id = TourOperations.UpdatedById
                );
            ");

            // Update TourTemplates
            migrationBuilder.Sql(@"
                UPDATE TourTemplates
                SET CreatedById = (
                    SELECT tc.UserId
                    FROM TourCompanies tc
                    WHERE tc.Id = TourTemplates.CreatedById
                )
                WHERE EXISTS (
                    SELECT 1 FROM TourCompanies tc
                    WHERE tc.Id = TourTemplates.CreatedById
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE TourTemplates
                SET UpdatedById = (
                    SELECT tc.UserId
                    FROM TourCompanies tc
                    WHERE tc.Id = TourTemplates.UpdatedById
                )
                WHERE UpdatedById IS NOT NULL
                AND EXISTS (
                    SELECT 1 FROM TourCompanies tc
                    WHERE tc.Id = TourTemplates.UpdatedById
                );
            ");

            // Add new foreign keys that point to Users
            migrationBuilder.AddForeignKey(
                name: "FK_TourDetails_Users_CreatedById",
                table: "TourDetails",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TourDetails_Users_UpdatedById",
                table: "TourDetails",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TourOperations_Users_CreatedById",
                table: "TourOperations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TourOperations_Users_UpdatedById",
                table: "TourOperations",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TourTemplates_Users_CreatedById",
                table: "TourTemplates",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
            // Drop the foreign keys that point to Users
            migrationBuilder.DropForeignKey(
                name: "FK_TourDetails_Users_CreatedById",
                table: "TourDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TourDetails_Users_UpdatedById",
                table: "TourDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TourOperations_Users_CreatedById",
                table: "TourOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_TourOperations_Users_UpdatedById",
                table: "TourOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_TourTemplates_Users_CreatedById",
                table: "TourTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_TourTemplates_Users_UpdatedById",
                table: "TourTemplates");

            // Restore the original foreign keys that point to TourCompanies
            migrationBuilder.AddForeignKey(
                name: "FK_TourDetails_TourCompanies_CreatedById",
                table: "TourDetails",
                column: "CreatedById",
                principalTable: "TourCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TourDetails_TourCompanies_UpdatedById",
                table: "TourDetails",
                column: "UpdatedById",
                principalTable: "TourCompanies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourOperations_TourCompanies_CreatedById",
                table: "TourOperations",
                column: "CreatedById",
                principalTable: "TourCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TourOperations_TourCompanies_UpdatedById",
                table: "TourOperations",
                column: "UpdatedById",
                principalTable: "TourCompanies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourTemplates_TourCompanies_CreatedById",
                table: "TourTemplates",
                column: "CreatedById",
                principalTable: "TourCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TourTemplates_TourCompanies_UpdatedById",
                table: "TourTemplates",
                column: "UpdatedById",
                principalTable: "TourCompanies",
                principalColumn: "Id");
        }
    }
}
