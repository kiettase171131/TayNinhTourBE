-- ✅ Debug script: Kiểm tra tại sao mất 1 TourSlot
-- TourDetailsId: 49989d61-0906-4d6b-8ebe-156444824a66

-- 1. Kiểm tra TẤT CẢ slots (kể cả deleted/inactive)
SELECT 
    Id,                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
    TourDate,
    Status, 
    MaxGuests,
    CurrentBookings,
    (MaxGuests - CurrentBookings) as AvailableSpots,
    IsActive,
    IsDeleted,
    CreatedAt,
    UpdatedAt,
    DeletedAt
FROM TourSlots 
WHERE TourDetailsId = '49989d61-0906-4d6b-8ebe-156444824a66'
ORDER BY TourDate;

-- 2. Kiểm tra slots bị soft delete
SELECT 
    Id,
    TourDate,
    Status,
    IsActive,
    IsDeleted,
    DeletedAt,
    'SOFT DELETED' as Issue
FROM TourSlots 
WHERE TourDetailsId = '49989d61-0906-4d6b-8ebe-156444824a66'
AND IsDeleted = 1;

-- 3. Kiểm tra slots bị deactivate
SELECT 
    Id,
    TourDate,
    Status,
    IsActive,
    IsDeleted,
    UpdatedAt,
    'DEACTIVATED' as Issue
FROM TourSlots 
WHERE TourDetailsId = '49989d61-0906-4d6b-8ebe-156444824a66'
AND IsActive = 0
AND IsDeleted = 0;

-- 4. Kiểm tra slots bị unassign từ TourDetails
SELECT 
    Id,
    TourDate,
    TourDetailsId,
    Status,
    IsActive,
    IsDeleted,
    'UNASSIGNED' as Issue
FROM TourSlots 
WHERE TourTemplateId = (
    SELECT TourTemplateId FROM TourDetails WHERE Id = '49989d61-0906-4d6b-8ebe-156444824a66'
)
AND TourDetailsId IS NULL;

-- 5. Kiểm tra total count theo template
SELECT 
    COUNT(*) as TotalSlots,
    COUNT(CASE WHEN TourDetailsId = '49989d61-0906-4d6b-8ebe-156444824a66' THEN 1 END) as AssignedSlots,
    COUNT(CASE WHEN TourDetailsId IS NULL THEN 1 END) as UnassignedSlots,
    COUNT(CASE WHEN IsDeleted = 1 THEN 1 END) as DeletedSlots,
    COUNT(CASE WHEN IsActive = 0 THEN 1 END) as InactiveSlots
FROM TourSlots 
WHERE TourTemplateId = (
    SELECT TourTemplateId FROM TourDetails WHERE Id = '49989d61-0906-4d6b-8ebe-156444824a66'
);

-- 6. Kiểm tra log audit trail (nếu có)
SELECT 
    Id,
    TourDate,
    Status,
    CreatedAt,
    UpdatedAt,
    DeletedAt,
    CreatedById,
    UpdatedById,
    CASE 
        WHEN DeletedAt IS NOT NULL THEN 'DELETED'
        WHEN UpdatedAt > CreatedAt THEN 'MODIFIED' 
        ELSE 'ORIGINAL'
    END as ChangeType
FROM TourSlots 
WHERE TourDetailsId = '49989d61-0906-4d6b-8ebe-156444824a66'
   OR (TourTemplateId = (SELECT TourTemplateId FROM TourDetails WHERE Id = '49989d61-0906-4d6b-8ebe-156444824a66') 
       AND TourDetailsId IS NULL)
ORDER BY TourDate, UpdatedAt DESC;

-- 7. Expected vs Actual count
SELECT 
    'Expected: 5 slots (based on previous API response)' as Expected,
    COUNT(*) as ActualCount,
    CASE 
        WHEN COUNT(*) < 5 THEN 'MISSING SLOTS DETECTED'
        WHEN COUNT(*) = 5 THEN 'COUNT OK - CHECK STATUS/FILTERING'
        ELSE 'UNEXPECTED COUNT'
    END as Diagnosis
FROM TourSlots 
WHERE TourDetailsId = '49989d61-0906-4d6b-8ebe-156444824a66'
AND IsDeleted = 0;
