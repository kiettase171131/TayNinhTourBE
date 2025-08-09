using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddTourSlotTimelineProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TourSlotTimelineProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, comment: "Primary key identifier", collation: "ascii_general_ci"),
                    TourSlotId = table.Column<Guid>(type: "char(36)", nullable: false, comment: "Reference to the specific tour slot", collation: "ascii_general_ci"),
                    TimelineItemId = table.Column<Guid>(type: "char(36)", nullable: false, comment: "Reference to the timeline item template", collation: "ascii_general_ci"),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false, comment: "Whether this timeline item has been completed for this tour slot"),
                    CompletedAt = table.Column<DateTime>(type: "datetime", nullable: true, comment: "Timestamp when the timeline item was completed"),
                    CompletionNotes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "Optional notes added when completing the timeline item")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true, comment: "Soft delete flag"),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, comment: "ID of the user who created this record", collation: "ascii_general_ci"),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true, comment: "ID of the user who last updated this record", collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Timestamp when the record was created"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, comment: "Timestamp when the record was last updated"),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourSlotTimelineProgress", x => x.Id);
                    table.CheckConstraint("CK_TourSlotTimelineProgress_Completion_Logic", "(IsCompleted = FALSE AND CompletedAt IS NULL) OR (IsCompleted = TRUE AND CompletedAt IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_TourSlotTimelineProgress_CreatedBy",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourSlotTimelineProgress_TimelineItem",
                        column: x => x.TimelineItemId,
                        principalTable: "TimelineItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourSlotTimelineProgress_TourSlot",
                        column: x => x.TourSlotId,
                        principalTable: "TourSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourSlotTimelineProgress_UpdatedBy",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tracks timeline completion progress for individual tour slots")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlotTimelineProgress_CompletedAt",
                table: "TourSlotTimelineProgress",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlotTimelineProgress_CreatedAt",
                table: "TourSlotTimelineProgress",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlotTimelineProgress_CreatedById",
                table: "TourSlotTimelineProgress",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlotTimelineProgress_IsCompleted",
                table: "TourSlotTimelineProgress",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlotTimelineProgress_TimelineItemId",
                table: "TourSlotTimelineProgress",
                column: "TimelineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlotTimelineProgress_TourSlot_Completed",
                table: "TourSlotTimelineProgress",
                columns: new[] { "TourSlotId", "IsCompleted", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TourSlotTimelineProgress_TourSlotId",
                table: "TourSlotTimelineProgress",
                column: "TourSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlotTimelineProgress_UpdatedById",
                table: "TourSlotTimelineProgress",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "UK_TourSlotTimeline",
                table: "TourSlotTimelineProgress",
                columns: new[] { "TourSlotId", "TimelineItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourSlotTimelineProgress");
        }
    }
}
