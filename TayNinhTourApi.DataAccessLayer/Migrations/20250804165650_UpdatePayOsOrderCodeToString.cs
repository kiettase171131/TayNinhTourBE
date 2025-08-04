using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePayOsOrderCodeToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PayOsOrderCode",
                table: "PaymentTransactions",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                comment: "PayOS Order Code with TNDT prefix",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "PayOS Order Code (số)")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "PayOsOrderCode",
                table: "PaymentTransactions",
                type: "bigint",
                nullable: true,
                comment: "PayOS Order Code (số)",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComment: "PayOS Order Code with TNDT prefix")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
