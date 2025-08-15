-- Assign tour guide to TourOperation for testing
-- Tour Guide 1 (tourguide1@example.com) ID: 02b962f9-1ff9-46b6-a7d8-445252fa454a
-- TourOperation ID: 7200005a-e3d4-4750-a9fe-8b33a6a85efb

UPDATE TourOperations 
SET TourGuideId = '02b962f9-1ff9-46b6-a7d8-445252fa454a',
    UpdatedAt = UTC_TIMESTAMP()
WHERE Id = '7200005a-e3d4-4750-a9fe-8b33a6a85efb';

-- Verify the update
SELECT
    t.Id as TourOperationId,
    t.TourGuideId,
    tg.FullName as TourGuideName,
    td.Title as TourTitle,
    t.Status as OperationStatus
FROM TourOperations t
LEFT JOIN TourGuides tg ON t.TourGuideId = tg.Id
LEFT JOIN TourDetails td ON t.TourDetailsId = td.Id
WHERE t.Id = '7200005a-e3d4-4750-a9fe-8b33a6a85efb';

SELECT 'Tour guide assigned successfully!' as Status;
