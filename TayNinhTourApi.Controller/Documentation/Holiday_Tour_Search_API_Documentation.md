# Holiday Tour Search API Documentation

## T?ng quan

Holiday Tour Search API cung c?p endpoint ?? l?y danh s�ch tour holiday (tour 1 ng�y/1 slot) kh�ng c?n authentication. API n�y ???c thi?t k? ??c bi?t cho c�c tour ng�y l?, s? ki?n ??c bi?t ch? di?n ra trong 1 ng�y duy nh?t.

**Base URL**: `https://api.tayninhour.com`  
**Version**: `v1`  
**Authentication**: Kh�ng c?n (Public API)  
**Controller**: `UserTourSearchController`

---

## ??c ?i?m Holiday Tour

Holiday Tour c� nh?ng ??c ?i?m ri�ng bi?t:

- ? **1 ng�y duy nh?t**: M?i tour ch? c� 1 slot (kh�ng ph?i tour ??nh k?)
- ? **B?t k? ng�y n�o**: C� th? di?n ra Monday-Sunday (kh�ng b? gi?i h?n cu?i tu?n)
- ? **S? ki?n ??c bi?t**: Th??ng l� tour ng�y l?, festival, s? ki?n
- ? **Template ri�ng**: S? d?ng Holiday Template v?i validation ri�ng

---

## Endpoint M?i

### Get Holiday Tours

L?y danh s�ch tour holiday v?i pagination v� filters.

```http
GET /api/UserTourSearch/holiday
```

#### Query Parameters

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `destination` | string | No | ?i?m ??n (t�m trong EndLocation) | `T�y Ninh` |
| `textSearch` | string | No | Text t�m ki?m trong title TourDetails | `t?t nguy�n ?�n` |
| `templateType` | string | No | Lo?i template (`FreeScenic`, `PaidAttraction`) | `FreeScenic` |
| `fromDate` | DateOnly | No | T�m t? ng�y (YYYY-MM-DD) | `2025-04-30` |
| `toDate` | DateOnly | No | T�m ??n ng�y (YYYY-MM-DD) | `2025-05-03` |
| `pageIndex` | integer | No | Trang hi?n t?i (default: 1) | `1` |
| `pageSize` | integer | No | S? items per page (default: 10, max: 50) | `20` |

#### Validation Rules

- `fromDate` kh�ng ???c l?n h?n `toDate`
- `pageIndex`: T?i thi?u 1
- `pageSize`: T? 1 ??n 50
- `templateType`: Ch? ch?p nh?n `FreeScenic` ho?c `PaidAttraction`

#### Response

**Status Code**: `200 OK`

```json
{
  "statusCode": 200,
  "message": "T�m th?y 8 holiday tour ph� h?p",
  "data": {
    "tours": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Tour N�i B� ?en - Ng�y L? 30/4",
        "description": "Tour ??c bi?t d?p l? 30/4 v?i nhi?u ho?t ??ng h?p d?n",
        "status": "Public",
        "skillsRequired": "TheoDoiKhachHang,GiaiThichLichSu",
        "imageUrls": [
          "/images/holiday-tour1.jpg",
          "/images/holiday-tour2.jpg"
        ],
        "createdAt": "2025-01-15T10:00:00Z",
        "tourTemplate": {
          "id": "template-id",
          "title": "Holiday Template N�i B� ?en",
          "templateType": "FreeScenic",
          "scheduleDays": "Wednesday",
          "scheduleDaysVietnamese": "Th? t?",
          "startLocation": "TP.HCM",
          "endLocation": "T�y Ninh",
          "month": 4,
          "year": 2025,
          "images": [
            {
              "id": "image-id",
              "url": "/images/holiday-template.jpg"
            }
          ],
          "createdBy": {
            "id": "user-id",
            "name": "C�ng ty Du l?ch ABC",
            "email": "contact@abc.com"
          }
        },
        "tourOperation": {
          "id": "operation-id",
          "price": 750000,
          "maxGuests": 30,
          "description": "V?n h�nh tour v?i guide chuy�n nghi?p cho ng�y l?",
          "notes": "C?n mang gi�y th? thao, n??c u?ng",
          "status": "Scheduled",
          "currentBookings": 8
        },
        "availableSlots": [
          {
            "id": "slot-id-1",
            "tourDate": "2025-04-30",
            "status": "Available",
            "maxGuests": 30,
            "currentBookings": 8,
            "availableSpots": 22
          }
        ]
      }
    ],
    "pagination": {
      "totalCount": 8,
      "pageIndex": 1,
      "pageSize": 10,
      "totalPages": 1,
      "hasNextPage": false,
      "hasPreviousPage": false
    },
    "searchCriteria": {
      "destination": "T�y Ninh",
      "textSearch": "l?",
      "templateType": "FreeScenic",
      "fromDate": "2025-04-30",
      "toDate": "2025-05-03"
    },
    "holidayTourInfo": {
      "description": "Holiday tours are single-day tours with exactly 1 slot",
      "characteristics": [
        "M?i tour ch? c� 1 ng�y (1 slot)",
        "Th??ng l� tour ng�y l? ??c bi?t",
        "C� th? di?n ra b?t k? ng�y n�o trong tu?n",
        "Kh�ng theo l?ch tr�nh ??nh k? nh? tour th??ng"
      ]
    }
  }
}
```

#### Error Responses

**Status Code**: `400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "T? ng�y kh�ng th? l?n h?n ??n ng�y"
}
```

---

## So s�nh v?i Regular Tours

| Aspect | Regular Tours | Holiday Tours |
|--------|---------------|---------------|
| **S? slots** | 4+ slots/th�ng theo l?ch | **1 slot duy nh?t** |
| **Ng�y di?n ra** | Saturday/Sunday | **B?t k? ng�y n�o** |
| **T?n su?t** | ??nh k? h�ng tu?n | **S? ki?n ??c bi?t** |
| **Template** | Regular Template | **Holiday Template** |
| **API endpoint** | `/search` | **`/holiday`** |
| **Filter ch�nh** | scheduleDay, month, year | **destination, date range** |
| **Use case** | Tour cu?i tu?n | **Tour ng�y l?, festival** |

---

## Endpoint Paginated (B? sung)

### Get Tours with Pagination

L?y danh s�ch t?t c? tours v?i pagination (t??ng t? search nh?ng kh�ng b?t bu?c filter).

```http
GET /api/UserTourSearch/paginated
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

#### Response Format

Response format gi?ng h?t `/search` endpoint nh?ng kh�ng y�u c?u filter n�o c?.

---

## Business Logic

### Holiday Tour Detection

```sql
-- Query logic ?? t�m Holiday Tours
SELECT td.* 
FROM TourDetails td
JOIN TourTemplate tt ON td.TourTemplateId = tt.Id
WHERE td.Status = 'Public' 
  AND td.IsActive = true
  AND tt.IsActive = true
  AND (
    SELECT COUNT(*) 
    FROM TourSlot ts 
    WHERE ts.TourDetailsId = td.Id 
      AND ts.IsActive = true 
      AND ts.IsDeleted = false
  ) = 1  -- Ch? c� 1 slot = Holiday Tour
```

### Filtering Logic

1. **Template Type Filter**: L?c theo lo?i template (FreeScenic/PaidAttraction)
2. **Destination Filter**: T�m ki?m trong `EndLocation` c?a TourTemplate
3. **Text Search**: T�m ki?m trong `Title` c?a TourDetails
4. **Date Range Filter**: So s�nh v?i `TourDate` c?a slot duy nh?t
5. **Only Public Tours**: Ch? hi?n th? tour c� status = Public
6. **Only Active**: Ch? hi?n th? tour v� template ?ang active
7. **Available Slots Only**: Slot ph?i c� ng�y >= h�m nay v� c�n ch? tr?ng

### Sorting

- Tours ???c s?p x?p theo `CreatedAt` gi?m d?n (m?i nh?t tr??c)
- Available slots ???c s?p x?p theo `TourDate` t?ng d?n

---

## Use Cases

### 1. T�m tour ng�y l? 30/4

```bash
GET /api/UserTourSearch/holiday?fromDate=2025-04-30&toDate=2025-04-30
```

### 2. T�m tour mi?n ph� trong th�ng 5

```bash
GET /api/UserTourSearch/holiday?templateType=FreeScenic&fromDate=2025-05-01&toDate=2025-05-31
```

### 3. T�m tour ??n T�y Ninh

```bash
GET /api/UserTourSearch/holiday?destination=T�y Ninh
```

### 4. T�m tour c� t? "t?t" trong t�n

```bash
GET /api/UserTourSearch/holiday?textSearch=t?t
```

---

## Examples

### cURL Examples

#### Get Holiday Tours - Basic
```bash
curl -X GET "https://api.tayninhour.com/api/UserTourSearch/holiday" \
  -H "Content-Type: application/json"
```

#### Get Holiday Tours - With Date Range
```bash
curl -X GET "https://api.tayninhour.com/api/UserTourSearch/holiday?fromDate=2025-04-30&toDate=2025-05-03" \
  -H "Content-Type: application/json"
```

#### Get Holiday Tours - With Filters
```bash
curl -X GET "https://api.tayninhour.com/api/UserTourSearch/holiday?destination=T�y%20Ninh&templateType=FreeScenic&pageSize=20" \
  -H "Content-Type: application/json"
```

#### Get Paginated Tours
```bash
curl -X GET "https://api.tayninhour.com/api/UserTourSearch/paginated?pageIndex=2&pageSize=15" \
  -H "Content-Type: application/json"
```

### JavaScript Examples

#### Fetch Holiday Tours for Date Range
```javascript
const getHolidayTours = async (fromDate, toDate) => {
  try {
    const response = await fetch(`/api/UserTourSearch/holiday?fromDate=${fromDate}&toDate=${toDate}`);
    const data = await response.json();
    
    if (data.statusCode === 200) {
      console.log(`Found ${data.data.pagination.totalCount} holiday tours`);
      data.data.tours.forEach(tour => {
        const slot = tour.availableSlots[0]; // Holiday tour ch? c� 1 slot
        console.log(`${tour.title} - ${slot?.tourDate} - ${tour.tourOperation?.price.toLocaleString()} VND`);
      });
    }
  } catch (error) {
    console.error('Error getting holiday tours:', error);
  }
};

// Usage
getHolidayTours('2025-04-30', '2025-05-03');
```

#### Fetch Holiday Tours by Destination
```javascript
const searchHolidayToursByDestination = async (destination) => {
  try {
    const response = await fetch(`/api/UserTourSearch/holiday?destination=${encodeURIComponent(destination)}&pageSize=20`);
    const data = await response.json();
    
    if (data.statusCode === 200) {
      console.log(`Found ${data.data.pagination.totalCount} holiday tours to ${destination}`);
      return data.data.tours;
    }
  } catch (error) {
    console.error('Error searching holiday tours:', error);
    return [];
  }
};

// Usage
searchHolidayToursByDestination('T�y Ninh');
```

---

## Performance Considerations

1. **Database Indexing**: ??m b?o c� index tr�n:
   - `TourTemplate.TemplateType`
   - `TourTemplate.EndLocation`
   - `TourDetails.Title`
   - `TourDetails.Status`
   - `TourSlot.TourDate`

2. **Query Optimization**: 
   - S? d?ng `Count()` ?? identify Holiday Tours (1 slot)
   - Include related entities ?? tr�nh N+1 queries

3. **Pagination**: Lu�n s? d?ng pagination ?? tr�nh load qu� nhi?u data

---

## API Endpoints Summary

| Endpoint | Method | Description | Use Case |
|----------|--------|-------------|----------|
| `/search` | GET | T�m regular tours v?i filters | Tour cu?i tu?n ??nh k? |
| **`/holiday`** | **GET** | **T�m holiday tours (1 slot)** | **Tour ng�y l? ??c bi?t** |
| `/paginated` | GET | L?y t?t c? tours v?i pagination | Browse t?ng quan |
| `/filters` | GET | L?y filter options | UI dropdown |

---

**Last Updated**: January 15, 2025  
**API Version**: v1.0  
**Contact**: support@tayninhour.com