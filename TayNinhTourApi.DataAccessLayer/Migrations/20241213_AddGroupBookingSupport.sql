-- Migration: Add Group Booking Support
-- Date: 2024-12-13
-- Description: Add columns to support Group Representative booking feature
-- Database: MySQL

-- Check if columns exist and add them if they don't
-- Note: MySQL doesn't support IF NOT EXISTS for columns directly in ALTER TABLE

-- Add new columns to TourBookings table
-- Using stored procedure to check if column exists
DELIMITER $$

DROP PROCEDURE IF EXISTS AddColumnIfNotExists$$
CREATE PROCEDURE AddColumnIfNotExists(
    IN tableName VARCHAR(255),
    IN columnName VARCHAR(255),
    IN columnDefinition VARCHAR(500)
)
BEGIN
    IF NOT EXISTS (
        SELECT * FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = tableName
        AND COLUMN_NAME = columnName
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', tableName, '` ADD COLUMN `', columnName, '` ', columnDefinition);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

DELIMITER ;

-- Add columns to TourBookings
CALL AddColumnIfNotExists('TourBookings', 'BookingType', 'VARCHAR(50) DEFAULT ''Individual''');
CALL AddColumnIfNotExists('TourBookings', 'GroupName', 'VARCHAR(200) NULL');
CALL AddColumnIfNotExists('TourBookings', 'GroupDescription', 'VARCHAR(500) NULL');
CALL AddColumnIfNotExists('TourBookings', 'GroupQRCodeData', 'TEXT NULL');

-- Add column to TourBookingGuests
CALL AddColumnIfNotExists('TourBookingGuests', 'IsGroupRepresentative', 'BOOLEAN DEFAULT FALSE');

-- Drop the temporary procedure
DROP PROCEDURE IF EXISTS AddColumnIfNotExists;

-- Update existing data to have default values
UPDATE TourBookings 
SET BookingType = 'Individual' 
WHERE BookingType IS NULL OR BookingType = '';

UPDATE TourBookingGuests 
SET IsGroupRepresentative = FALSE 
WHERE IsGroupRepresentative IS NULL;

-- Add indexes if they don't exist
-- MySQL doesn't have IF NOT EXISTS for indexes in older versions, so we use a different approach
DELIMITER $$

DROP PROCEDURE IF EXISTS AddIndexIfNotExists$$
CREATE PROCEDURE AddIndexIfNotExists(
    IN tableName VARCHAR(255),
    IN indexName VARCHAR(255),
    IN columnName VARCHAR(255)
)
BEGIN
    IF NOT EXISTS (
        SELECT * FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = tableName
        AND INDEX_NAME = indexName
    ) THEN
        SET @sql = CONCAT('CREATE INDEX `', indexName, '` ON `', tableName, '`(`', columnName, '`)');
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

DELIMITER ;

-- Add indexes
CALL AddIndexIfNotExists('TourBookings', 'IX_TourBookings_BookingType', 'BookingType');
CALL AddIndexIfNotExists('TourBookingGuests', 'IX_TourBookingGuests_IsGroupRepresentative', 'IsGroupRepresentative');

-- Drop the temporary procedure
DROP PROCEDURE IF EXISTS AddIndexIfNotExists;

-- Verify the changes
SELECT 
    'TourBookings' AS TableName,
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = DATABASE()
AND TABLE_NAME = 'TourBookings' 
AND COLUMN_NAME IN ('BookingType', 'GroupName', 'GroupDescription', 'GroupQRCodeData');

SELECT 
    'TourBookingGuests' AS TableName,
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = DATABASE()
AND TABLE_NAME = 'TourBookingGuests' 
AND COLUMN_NAME = 'IsGroupRepresentative';

-- Success message
SELECT 'Migration completed: Group Booking Support added successfully' AS Result;