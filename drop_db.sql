-- Drop database completely
DROP DATABASE IF EXISTS `tayninhtourdb_local`;

-- Create fresh database
CREATE DATABASE `tayninhtourdb_local` 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

-- Show result
SELECT 'Database reset successfully!' as Status;
