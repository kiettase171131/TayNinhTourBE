using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Url = table.Column<string>(type: "longtext", nullable: false),
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
                    table.PrimaryKey("PK_Images", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
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
                    table.PrimaryKey("PK_Roles", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Email = table.Column<string>(type: "longtext", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: false),
                    Avatar = table.Column<string>(type: "longtext", nullable: false),
                    TOtpSecret = table.Column<string>(type: "longtext", nullable: true),
                    IsVerified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RefreshToken = table.Column<string>(type: "longtext", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: false),
                    AuthorName = table.Column<string>(type: "longtext", nullable: false),
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
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

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
                name: "SupportTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AdminId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Title = table.Column<string>(type: "longtext", nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SupportTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TourGuideApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Email = table.Column<string>(type: "longtext", nullable: false),
                    CurriculumVitae = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "longtext", nullable: true),
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
                    table.PrimaryKey("PK_TourGuideApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourGuideApplications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxGuests = table.Column<int>(type: "int", nullable: false),
                    TourType = table.Column<string>(type: "longtext", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    IsApproved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CommentApproved = table.Column<string>(type: "longtext", nullable: true),
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
                    table.PrimaryKey("PK_Tours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tours_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tours_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
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
                name: "BlogImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    BlogId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Url = table.Column<string>(type: "longtext", nullable: false),
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
                    table.PrimaryKey("PK_BlogImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogImages_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SupportTicketComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    SupportTicketId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false),
                    CommentText = table.Column<string>(type: "longtext", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTicketComments_SupportTickets_SupportTicketId",
                        column: x => x.SupportTicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportTicketComments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SupportTicketImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    SupportTicketId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Url = table.Column<string>(type: "longtext", nullable: false),
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
                    table.PrimaryKey("PK_SupportTicketImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTicketImages_SupportTickets_SupportTicketId",
                        column: x => x.SupportTicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ImageTour",
                columns: table => new
                {
                    ImagesId = table.Column<Guid>(type: "char(36)", nullable: false),
                    TourId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageTour", x => new { x.ImagesId, x.TourId });
                    table.ForeignKey(
                        name: "FK_ImageTour_Images_ImagesId",
                        column: x => x.ImagesId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageTour_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "TourDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    TourTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, comment: "ID của tour template mà chi tiết này thuộc về"),
                    TimeSlot = table.Column<TimeOnly>(type: "time", nullable: false, comment: "Thời gian trong ngày cho hoạt động này"),
                    Location = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "Địa điểm hoặc tên hoạt động"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true, comment: "Mô tả chi tiết về hoạt động"),
                    ShopId = table.Column<Guid>(type: "char(36)", nullable: true, comment: "ID của shop liên quan (nếu có)"),
                    SortOrder = table.Column<int>(type: "int", nullable: false, comment: "Thứ tự sắp xếp trong timeline"),
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
                    table.PrimaryKey("PK_TourDetails", x => x.Id);
                    table.CheckConstraint("CK_TourDetails_SortOrder_Positive", "SortOrder > 0");
                    table.ForeignKey(
                        name: "FK_TourDetails_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TourDetails_TourTemplates_TourTemplateId",
                        column: x => x.TourTemplateId,
                        principalTable: "TourTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourDetails_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourDetails_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TourSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    TourTemplateId = table.Column<Guid>(type: "char(36)", nullable: false, comment: "ID của TourTemplate mà slot này được tạo từ"),
                    TourDate = table.Column<DateOnly>(type: "date", nullable: false, comment: "Ngày tour cụ thể sẽ diễn ra"),
                    ScheduleDay = table.Column<int>(type: "int", nullable: false, comment: "Ngày trong tuần của tour (Saturday hoặc Sunday)"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1, comment: "Trạng thái của tour slot"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true, comment: "Trạng thái slot có sẵn sàng để booking không"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourSlots_TourTemplates_TourTemplateId",
                        column: x => x.TourTemplateId,
                        principalTable: "TourTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourSlots_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourSlots_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TourOperations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    TourSlotId = table.Column<Guid>(type: "char(36)", nullable: false, comment: "ID của TourSlot mà operation này thuộc về"),
                    GuideId = table.Column<Guid>(type: "char(36)", nullable: false, comment: "ID của User làm hướng dẫn viên cho tour này"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, comment: "Giá tour cho operation này"),
                    MaxGuests = table.Column<int>(type: "int", nullable: false, comment: "Số lượng khách tối đa cho tour operation này"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true, comment: "Mô tả bổ sung cho tour operation"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true, comment: "Trạng thái hoạt động của tour operation"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourOperations", x => x.Id);
                    table.CheckConstraint("CK_TourOperations_MaxGuests_Positive", "MaxGuests > 0");
                    table.CheckConstraint("CK_TourOperations_Price_Positive", "Price > 0");
                    table.ForeignKey(
                        name: "FK_TourOperations_TourSlots_TourSlotId",
                        column: x => x.TourSlotId,
                        principalTable: "TourSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourOperations_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourOperations_Users_GuideId",
                        column: x => x.GuideId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TourOperations_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BlogImages_BlogId",
                table: "BlogImages",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_UserId",
                table: "Blogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageTour_TourId",
                table: "ImageTour",
                column: "TourId");

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
                name: "IX_SupportTicketComments_CreatedById",
                table: "SupportTicketComments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketComments_SupportTicketId",
                table: "SupportTicketComments",
                column: "SupportTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketImages_SupportTicketId",
                table: "SupportTicketImages",
                column: "SupportTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_AdminId",
                table: "SupportTickets",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_UserId",
                table: "SupportTickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_CreatedById",
                table: "TourDetails",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_ShopId",
                table: "TourDetails",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TimeSlot",
                table: "TourDetails",
                column: "TimeSlot");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TourTemplateId",
                table: "TourDetails",
                column: "TourTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TourTemplateId_SortOrder",
                table: "TourDetails",
                columns: new[] { "TourTemplateId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TourTemplateId_TimeSlot",
                table: "TourDetails",
                columns: new[] { "TourTemplateId", "TimeSlot" });

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_UpdatedById",
                table: "TourDetails",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TourGuideApplications_UserId",
                table: "TourGuideApplications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_CreatedById",
                table: "TourOperations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_GuideId",
                table: "TourOperations",
                column: "GuideId");

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_GuideId_IsActive",
                table: "TourOperations",
                columns: new[] { "GuideId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_IsActive",
                table: "TourOperations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_TourSlotId_Unique",
                table: "TourOperations",
                column: "TourSlotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourOperations_UpdatedById",
                table: "TourOperations",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_CreatedById",
                table: "Tours",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_UpdatedById",
                table: "Tours",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_CreatedById",
                table: "TourSlots",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_IsActive",
                table: "TourSlots",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_ScheduleDay",
                table: "TourSlots",
                column: "ScheduleDay");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_ScheduleDay_IsActive",
                table: "TourSlots",
                columns: new[] { "ScheduleDay", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_TourDate",
                table: "TourSlots",
                column: "TourDate");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_TourDate_IsActive",
                table: "TourSlots",
                columns: new[] { "TourDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_TourTemplateId",
                table: "TourSlots",
                column: "TourTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_TourTemplateId_TourDate",
                table: "TourSlots",
                columns: new[] { "TourTemplateId", "TourDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourSlots_UpdatedById",
                table: "TourSlots",
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

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogImages");

            migrationBuilder.DropTable(
                name: "ImageTour");

            migrationBuilder.DropTable(
                name: "ImageTourTemplate");

            migrationBuilder.DropTable(
                name: "SupportTicketComments");

            migrationBuilder.DropTable(
                name: "SupportTicketImages");

            migrationBuilder.DropTable(
                name: "TourDetails");

            migrationBuilder.DropTable(
                name: "TourGuideApplications");

            migrationBuilder.DropTable(
                name: "TourOperations");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Tours");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropTable(
                name: "Shops");

            migrationBuilder.DropTable(
                name: "TourSlots");

            migrationBuilder.DropTable(
                name: "TourTemplates");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
