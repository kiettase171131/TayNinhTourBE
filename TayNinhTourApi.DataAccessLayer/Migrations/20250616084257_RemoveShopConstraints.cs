using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShopConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraint from TourDetails to Shops
            migrationBuilder.DropForeignKey(
                name: "FK_TourDetails_Shops_ShopId",
                table: "TourDetails");

            // Drop ShopId column from TourDetails if it exists
            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "TourDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: This rollback would require recreating the Shop table first
            // and migrating data from SpecialtyShop back to Shop
            // For now, leaving empty as this is part of a planned merge operation
        }
    }
}
