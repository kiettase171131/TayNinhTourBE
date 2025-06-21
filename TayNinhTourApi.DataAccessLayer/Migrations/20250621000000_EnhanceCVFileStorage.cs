using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceCVFileStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CvOriginalFileName",
                table: "TourGuideApplications",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "CvFileSize",
                table: "TourGuideApplications",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvContentType",
                table: "TourGuideApplications",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CvFilePath",
                table: "TourGuideApplications",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvOriginalFileName",
                table: "TourGuideApplications");

            migrationBuilder.DropColumn(
                name: "CvFileSize",
                table: "TourGuideApplications");

            migrationBuilder.DropColumn(
                name: "CvContentType",
                table: "TourGuideApplications");

            migrationBuilder.DropColumn(
                name: "CvFilePath",
                table: "TourGuideApplications");
        }
    }
}
