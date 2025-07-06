-- Simple script to check current database state
USE tayninhtourdb_local;

-- Check if tables exist
SELECT 
    'Table Existence Check' as Info,
    TABLE_NAME,
    TABLE_ROWS
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'tayninhtourdb_local' 
  AND TABLE_NAME IN ('Users', 'Roles', 'TourGuideApplications', 'TourGuides')
ORDER BY TABLE_NAME;

-- Check Users table
SELECT 'Users Count' as Info, COUNT(*) as Count FROM Users;

-- Check Roles table  
SELECT 'Roles Count' as Info, COUNT(*) as Count FROM Roles;

-- Check TourGuideApplications table
SELECT 'TourGuideApplications Count' as Info, COUNT(*) as Count FROM TourGuideApplications;

-- Check TourGuides table
SELECT 'TourGuides Count' as Info, COUNT(*) as Count FROM TourGuides;

-- Show TourGuides table structure
DESCRIBE TourGuides;
