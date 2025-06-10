using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTourSlotUniqueConstraintForCloneLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TourSlots_TourTemplateId_TourDate",
                table: "TourSlots");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_TourTemplateId_TourDate_TourDetailsId",
                table: "TourSlots",
                columns: new[] { "TourTemplateId", "TourDate", "TourDetailsId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TourSlots_TourTemplateId_TourDate_TourDetailsId",
                table: "TourSlots");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_TourTemplateId_TourDate",
                table: "TourSlots",
                columns: new[] { "TourTemplateId", "TourDate" },
                unique: true);
        }
    }
}
