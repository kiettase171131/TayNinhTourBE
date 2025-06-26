using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePriceConstraintToAllowZero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TourOperations_Price_Positive",
                table: "TourOperations");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TourOperations_Price_Positive",
                table: "TourOperations",
                sql: "Price >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TourOperations_Price_Positive",
                table: "TourOperations");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TourOperations_Price_Positive",
                table: "TourOperations",
                sql: "Price > 0");
        }
    }
}
