using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTourDetailsImageUrlsToJsonArray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add new ImageUrls column
            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "TourDetails",
                type: "JSON",
                nullable: true,
                comment: "Danh sách URL hình ảnh cho tour details này (JSON array)")
                .Annotation("MySql:CharSet", "utf8mb4");

            // Step 2: Migrate existing ImageUrl data to ImageUrls as JSON array
            migrationBuilder.Sql(@"
                UPDATE TourDetails
                SET ImageUrls = CASE
                    WHEN ImageUrl IS NOT NULL AND ImageUrl != ''
                    THEN JSON_ARRAY(ImageUrl)
                    ELSE JSON_ARRAY()
                END
                WHERE ImageUrl IS NOT NULL;
            ");

            // Step 3: Set empty JSON array for null ImageUrl records
            migrationBuilder.Sql(@"
                UPDATE TourDetails
                SET ImageUrls = JSON_ARRAY()
                WHERE ImageUrl IS NULL;
            ");

            // Step 4: Drop old ImageUrl column
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "TourDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add back ImageUrl column
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "TourDetails",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "URL hình ảnh đại diện cho tour details này")
                .Annotation("MySql:CharSet", "utf8mb4");

            // Step 2: Migrate first image from ImageUrls JSON array back to ImageUrl
            migrationBuilder.Sql(@"
                UPDATE TourDetails
                SET ImageUrl = CASE
                    WHEN JSON_LENGTH(ImageUrls) > 0
                    THEN JSON_UNQUOTE(JSON_EXTRACT(ImageUrls, '$[0]'))
                    ELSE NULL
                END
                WHERE ImageUrls IS NOT NULL;
            ");

            // Step 3: Drop ImageUrls column
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "TourDetails");
        }
    }
}
