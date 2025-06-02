using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddTourOperationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TourOperations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    TourSlotId = table.Column<Guid>(type: "char(36)", nullable: false, comment: "ID của TourSlot mà operation này thuộc về"),
                    GuideId = table.Column<Guid>(type: "char(36)", nullable: false, comment: "ID của User làm hướng dẫn viên cho tour này"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, comment: "Giá tour cho operation này"),
                    MaxGuests = table.Column<int>(type: "int", nullable: false, comment: "Số lượng khách tối đa cho tour operation này"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true, comment: "Mô tả bổ sung cho tour operation"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true, comment: "Trạng thái hoạt động của tour operation"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourOperations", x => x.Id);
                    table.CheckConstraint("CK_TourOperations_MaxGuests_Positive", "MaxGuests > 0");
                    table.CheckConstraint("CK_TourOperations_Price_Positive", "Price > 0");
                    table.ForeignKey(
                        name: "FK_TourOperations_TourSlots_TourSlotId",
                        column: x => x.TourSlotId,
                        principalTable: "TourSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourOperations_Users_GuideId",
                        column: x => x.GuideId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_GuideId",
                table: "TourOperations",
                column: "GuideId");

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_GuideId_IsActive",
                table: "TourOperations",
                columns: new[] { "GuideId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_IsActive",
                table: "TourOperations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_TourSlotId_Unique",
                table: "TourOperations",
                column: "TourSlotId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourOperations");
        }
    }
}
