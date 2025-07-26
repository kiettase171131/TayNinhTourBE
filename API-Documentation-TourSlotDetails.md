# API Endpoint: L?y chi ti?t slot v?i th�ng tin tour v� danh s�ch user ?� book

## Endpoint
```
GET /api/TourSlot/{id}/tour-details-and-bookings
```

## M� t?
API n�y cho ph�p l?y th�ng tin chi ti?t c?a m?t tour slot bao g?m:
- Th�ng tin c? b?n c?a slot (ng�y, th?i gian, capacity, etc.)
- Th�ng tin chi ti?t c?a tour (ti�u ??, m� t?, h�nh ?nh, k? n?ng y�u c?u, etc.)
- Danh s�ch t?t c? user ?� book tour n�y
- Th?ng k� booking (t?ng s? booking, doanh thu, t? l? l?p ??y, etc.)

## Parameters
- **id** (Guid, required): ID c?a TourSlot c?n l?y th�ng tin

## Response Format
```json
{
  "success": true,
  "message": "L?y chi ti?t slot v?i th�ng tin tour v� bookings th�nh c�ng",
  "data": {
    "slot": {
      "id": "guid",
      "tourTemplateId": "guid",
      "tourDetailsId": "guid",
      "tourDate": "2024-03-15",
      "scheduleDay": 1,
      "scheduleDayName": "Ch? nh?t",
      "status": 1,
      "statusName": "C� s?n",
      "maxGuests": 20,
      "currentBookings": 15,
      "availableSpots": 5,
      "isActive": true,
      "isBookable": true,
      "formattedDate": "15/03/2024",
      "formattedDateWithDay": "Ch? nh?t - 15/03/2024",
      "tourTemplate": {
        "id": "guid",
        "title": "Tour T�y Ninh",
        "startLocation": "TP.HCM",
        "endLocation": "T�y Ninh",
        "templateType": 1
      },
      "tourDetails": {
        "id": "guid",
        "title": "Tour VIP T�y Ninh",
        "description": "L?ch tr�nh cao c?p",
        "status": 8,
        "statusName": "C�ng khai"
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
      "title": "Tour VIP T�y Ninh",
      "description": "L?ch tr�nh cao c?p v?i c�c d?ch v? VIP",
      "imageUrls": ["url1", "url2"],
      "skillsRequired": ["English", "Photography"],
      "status": 8,
      "statusName": "C�ng khai",
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
        "statusName": "?� x�c nh?n",
        "bookingDate": "2024-03-01T10:00:00Z",
        "confirmedDate": "2024-03-01T11:00:00Z",
        "bookingCode": "TB20240301123456",
        "customerNotes": "Y�u c?u ph�ng ??n"
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
- **tourDate**: Ng�y di?n ra tour
- **maxGuests**: S? l??ng kh�ch t?i ?a
- **currentBookings**: S? l??ng kh�ch hi?n t?i ?� book
- **availableSpots**: S? ch? c�n l?i
- **isBookable**: C� th? book ???c kh�ng

### TourDetails Object
- **title**: Ti�u ?? tour
- **description**: M� t? chi ti?t
- **imageUrls**: Danh s�ch URL h�nh ?nh
- **skillsRequired**: K? n?ng y�u c?u cho h??ng d?n vi�n
- **status**: Tr?ng th�i tour details

### BookedUsers Array
Danh s�ch c�c user ?� book tour, bao g?m:
- **userName**: T�n ng??i d�ng
- **numberOfGuests**: S? l??ng kh�ch
- **totalPrice**: T?ng gi� ti?n ?� thanh to�n
- **status**: Tr?ng th�i booking
- **bookingCode**: M� booking

### Statistics Object
- **totalBookings**: T?ng s? booking
- **totalGuests**: T?ng s? kh�ch
- **confirmedBookings**: S? booking ?� x�c nh?n
- **totalRevenue**: T?ng doanh thu
- **occupancyRate**: T? l? l?p ??y (%)

## Error Responses
```json
{
  "success": false,
  "message": "Kh�ng t�m th?y tour slot"
}
```

```json
{
  "success": false,
  "message": "C� l?i x?y ra khi l?y chi ti?t slot",
  "error": "Error details"
}
```

## Use Cases
1. **Tour Management**: Xem chi ti?t slot ?? qu?n l� capacity v� bookings
2. **Revenue Analysis**: Ph�n t�ch doanh thu v� t? l? l?p ??y
3. **Customer Management**: Xem danh s�ch kh�ch h�ng ?� ??t tour
4. **Operations**: Chu?n b? cho ng�y tour v?i th�ng tin kh�ch h�ng

## Security
- Endpoint n�y c� th? c?n authentication t�y theo y�u c?u business
- Th�ng tin user ch? hi?n th? nh?ng tr??ng c?n thi?t (kh�ng c� th�ng tin nh?y c?m)

## Examples

### Request
```bash
GET /api/TourSlot/123e4567-e89b-12d3-a456-426614174000/tour-details-and-bookings
```

### Success Response
```json
{
  "success": true,
  "message": "L?y chi ti?t slot v?i th�ng tin tour v� bookings th�nh c�ng",
  "data": {
    // ... data object as described above
  }
}
```