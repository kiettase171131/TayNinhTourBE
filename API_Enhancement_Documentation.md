# API Enhancement: TourDetails Search v?i Enhanced Filters

## T?ng quan

?ã c?p nh?t API `GET /api/TourDetails/paginated` ?? h? tr? tìm ki?m và l?c tour details v?i các tham s? m?i:

- **searchTerm**: Tìm ki?m theo title và description c?a tour
- **minPrice/maxPrice**: L?c theo giá c?a tour operation
- **scheduleDay**: L?c theo th? trong tu?n (Saturday/Sunday) t? tour template
- **startLocation**: L?c theo ?i?m b?t ??u t? tour template  
- **endLocation**: L?c theo ?i?m k?t thúc t? tour template

## API Endpoints ?ã C?p Nh?t

### GET /api/TourDetails/paginated

**Mô t?**: L?y danh sách tour details v?i phân trang và b? l?c nâng cao

#### Query Parameters

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `pageIndex` | integer | No | Ch? s? trang (0-based, default: 0) | `0` |
| `pageSize` | integer | No | Kích th??c trang (default: 10) | `10` |
| **`searchTerm`** | **string** | **No** | **?? Tìm ki?m theo title và description c?a tour** | **`"Núi Bà ?en"`** |
| **`minPrice`** | **decimal** | **No** | **?? Giá t?i thi?u c?a tour operation** | **`500000`** |
| **`maxPrice`** | **decimal** | **No** | **?? Giá t?i ?a c?a tour operation** | **`2000000`** |
| **`scheduleDay`** | **string** | **No** | **?? Th? trong tu?n (Saturday/Sunday)** | **`"Saturday"`** |
| **`startLocation`** | **string** | **No** | **?? ?i?m b?t ??u t? tour template** | **`"TP.HCM"`** |
| **`endLocation`** | **string** | **No** | **?? ?i?m k?t thúc t? tour template** | **`"Tây Ninh"`** |
| `includeInactive` | boolean | No | Bao g?m tour không active (default: false) | `false` |

#### Ví d? Request

```http
GET /api/TourDetails/paginated?searchTerm=Núi Bà ?en&minPrice=500000&maxPrice=1500000&scheduleDay=Saturday&startLocation=TP.HCM&pageSize=20
```

#### Response Example

```json
{
  "statusCode": 200,
  "message": "Tìm th?y 15 tour phù h?p v?i b? l?c: tìm ki?m: 'Núi Bà ?en', giá t?: 500,000?, giá ??n: 1,500,000?, th?: Saturday, xu?t phát: TP.HCM",
  "success": true,
  "data": [
    {
      "id": "tour-details-uuid",
      "title": "Tour Núi Bà ?en Premium",
      "description": "Tour khám phá núi Bà ?en v?i d?ch v? cao c?p",
      "status": "Public",
      "skillsRequired": "English,Vietnamese",
      "imageUrls": ["image1.jpg", "image2.jpg"],
      "createdAt": "2025-01-15T10:00:00Z",
      "tourTemplate": {
        "id": "template-uuid",
        "title": "Template Núi Bà ?en",
        "templateType": "FreeScenic",
        "scheduleDays": "Saturday",
        "scheduleDaysVietnamese": "Th? 7",
        "startLocation": "TP.HCM",
        "endLocation": "Tây Ninh",
        "month": 2,
        "year": 2025,
        "images": [
          {
            "id": "image-uuid",
            "url": "/images/template1.jpg"
          }
        ],
        "createdBy": {
          "id": "user-uuid",
          "name": "Tour Company ABC",
          "email": "company@example.com"
        }
      },
      "tourOperation": {
        "id": "operation-uuid",
        "price": 750000,
        "maxGuests": 20,
        "description": "Tour operation v?i guide chuyên nghi?p",
        "notes": "Bao g?m ?n tr?a",
        "status": "Scheduled",
        "currentBookings": 5
      },
      "availableSlots": [
        {
          "id": "slot-uuid",
          "tourDate": "2025-02-08",
          "status": "Available",
          "maxGuests": 20,
          "currentBookings": 5,
          "availableSpots": 15
        }
      ]
    }
  ],
  "totalCount": 15,
  "pageIndex": 0,
  "pageSize": 20,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

## Cách s? d?ng các Filter

### 1. Tìm ki?m theo t? khóa (searchTerm)

```http
GET /api/TourDetails/paginated?searchTerm=Núi Bà ?en
```

**Tính n?ng**: Tìm ki?m trong **c? title VÀ description** c?a tour details (case-insensitive).

**Ví d?**: 
- Tour có title: "Tour Núi Bà ?en"
- Tour có description: "Khám phá thiên nhiên hùng v?"
- `searchTerm=thiên nhiên` ? ? Tìm th?y (vì có trong description)
- `searchTerm=núi` ? ? Tìm th?y (case-insensitive)

### 2. L?c theo kho?ng giá (minPrice, maxPrice)

```http
# Giá t? 500,000?
GET /api/TourDetails/paginated?minPrice=500000

# Giá t? 500,000? ??n 1,500,000?  
GET /api/TourDetails/paginated?minPrice=500000&maxPrice=1500000

# Giá t?i ?a 1,000,000?
GET /api/TourDetails/paginated?maxPrice=1000000
```

### 3. L?c theo th? trong tu?n (scheduleDay)

```http
# Ch? tour th? 7
GET /api/TourDetails/paginated?scheduleDay=Saturday

# Ch? tour ch? nh?t
GET /api/TourDetails/paginated?scheduleDay=Sunday
```

**L?u ý**: Giá tr? h?p l? cho `scheduleDay` là `Saturday` ho?c `Sunday` (case-insensitive).

### 4. L?c theo ??a ?i?m (startLocation, endLocation)

```http
# Tour kh?i hành t? TP.HCM
GET /api/TourDetails/paginated?startLocation=TP.HCM

# Tour ??n Tây Ninh
GET /api/TourDetails/paginated?endLocation=Tây Ninh

# Tour t? TP.HCM ??n Tây Ninh
GET /api/TourDetails/paginated?startLocation=TP.HCM&endLocation=Tây Ninh
```

### 5. K?t h?p nhi?u filter

```http
GET /api/TourDetails/paginated?searchTerm=Núi&minPrice=500000&maxPrice=2000000&scheduleDay=Saturday&startLocation=TP.HCM&endLocation=Tây Ninh&pageSize=10
```

## X? lý l?i

### Invalid Schedule Day

**Request:**
```http
GET /api/TourDetails/paginated?scheduleDay=Monday
```

**Response:**
```json
{
  "statusCode": 400,
  "message": "Giá tr? th? trong tu?n không h?p l?: Monday. Giá tr? h?p l?: Saturday, Sunday",
  "success": false,
  "data": [],
  "totalCount": 0
}
```

### Invalid Price Range

N?u `minPrice > maxPrice`, API v?n ho?t ??ng nh?ng có th? tr? v? k?t qu? tr?ng.

## JavaScript/TypeScript Examples

### Fetch API

```javascript
const searchTours = async (filters) => {
  const params = new URLSearchParams();
  
  if (filters.searchTerm) params.append('searchTerm', filters.searchTerm);
  if (filters.minPrice) params.append('minPrice', filters.minPrice.toString());
  if (filters.maxPrice) params.append('maxPrice', filters.maxPrice.toString());
  if (filters.scheduleDay) params.append('scheduleDay', filters.scheduleDay);
  if (filters.startLocation) params.append('startLocation', filters.startLocation);
  if (filters.endLocation) params.append('endLocation', filters.endLocation);
  if (filters.pageIndex) params.append('pageIndex', filters.pageIndex.toString());
  if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());

  const response = await fetch(`/api/TourDetails/paginated?${params}`);
  return await response.json();
};

// S? d?ng
const tours = await searchTours({
  searchTerm: 'Núi Bà ?en',
  minPrice: 500000,
  maxPrice: 1500000,
  scheduleDay: 'Saturday',
  startLocation: 'TP.HCM',
  pageSize: 20
});
```

### Axios

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const searchTours = async (filters) => {
  const response = await api.get('/TourDetails/paginated', {
    params: filters
  });
  return response.data;
};
```

## Cách test API

### cURL Examples

```bash
# Tìm ki?m c? b?n
curl "http://localhost:5267/api/TourDetails/paginated?searchTerm=Núi%20Bà%20?en"

# Filter theo giá
curl "http://localhost:5267/api/TourDetails/paginated?minPrice=500000&maxPrice=1500000"

# Filter theo ngày và ??a ?i?m
curl "http://localhost:5267/api/TourDetails/paginated?scheduleDay=Saturday&startLocation=TP.HCM"

# K?t h?p t?t c? filters
curl "http://localhost:5267/api/TourDetails/paginated?searchTerm=Núi&minPrice=500000&maxPrice=2000000&scheduleDay=Saturday&startLocation=TP.HCM&endLocation=Tây%20Ninh&pageSize=10"
```

## Implementation Details

### Service Layer Changes

1. **ITourDetailsService.cs**: ? Xóa templateId parameter, API ??n gi?n h?n
2. **TourDetailsService.cs**: ? Implement logic filter v?i:
   - String search v?i `ToLower()` và `Contains()` trong c? title và description
   - Price range v?i `>=` và `<=` operators
   - Enum parsing cho `ScheduleDay`
   - Location search v?i `Contains()` (case-insensitive)

### Controller Layer Changes

1. **TourDetailsController.cs**: ? Xóa templateId parameter ?? API clean h?n
2. Enhanced logging ?? track các filters ???c áp d?ng

### Database Performance

- Filters ???c áp d?ng ? database level thông qua Entity Framework LINQ
- Include relationships ch? khi c?n thi?t
- Pagination ???c áp d?ng sau filtering ?? t?i ?u performance

## Parameters ?ã ???c lo?i b?

| Parameter (?ã xóa) | Lý do xóa |
|-------------------|-----------|
| `titleFilter` | Thay th? b?ng `searchTerm` m?nh m? h?n |
| `templateId` | Không c?n thi?t, API search t?ng quát |

**K?t qu?**: API ??n gi?n h?n, d? s? d?ng h?n v?i ch? các parameters c?n thi?t.

## Future Enhancements

Có th? thêm các filters sau:

1. **`templateType`**: Filter theo lo?i template (FreeScenic/PaidAttraction)
2. **`month/year`**: Filter theo tháng/n?m c?a template
3. **`guideSkills`**: Filter theo skills c?a guide
4. **`hasAvailableSlots`**: Ch? hi?n tours có slots available
5. **`sortBy`**: S?p x?p theo giá, ngày t?o, popularity, etc.

## Testing

```bash
# Test v?i t?t c? filters
dotnet test --filter "Category=TourDetailsApi" --verbosity normal
```

Ho?c test manual v?i Swagger UI t?i: `http://localhost:5267/swagger`