-- Migration: Add Group Booking Support for MySQL
-- Date: 2024-12-13
-- Description: Add columns to support Group Representative booking feature
-- Database: MySQL

-- Add new columns to TourBookings table
ALTER TABLE TourBookings 
ADD COLUMN IF NOT EXISTS BookingType VARCHAR(50) NOT NULL DEFAULT 'Individual',
ADD COLUMN IF NOT EXISTS GroupName VARCHAR(200) NULL,
ADD COLUMN IF NOT EXISTS GroupDescription VARCHAR(500) NULL,
ADD COLUMN IF NOT EXISTS GroupQRCodeData TEXT NULL;

-- Add IsGroupRepresentative flag to TourBookingGuests table
ALTER TABLE TourBookingGuests 
ADD COLUMN IF NOT EXISTS IsGroupRepresentative BOOLEAN NOT NULL DEFAULT FALSE;

-- Update existing data to have default values (for safety)
UPDATE TourBookings 
SET BookingType = 'Individual' 
WHERE BookingType IS NULL OR BookingType = '';

UPDATE TourBookingGuests 
SET IsGroupRepresentative = FALSE 
WHERE IsGroupRepresentative IS NULL;

-- Add indexes for better performance
CREATE INDEX IF NOT EXISTS IX_TourBookings_BookingType 
ON TourBookings(BookingType);

CREATE INDEX IF NOT EXISTS IX_TourBookingGuests_IsGroupRepresentative 
ON TourBookingGuests(IsGroupRepresentative);

-- Verify the migration
SELECT 'Migration completed: Group Booking Support added successfully' as Result;