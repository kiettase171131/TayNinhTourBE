using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddTourDetailsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TourDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    TourTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, comment: "ID của tour template mà chi tiết này thuộc về"),
                    TimeSlot = table.Column<TimeOnly>(type: "time", nullable: false, comment: "Thời gian trong ngày cho hoạt động này"),
                    Location = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "Địa điểm hoặc tên hoạt động"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true, comment: "Mô tả chi tiết về hoạt động"),
                    ShopId = table.Column<Guid>(type: "char(36)", nullable: true, comment: "ID của shop liên quan (nếu có)"),
                    SortOrder = table.Column<int>(type: "int", nullable: false, comment: "Thứ tự sắp xếp trong timeline"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourDetails", x => x.Id);
                    table.CheckConstraint("CK_TourDetails_SortOrder_Positive", "SortOrder > 0");
                    table.ForeignKey(
                        name: "FK_TourDetails_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TourDetails_TourTemplates_TourTemplateId",
                        column: x => x.TourTemplateId,
                        principalTable: "TourTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_ShopId",
                table: "TourDetails",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TimeSlot",
                table: "TourDetails",
                column: "TimeSlot");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TourTemplateId",
                table: "TourDetails",
                column: "TourTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TourTemplateId_SortOrder",
                table: "TourDetails",
                columns: new[] { "TourTemplateId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TourTemplateId_TimeSlot",
                table: "TourDetails",
                columns: new[] { "TourTemplateId", "TimeSlot" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourDetails");
        }
    }
}
