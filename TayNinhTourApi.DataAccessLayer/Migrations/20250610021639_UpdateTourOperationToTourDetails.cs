using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTourOperationToTourDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourDetails_Shops_ShopId",
                table: "TourDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TourOperations_TourSlots_TourSlotId",
                table: "TourOperations");

            migrationBuilder.DropIndex(
                name: "IX_TourOperations_TourSlotId_Unique",
                table: "TourOperations");

            migrationBuilder.DropIndex(
                name: "IX_TourDetails_TimeSlot",
                table: "TourDetails");

            migrationBuilder.DropIndex(
                name: "IX_TourDetails_TourTemplateId_SortOrder",
                table: "TourDetails");

            migrationBuilder.DropIndex(
                name: "IX_TourDetails_TourTemplateId_TimeSlot",
                table: "TourDetails");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TourDetails_SortOrder_Positive",
                table: "TourDetails");

            migrationBuilder.DropColumn(
                name: "TourSlotId",
                table: "TourOperations");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "TourDetails");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "TourDetails");

            migrationBuilder.DropColumn(
                name: "TimeSlot",
                table: "TourDetails");

            migrationBuilder.AddColumn<Guid>(
                name: "TourDetailsId",
                table: "TourSlots",
                type: "char(36)",
                nullable: true,
                comment: "ID của TourDetails được assign cho slot này",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TourDetailsId",
                table: "TourOperations",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "ID của TourDetails mà operation này thuộc về",
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "ShopId",
                table: "TourDetails",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true,
                oldComment: "ID của shop liên quan (nếu có)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TourDetails",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Mô tả về lịch trình này",
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "Mô tả chi tiết về hoạt động")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TourDetails",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                comment: "Tiêu đề của lịch trình")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TimelineItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    TourDetailsId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    CheckInTime = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    Activity = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShopId = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedById = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    UpdatedById = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimelineItem_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TimelineItem_TourDetails_TourDetailsId",
                        column: x => x.TourDetailsId,
                        principalTable: "TourDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimelineItem_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimelineItem_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_TourDetailsId",
                table: "TourSlots",
                column: "TourDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_TourDetailsId_Unique",
                table: "TourOperations",
                column: "TourDetailsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_Title",
                table: "TourDetails",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineItem_CreatedById",
                table: "TimelineItem",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineItem_ShopId",
                table: "TimelineItem",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineItem_TourDetailsId",
                table: "TimelineItem",
                column: "TourDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_TimelineItem_TourDetailsId_SortOrder",
                table: "TimelineItem",
                columns: new[] { "TourDetailsId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TimelineItem_UpdatedById",
                table: "TimelineItem",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_TourDetails_Shops_ShopId",
                table: "TourDetails",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourOperations_TourDetails_TourDetailsId",
                table: "TourOperations",
                column: "TourDetailsId",
                principalTable: "TourDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TourSlots_TourDetails_TourDetailsId",
                table: "TourSlots",
                column: "TourDetailsId",
                principalTable: "TourDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourDetails_Shops_ShopId",
                table: "TourDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TourOperations_TourDetails_TourDetailsId",
                table: "TourOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_TourSlots_TourDetails_TourDetailsId",
                table: "TourSlots");

            migrationBuilder.DropTable(
                name: "TimelineItem");

            migrationBuilder.DropIndex(
                name: "IX_TourSlots_TourDetailsId",
                table: "TourSlots");

            migrationBuilder.DropIndex(
                name: "IX_TourOperations_TourDetailsId_Unique",
                table: "TourOperations");

            migrationBuilder.DropIndex(
                name: "IX_TourDetails_Title",
                table: "TourDetails");

            migrationBuilder.DropColumn(
                name: "TourDetailsId",
                table: "TourSlots");

            migrationBuilder.DropColumn(
                name: "TourDetailsId",
                table: "TourOperations");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TourDetails");

            migrationBuilder.AddColumn<Guid>(
                name: "TourSlotId",
                table: "TourOperations",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "ID của TourSlot mà operation này thuộc về",
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "ShopId",
                table: "TourDetails",
                type: "char(36)",
                nullable: true,
                comment: "ID của shop liên quan (nếu có)",
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TourDetails",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Mô tả chi tiết về hoạt động",
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "Mô tả về lịch trình này")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "TourDetails",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "Địa điểm hoặc tên hoạt động")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "TourDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Thứ tự sắp xếp trong timeline");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TimeSlot",
                table: "TourDetails",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                comment: "Thời gian trong ngày cho hoạt động này");

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_TourSlotId_Unique",
                table: "TourOperations",
                column: "TourSlotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TimeSlot",
                table: "TourDetails",
                column: "TimeSlot");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TourTemplateId_SortOrder",
                table: "TourDetails",
                columns: new[] { "TourTemplateId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TourTemplateId_TimeSlot",
                table: "TourDetails",
                columns: new[] { "TourTemplateId", "TimeSlot" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_TourDetails_SortOrder_Positive",
                table: "TourDetails",
                sql: "SortOrder > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_TourDetails_Shops_ShopId",
                table: "TourDetails",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TourOperations_TourSlots_TourSlotId",
                table: "TourOperations",
                column: "TourSlotId",
                principalTable: "TourSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
