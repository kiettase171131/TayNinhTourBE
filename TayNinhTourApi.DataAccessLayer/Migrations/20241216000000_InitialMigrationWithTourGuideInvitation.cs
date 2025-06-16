using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TayNinhTourApi.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrationWithTourGuideInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create all tables including TourGuideInvitations
            migrationBuilder.Sql(@"
                -- Create Roles table
                CREATE TABLE IF NOT EXISTS `Roles` (
                    `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    `Name` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
                    `NormalizedName` varchar(256) CHARACTER SET utf8mb4 NULL,
                    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
                    CONSTRAINT `PK_Roles` PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;

                -- Create Users table
                CREATE TABLE IF NOT EXISTS `Users` (
                    `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    `UserName` varchar(256) CHARACTER SET utf8mb4 NULL,
                    `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 NULL,
                    `Email` varchar(256) CHARACTER SET utf8mb4 NULL,
                    `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
                    `EmailConfirmed` tinyint(1) NOT NULL,
                    `PasswordHash` longtext CHARACTER SET utf8mb4 NULL,
                    `SecurityStamp` longtext CHARACTER SET utf8mb4 NULL,
                    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
                    `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
                    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
                    `TwoFactorEnabled` tinyint(1) NOT NULL,
                    `LockoutEnd` datetime(6) NULL,
                    `LockoutEnabled` tinyint(1) NOT NULL,
                    `AccessFailedCount` int NOT NULL,
                    `FullName` longtext CHARACTER SET utf8mb4 NULL,
                    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
                    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                    `UpdatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
                    `RefreshToken` longtext CHARACTER SET utf8mb4 NULL,
                    `RefreshTokenExpiryTime` datetime(6) NOT NULL,
                    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;

                -- Create UserRoles table
                CREATE TABLE IF NOT EXISTS `UserRoles` (
                    `UserId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    `RoleId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    CONSTRAINT `PK_UserRoles` PRIMARY KEY (`UserId`, `RoleId`),
                    CONSTRAINT `FK_UserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE,
                    CONSTRAINT `FK_UserRoles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;

                -- Create TourGuideInvitations table
                CREATE TABLE IF NOT EXISTS `TourGuideInvitations` (
                    `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    `TourDetailsId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    `GuideId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    `InvitationType` int NOT NULL DEFAULT 1,
                    `Status` int NOT NULL DEFAULT 1,
                    `InvitedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                    `RespondedAt` datetime(6) NULL,
                    `ExpiresAt` datetime(6) NOT NULL,
                    `RejectionReason` varchar(500) CHARACTER SET utf8mb4 NULL,
                    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                    `UpdatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
                    `DeletedAt` datetime(6) NULL,
                    `CreatedById` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    `UpdatedById` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
                    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
                    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
                    CONSTRAINT `PK_TourGuideInvitations` PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_TourGuideInvitations_Users_GuideId` FOREIGN KEY (`GuideId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
                    CONSTRAINT `FK_TourGuideInvitations_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
                    CONSTRAINT `FK_TourGuideInvitations_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
                ) CHARACTER SET=utf8mb4;

                -- Add indexes for performance
                CREATE INDEX `IX_TourGuideInvitations_GuideId` ON `TourGuideInvitations` (`GuideId`);
                CREATE INDEX `IX_TourGuideInvitations_Status` ON `TourGuideInvitations` (`Status`);
                CREATE INDEX `IX_TourGuideInvitations_ExpiresAt` ON `TourGuideInvitations` (`ExpiresAt`);
                CREATE INDEX `IX_TourGuideInvitations_CreatedById` ON `TourGuideInvitations` (`CreatedById`);
                CREATE INDEX `IX_TourGuideInvitations_UpdatedById` ON `TourGuideInvitations` (`UpdatedById`);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS `TourGuideInvitations`;
                DROP TABLE IF EXISTS `UserRoles`;
                DROP TABLE IF EXISTS `Users`;
                DROP TABLE IF EXISTS `Roles`;
            ");
        }
    }
}
