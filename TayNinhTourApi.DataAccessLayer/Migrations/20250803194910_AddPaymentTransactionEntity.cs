using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTransactionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrderId = table.Column<Guid>(type: "char(36)", nullable: true, comment: "ID của Order (cho product payment)", collation: "ascii_general_ci"),
                    TourBookingId = table.Column<Guid>(type: "char(36)", nullable: true, comment: "ID của TourBooking (cho tour booking payment)", collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, comment: "Số tiền giao dịch"),
                    Status = table.Column<int>(type: "int", nullable: false, comment: "Trạng thái giao dịch"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "Mô tả giao dịch")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true, comment: "Thời gian hết hạn giao dịch"),
                    Gateway = table.Column<int>(type: "int", nullable: false, comment: "Cổng thanh toán sử dụng"),
                    PayOsOrderCode = table.Column<long>(type: "bigint", nullable: true, comment: "PayOS Order Code (số)"),
                    PayOsTransactionId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, comment: "PayOS Transaction ID")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CheckoutUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true, comment: "URL checkout PayOS")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QrCode = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true, comment: "QR Code data từ PayOS")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FailureReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "Lý do thất bại (nếu có)")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentTransactionId = table.Column<Guid>(type: "char(36)", nullable: true, comment: "ID của transaction cha (cho retry chain)", collation: "ascii_general_ci"),
                    WebhookPayload = table.Column<string>(type: "longtext", nullable: true, comment: "Webhook payload từ PayOS (JSON)")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.CheckConstraint("CK_PaymentTransactions_Amount_Positive", "Amount > 0");
                    table.CheckConstraint("CK_PaymentTransactions_OrderOrBooking", "(OrderId IS NOT NULL AND TourBookingId IS NULL) OR (OrderId IS NULL AND TourBookingId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_PaymentTransactions_ParentTransactionId",
                        column: x => x.ParentTransactionId,
                        principalTable: "PaymentTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_TourBookings_TourBookingId",
                        column: x => x.TourBookingId,
                        principalTable: "TourBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ExpiredAt",
                table: "PaymentTransactions",
                column: "ExpiredAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Gateway",
                table: "PaymentTransactions",
                column: "Gateway");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Order_Gateway_Status",
                table: "PaymentTransactions",
                columns: new[] { "OrderId", "Gateway", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_OrderId",
                table: "PaymentTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ParentTransactionId",
                table: "PaymentTransactions",
                column: "ParentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PayOsOrderCode",
                table: "PaymentTransactions",
                column: "PayOsOrderCode");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PayOsTransactionId",
                table: "PaymentTransactions",
                column: "PayOsTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Status",
                table: "PaymentTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Status_CreatedAt",
                table: "PaymentTransactions",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_TourBooking_Gateway_Status",
                table: "PaymentTransactions",
                columns: new[] { "TourBookingId", "Gateway", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_TourBookingId",
                table: "PaymentTransactions",
                column: "TourBookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentTransactions");
        }
    }
}
