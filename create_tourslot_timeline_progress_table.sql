-- Create TourSlotTimelineProgress table for independent timeline progress tracking per tour slot
CREATE TABLE IF NOT EXISTS `TourSlotTimelineProgress` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `TourSlotId` char(36) COLLATE ascii_general_ci NOT NULL COMMENT 'Reference to the specific tour slot',
    `TimelineItemId` char(36) COLLATE ascii_general_ci NOT NULL COMMENT 'Reference to the timeline item template',
    `IsCompleted` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Whether this timeline item has been completed for this tour slot',
    `CompletedAt` datetime NULL COMMENT 'Timestamp when the timeline item was completed',
    `CompletionNotes` varchar(500) CHARACTER SET utf8mb4 NULL COMMENT 'Optional notes added when completing the timeline item',
    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Whether the progress record is active',
    `CreatedById` char(36) COLLATE ascii_general_ci NOT NULL COMMENT 'User who created the progress record',
    `UpdatedById` char(36) COLLATE ascii_general_ci NULL COMMENT 'User who last updated the progress record',
    `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'When the progress record was created',
    `UpdatedAt` datetime NULL COMMENT 'When the progress record was last updated',
    `DeletedAt` datetime(6) NULL COMMENT 'Soft delete timestamp',

    PRIMARY KEY (`Id`),

    -- Unique constraint to ensure one progress record per tour slot per timeline item
    UNIQUE KEY `UK_TourSlotTimeline` (`TourSlotId`, `TimelineItemId`),

    -- Indexes for performance
    INDEX `IX_TourSlotTimelineProgress_TourSlot` (`TourSlotId`),
    INDEX `IX_TourSlotTimelineProgress_TimelineItem` (`TimelineItemId`),
    INDEX `IX_TourSlotTimelineProgress_Completed` (`TourSlotId`, `IsCompleted`, `CompletedAt`),
    INDEX `IX_TourSlotTimelineProgress_Active` (`IsActive`, `IsDeleted`),
    INDEX `IX_TourSlotTimelineProgress_CreatedBy` (`CreatedById`),
    INDEX `IX_TourSlotTimelineProgress_UpdatedBy` (`UpdatedById`)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Tracks timeline completion progress for individual tour slots';
