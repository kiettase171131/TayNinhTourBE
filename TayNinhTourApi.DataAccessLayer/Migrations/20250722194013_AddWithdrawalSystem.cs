using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddWithdrawalSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BankName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountHolderName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VerifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VerifiedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.CheckConstraint("CK_BankAccount_AccountNumber_Numeric", "AccountNumber REGEXP '^[0-9]+$'");
                    table.ForeignKey(
                        name: "FK_BankAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Users_VerifiedById",
                        column: x => x.VerifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WithdrawalRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BankAccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RequestedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP()"),
                    ProcessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProcessedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AdminNotes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserNotes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionReference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WalletBalanceAtRequest = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WithdrawalFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
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
                    table.PrimaryKey("PK_WithdrawalRequests", x => x.Id);
                    table.CheckConstraint("CK_WithdrawalRequest_Amount_Positive", "Amount > 0");
                    table.CheckConstraint("CK_WithdrawalRequest_ProcessedAt_Logic", "(Status = 0 AND ProcessedAt IS NULL) OR (Status != 0 AND ProcessedAt IS NOT NULL)");
                    table.CheckConstraint("CK_WithdrawalRequest_WalletBalance_NonNegative", "WalletBalanceAtRequest >= 0");
                    table.CheckConstraint("CK_WithdrawalRequest_WithdrawalFee_NonNegative", "WithdrawalFee >= 0");
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_Users_ProcessedById",
                        column: x => x.ProcessedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_BankName",
                table: "BankAccounts",
                column: "BankName");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_IsDefault",
                table: "BankAccounts",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_UserId",
                table: "BankAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_UserId_BankName_AccountNumber_Unique",
                table: "BankAccounts",
                columns: new[] { "UserId", "BankName", "AccountNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_UserId_IsDefault",
                table: "BankAccounts",
                columns: new[] { "UserId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_VerifiedById",
                table: "BankAccounts",
                column: "VerifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequest_BankAccountId",
                table: "WithdrawalRequests",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequest_ProcessedAt",
                table: "WithdrawalRequests",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequest_ProcessedById",
                table: "WithdrawalRequests",
                column: "ProcessedById");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequest_ProcessedById_ProcessedAt",
                table: "WithdrawalRequests",
                columns: new[] { "ProcessedById", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequest_RequestedAt",
                table: "WithdrawalRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequest_Status",
                table: "WithdrawalRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequest_Status_RequestedAt",
                table: "WithdrawalRequests",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequest_UserId",
                table: "WithdrawalRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequest_UserId_Status",
                table: "WithdrawalRequests",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WithdrawalRequests");

            migrationBuilder.DropTable(
                name: "BankAccounts");
        }
    }
}
