using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class MigrateTourTemplateTypeDataOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DATA MIGRATION: Update TourTemplateType values to new enum
            // Old enum values: Standard=1, Premium=2, Custom=3, Group=4, Private=5, Adventure=6, Cultural=7, Culinary=8, Eco=9, Historical=10
            // New enum values: FreeScenic=1, PaidAttraction=2

            // Map old values to new values based on business logic
            // FreeScenic (1): Standard, Cultural, Historical, Eco (scenic/cultural tours)
            // PaidAttraction (2): Premium, Custom, Group, Private, Adventure, Culinary (paid/entertainment tours)

            migrationBuilder.Sql(@"
                UPDATE TourTemplates 
                SET TemplateType = 1 
                WHERE TemplateType IN (1, 7, 9, 10);  -- Standard, Cultural, Eco, Historical -> FreeScenic
            ");

            migrationBuilder.Sql(@"
                UPDATE TourTemplates 
                SET TemplateType = 2 
                WHERE TemplateType IN (2, 3, 4, 5, 6, 8);  -- Premium, Custom, Group, Private, Adventure, Culinary -> PaidAttraction
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback migration - restore original values (best effort)
            // Note: This is not perfect since we lose information about original specific types
            // All FreeScenic will become Standard, all PaidAttraction will become Premium

            migrationBuilder.Sql(@"
                UPDATE TourTemplates 
                SET TemplateType = 1 
                WHERE TemplateType = 1;  -- FreeScenic -> Standard
            ");

            migrationBuilder.Sql(@"
                UPDATE TourTemplates 
                SET TemplateType = 2 
                WHERE TemplateType = 2;  -- PaidAttraction -> Premium
            ");
        }
    }
}
