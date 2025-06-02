using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddTourTemplateAndShopEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    OpeningHours = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ShopType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shops_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shops_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TourTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxGuests = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TemplateType = table.Column<int>(type: "int", nullable: false),
                    ScheduleDays = table.Column<int>(type: "int", nullable: false),
                    StartLocation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    EndLocation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    SpecialRequirements = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    MinGuests = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ChildPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChildMaxAge = table.Column<int>(type: "int", nullable: true),
                    Transportation = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    MealsIncluded = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    AccommodationInfo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    IncludedServices = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ExcludedServices = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CancellationPolicy = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourTemplates_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourTemplates_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ImageTourTemplate",
                columns: table => new
                {
                    ImagesId = table.Column<Guid>(type: "char(36)", nullable: false),
                    TourTemplateId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageTourTemplate", x => new { x.ImagesId, x.TourTemplateId });
                    table.ForeignKey(
                        name: "FK_ImageTourTemplate_Images_ImagesId",
                        column: x => x.ImagesId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageTourTemplate_TourTemplates_TourTemplateId",
                        column: x => x.TourTemplateId,
                        principalTable: "TourTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ImageTourTemplate_TourTemplateId",
                table: "ImageTourTemplate",
                column: "TourTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Shop_CreatedById",
                table: "Shops",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Shop_IsActive",
                table: "Shops",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Shop_Location",
                table: "Shops",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_Shop_Name",
                table: "Shops",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Shop_Rating_IsActive",
                table: "Shops",
                columns: new[] { "Rating", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Shop_ShopType",
                table: "Shops",
                column: "ShopType");

            migrationBuilder.CreateIndex(
                name: "IX_Shop_ShopType_IsActive",
                table: "Shops",
                columns: new[] { "ShopType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Shops_UpdatedById",
                table: "Shops",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TourTemplate_CreatedById",
                table: "TourTemplates",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TourTemplate_EndLocation",
                table: "TourTemplates",
                column: "EndLocation");

            migrationBuilder.CreateIndex(
                name: "IX_TourTemplate_IsActive",
                table: "TourTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TourTemplate_Price_IsActive",
                table: "TourTemplates",
                columns: new[] { "Price", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TourTemplate_StartLocation",
                table: "TourTemplates",
                column: "StartLocation");

            migrationBuilder.CreateIndex(
                name: "IX_TourTemplate_TemplateType",
                table: "TourTemplates",
                column: "TemplateType");

            migrationBuilder.CreateIndex(
                name: "IX_TourTemplate_TemplateType_IsActive",
                table: "TourTemplates",
                columns: new[] { "TemplateType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TourTemplates_UpdatedById",
                table: "TourTemplates",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageTourTemplate");

            migrationBuilder.DropTable(
                name: "Shops");

            migrationBuilder.DropTable(
                name: "TourTemplates");
        }
    }
}
