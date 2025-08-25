# Holiday Tour Search API Documentation

## T?ng quan

Holiday Tour Search API cung c?p endpoint ?? l?y danh sách tour holiday (tour 1 ngày/1 slot) không c?n authentication. API này ???c thi?t k? ??c bi?t cho các tour ngày l?, s? ki?n ??c bi?t ch? di?n ra trong 1 ngày duy nh?t.

**Base URL**: `https://api.tayninhour.com`  
**Version**: `v1`  
**Authentication**: Không c?n (Public API)  
**Controller**: `UserTourSearchController`

---

## ??c ?i?m Holiday Tour

Holiday Tour có nh?ng ??c ?i?m riêng bi?t:

- ? **1 ngày duy nh?t**: M?i tour ch? có 1 slot (không ph?i tour ??nh k?)
- ? **B?t k? ngày nào**: Có th? di?n ra Monday-Sunday (không b? gi?i h?n cu?i tu?n)
- ? **S? ki?n ??c bi?t**: Th??ng là tour ngày l?, festival, s? ki?n
- ? **Template riêng**: S? d?ng Holiday Template v?i validation riêng

---

## Endpoint M?i

### Get Holiday Tours

L?y danh sách tour holiday v?i pagination và filters.

```http
GET /api/UserTourSearch/holiday
```

#### Query Parameters

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `destination` | string | No | ?i?m ??n (tìm trong EndLocation) | `Tây Ninh` |
| `textSearch` | string | No | Text tìm ki?m trong title TourDetails | `t?t nguyên ?án` |
| `templateType` | string | No | Lo?i template (`FreeScenic`, `PaidAttraction`) | `FreeScenic` |
| `fromDate` | DateOnly | No | Tìm t? ngày (YYYY-MM-DD) | `2025-04-30` |
| `toDate` | DateOnly | No | Tìm ??n ngày (YYYY-MM-DD) | `2025-05-03` |
| `pageIndex` | integer | No | Trang hi?n t?i (default: 1) | `1` |
| `pageSize` | integer | No | S? items per page (default: 10, max: 50) | `20` |

#### Validation Rules

- `fromDate` không ???c l?n h?n `toDate`
- `pageIndex`: T?i thi?u 1
- `pageSize`: T? 1 ??n 50
- `templateType`: Ch? ch?p nh?n `FreeScenic` ho?c `PaidAttraction`

#### Response

**Status Code**: `200 OK`

```json
{
  "statusCode": 200,
  "message": "Tìm th?y 8 holiday tour phù h?p",
  "data": {
    "tours": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Tour Núi Bà ?en - Ngày L? 30/4",
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
          "title": "Holiday Template Núi Bà ?en",
          "templateType": "FreeScenic",
          "scheduleDays": "Wednesday",
          "scheduleDaysVietnamese": "Th? t?",
          "startLocation": "TP.HCM",
          "endLocation": "Tây Ninh",
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
            "name": "Công ty Du l?ch ABC",
            "email": "contact@abc.com"
          }
        },
        "tourOperation": {
          "id": "operation-id",
          "price": 750000,
          "maxGuests": 30,
          "description": "V?n hành tour v?i guide chuyên nghi?p cho ngày l?",
          "notes": "C?n mang giày th? thao, n??c u?ng",
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
      "destination": "Tây Ninh",
      "textSearch": "l?",
      "templateType": "FreeScenic",
      "fromDate": "2025-04-30",
      "toDate": "2025-05-03"
    },
    "holidayTourInfo": {
      "description": "Holiday tours are single-day tours with exactly 1 slot",
      "characteristics": [
        "M?i tour ch? có 1 ngày (1 slot)",
        "Th??ng là tour ngày l? ??c bi?t",
        "Có th? di?n ra b?t k? ngày nào trong tu?n",
        "Không theo l?ch trình ??nh k? nh? tour th??ng"
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
  "message": "T? ngày không th? l?n h?n ??n ngày"
}
```

---

## So sánh v?i Regular Tours

| Aspect | Regular Tours | Holiday Tours |
|--------|---------------|---------------|
| **S? slots** | 4+ slots/tháng theo l?ch | **1 slot duy nh?t** |
| **Ngày di?n ra** | Saturday/Sunday | **B?t k? ngày nào** |
| **T?n su?t** | ??nh k? hàng tu?n | **S? ki?n ??c bi?t** |
| **Template** | Regular Template | **Holiday Template** |
| **API endpoint** | `/search` | **`/holiday`** |
| **Filter chính** | scheduleDay, month, year | **destination, date range** |
| **Use case** | Tour cu?i tu?n | **Tour ngày l?, festival** |

---

## Endpoint Paginated (B? sung)

### Get Tours with Pagination

L?y danh sách t?t c? tours v?i pagination (t??ng t? search nh?ng không b?t bu?c filter).

```http
GET /api/UserTourSearch/paginated
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

#### Response Format

Response format gi?ng h?t `/search` endpoint nh?ng không yêu c?u filter nào c?.

---

## Business Logic

### Holiday Tour Detection

```sql
-- Query logic ?? tìm Holiday Tours
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
  ) = 1  -- Ch? có 1 slot = Holiday Tour
```

### Filtering Logic

1. **Template Type Filter**: L?c theo lo?i template (FreeScenic/PaidAttraction)
2. **Destination Filter**: Tìm ki?m trong `EndLocation` c?a TourTemplate
3. **Text Search**: Tìm ki?m trong `Title` c?a TourDetails
4. **Date Range Filter**: So sánh v?i `TourDate` c?a slot duy nh?t
5. **Only Public Tours**: Ch? hi?n th? tour có status = Public
6. **Only Active**: Ch? hi?n th? tour và template ?ang active
7. **Available Slots Only**: Slot ph?i có ngày >= hôm nay và còn ch? tr?ng

### Sorting

- Tours ???c s?p x?p theo `CreatedAt` gi?m d?n (m?i nh?t tr??c)
- Available slots ???c s?p x?p theo `TourDate` t?ng d?n

---

## Use Cases

### 1. Tìm tour ngày l? 30/4

```bash
GET /api/UserTourSearch/holiday?fromDate=2025-04-30&toDate=2025-04-30
```

### 2. Tìm tour mi?n phí trong tháng 5

```bash
GET /api/UserTourSearch/holiday?templateType=FreeScenic&fromDate=2025-05-01&toDate=2025-05-31
```

### 3. Tìm tour ??n Tây Ninh

```bash
GET /api/UserTourSearch/holiday?destination=Tây Ninh
```

### 4. Tìm tour có t? "t?t" trong tên

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
curl -X GET "https://api.tayninhour.com/api/UserTourSearch/holiday?destination=Tây%20Ninh&templateType=FreeScenic&pageSize=20" \
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
        const slot = tour.availableSlots[0]; // Holiday tour ch? có 1 slot
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
searchHolidayToursByDestination('Tây Ninh');
```

---

## Performance Considerations

1. **Database Indexing**: ??m b?o có index trên:
   - `TourTemplate.TemplateType`
   - `TourTemplate.EndLocation`
   - `TourDetails.Title`
   - `TourDetails.Status`
   - `TourSlot.TourDate`

2. **Query Optimization**: 
   - S? d?ng `Count()` ?? identify Holiday Tours (1 slot)
   - Include related entities ?? tránh N+1 queries

3. **Pagination**: Luôn s? d?ng pagination ?? tránh load quá nhi?u data

---

## API Endpoints Summary

| Endpoint | Method | Description | Use Case |
|----------|--------|-------------|----------|
| `/search` | GET | Tìm regular tours v?i filters | Tour cu?i tu?n ??nh k? |
| **`/holiday`** | **GET** | **Tìm holiday tours (1 slot)** | **Tour ngày l? ??c bi?t** |
| `/paginated` | GET | L?y t?t c? tours v?i pagination | Browse t?ng quan |
| `/filters` | GET | L?y filter options | UI dropdown |

---

**Last Updated**: January 15, 2025  
**API Version**: v1.0  
**Contact**: support@tayninhour.com