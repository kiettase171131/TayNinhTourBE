# Timeline Progress Integration Test Guide

## 🎯 **Test Overview**

This document provides comprehensive testing procedures for the new Timeline Progress per TourSlot feature.

## 📋 **Pre-Test Setup**

### 1. Database Migration
```sql
-- Run the migration script
SOURCE Database_Migrations/001_Create_TourSlotTimelineProgress.sql;

-- Verify table creation
DESCRIBE TourSlotTimelineProgress;

-- Check data migration
SELECT COUNT(*) FROM TourSlotTimelineProgress;
```

### 2. Service Registration
Ensure `ITourGuideTimelineService` is registered in DI container:

```csharp
// In Program.cs or Startup.cs
services.AddScoped<ITourGuideTimelineService, TourGuideTimelineService>();
```

### 3. Test Data Preparation
```sql
-- Ensure we have test data
SELECT 
    ts.Id as TourSlotId,
    td.Title as TourTitle,
    COUNT(ti.Id) as TimelineItemCount
FROM TourSlot ts
INNER JOIN TourDetails td ON ts.TourDetailsId = td.Id
INNER JOIN TimelineItem ti ON ti.TourDetailsId = td.Id
WHERE ts.IsActive = TRUE
GROUP BY ts.Id, td.Title
LIMIT 5;
```

## 🧪 **API Testing Scenarios**

### Test 1: Get Timeline with Progress
```bash
# Login as tour guide
POST /api/Authentication/login
{
  "email": "tourguide1@example.com",
  "password": "12345678h@"
}

# Get timeline with progress (NEW API)
GET /api/TourGuide/tour-slot/{tourSlotId}/timeline
Authorization: Bearer {token}

# Expected Response:
{
  "statusCode": 200,
  "message": "Lấy timeline với progress thành công",
  "data": {
    "timeline": [...],
    "summary": {
      "tourSlotId": "...",
      "totalItems": 5,
      "completedItems": 2,
      "progressPercentage": 40
    },
    "tourSlot": {...},
    "tourDetails": {...}
  },
  "success": true
}
```

### Test 2: Complete Timeline Item
```bash
# Complete timeline item (NEW API)
POST /api/TourGuide/tour-slot/{tourSlotId}/timeline/{timelineItemId}/complete
Authorization: Bearer {token}
{
  "notes": "Đã hoàn thành check-in tại điểm tham quan"
}

# Expected Response:
{
  "statusCode": 200,
  "message": "Timeline item đã được hoàn thành thành công",
  "data": {
    "success": true,
    "completedItem": {...},
    "summary": {...},
    "nextItem": {...},
    "isTimelineCompleted": false
  },
  "success": true
}
```

### Test 3: Sequential Completion Validation
```bash
# Try to complete item out of order (should fail)
POST /api/TourGuide/tour-slot/{tourSlotId}/timeline/{laterTimelineItemId}/complete
Authorization: Bearer {token}
{
  "notes": "Trying to complete out of order"
}

# Expected Response:
{
  "statusCode": 400,
  "message": "Timeline item này chưa thể hoàn thành. Vui lòng hoàn thành các item trước đó theo thứ tự."
}
```

### Test 4: Bulk Complete Timeline Items
```bash
# Bulk complete multiple items
POST /api/TourGuide/timeline/bulk-complete
Authorization: Bearer {token}
{
  "tourSlotId": "{tourSlotId}",
  "timelineItemIds": ["{item1Id}", "{item2Id}"],
  "notes": "Bulk completion test",
  "respectSequentialOrder": true
}

# Expected Response:
{
  "statusCode": 200,
  "data": {
    "successCount": 2,
    "failureCount": 0,
    "totalCount": 2,
    "message": "Tất cả timeline items đã được hoàn thành thành công"
  }
}
```

### Test 5: Reset Timeline Item
```bash
# Reset completed timeline item
POST /api/TourGuide/tour-slot/{tourSlotId}/timeline/{timelineItemId}/reset
Authorization: Bearer {token}
{
  "reason": "Reset for testing purposes",
  "resetSubsequentItems": true
}

# Expected Response:
{
  "statusCode": 200,
  "data": {
    "success": true,
    "message": "Timeline item đã được reset thành công"
  }
}
```

### Test 6: Get Progress Summary
```bash
# Get progress summary
GET /api/TourGuide/tour-slot/{tourSlotId}/progress-summary
Authorization: Bearer {token}

# Expected Response:
{
  "statusCode": 200,
  "data": {
    "tourSlotId": "...",
    "totalItems": 5,
    "completedItems": 3,
    "progressPercentage": 60,
    "isFullyCompleted": false,
    "nextItem": {...}
  }
}
```

### Test 7: Get Timeline Statistics
```bash
# Get timeline statistics
GET /api/TourGuide/tour-slot/{tourSlotId}/statistics
Authorization: Bearer {token}

# Expected Response:
{
  "statusCode": 200,
  "data": {
    "tourSlotId": "...",
    "averageCompletionTimeMinutes": 15.5,
    "completionRate": 60.0,
    "onTimeCompletions": 2,
    "overdueCompletions": 1,
    "completionTrend": [...]
  }
}
```

## 🔄 **Backward Compatibility Testing**

### Test 8: Legacy API Still Works
```bash
# Old timeline API (should still work)
GET /api/TourGuide/tour/{operationId}/timeline
Authorization: Bearer {token}

# Old complete API (should still work)
POST /api/TourGuide/timeline/{timelineId}/complete
Authorization: Bearer {token}
{
  "notes": "Legacy API test"
}
```

## 📱 **Mobile App Testing**

### Test 9: Mobile API Integration
```dart
// Test new mobile API service methods
final apiService = TourGuideApiService(dio);

// Get timeline with progress
final timelineResponse = await apiService.getTourSlotTimeline(
  tourSlotId,
  false, // includeInactive
  true,  // includeShopInfo
);

// Complete timeline item
final completeResponse = await apiService.completeTimelineItemForSlot(
  tourSlotId,
  timelineItemId,
  CompleteTimelineRequest(notes: 'Mobile test'),
);
```

## 🔍 **Database Validation Tests**

### Test 10: Data Integrity
```sql
-- Check for orphaned records
SELECT COUNT(*) FROM TourSlotTimelineProgress tp
LEFT JOIN TourSlot ts ON tp.TourSlotId = ts.Id
LEFT JOIN TimelineItem ti ON tp.TimelineItemId = ti.Id
WHERE ts.Id IS NULL OR ti.Id IS NULL;
-- Expected: 0

-- Check unique constraints
SELECT TourSlotId, TimelineItemId, COUNT(*)
FROM TourSlotTimelineProgress
GROUP BY TourSlotId, TimelineItemId
HAVING COUNT(*) > 1;
-- Expected: 0 rows

-- Check sequential completion logic
SELECT 
    ts.Id as TourSlotId,
    ti.SortOrder,
    tp.IsCompleted,
    LAG(tp.IsCompleted) OVER (PARTITION BY ts.Id ORDER BY ti.SortOrder) as PrevCompleted
FROM TourSlot ts
INNER JOIN TimelineItem ti ON ti.TourDetailsId = ts.TourDetailsId
INNER JOIN TourSlotTimelineProgress tp ON tp.TourSlotId = ts.Id AND tp.TimelineItemId = ti.Id
WHERE tp.IsCompleted = TRUE
  AND LAG(tp.IsCompleted) OVER (PARTITION BY ts.Id ORDER BY ti.SortOrder) = FALSE;
-- Expected: 0 rows (no gaps in completion sequence)
```

## 🚨 **Error Handling Tests**

### Test 11: Authorization Errors
```bash
# Test without authentication
GET /api/TourGuide/tour-slot/{tourSlotId}/timeline
# Expected: 401 Unauthorized

# Test with wrong role
GET /api/TourGuide/tour-slot/{tourSlotId}/timeline
Authorization: Bearer {customerToken}
# Expected: 403 Forbidden

# Test accessing other guide's tour
GET /api/TourGuide/tour-slot/{otherGuideTourSlotId}/timeline
Authorization: Bearer {tourGuideToken}
# Expected: 403 Forbidden
```

### Test 12: Validation Errors
```bash
# Test with invalid tour slot ID
GET /api/TourGuide/tour-slot/invalid-id/timeline
# Expected: 400 Bad Request

# Test completion with too long notes
POST /api/TourGuide/tour-slot/{tourSlotId}/timeline/{timelineItemId}/complete
{
  "notes": "Very long notes exceeding 500 characters..."
}
# Expected: 400 Bad Request
```

## 📊 **Performance Tests**

### Test 13: Load Testing
```bash
# Test with multiple concurrent requests
for i in {1..10}; do
  curl -X GET "/api/TourGuide/tour-slot/{tourSlotId}/timeline" \
    -H "Authorization: Bearer {token}" &
done
wait

# Monitor response times and database performance
```

## ✅ **Success Criteria**

### API Tests
- [ ] All new APIs return correct response format
- [ ] Sequential completion logic works correctly
- [ ] Progress calculation is accurate
- [ ] Notifications are sent to guests
- [ ] Error handling works as expected

### Database Tests
- [ ] No orphaned records
- [ ] Unique constraints enforced
- [ ] Foreign key constraints work
- [ ] Performance is acceptable

### Mobile Tests
- [ ] New API service methods work
- [ ] UI updates correctly with progress
- [ ] Error handling in mobile app

### Backward Compatibility
- [ ] Legacy APIs still function
- [ ] Existing mobile app continues to work
- [ ] No breaking changes for website

## 🐛 **Common Issues & Solutions**

### Issue 1: Service Not Registered
```
Error: Unable to resolve service for type 'ITourGuideTimelineService'
Solution: Add service registration in DI container
```

### Issue 2: Migration Fails
```
Error: Table 'TourSlotTimelineProgress' already exists
Solution: Check if migration was already run, or drop table first
```

### Issue 3: Authorization Fails
```
Error: Tour guide không có quyền truy cập tour slot này
Solution: Verify tour guide is assigned to the tour operation
```

### Issue 4: Sequential Validation Fails
```
Error: Timeline item này chưa thể hoàn thành
Solution: Complete previous timeline items in order first
```

## 📝 **Test Report Template**

```
Timeline Progress Feature Test Report
=====================================

Test Date: ___________
Tester: ___________
Environment: ___________

API Tests:
[ ] Get Timeline with Progress - PASS/FAIL
[ ] Complete Timeline Item - PASS/FAIL
[ ] Bulk Complete - PASS/FAIL
[ ] Reset Timeline Item - PASS/FAIL
[ ] Progress Summary - PASS/FAIL
[ ] Statistics - PASS/FAIL

Database Tests:
[ ] Data Integrity - PASS/FAIL
[ ] Performance - PASS/FAIL
[ ] Constraints - PASS/FAIL

Mobile Tests:
[ ] API Integration - PASS/FAIL
[ ] UI Updates - PASS/FAIL

Backward Compatibility:
[ ] Legacy APIs - PASS/FAIL
[ ] Existing Apps - PASS/FAIL

Issues Found:
1. ___________
2. ___________

Overall Status: PASS/FAIL
```
