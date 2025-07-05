-- Data Migration Script: Convert Approved TourGuideApplications to TourGuide Records
-- This script migrates existing approved TourGuide applications to operational TourGuide records
-- Run this script after the TourGuides table has been created

USE tayninhtourdb_local;

-- Step 1: Check current state
SELECT 
    'Current State Check' as Step,
    (SELECT COUNT(*) FROM TourGuideApplications WHERE Status = 1) as ApprovedApplications,
    (SELECT COUNT(*) FROM TourGuides) as ExistingTourGuides,
    (SELECT COUNT(*) FROM Users WHERE RoleId = (SELECT Id FROM Roles WHERE Name = 'TourGuide')) as UsersWithTourGuideRole;

-- Step 2: Show approved applications that don't have TourGuide records yet
SELECT 
    'Applications Missing TourGuide Records' as Info,
    tga.Id as ApplicationId,
    tga.UserId,
    tga.FullName,
    tga.Email,
    tga.ProcessedAt,
    tga.ProcessedById
FROM TourGuideApplications tga
LEFT JOIN TourGuides tg ON tga.Id = tg.ApplicationId
WHERE tga.Status = 1 -- Approved
  AND tg.Id IS NULL; -- No corresponding TourGuide record

-- Step 3: Create TourGuide records for approved applications (if any exist)
INSERT INTO TourGuides (
    Id,
    UserId,
    ApplicationId,
    FullName,
    PhoneNumber,
    Email,
    Experience,
    Skills,
    IsAvailable,
    Rating,
    TotalToursGuided,
    ApprovedAt,
    ApprovedById,
    CreatedAt,
    CreatedById
)
SELECT 
    UUID() as Id,
    tga.UserId,
    tga.Id as ApplicationId,
    tga.FullName,
    tga.PhoneNumber,
    tga.Email,
    tga.Experience,
    tga.Skills,
    1 as IsAvailable, -- Default to available
    0 as Rating, -- Default rating
    0 as TotalToursGuided, -- Default tours guided
    COALESCE(tga.ProcessedAt, tga.UpdatedAt, tga.CreatedAt) as ApprovedAt,
    tga.ProcessedById as ApprovedById,
    NOW() as CreatedAt,
    tga.ProcessedById as CreatedById
FROM TourGuideApplications tga
LEFT JOIN TourGuides tg ON tga.Id = tg.ApplicationId
WHERE tga.Status = 1 -- Approved
  AND tg.Id IS NULL -- No corresponding TourGuide record exists
  AND tga.IsActive = 1
  AND tga.IsDeleted = 0;

-- Step 4: Verify migration results
SELECT 
    'Migration Results' as Step,
    (SELECT COUNT(*) FROM TourGuideApplications WHERE Status = 1) as ApprovedApplications,
    (SELECT COUNT(*) FROM TourGuides) as TotalTourGuides,
    (SELECT COUNT(*) FROM TourGuides WHERE CreatedAt >= DATE_SUB(NOW(), INTERVAL 1 MINUTE)) as NewlyCreatedTourGuides;

-- Step 5: Show all TourGuide records with their application info
SELECT 
    'Final TourGuide Records' as Info,
    tg.Id as TourGuideId,
    tg.UserId,
    tg.ApplicationId,
    tg.FullName,
    tg.Email,
    tg.IsAvailable,
    tg.Rating,
    tg.TotalToursGuided,
    tg.ApprovedAt,
    tga.SubmittedAt as ApplicationSubmittedAt,
    u.Name as UserName,
    u.Email as UserEmail
FROM TourGuides tg
JOIN TourGuideApplications tga ON tg.ApplicationId = tga.Id
JOIN Users u ON tg.UserId = u.Id
ORDER BY tg.CreatedAt DESC;

-- Step 6: Verify data integrity
SELECT 
    'Data Integrity Check' as Info,
    COUNT(*) as TotalChecks,
    SUM(CASE WHEN tg.UserId = tga.UserId THEN 1 ELSE 0 END) as MatchingUserIds,
    SUM(CASE WHEN tg.FullName = tga.FullName THEN 1 ELSE 0 END) as MatchingNames,
    SUM(CASE WHEN tg.Email = tga.Email THEN 1 ELSE 0 END) as MatchingEmails
FROM TourGuides tg
JOIN TourGuideApplications tga ON tg.ApplicationId = tga.Id;

-- Step 7: Show any potential issues
SELECT 
    'Potential Issues' as Info,
    'Duplicate TourGuide records for same user' as IssueType,
    COUNT(*) as Count
FROM (
    SELECT UserId, COUNT(*) as RecordCount
    FROM TourGuides
    GROUP BY UserId
    HAVING COUNT(*) > 1
) duplicates

UNION ALL

SELECT 
    'Potential Issues' as Info,
    'TourGuide without corresponding application' as IssueType,
    COUNT(*) as Count
FROM TourGuides tg
LEFT JOIN TourGuideApplications tga ON tg.ApplicationId = tga.Id
WHERE tga.Id IS NULL

UNION ALL

SELECT 
    'Potential Issues' as Info,
    'Approved applications without TourGuide record' as IssueType,
    COUNT(*) as Count
FROM TourGuideApplications tga
LEFT JOIN TourGuides tg ON tga.Id = tg.ApplicationId
WHERE tga.Status = 1 AND tg.Id IS NULL;
