using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddTourSlotIdToTourBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TourSlotId",
                table: "TourBookings",
                type: "char(36)",
                nullable: true,
                comment: "ID của TourSlot cụ thể được booking (optional)",
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_TourBookings_TourSlotId",
                table: "TourBookings",
                column: "TourSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_TourBookings_TourSlots_TourSlotId",
                table: "TourBookings",
                column: "TourSlotId",
                principalTable: "TourSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourBookings_TourSlots_TourSlotId",
                table: "TourBookings");

            migrationBuilder.DropIndex(
                name: "IX_TourBookings_TourSlotId",
                table: "TourBookings");

            migrationBuilder.DropColumn(
                name: "TourSlotId",
                table: "TourBookings");
        }
    }
}
