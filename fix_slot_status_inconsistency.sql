-- ✅ Fix TourSlot status inconsistency 
-- Slot bd93eb91-b7be-4568-b76d-fb95fe006e3c có AvailableSpots=2 nhưng Status=FullyBooked

-- 1. Fix slot có status inconsistency
UPDATE TourSlots 
SET 
    Status = CASE 
        WHEN (MaxGuests - CurrentBookings) > 0 THEN 1  -- Available
        ELSE 2  -- FullyBooked
    END,
    UpdatedAt = NOW()
WHERE TourDetailsId = '49989d61-0906-4d6b-8ebe-156444824a66'
AND IsDeleted = 0
AND (
    -- Fix slots có available spots nhưng status = FullyBooked
    (Status = 2 AND (MaxGuests - CurrentBookings) > 0) OR
    -- Fix slots hết chỗ nhưng status = Available  
    (Status = 1 AND (MaxGuests - CurrentBookings) <= 0)
);

-- 2. Verify fix results
SELECT 
    Id,
    TourDate,
    CASE Status 
        WHEN 1 THEN 'Available'
        WHEN 2 THEN 'FullyBooked' 
        WHEN 3 THEN 'Cancelled'
        WHEN 4 THEN 'Completed'
        WHEN 5 THEN 'InProgress'
        ELSE 'Unknown'
    END as StatusName,
    MaxGuests,
    CurrentBookings,
    (MaxGuests - CurrentBookings) as AvailableSpots,
    CASE 
        WHEN (MaxGuests - CurrentBookings) > 0 AND Status = 1 THEN '✅ CORRECT'
        WHEN (MaxGuests - CurrentBookings) <= 0 AND Status = 2 THEN '✅ CORRECT'
        WHEN (MaxGuests - CurrentBookings) > 0 AND Status = 2 THEN '❌ INCONSISTENT - Fixed'
        WHEN (MaxGuests - CurrentBookings) <= 0 AND Status = 1 THEN '❌ INCONSISTENT - Fixed'
        ELSE '⚠️ CHECK'
    END as StatusCheck,
    UpdatedAt
FROM TourSlots 
WHERE TourDetailsId = '49989d61-0906-4d6b-8ebe-156444824a66'
AND IsDeleted = 0
ORDER BY TourDate;

-- 3. Count fix results
SELECT 
    COUNT(*) as TotalSlots,
    COUNT(CASE WHEN Status = 1 THEN 1 END) as AvailableSlots,
    COUNT(CASE WHEN Status = 2 THEN 1 END) as FullyBookedSlots,
    COUNT(CASE WHEN (MaxGuests - CurrentBookings) > 0 AND Status = 2 THEN 1 END) as InconsistentSlots,
    'All inconsistent slots should be 0 after fix' as Note
FROM TourSlots 
WHERE TourDetailsId = '49989d61-0906-4d6b-8ebe-156444824a66'
AND IsDeleted = 0;
