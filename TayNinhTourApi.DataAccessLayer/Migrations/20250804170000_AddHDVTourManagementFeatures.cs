using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddHDVTourManagementFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add check-in fields to TourBookings table
            migrationBuilder.AddColumn<bool>(
                name: "IsCheckedIn",
                table: "TourBookings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime",
                table: "TourBookings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckInNotes",
                table: "TourBookings",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Add completion fields to TimelineItems table
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "TimelineItems",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "TimelineItems",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionNotes",
                table: "TimelineItems",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create TourIncidents table
            migrationBuilder.CreateTable(
                name: "TourIncidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TourOperationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReportedByGuideId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Severity = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrls = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AdminNotes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourIncidents_TourGuides_ReportedByGuideId",
                        column: x => x.ReportedByGuideId,
                        principalTable: "TourGuides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourIncidents_TourOperations_TourOperationId",
                        column: x => x.TourOperationId,
                        principalTable: "TourOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourIncidents_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourIncidents_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create indexes for TourIncidents
            migrationBuilder.CreateIndex(
                name: "IX_TourIncidents_TourOperationId",
                table: "TourIncidents",
                column: "TourOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_TourIncidents_ReportedByGuideId",
                table: "TourIncidents",
                column: "ReportedByGuideId");

            migrationBuilder.CreateIndex(
                name: "IX_TourIncidents_CreatedById",
                table: "TourIncidents",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TourIncidents_UpdatedById",
                table: "TourIncidents",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop TourIncidents table
            migrationBuilder.DropTable(
                name: "TourIncidents");

            // Remove completion fields from TimelineItems table
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "TimelineItems");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "TimelineItems");

            migrationBuilder.DropColumn(
                name: "CompletionNotes",
                table: "TimelineItems");

            // Remove check-in fields from TourBookings table
            migrationBuilder.DropColumn(
                name: "IsCheckedIn",
                table: "TourBookings");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "TourBookings");

            migrationBuilder.DropColumn(
                name: "CheckInNotes",
                table: "TourBookings");
        }
    }
}
