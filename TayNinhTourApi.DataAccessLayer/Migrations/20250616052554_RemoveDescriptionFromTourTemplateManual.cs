using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDescriptionFromTourTemplateManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if Description column exists before dropping it
            migrationBuilder.Sql(@"
                SET @column_exists = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME = 'TourTemplates'
                    AND COLUMN_NAME = 'Description'
                );

                SET @sql = IF(@column_exists > 0,
                    'ALTER TABLE TourTemplates DROP COLUMN Description;',
                    'SELECT ''Column Description does not exist'' as message;'
                );

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add Description column back if it doesn't exist
            migrationBuilder.Sql(@"
                SET @column_exists = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME = 'TourTemplates'
                    AND COLUMN_NAME = 'Description'
                );

                SET @sql = IF(@column_exists = 0,
                    'ALTER TABLE TourTemplates ADD COLUMN Description varchar(1000) NULL;',
                    'SELECT ''Column Description already exists'' as message;'
                );

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }
    }
}
