using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddTourSlotIndexesAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "TourTemplateId",
                table: "TourSlots",
                type: "char(36)",
                nullable: false,
                comment: "ID của TourTemplate mà slot này được tạo từ",
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TourDate",
                table: "TourSlots",
                type: "date",
                nullable: false,
                comment: "Ngày tour cụ thể sẽ diễn ra",
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<int>(
                name: "ScheduleDay",
                table: "TourSlots",
                type: "int",
                nullable: false,
                comment: "Ngày trong tuần của tour (Saturday hoặc Sunday)",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TourSlots",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true,
                comment: "Trạng thái slot có sẵn sàng để booking không",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_IsActive",
                table: "TourSlots",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_ScheduleDay",
                table: "TourSlots",
                column: "ScheduleDay");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_ScheduleDay_IsActive",
                table: "TourSlots",
                columns: new[] { "ScheduleDay", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_TourDate",
                table: "TourSlots",
                column: "TourDate");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_TourDate_IsActive",
                table: "TourSlots",
                columns: new[] { "TourDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_TourTemplateId_TourDate",
                table: "TourSlots",
                columns: new[] { "TourTemplateId", "TourDate" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_TourSlots_TourDate_NotPast",
                table: "TourSlots",
                sql: "TourDate >= CURDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TourSlots_IsActive",
                table: "TourSlots");

            migrationBuilder.DropIndex(
                name: "IX_TourSlots_ScheduleDay",
                table: "TourSlots");

            migrationBuilder.DropIndex(
                name: "IX_TourSlots_ScheduleDay_IsActive",
                table: "TourSlots");

            migrationBuilder.DropIndex(
                name: "IX_TourSlots_TourDate",
                table: "TourSlots");

            migrationBuilder.DropIndex(
                name: "IX_TourSlots_TourDate_IsActive",
                table: "TourSlots");

            migrationBuilder.DropIndex(
                name: "IX_TourSlots_TourTemplateId_TourDate",
                table: "TourSlots");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TourSlots_TourDate_NotPast",
                table: "TourSlots");

            migrationBuilder.AlterColumn<Guid>(
                name: "TourTemplateId",
                table: "TourSlots",
                type: "char(36)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldComment: "ID của TourTemplate mà slot này được tạo từ");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TourDate",
                table: "TourSlots",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldComment: "Ngày tour cụ thể sẽ diễn ra");

            migrationBuilder.AlterColumn<int>(
                name: "ScheduleDay",
                table: "TourSlots",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Ngày trong tuần của tour (Saturday hoặc Sunday)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "TourSlots",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: true,
                oldComment: "Trạng thái slot có sẵn sàng để booking không");
        }
    }
}
