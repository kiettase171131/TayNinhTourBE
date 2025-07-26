# API Endpoint: L?y chi ti?t slot v?i thông tin tour và danh sách user ?ã book

## Endpoint
```
GET /api/TourSlot/{id}/tour-details-and-bookings
```

## Mô t?
API này cho phép l?y thông tin chi ti?t c?a m?t tour slot bao g?m:
- Thông tin c? b?n c?a slot (ngày, th?i gian, capacity, etc.)
- Thông tin chi ti?t c?a tour (tiêu ??, mô t?, hình ?nh, k? n?ng yêu c?u, etc.)
- Danh sách t?t c? user ?ã book tour này
- Th?ng kê booking (t?ng s? booking, doanh thu, t? l? l?p ??y, etc.)

## Parameters
- **id** (Guid, required): ID c?a TourSlot c?n l?y thông tin

## Response Format
```json
{
  "success": true,
  "message": "L?y chi ti?t slot v?i thông tin tour và bookings thành công",
  "data": {
    "slot": {
      "id": "guid",
      "tourTemplateId": "guid",
      "tourDetailsId": "guid",
      "tourDate": "2024-03-15",
      "scheduleDay": 1,
      "scheduleDayName": "Ch? nh?t",
      "status": 1,
      "statusName": "Có s?n",
      "maxGuests": 20,
      "currentBookings": 15,
      "availableSpots": 5,
      "isActive": true,
      "isBookable": true,
      "formattedDate": "15/03/2024",
      "formattedDateWithDay": "Ch? nh?t - 15/03/2024",
      "tourTemplate": {
        "id": "guid",
        "title": "Tour Tây Ninh",
        "startLocation": "TP.HCM",
        "endLocation": "Tây Ninh",
        "templateType": 1
      },
      "tourDetails": {
        "id": "guid",
        "title": "Tour VIP Tây Ninh",
        "description": "L?ch trình cao c?p",
        "status": 8,
        "statusName": "Công khai"
      },
      "tourOperation": {
        "id": "guid",
        "price": 500000,
        "maxGuests": 20,
        "currentBookings": 15,
        "availableSpots": 5,
        "status": 1,
        "isActive": true
      }
    },
    "tourDetails": {
      "id": "guid",
      "title": "Tour VIP Tây Ninh",
      "description": "L?ch trình cao c?p v?i các d?ch v? VIP",
      "imageUrls": ["url1", "url2"],
      "skillsRequired": ["English", "Photography"],
      "status": 8,
      "statusName": "Công khai",
      "createdAt": "2024-01-15T10:00:00Z",
      "tourTemplate": { /* same as above */ },
      "tourOperation": { /* same as above */ }
    },
    "bookedUsers": [
      {
        "bookingId": "guid",
        "userId": "guid",
        "userName": "Nguy?n V?n A",
        "userEmail": "user@example.com",
        "contactName": "Nguy?n V?n A",
        "contactPhone": "0123456789",
        "contactEmail": "contact@example.com",
        "numberOfGuests": 2,
        "totalPrice": 900000,
        "originalPrice": 1000000,
        "discountPercent": 10,
        "status": 1,
        "statusName": "?ã xác nh?n",
        "bookingDate": "2024-03-01T10:00:00Z",
        "confirmedDate": "2024-03-01T11:00:00Z",
        "bookingCode": "TB20240301123456",
        "customerNotes": "Yêu c?u phòng ??n"
      }
    ],
    "statistics": {
      "totalBookings": 8,
      "totalGuests": 15,
      "confirmedBookings": 7,
      "pendingBookings": 1,
      "cancelledBookings": 0,
      "totalRevenue": 7500000,
      "confirmedRevenue": 6300000,
      "occupancyRate": 75.0
    }
  }
}
```

## Response Fields

### Slot Object
- **id**: ID c?a slot
- **tourDate**: Ngày di?n ra tour
- **maxGuests**: S? l??ng khách t?i ?a
- **currentBookings**: S? l??ng khách hi?n t?i ?ã book
- **availableSpots**: S? ch? còn l?i
- **isBookable**: Có th? book ???c không

### TourDetails Object
- **title**: Tiêu ?? tour
- **description**: Mô t? chi ti?t
- **imageUrls**: Danh sách URL hình ?nh
- **skillsRequired**: K? n?ng yêu c?u cho h??ng d?n viên
- **status**: Tr?ng thái tour details

### BookedUsers Array
Danh sách các user ?ã book tour, bao g?m:
- **userName**: Tên ng??i dùng
- **numberOfGuests**: S? l??ng khách
- **totalPrice**: T?ng giá ti?n ?ã thanh toán
- **status**: Tr?ng thái booking
- **bookingCode**: Mã booking

### Statistics Object
- **totalBookings**: T?ng s? booking
- **totalGuests**: T?ng s? khách
- **confirmedBookings**: S? booking ?ã xác nh?n
- **totalRevenue**: T?ng doanh thu
- **occupancyRate**: T? l? l?p ??y (%)

## Error Responses
```json
{
  "success": false,
  "message": "Không tìm th?y tour slot"
}
```

```json
{
  "success": false,
  "message": "Có l?i x?y ra khi l?y chi ti?t slot",
  "error": "Error details"
}
```

## Use Cases
1. **Tour Management**: Xem chi ti?t slot ?? qu?n lý capacity và bookings
2. **Revenue Analysis**: Phân tích doanh thu và t? l? l?p ??y
3. **Customer Management**: Xem danh sách khách hàng ?ã ??t tour
4. **Operations**: Chu?n b? cho ngày tour v?i thông tin khách hàng

## Security
- Endpoint này có th? c?n authentication tùy theo yêu c?u business
- Thông tin user ch? hi?n th? nh?ng tr??ng c?n thi?t (không có thông tin nh?y c?m)

## Examples

### Request
```bash
GET /api/TourSlot/123e4567-e89b-12d3-a456-426614174000/tour-details-and-bookings
```

### Success Response
```json
{
  "success": true,
  "message": "L?y chi ti?t slot v?i thông tin tour và bookings thành công",
  "data": {
    // ... data object as described above
  }
}
```