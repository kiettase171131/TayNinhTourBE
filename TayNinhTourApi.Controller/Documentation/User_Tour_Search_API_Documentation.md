# User Tour Search API Documentation

## Overview

User Tour Search API cung c?p endpoints cho user t�m ki?m tour kh�ng c?n authentication. API n�y cho ph�p t�m ki?m tour theo schedule day (th? 7, ch? nh?t), th�ng, n?m, ?i?m ??n v� text search trong title c?a tour details.

**Base URL**: `https://api.tayninhour.com`  
**Version**: `v1`  
**Authentication**: Kh�ng c?n (Public API)  
**Controller**: `UserTourSearchController`

---

## Endpoints

### 1. Search Tours

T�m ki?m tour theo c�c ti�u ch� filter.

```http
GET /api/UserTourSearch/search
```

#### Query Parameters

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `scheduleDay` | string | No | Th? trong tu?n (`Saturday`, `Sunday`) | `Saturday` |
| `month` | integer | No | Th�ng (1-12) | `6` |
| `year` | integer | No | N?m (2024-2030) | `2025` |
| `destination` | string | No | ?i?m ??n (t�m trong EndLocation) | `T�y Ninh` |
| `textSearch` | string | No | Text t�m ki?m trong title TourDetails | `n�i b� ?en` |
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
  "message": "T�m th?y 15 tour ph� h?p",
  "data": {
    "tours": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "L?ch tr�nh VIP N�i B� ?en",
        "description": "L?ch tr�nh cao c?p v?i d?ch v? VIP",
        "status": "Public",
        "skillsRequired": "TheoDoiKhachHang,GiaiThichLichSu",
        "imageUrls": [
          "/images/tour1.jpg",
          "/images/tour2.jpg"
        ],
        "createdAt": "2025-06-03T10:00:00Z",
        "tourTemplate": {
          "id": "template-id",
          "title": "Tour N�i B� ?en",
          "templateType": "FreeScenic",
          "scheduleDays": "Saturday",
          "scheduleDaysVietnamese": "Th? b?y",
          "startLocation": "TP.HCM",
          "endLocation": "T�y Ninh",
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
            "name": "C�ng ty ABC",
            "email": "contact@abc.com"
          }
        },
        "tourOperation": {
          "id": "operation-id",
          "price": 500000,
          "maxGuests": 20,
          "description": "V?n h�nh tour v?i guide chuy�n nghi?p",
          "notes": "C?n mang gi�y th? thao",
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
      "destination": "T�y Ninh",
      "textSearch": "n�i b� ?en"
    }
  }
}
```

#### Error Responses

**Status Code**: `400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "Hi?n t?i h? th?ng ch? h? tr? tour v�o th? 7 (Saturday) v� ch? nh?t (Sunday)"
}
```

**Status Code**: `400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "Th�ng ph?i t? 1 ??n 12"
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

L?y danh s�ch c�c t�y ch?n filter c� s?n trong h? th?ng.

```http
GET /api/UserTourSearch/filters
```

#### Response

**Status Code**: `200 OK`

```json
{
  "statusCode": 200,
  "message": "L?y t�y ch?n filter th�nh c�ng",
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
      "T�y Ninh",
      "N�i B� ?en",
      "Ch�a Cao ?�i",
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
    "note": "Hi?n t?i h? th?ng ch? h? tr? tour v�o th? 7 v� ch? nh?t"
  }
}
```

---

## Data Models

### TourSearchResultDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | ID c?a TourDetails |
| `title` | string | Ti�u ?? l?ch tr�nh |
| `description` | string? | M� t? l?ch tr�nh |
| `status` | string | Tr?ng th�i (Public, Draft, etc.) |
| `skillsRequired` | string? | K? n?ng y�u c?u cho guide |
| `imageUrls` | List<string> | Danh s�ch URL h�nh ?nh |
| `createdAt` | DateTime | Ng�y t?o |
| `tourTemplate` | TourTemplateBasicDto | Th�ng tin template |
| `tourOperation` | TourOperationBasicDto? | Th�ng tin v?n h�nh |
| `availableSlots` | List<AvailableSlotDto> | Danh s�ch slot kh? d?ng |

### TourTemplateBasicDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | ID c?a TourTemplate |
| `title` | string | Ti�u ?? template |
| `templateType` | string | Lo?i template (FreeScenic, PaidAttraction) |
| `scheduleDays` | string | Ng�y trong tu?n (Saturday, Sunday) |
| `scheduleDaysVietnamese` | string | T�n ti?ng Vi?t c?a ng�y |
| `startLocation` | string | ?i?m kh?i h�nh |
| `endLocation` | string | ?i?m ??n |
| `month` | int | Th�ng �p d?ng |
| `year` | int | N?m �p d?ng |
| `images` | List<ImageDto> | Danh s�ch h�nh ?nh |
| `createdBy` | CreatedByDto | Th�ng tin ng??i t?o |

### TourOperationBasicDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | ID c?a TourOperation |
| `price` | decimal | Gi� tour |
| `maxGuests` | int | S? kh�ch t?i ?a |
| `description` | string? | M� t? |
| `notes` | string? | Ghi ch� |
| `status` | string | Tr?ng th�i v?n h�nh |
| `currentBookings` | int | S? booking hi?n t?i |

### AvailableSlotDto

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | ID c?a TourSlot |
| `tourDate` | DateOnly | Ng�y tour |
| `status` | string | Tr?ng th�i slot |
| `maxGuests` | int | S? kh�ch t?i ?a |
| `currentBookings` | int | S? booking hi?n t?i |
| `availableSpots` | int | S? ch? c�n tr?ng |

---

## Business Logic

### Search Logic

1. **Filter by Schedule Day**: Ch? t�m tour v�o th? 7 ho?c ch? nh?t
2. **Filter by Month/Year**: T�m trong TourTemplate c� th�ng/n?m t??ng ?ng
3. **Filter by Destination**: T�m ki?m trong `EndLocation` c?a TourTemplate
4. **Text Search**: T�m ki?m trong `Title` c?a TourDetails
5. **Only Public Tours**: Ch? hi?n th? tour c� status = Public
6. **Only Active Tours**: Ch? hi?n th? tour v� template ?ang active
7. **Available Slots Only**: Ch? hi?n th? slot c� ng�y >= h�m nay, status = Available, v� c� ch? tr?ng

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
curl -X GET "https://api.tayninhour.com/api/UserTourSearch/search?textSearch=n�i%20b�%20?en&destination=T�y%20Ninh" \
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

### 1. User t�m tour cu?i tu?n

```
Scenario: User mu?n t�m tour v�o th? b?y th�ng 6/2025 ? T�y Ninh
Request: GET /api/UserTourSearch/search?scheduleDay=Saturday&month=6&year=2025&destination=T�y Ninh
Result: Danh s�ch tour th?a m�n ?i?u ki?n v?i th�ng tin ??y ?? v� slot kh? d?ng
```

### 2. User t�m tour theo t�n

```
Scenario: User mu?n t�m tour c� ch? "n�i b� ?en" trong t�n
Request: GET /api/UserTourSearch/search?textSearch=n�i b� ?en
Result: Danh s�ch tour c� title ch?a "n�i b� ?en"
```

### 3. User xem c�c t�y ch?n filter

```
Scenario: User mu?n bi?t c� nh?ng ?i?m ??n n�o v� th�ng n�o c� tour
Request: GET /api/UserTourSearch/filters
Result: Danh s�ch ??y ?? c�c t�y ch?n filter hi?n c� trong h? th?ng
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

1. **Database Indexing**: ??m b?o c� index tr�n:
   - `TourTemplate.ScheduleDays`
   - `TourTemplate.Month, Year`
   - `TourTemplate.EndLocation`
   - `TourDetails.Title`
   - `TourDetails.Status`

2. **Pagination**: Lu�n s? d?ng pagination ?? tr�nh load qu� nhi?u data

3. **Caching**: C� th? cache k?t qu? filter options v� �t thay ??i

---

**Last Updated**: January 15, 2025  
**API Version**: v1.0  
**Contact**: support@tayninhour.com