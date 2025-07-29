using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationMessageToTourGuideInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvitationMessage",
                table: "TourGuideInvitations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "Tin nhắn từ TourCompany khi gửi lời mời")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvitationMessage",
                table: "TourGuideInvitations");
        }
    }
}
