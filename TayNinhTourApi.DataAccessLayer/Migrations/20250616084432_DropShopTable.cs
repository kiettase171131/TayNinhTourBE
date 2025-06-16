using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class DropShopTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop Shop table completely
            migrationBuilder.DropTable(
                name: "Shops");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: This is a destructive migration.
            // Recreating Shop table would require manual data migration from SpecialtyShop
            // if needed for rollback scenarios.

            // For now, leaving empty as this is part of a planned merge operation
            // and rollback should be handled at the application level
        }
    }
}
