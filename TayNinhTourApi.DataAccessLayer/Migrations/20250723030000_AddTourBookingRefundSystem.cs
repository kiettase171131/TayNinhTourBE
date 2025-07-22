using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddTourBookingRefundSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create RefundPolicies table
            migrationBuilder.CreateTable(
                name: "RefundPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RefundType = table.Column<int>(type: "int", nullable: false),
                    MinDaysBeforeEvent = table.Column<int>(type: "int", nullable: false),
                    MaxDaysBeforeEvent = table.Column<int>(type: "int", nullable: true),
                    RefundPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProcessingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ProcessingFeePercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    EffectiveTo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    InternalNotes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundPolicies", x => x.Id);
                    table.CheckConstraint("CK_RefundPolicy_MinDaysBeforeEvent_NonNegative", "MinDaysBeforeEvent >= 0");
                    table.CheckConstraint("CK_RefundPolicy_MaxDaysBeforeEvent_Valid", "MaxDaysBeforeEvent IS NULL OR MaxDaysBeforeEvent >= MinDaysBeforeEvent");
                    table.CheckConstraint("CK_RefundPolicy_RefundPercentage_Valid", "RefundPercentage >= 0 AND RefundPercentage <= 100");
                    table.CheckConstraint("CK_RefundPolicy_ProcessingFee_NonNegative", "ProcessingFee >= 0");
                    table.CheckConstraint("CK_RefundPolicy_ProcessingFeePercentage_Valid", "ProcessingFeePercentage >= 0 AND ProcessingFeePercentage <= 100");
                    table.CheckConstraint("CK_RefundPolicy_Priority_Valid", "Priority >= 1 AND Priority <= 100");
                    table.CheckConstraint("CK_RefundPolicy_EffectiveTo_Logic", "EffectiveTo IS NULL OR EffectiveTo > EffectiveFrom");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create TourBookingRefunds table
            migrationBuilder.CreateTable(
                name: "TourBookingRefunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TourBookingId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RefundType = table.Column<int>(type: "int", nullable: false),
                    RefundReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ProcessingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RequestedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ProcessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProcessedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AdminNotes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerNotes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionReference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerBankName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerAccountNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerAccountHolder = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DaysBeforeTour = table.Column<int>(type: "int", nullable: true),
                    RefundPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourBookingRefunds", x => x.Id);
                    table.CheckConstraint("CK_TourBookingRefund_OriginalAmount_Positive", "OriginalAmount > 0");
                    table.CheckConstraint("CK_TourBookingRefund_RequestedAmount_NonNegative", "RequestedAmount >= 0");
                    table.CheckConstraint("CK_TourBookingRefund_ApprovedAmount_NonNegative", "ApprovedAmount IS NULL OR ApprovedAmount >= 0");
                    table.CheckConstraint("CK_TourBookingRefund_ProcessingFee_NonNegative", "ProcessingFee >= 0");
                    table.CheckConstraint("CK_TourBookingRefund_RefundPercentage_Valid", "RefundPercentage IS NULL OR (RefundPercentage >= 0 AND RefundPercentage <= 100)");
                    table.CheckConstraint("CK_TourBookingRefund_DaysBeforeTour_NonNegative", "DaysBeforeTour IS NULL OR DaysBeforeTour >= 0");
                    table.CheckConstraint("CK_TourBookingRefund_ProcessedAt_Logic", "(Status = 0 AND ProcessedAt IS NULL) OR (Status != 0 AND ProcessedAt IS NOT NULL)");
                    table.CheckConstraint("CK_TourBookingRefund_CompletedAt_Logic", "(Status != 3 AND CompletedAt IS NULL) OR (Status = 3 AND CompletedAt IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_TourBookingRefunds_TourBookings_TourBookingId",
                        column: x => x.TourBookingId,
                        principalTable: "TourBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourBookingRefunds_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourBookingRefunds_Users_ProcessedById",
                        column: x => x.ProcessedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create indexes for RefundPolicies
            migrationBuilder.CreateIndex(
                name: "IX_RefundPolicy_RefundType",
                table: "RefundPolicies",
                column: "RefundType");

            migrationBuilder.CreateIndex(
                name: "IX_RefundPolicy_IsActive",
                table: "RefundPolicies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RefundPolicy_Priority",
                table: "RefundPolicies",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_RefundPolicy_EffectiveFrom",
                table: "RefundPolicies",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_RefundPolicy_EffectiveTo",
                table: "RefundPolicies",
                column: "EffectiveTo");

            migrationBuilder.CreateIndex(
                name: "IX_RefundPolicy_RefundType_IsActive",
                table: "RefundPolicies",
                columns: new[] { "RefundType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RefundPolicy_RefundType_IsActive_Priority",
                table: "RefundPolicies",
                columns: new[] { "RefundType", "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_RefundPolicy_Active_Effective",
                table: "RefundPolicies",
                columns: new[] { "IsActive", "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_RefundPolicy_RefundType_DaysRange",
                table: "RefundPolicies",
                columns: new[] { "RefundType", "MinDaysBeforeEvent", "MaxDaysBeforeEvent" });

            migrationBuilder.CreateIndex(
                name: "IX_RefundPolicy_Unique_Range",
                table: "RefundPolicies",
                columns: new[] { "RefundType", "MinDaysBeforeEvent", "MaxDaysBeforeEvent", "IsActive" },
                unique: true,
                filter: "IsActive = 1");

            // Create indexes for TourBookingRefunds
            migrationBuilder.CreateIndex(
                name: "IX_TourBookingRefund_TourBookingId",
                table: "TourBookingRefunds",
                column: "TourBookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourBookingRefund_UserId",
                table: "TourBookingRefunds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TourBookingRefund_Status",
                table: "TourBookingRefunds",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TourBookingRefund_RefundType",
                table: "TourBookingRefunds",
                column: "RefundType");

            migrationBuilder.CreateIndex(
                name: "IX_TourBookingRefund_RequestedAt",
                table: "TourBookingRefunds",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TourBookingRefund_ProcessedAt",
                table: "TourBookingRefunds",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TourBookingRefund_ProcessedById",
                table: "TourBookingRefunds",
                column: "ProcessedById");

            migrationBuilder.CreateIndex(
                name: "IX_TourBookingRefund_UserId_Status",
                table: "TourBookingRefunds",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TourBookingRefund_Status_RequestedAt",
                table: "TourBookingRefunds",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TourBookingRefund_RefundType_Status",
                table: "TourBookingRefunds",
                columns: new[] { "RefundType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TourBookingRefund_ProcessedById_ProcessedAt",
                table: "TourBookingRefunds",
                columns: new[] { "ProcessedById", "ProcessedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourBookingRefunds");

            migrationBuilder.DropTable(
                name: "RefundPolicies");
        }
    }
}
