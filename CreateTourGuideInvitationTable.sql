-- Create TourGuideInvitations table for TayNinhTourBE
-- Execute this SQL script in MySQL database: myapp_db

CREATE TABLE IF NOT EXISTS `TourGuideInvitations` (
    `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
    `TourDetailsId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
    `GuideId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
    `InvitationType` int NOT NULL DEFAULT 1,
    `Status` int NOT NULL DEFAULT 1,
    `InvitedAt` datetime(6) NOT NULL DEFAULT UTC_TIMESTAMP(6),
    `RespondedAt` datetime(6) NULL,
    `ExpiresAt` datetime(6) NOT NULL,
    `RejectionReason` varchar(500) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL DEFAULT UTC_TIMESTAMP(6),
    `UpdatedAt` datetime(6) NOT NULL DEFAULT UTC_TIMESTAMP(6) ON UPDATE UTC_TIMESTAMP(6),
    `DeletedAt` datetime(6) NULL,
    `CreatedById` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
    `UpdatedById` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
    CONSTRAINT `PK_TourGuideInvitations` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_TourGuideInvitations_TourDetails_TourDetailsId` FOREIGN KEY (`TourDetailsId`) REFERENCES `TourDetails` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_TourGuideInvitations_Users_GuideId` FOREIGN KEY (`GuideId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TourGuideInvitations_Users_CreatedById` FOREIGN KEY (`CreatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TourGuideInvitations_Users_UpdatedById` FOREIGN KEY (`UpdatedById`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

-- Add indexes for performance
CREATE INDEX IF NOT EXISTS `IX_TourGuideInvitations_TourDetailsId` ON `TourGuideInvitations` (`TourDetailsId`);
CREATE INDEX IF NOT EXISTS `IX_TourGuideInvitations_GuideId` ON `TourGuideInvitations` (`GuideId`);
CREATE INDEX IF NOT EXISTS `IX_TourGuideInvitations_Status` ON `TourGuideInvitations` (`Status`);
CREATE INDEX IF NOT EXISTS `IX_TourGuideInvitations_ExpiresAt` ON `TourGuideInvitations` (`ExpiresAt`);
CREATE INDEX IF NOT EXISTS `IX_TourGuideInvitations_CreatedById` ON `TourGuideInvitations` (`CreatedById`);
CREATE INDEX IF NOT EXISTS `IX_TourGuideInvitations_UpdatedById` ON `TourGuideInvitations` (`UpdatedById`);

-- Verify table creation
SELECT 'TourGuideInvitations table created successfully' as Result;
