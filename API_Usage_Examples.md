# My Bookings API - Updated Usage Examples

## API Endpoint
`GET /api/UserTourBooking/my-bookings`

## Updated Parameters

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| pageIndex | int | Trang hi?n t?i (m?c ??nh: 1) | 1 |
| pageSize | int | S? items per page (m?c ??nh: 10) | 10 |
| status | BookingStatus? | L?c theo tr?ng thái | confirmed, pending, cancelled |
| startDate | DateTime? | L?c t? ngày booking | 2024-01-01 |
| endDate | DateTime? | L?c ??n ngày booking | 2024-12-31 |
| searchTerm | string? | Tìm ki?m theo tên công ty | "Công ty ABC" |
| **bookingCode** | string? | **?? NEW: Mã PayOsOrderCode ?? tìm ki?m** | **TNDT5424028424** |

## Important Changes

?? **BREAKING CHANGE**: The `bookingCode` parameter now searches by `PayOsOrderCode` instead of the regular `BookingCode`.

### Before (Old Behavior)
```
GET /api/UserTourBooking/my-bookings?bookingCode=TB20241201123456
// Would search by TourBooking.BookingCode
```

### After (New Behavior) 
```
GET /api/UserTourBooking/my-bookings?bookingCode=TNDT5424028424
// Now searches by TourBooking.PayOsOrderCode
```

## Example API Calls

### 1. Search by PayOsOrderCode (Exact Match)
```
GET /api/UserTourBooking/my-bookings?bookingCode=TNDT5424028424
```

### 2. Search by PayOsOrderCode (Partial Match)
```
GET /api/UserTourBooking/my-bookings?bookingCode=TNDT542
```

### 3. Combined Filter with PayOsOrderCode
```
GET /api/UserTourBooking/my-bookings?bookingCode=TNDT5424028424&status=confirmed&pageSize=5
```

## Response Format

```json
{
  "success": true,
  "message": "L?y danh sách bookings thành công",
  "data": {
    "items": [
      {
        "id": "guid",
        "bookingCode": "TB20241201123456",
        "payOsOrderCode": "TNDT5424028424",
        "status": "Confirmed",
        "totalPrice": 500000,
        // ... other booking details
      }
    ],
    "totalCount": 1,
    "pageIndex": 1,
    "pageSize": 10
  },
  "note": "Tìm ki?m theo PayOsOrderCode: TNDT5424028424"
}
```

## Backend Changes Made

1. **ITourBookingRepository.cs**: Added `bookingCode` parameter to `GetUserBookingsWithFilterAsync` method
2. **TourBookingRepository.cs**: Updated to filter by `PayOsOrderCode` when `bookingCode` parameter is provided
3. **IUserTourBookingService.cs**: Added `bookingCode` parameter to `GetUserBookingsAsync` method
4. **UserTourBookingService.cs**: Updated to pass `bookingCode` parameter to repository
5. **UserTourBookingController.cs**: Added `bookingCode` parameter to `GetMyBookings` endpoint

## Testing

You can test the updated API with tools like Postman or curl:

```bash
# Test with exact PayOsOrderCode
curl -X GET "https://your-api.com/api/UserTourBooking/my-bookings?bookingCode=TNDT5424028424" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Test with partial PayOsOrderCode
curl -X GET "https://your-api.com/api/UserTourBooking/my-bookings?bookingCode=TNDT542" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
```