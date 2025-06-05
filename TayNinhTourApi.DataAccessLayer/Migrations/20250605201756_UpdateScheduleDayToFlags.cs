using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScheduleDayToFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Tours",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "TimeSlot",
                table: "TourDetails",
                type: "time(6)",
                nullable: false,
                comment: "Thời gian trong ngày cho hoạt động này",
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldComment: "Thời gian trong ngày cho hoạt động này");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Tours",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "TimeSlot",
                table: "TourDetails",
                type: "time",
                nullable: false,
                comment: "Thời gian trong ngày cho hoạt động này",
                oldClrType: typeof(TimeOnly),
                oldType: "time(6)",
                oldComment: "Thời gian trong ngày cho hoạt động này");
        }
    }
}
