-- Migration Script: Convert TourCompany.Id references to User.Id
-- This script updates existing data to use User.Id instead of TourCompany.Id
-- for CreatedById and UpdatedById fields in TourDetails, TourTemplate, and TourOperation
--
-- IMPORTANT: Run this script BEFORE applying the EF Core migration!

-- =====================================================
-- STEP 1: Update TourDetails table
-- =====================================================

-- Update CreatedById: Convert TourCompany.Id to User.Id
UPDATE TourDetails
SET CreatedById = (
    SELECT tc.UserId
    FROM TourCompanies tc
    WHERE tc.Id = TourDetails.CreatedById
)
WHERE EXISTS (
    SELECT 1 FROM TourCompanies tc
    WHERE tc.Id = TourDetails.CreatedById
);

-- Update UpdatedById: Convert TourCompany.Id to User.Id (where not null)
UPDATE TourDetails
SET UpdatedById = (
    SELECT tc.UserId
    FROM TourCompanies tc
    WHERE tc.Id = TourDetails.UpdatedById
)
WHERE UpdatedById IS NOT NULL
AND EXISTS (
    SELECT 1 FROM TourCompanies tc
    WHERE tc.Id = TourDetails.UpdatedById
);

-- =====================================================
-- STEP 2: Update TourTemplate table
-- =====================================================

-- Update CreatedById: Convert TourCompany.Id to User.Id
UPDATE TourTemplate
SET CreatedById = (
    SELECT tc.UserId
    FROM TourCompanies tc
    WHERE tc.Id = TourTemplate.CreatedById
)
WHERE EXISTS (
    SELECT 1 FROM TourCompanies tc
    WHERE tc.Id = TourTemplate.CreatedById
);

-- Update UpdatedById: Convert TourCompany.Id to User.Id (where not null)
UPDATE TourTemplate
SET UpdatedById = (
    SELECT tc.UserId
    FROM TourCompanies tc
    WHERE tc.Id = TourTemplate.UpdatedById
)
WHERE UpdatedById IS NOT NULL
AND EXISTS (
    SELECT 1 FROM TourCompanies tc
    WHERE tc.Id = TourTemplate.UpdatedById
);

-- =====================================================
-- STEP 3: Update TourOperation table
-- =====================================================

-- Update CreatedById: Convert TourCompany.Id to User.Id
UPDATE TourOperation
SET CreatedById = (
    SELECT tc.UserId
    FROM TourCompanies tc
    WHERE tc.Id = TourOperation.CreatedById
)
WHERE EXISTS (
    SELECT 1 FROM TourCompanies tc
    WHERE tc.Id = TourOperation.CreatedById
);

-- Update UpdatedById: Convert TourCompany.Id to User.Id (where not null)
UPDATE TourOperation
SET UpdatedById = (
    SELECT tc.UserId
    FROM TourCompanies tc
    WHERE tc.Id = TourOperation.UpdatedById
)
WHERE UpdatedById IS NOT NULL
AND EXISTS (
    SELECT 1 FROM TourCompanies tc
    WHERE tc.Id = TourOperation.UpdatedById
);

-- =====================================================
-- STEP 4: Foreign Key Constraints
-- =====================================================
-- Note: Foreign key constraints will be updated by EF Core migration
-- This script only handles data conversion

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Verify TourDetails conversion
SELECT 
    'TourDetails' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN CreatedById IN (SELECT Id FROM Users) THEN 1 END) as ValidCreatedById,
    COUNT(CASE WHEN UpdatedById IS NULL OR UpdatedById IN (SELECT Id FROM Users) THEN 1 END) as ValidUpdatedById
FROM TourDetails;

-- Verify TourTemplate conversion  
SELECT 
    'TourTemplate' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN CreatedById IN (SELECT Id FROM Users) THEN 1 END) as ValidCreatedById,
    COUNT(CASE WHEN UpdatedById IS NULL OR UpdatedById IN (SELECT Id FROM Users) THEN 1 END) as ValidUpdatedById
FROM TourTemplate;

-- Verify TourOperation conversion
SELECT 
    'TourOperation' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN CreatedById IN (SELECT Id FROM Users) THEN 1 END) as ValidCreatedById,
    COUNT(CASE WHEN UpdatedById IS NULL OR UpdatedById IN (SELECT Id FROM Users) THEN 1 END) as ValidUpdatedById
FROM TourOperation;
