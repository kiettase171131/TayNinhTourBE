# User Tour Search API Documentation

## Overview

User Tour Search API cung c?p endpoints cho user tìm ki?m tour không c?n authentication. API này cho phép tìm ki?m tour theo schedule day (th? 7, ch? nh?t), tháng, n?m, ?i?m ??n và text search trong title c?a tour details.

**Base URL**: `https://api.tayninhour.com`  
**Version**: `v1`  
**Authentication**: Không c?n (Public API)  
**Controller**: `UserTourSearchController`

---

## Endpoints

### 1. Search Tours

Tìm ki?m tour theo các tiêu chí filter.

```http
GET /api/UserTourSearch/search
```

#### Query Parameters

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `scheduleDay` | string | No | Th? trong tu?n (`Saturday`, `Sunday`) | `Saturday` |
| `month` | integer | No | Tháng (1-12) | `6` |
| `year` | integer | No | N?m (2024-2030) | `2025` |
| `destination` | string | No | ?i?m ??n (tìm trong EndLocation) | `Tây Ninh` |
| `textSearch` | string | No | Text tìm ki?m trong title TourDetails | `núi bà ?en` |
| `pageIndex` | integer | No | Trang hi?n t?i (default: 1) | `1` |
| `pageSize` | integer | No | S? items per page (default: 10, max: 50) | `20` |

#### Validation Rules

- `scheduleDay`: Ch? ch?p nh?n `Saturday` ho?c `Sunday` (h? th?ng ch? h? tr? tour cu?i tu?n)
- `month`: Ph?i t? 1 ??n 12
- `year`: Ph?i t? 2024 ??n 2030
- `pageIndex`: T?i thi?u 1
- `pageSize`: T? 1 ??n 50

#### Response

**Status Code**: `200 OK`

```json
{
  "statusCode": 200,
  "message": "Tìm th?y 15 tour phù h?p",
  "data": {
    "tours": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "L?ch trình VIP Núi Bà ?en",
        "description": "L?ch trình cao c?p v?i d?ch v? VIP",
        "status": "Public",
        "skillsRequired": "TheoDoiKhachHang,GiaiThichLichSu",
        "imageUrls": [
          "/images/tour1.jpg",
          "/images/tour2.jpg"
        ],
        "createdAt": "2025-06-03T10:00:00Z",
        "tourTemplate": {
          "id": "template-id",
          "title": "Tour Núi Bà ?en",
          "templateType": "FreeScenic",
          "scheduleDays": "Saturday",
          "scheduleDaysVietnamese": "Th? b?y",
          "startLocation": "TP.HCM",
          "endLocation": "Tây Ninh",
          "month": 6,
          "year": 2025,
          "images": [
            {
              "id": "image-id",
              "url": "/images/template1.jpg"
            }
          ],
          "createdBy": {
            "id": "user-id",
            "name": "Công ty ABC",
            "email": "contact@abc.com"
          }
        },
        "tourOperation": {
          "id": "operation-id",
          "price": 500000,
          "maxGuests": 20,
          "description": "V?n hành tour v?i guide chuyên nghi?p",
          "notes": "C?n mang giày th? thao",
          "status": "Scheduled",
          "currentBookings": 5
        },
        "availableSlots": [
          {
            "id": "slot-id-1",
            "tourDate": "2025-06-07",
            "status": "Available",
            "maxGuests": 20,
            "currentBookings": 5,
            "availableSpots": 15
          },
          {
            "id": "slot-id-2",
            "tourDate": "2025-06-14",
            "status": "Available",
            "maxGuests": 20,
            "currentBookings": 2,
            "availableSpots": 18
          }
        ]
      }
    ],
    "pagination": {
      "totalCount": 15,
      "pageIndex": 1,
      "pageSize": 10,
      "totalPages": 2,
      "hasNextPage": true,
      "hasPreviousPage": false
    },
    "searchCriteria": {
      "scheduleDay": "Saturday",
      "scheduleDayVietnamese": "Th? b?y",
      "month": 6,
      "year": 2025,
      "destination": "Tây Ninh",
      "textSearch": "núi bà ?en"
    }
  }
}
```

#### Error Responses

**Status Code**: `400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "Hi?n t?i h? th?ng ch? h? tr? tour vào th? 7 (Saturday) và ch? nh?t (Sunday)"
}
```

**Status Code**: `400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "Tháng ph?i t? 1 ??n 12"
}
```

**Status Code**: `400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "N?m ph?i t? 2024 ??n 2030"
}
```

---

### 2. Get Filter Options

L?y danh sách các tùy ch?n filter có s?n trong h? th?ng.

```http
GET /api/UserTourSearch/filters
```

#### Response

**Status Code**: `200 OK`

```json
{
  "statusCode": 200,
  "message": "L?y tùy ch?n filter thành công",
  "data": {
    "scheduleDays": [
      {
        "value": "Saturday",
        "display": "Th? b?y"
      },
      {
        "value": "Sunday",
        "display": "Ch? nh?t"
      }
    ],
    "destinations": [
      "Tây Ninh",
      "Núi Bà ?en",
      "Chùa Cao ?ài",
      "??a ??o C? Chi"
    ],
    "availableMonthsYears": [
      {
        "month": 6,
        "year": 2025
      },
      {
        "month": 7,
        "year": 2025
      },
      {
        "month": 12,
        "year": 2024
      }
    ],
    "yearRange": {
      "min": 2024,
      "max": 2030
    },
    "supportedScheduleDays": ["Saturday", "Sunday"],
    "note": "Hi?n t?i h? th?ng ch? h? tr? tour vào th? 7 và ch? nh?t"
  }
}
```

---

## Data Models

### TourSearchResultDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | ID c?a TourDetails |
| `title` | string | Tiêu ?? l?ch trình |
| `description` | string? | Mô t? l?ch trình |
| `status` | string | Tr?ng thái (Public, Draft, etc.) |
| `skillsRequired` | string? | K? n?ng yêu c?u cho guide |
| `imageUrls` | List<string> | Danh sách URL hình ?nh |
| `createdAt` | DateTime | Ngày t?o |
| `tourTemplate` | TourTemplateBasicDto | Thông tin template |
| `tourOperation` | TourOperationBasicDto? | Thông tin v?n hành |
| `availableSlots` | List<AvailableSlotDto> | Danh sách slot kh? d?ng |

### TourTemplateBasicDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | ID c?a TourTemplate |
| `title` | string | Tiêu ?? template |
| `templateType` | string | Lo?i template (FreeScenic, PaidAttraction) |
| `scheduleDays` | string | Ngày trong tu?n (Saturday, Sunday) |
| `scheduleDaysVietnamese` | string | Tên ti?ng Vi?t c?a ngày |
| `startLocation` | string | ?i?m kh?i hành |
| `endLocation` | string | ?i?m ??n |
| `month` | int | Tháng áp d?ng |
| `year` | int | N?m áp d?ng |
| `images` | List<ImageDto> | Danh sách hình ?nh |
| `createdBy` | CreatedByDto | Thông tin ng??i t?o |

### TourOperationBasicDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | ID c?a TourOperation |
| `price` | decimal | Giá tour |
| `maxGuests` | int | S? khách t?i ?a |
| `description` | string? | Mô t? |
| `notes` | string? | Ghi chú |
| `status` | string | Tr?ng thái v?n hành |
| `currentBookings` | int | S? booking hi?n t?i |

### AvailableSlotDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | ID c?a TourSlot |
| `tourDate` | DateOnly | Ngày tour |
| `status` | string | Tr?ng thái slot |
| `maxGuests` | int | S? khách t?i ?a |
| `currentBookings` | int | S? booking hi?n t?i |
| `availableSpots` | int | S? ch? còn tr?ng |

---

## Business Logic

### Search Logic

1. **Filter by Schedule Day**: Ch? tìm tour vào th? 7 ho?c ch? nh?t
2. **Filter by Month/Year**: Tìm trong TourTemplate có tháng/n?m t??ng ?ng
3. **Filter by Destination**: Tìm ki?m trong `EndLocation` c?a TourTemplate
4. **Text Search**: Tìm ki?m trong `Title` c?a TourDetails
5. **Only Public Tours**: Ch? hi?n th? tour có status = Public
6. **Only Active Tours**: Ch? hi?n th? tour và template ?ang active
7. **Available Slots Only**: Ch? hi?n th? slot có ngày >= hôm nay, status = Available, và có ch? tr?ng

### Sorting

- Tours ???c s?p x?p theo `CreatedAt` gi?m d?n (m?i nh?t tr??c)
- Available slots ???c s?p x?p theo `TourDate` t?ng d?n (g?n nh?t tr??c)

---

## Examples

### cURL Examples

#### Search Tours - Basic
```bash
curl -X GET "https://api.tayninhour.com/api/UserTourSearch/search?scheduleDay=Saturday&month=6&year=2025" \
  -H "Content-Type: application/json"
```

#### Search Tours - With Text Search
```bash
curl -X GET "https://api.tayninhour.com/api/UserTourSearch/search?textSearch=núi%20bà%20?en&destination=Tây%20Ninh" \
  -H "Content-Type: application/json"
```

#### Search Tours - With Pagination
```bash
curl -X GET "https://api.tayninhour.com/api/UserTourSearch/search?pageIndex=2&pageSize=20" \
  -H "Content-Type: application/json"
```

#### Get Filter Options
```bash
curl -X GET "https://api.tayninhour.com/api/UserTourSearch/filters" \
  -H "Content-Type: application/json"
```

### JavaScript Examples

#### Fetch Tours for Weekend
```javascript
const searchTours = async () => {
  try {
    const response = await fetch('/api/UserTourSearch/search?scheduleDay=Saturday&month=6&year=2025');
    const data = await response.json();
    
    if (data.statusCode === 200) {
      console.log(`Found ${data.data.pagination.totalCount} tours`);
      data.data.tours.forEach(tour => {
        console.log(`${tour.title} - ${tour.tourOperation?.price.toLocaleString()} VND`);
      });
    }
  } catch (error) {
    console.error('Error searching tours:', error);
  }
};
```

#### Get Available Filters
```javascript
const getFilters = async () => {
  try {
    const response = await fetch('/api/UserTourSearch/filters');
    const data = await response.json();
    
    if (data.statusCode === 200) {
      console.log('Available destinations:', data.data.destinations);
      console.log('Available schedule days:', data.data.scheduleDays);
    }
  } catch (error) {
    console.error('Error getting filters:', error);
  }
};
```

---

## Use Cases

### 1. User tìm tour cu?i tu?n

```
Scenario: User mu?n tìm tour vào th? b?y tháng 6/2025 ? Tây Ninh
Request: GET /api/UserTourSearch/search?scheduleDay=Saturday&month=6&year=2025&destination=Tây Ninh
Result: Danh sách tour th?a mãn ?i?u ki?n v?i thông tin ??y ?? và slot kh? d?ng
```

### 2. User tìm tour theo tên

```
Scenario: User mu?n tìm tour có ch? "núi bà ?en" trong tên
Request: GET /api/UserTourSearch/search?textSearch=núi bà ?en
Result: Danh sách tour có title ch?a "núi bà ?en"
```

### 3. User xem các tùy ch?n filter

```
Scenario: User mu?n bi?t có nh?ng ?i?m ??n nào và tháng nào có tour
Request: GET /api/UserTourSearch/filters
Result: Danh sách ??y ?? các tùy ch?n filter hi?n có trong h? th?ng
```

---

## Error Handling

| Status Code | Description | Action |
|-------------|-------------|---------|
| `200` | OK | Success |
| `400` | Bad Request | Check parameter validation |
| `500` | Internal Server Error | Check server logs |

---

## Performance Considerations

1. **Database Indexing**: ??m b?o có index trên:
   - `TourTemplate.ScheduleDays`
   - `TourTemplate.Month, Year`
   - `TourTemplate.EndLocation`
   - `TourDetails.Title`
   - `TourDetails.Status`

2. **Pagination**: Luôn s? d?ng pagination ?? tránh load quá nhi?u data

3. **Caching**: Có th? cache k?t qu? filter options vì ít thay ??i

---

**Last Updated**: January 15, 2025  
**API Version**: v1.0  
**Contact**: support@tayninhour.com