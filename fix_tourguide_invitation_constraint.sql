-- Fix TourGuideInvitation foreign key constraint
-- The constraint should point to Users table, not TourGuides table

USE tayninhtourdb_local;

-- 1. Drop the existing foreign key constraint that points to TourGuides
ALTER TABLE `TourGuideInvitations` 
DROP FOREIGN KEY `FK_TourGuideInvitations_TourGuides_GuideId`;

-- 2. Add the correct foreign key constraint that points to Users
ALTER TABLE `TourGuideInvitations` 
ADD CONSTRAINT `FK_TourGuideInvitations_Users_GuideId` 
FOREIGN KEY (`GuideId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT;

-- 3. Verify the constraint was created correctly
SELECT 
    CONSTRAINT_NAME,
    TABLE_NAME,
    COLUMN_NAME,
    REFERENCED_TABLE_NAME,
    REFERENCED_COLUMN_NAME
FROM information_schema.KEY_COLUMN_USAGE 
WHERE TABLE_SCHEMA = 'tayninhtourdb_local' 
AND TABLE_NAME = 'TourGuideInvitations'
AND COLUMN_NAME = 'GuideId'
AND REFERENCED_TABLE_NAME IS NOT NULL;
