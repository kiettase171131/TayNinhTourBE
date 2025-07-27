using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentBookingsColumnToTourSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // Check if CurrentBookings column exists in TourSlots table
            migrationBuilder.Sql(@"
                SET @column_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                      AND TABLE_NAME = 'TourSlots' 
                      AND COLUMN_NAME = 'CurrentBookings'
                );

                SET @sql = IF(@column_exists = 0, 
                    'ALTER TABLE TourSlots ADD COLUMN CurrentBookings int NOT NULL DEFAULT 0;',
                    'SELECT ''CurrentBookings column already exists'' as Status;'
                );

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "PhoneNumber",
                keyValue: null,
                column: "PhoneNumber",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // Remove CurrentBookings column if it exists
            migrationBuilder.Sql(@"
                SET @column_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                      AND TABLE_NAME = 'TourSlots' 
                      AND COLUMN_NAME = 'CurrentBookings'
                );

                SET @sql = IF(@column_exists > 0, 
                    'ALTER TABLE TourSlots DROP COLUMN CurrentBookings;',
                    'SELECT ''CurrentBookings column does not exist'' as Status;'
                );

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }
    }
}
