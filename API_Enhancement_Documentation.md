# API Enhancement: TourDetails Search v?i Enhanced Filters

## T?ng quan

?� c?p nh?t API `GET /api/TourDetails/paginated` ?? h? tr? t�m ki?m v� l?c tour details v?i c�c tham s? m?i:

- **searchTerm**: T�m ki?m theo title v� description c?a tour
- **minPrice/maxPrice**: L?c theo gi� c?a tour operation
- **scheduleDay**: L?c theo th? trong tu?n (Saturday/Sunday) t? tour template
- **startLocation**: L?c theo ?i?m b?t ??u t? tour template  
- **endLocation**: L?c theo ?i?m k?t th�c t? tour template

## API Endpoints ?� C?p Nh?t

### GET /api/TourDetails/paginated

**M� t?**: L?y danh s�ch tour details v?i ph�n trang v� b? l?c n�ng cao

#### Query Parameters

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `pageIndex` | integer | No | Ch? s? trang (0-based, default: 0) | `0` |
| `pageSize` | integer | No | K�ch th??c trang (default: 10) | `10` |
| **`searchTerm`** | **string** | **No** | **?? T�m ki?m theo title v� description c?a tour** | **`"N�i B� ?en"`** |
| **`minPrice`** | **decimal** | **No** | **?? Gi� t?i thi?u c?a tour operation** | **`500000`** |
| **`maxPrice`** | **decimal** | **No** | **?? Gi� t?i ?a c?a tour operation** | **`2000000`** |
| **`scheduleDay`** | **string** | **No** | **?? Th? trong tu?n (Saturday/Sunday)** | **`"Saturday"`** |
| **`startLocation`** | **string** | **No** | **?? ?i?m b?t ??u t? tour template** | **`"TP.HCM"`** |
| **`endLocation`** | **string** | **No** | **?? ?i?m k?t th�c t? tour template** | **`"T�y Ninh"`** |
| `includeInactive` | boolean | No | Bao g?m tour kh�ng active (default: false) | `false` |

#### V� d? Request

```http
GET /api/TourDetails/paginated?searchTerm=N�i B� ?en&minPrice=500000&maxPrice=1500000&scheduleDay=Saturday&startLocation=TP.HCM&pageSize=20
```

#### Response Example

```json
{
  "statusCode": 200,
  "message": "T�m th?y 15 tour ph� h?p v?i b? l?c: t�m ki?m: 'N�i B� ?en', gi� t?: 500,000?, gi� ??n: 1,500,000?, th?: Saturday, xu?t ph�t: TP.HCM",
  "success": true,
  "data": [
    {
      "id": "tour-details-uuid",
      "title": "Tour N�i B� ?en Premium",
      "description": "Tour kh�m ph� n�i B� ?en v?i d?ch v? cao c?p",
      "status": "Public",
      "skillsRequired": "English,Vietnamese",
      "imageUrls": ["image1.jpg", "image2.jpg"],
      "createdAt": "2025-01-15T10:00:00Z",
      "tourTemplate": {
        "id": "template-uuid",
        "title": "Template N�i B� ?en",
        "templateType": "FreeScenic",
        "scheduleDays": "Saturday",
        "scheduleDaysVietnamese": "Th? 7",
        "startLocation": "TP.HCM",
        "endLocation": "T�y Ninh",
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
        "description": "Tour operation v?i guide chuy�n nghi?p",
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

## C�ch s? d?ng c�c Filter

### 1. T�m ki?m theo t? kh�a (searchTerm)

```http
GET /api/TourDetails/paginated?searchTerm=N�i B� ?en
```

**T�nh n?ng**: T�m ki?m trong **c? title V� description** c?a tour details (case-insensitive).

**V� d?**: 
- Tour c� title: "Tour N�i B� ?en"
- Tour c� description: "Kh�m ph� thi�n nhi�n h�ng v?"
- `searchTerm=thi�n nhi�n` ? ? T�m th?y (v� c� trong description)
- `searchTerm=n�i` ? ? T�m th?y (case-insensitive)

### 2. L?c theo kho?ng gi� (minPrice, maxPrice)

```http
# Gi� t? 500,000?
GET /api/TourDetails/paginated?minPrice=500000

# Gi� t? 500,000? ??n 1,500,000?  
GET /api/TourDetails/paginated?minPrice=500000&maxPrice=1500000

# Gi� t?i ?a 1,000,000?
GET /api/TourDetails/paginated?maxPrice=1000000
```

### 3. L?c theo th? trong tu?n (scheduleDay)

```http
# Ch? tour th? 7
GET /api/TourDetails/paginated?scheduleDay=Saturday

# Ch? tour ch? nh?t
GET /api/TourDetails/paginated?scheduleDay=Sunday
```

**L?u �**: Gi� tr? h?p l? cho `scheduleDay` l� `Saturday` ho?c `Sunday` (case-insensitive).

### 4. L?c theo ??a ?i?m (startLocation, endLocation)

```http
# Tour kh?i h�nh t? TP.HCM
GET /api/TourDetails/paginated?startLocation=TP.HCM

# Tour ??n T�y Ninh
GET /api/TourDetails/paginated?endLocation=T�y Ninh

# Tour t? TP.HCM ??n T�y Ninh
GET /api/TourDetails/paginated?startLocation=TP.HCM&endLocation=T�y Ninh
```

### 5. K?t h?p nhi?u filter

```http
GET /api/TourDetails/paginated?searchTerm=N�i&minPrice=500000&maxPrice=2000000&scheduleDay=Saturday&startLocation=TP.HCM&endLocation=T�y Ninh&pageSize=10
```

## X? l� l?i

### Invalid Schedule Day

**Request:**
```http
GET /api/TourDetails/paginated?scheduleDay=Monday
```

**Response:**
```json
{
  "statusCode": 400,
  "message": "Gi� tr? th? trong tu?n kh�ng h?p l?: Monday. Gi� tr? h?p l?: Saturday, Sunday",
  "success": false,
  "data": [],
  "totalCount": 0
}
```

### Invalid Price Range

N?u `minPrice > maxPrice`, API v?n ho?t ??ng nh?ng c� th? tr? v? k?t qu? tr?ng.

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
  searchTerm: 'N�i B� ?en',
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

## C�ch test API

### cURL Examples

```bash
# T�m ki?m c? b?n
curl "http://localhost:5267/api/TourDetails/paginated?searchTerm=N�i%20B�%20?en"

# Filter theo gi�
curl "http://localhost:5267/api/TourDetails/paginated?minPrice=500000&maxPrice=1500000"

# Filter theo ng�y v� ??a ?i?m
curl "http://localhost:5267/api/TourDetails/paginated?scheduleDay=Saturday&startLocation=TP.HCM"

# K?t h?p t?t c? filters
curl "http://localhost:5267/api/TourDetails/paginated?searchTerm=N�i&minPrice=500000&maxPrice=2000000&scheduleDay=Saturday&startLocation=TP.HCM&endLocation=T�y%20Ninh&pageSize=10"
```

## Implementation Details

### Service Layer Changes

1. **ITourDetailsService.cs**: ? X�a templateId parameter, API ??n gi?n h?n
2. **TourDetailsService.cs**: ? Implement logic filter v?i:
   - String search v?i `ToLower()` v� `Contains()` trong c? title v� description
   - Price range v?i `>=` v� `<=` operators
   - Enum parsing cho `ScheduleDay`
   - Location search v?i `Contains()` (case-insensitive)

### Controller Layer Changes

1. **TourDetailsController.cs**: ? X�a templateId parameter ?? API clean h?n
2. Enhanced logging ?? track c�c filters ???c �p d?ng

### Database Performance

- Filters ???c �p d?ng ? database level th�ng qua Entity Framework LINQ
- Include relationships ch? khi c?n thi?t
- Pagination ???c �p d?ng sau filtering ?? t?i ?u performance

## Parameters ?� ???c lo?i b?

| Parameter (?� x�a) | L� do x�a |
|-------------------|-----------|
| `titleFilter` | Thay th? b?ng `searchTerm` m?nh m? h?n |
| `templateId` | Kh�ng c?n thi?t, API search t?ng qu�t |

**K?t qu?**: API ??n gi?n h?n, d? s? d?ng h?n v?i ch? c�c parameters c?n thi?t.

## Future Enhancements

C� th? th�m c�c filters sau:

1. **`templateType`**: Filter theo lo?i template (FreeScenic/PaidAttraction)
2. **`month/year`**: Filter theo th�ng/n?m c?a template
3. **`guideSkills`**: Filter theo skills c?a guide
4. **`hasAvailableSlots`**: Ch? hi?n tours c� slots available
5. **`sortBy`**: S?p x?p theo gi�, ng�y t?o, popularity, etc.

## Testing

```bash
# Test v?i t?t c? filters
dotnet test --filter "Category=TourDetailsApi" --verbosity normal
```

Ho?c test manual v?i Swagger UI t?i: `http://localhost:5267/swagger`