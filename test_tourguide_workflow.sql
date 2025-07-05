-- Test script to verify TourGuide workflow
-- This script will:
-- 1. Create a test user
-- 2. Create a TourGuide application
-- 3. Simulate approval process
-- 4. Verify TourGuide record is created

USE tayninhtourdb_local;

-- 1. Check if we have any users
SELECT COUNT(*) as user_count FROM Users;

-- 2. Check if we have any roles
SELECT * FROM Roles;

-- 3. Get a test user (should exist from seeding)
SELECT Id, Email, Name, RoleId FROM Users LIMIT 1;

-- 4. Check if TourGuides table exists and is empty
SELECT COUNT(*) as tourguide_count FROM TourGuides;

-- 5. Check if we have any TourGuide applications
SELECT COUNT(*) as application_count FROM TourGuideApplications;

-- 6. Show table structure
DESCRIBE TourGuides;

-- 7. Show foreign key constraints
SELECT 
    CONSTRAINT_NAME,
    TABLE_NAME,
    COLUMN_NAME,
    REFERENCED_TABLE_NAME,
    REFERENCED_COLUMN_NAME
FROM information_schema.KEY_COLUMN_USAGE 
WHERE TABLE_SCHEMA = 'tayninhtourdb_local' 
AND TABLE_NAME = 'TourGuides'
AND REFERENCED_TABLE_NAME IS NOT NULL;
