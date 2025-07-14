using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTourDetailsUpdatedByConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint for UpdatedById in TourDetails
            migrationBuilder.DropForeignKey(
                name: "FK_TourDetails_TourCompanies_UpdatedById",
                table: "TourDetails");

            // Make UpdatedById nullable and remove the constraint
            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedById",
                table: "TourDetails",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-add the foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_TourDetails_TourCompanies_UpdatedById",
                table: "TourDetails",
                column: "UpdatedById",
                principalTable: "TourCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
