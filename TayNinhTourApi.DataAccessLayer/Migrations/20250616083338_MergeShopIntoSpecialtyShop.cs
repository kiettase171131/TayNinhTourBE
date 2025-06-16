using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class MergeShopIntoSpecialtyShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimelineItem_Shops_ShopId",
                table: "TimelineItem");

            migrationBuilder.RenameColumn(
                name: "ShopId",
                table: "TimelineItem",
                newName: "SpecialtyShopId");

            migrationBuilder.RenameIndex(
                name: "IX_TimelineItem_ShopId",
                table: "TimelineItem",
                newName: "IX_TimelineItem_SpecialtyShopId");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "SpecialtyShops",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_TimelineItem_SpecialtyShops_SpecialtyShopId",
                table: "TimelineItem",
                column: "SpecialtyShopId",
                principalTable: "SpecialtyShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimelineItem_SpecialtyShops_SpecialtyShopId",
                table: "TimelineItem");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "SpecialtyShops");

            migrationBuilder.RenameColumn(
                name: "SpecialtyShopId",
                table: "TimelineItem",
                newName: "ShopId");

            migrationBuilder.RenameIndex(
                name: "IX_TimelineItem_SpecialtyShopId",
                table: "TimelineItem",
                newName: "IX_TimelineItem_ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimelineItem_Shops_ShopId",
                table: "TimelineItem",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
