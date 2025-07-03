-- =====================================================
-- Script: Migrate Languages to Skills in TourGuideApplication
-- Purpose: Convert existing Languages data to new Skills format
-- Author: TayNinhTour Development Team
-- Date: 2025-06-26
-- =====================================================

-- Step 1: Backup existing data (optional, for safety)
-- CREATE TABLE TourGuideApplications_Backup AS SELECT * FROM TourGuideApplications;

-- Step 2: Update Skills field based on Languages field
-- This script maps common language values to TourGuideSkill enum values

UPDATE TourGuideApplications 
SET Skills = CASE 
    -- Vietnamese mappings
    WHEN Languages REGEXP '(vietnamese|vn|vi|tiếng việt|tieng viet|việt nam|viet nam)' 
        AND Languages REGEXP '(english|en|tiếng anh|tieng anh)' 
        AND Languages REGEXP '(chinese|cn|zh|tiếng trung|tieng trung|mandarin)' 
        THEN 'Vietnamese,English,Chinese'
    
    WHEN Languages REGEXP '(vietnamese|vn|vi|tiếng việt|tieng viet|việt nam|viet nam)' 
        AND Languages REGEXP '(english|en|tiếng anh|tieng anh)' 
        THEN 'Vietnamese,English'
    
    WHEN Languages REGEXP '(vietnamese|vn|vi|tiếng việt|tieng viet|việt nam|viet nam)' 
        AND Languages REGEXP '(chinese|cn|zh|tiếng trung|tieng trung|mandarin)' 
        THEN 'Vietnamese,Chinese'
    
    WHEN Languages REGEXP '(english|en|tiếng anh|tieng anh)' 
        AND Languages REGEXP '(chinese|cn|zh|tiếng trung|tieng trung|mandarin)' 
        THEN 'English,Chinese'
    
    -- Single language mappings
    WHEN Languages REGEXP '(vietnamese|vn|vi|tiếng việt|tieng viet|việt nam|viet nam)' 
        THEN 'Vietnamese'
    
    WHEN Languages REGEXP '(english|en|tiếng anh|tieng anh)' 
        THEN 'English'
    
    WHEN Languages REGEXP '(chinese|cn|zh|tiếng trung|tieng trung|mandarin)' 
        THEN 'Chinese'
    
    WHEN Languages REGEXP '(japanese|jp|ja|tiếng nhật|tieng nhat)' 
        THEN 'Japanese'
    
    WHEN Languages REGEXP '(korean|kr|ko|tiếng hàn|tieng han)' 
        THEN 'Korean'
    
    WHEN Languages REGEXP '(french|fr|tiếng pháp|tieng phap)' 
        THEN 'French'
    
    WHEN Languages REGEXP '(german|de|tiếng đức|tieng duc)' 
        THEN 'German'
    
    WHEN Languages REGEXP '(russian|ru|tiếng nga|tieng nga)' 
        THEN 'Russian'
    
    -- Default case: if Languages is not null but doesn't match patterns, 
    -- try to preserve as much as possible
    WHEN Languages IS NOT NULL AND TRIM(Languages) != '' 
        THEN CONCAT('Vietnamese,', 
            CASE 
                WHEN Languages LIKE '%english%' OR Languages LIKE '%anh%' THEN 'English'
                ELSE 'English'  -- Default to English as second language
            END)
    
    -- If Languages is null or empty, default to Vietnamese
    ELSE 'Vietnamese'
END
WHERE Skills IS NULL OR Skills = '';

-- Step 3: Verification queries (run these to check the migration)
-- SELECT Languages, Skills, COUNT(*) as Count 
-- FROM TourGuideApplications 
-- GROUP BY Languages, Skills 
-- ORDER BY Count DESC;

-- Step 4: Check for any unmapped cases
-- SELECT Languages, Skills 
-- FROM TourGuideApplications 
-- WHERE Languages IS NOT NULL 
--   AND Languages != '' 
--   AND (Skills IS NULL OR Skills = '');

-- Step 5: Update any remaining NULL Skills to default
UPDATE TourGuideApplications 
SET Skills = 'Vietnamese' 
WHERE Skills IS NULL OR Skills = '';

-- =====================================================
-- Migration Summary:
-- - Converted Languages field to Skills field
-- - Mapped common language codes to TourGuideSkill enum values
-- - Preserved existing language combinations where possible
-- - Set default to 'Vietnamese' for empty/null cases
-- =====================================================
