# TourTemplate API Documentation

## Overview

TourTemplate API cung cấp các endpoints để quản lý tour templates trong hệ thống TayNinhTour. API này chỉ dành cho users có role `TOURCOMPANY`.

**Base URL**: `https://api.tayninhour.com`  
**Version**: `v1`  
**Authentication**: Bearer Token (JWT)  
**Required Role**: `TOURCOMPANY`

---

## Authentication

Tất cả endpoints yêu cầu JWT token trong header:

```http
Authorization: Bearer {your-jwt-token}
```

---

## Endpoints

### 1. Get Tour Templates

Lấy danh sách tour templates với pagination và filters.

```http
GET /api/TourCompany/template
```

#### Query Parameters

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `pageIndex` | integer | No | Trang hiện tại (default: 1) | `1` |
| `pageSize` | integer | No | Số items per page (default: 10, max: 100) | `20` |
| `templateType` | string | No | Loại template (`FreeScenic`, `PaidAttraction`) | `FreeScenic` |
| `startLocation` | string | No | Điểm khởi hành | `TP.HCM` |
| `includeInactive` | boolean | No | Bao gồm templates không active (default: false) | `true` |

**⚠️ Đã xóa**: `minPrice`, `maxPrice`, `endLocation`, `searchKeyword` filters (Price field đã bị xóa khỏi TourTemplate)

#### Response

**Status Code**: `200 OK`

```json
{
  "statusCode": 200,
  "message": "Lấy danh sách tour templates thành công",
  "data": {
    "templates": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Tour Núi Bà Đen",
        "description": "Tour khám phá núi Bà Đen với cảnh đẹp thiên nhiên",
        "price": 0,
        "childPrice": null,
        "childMaxAge": null,
        "maxGuests": 20,
        "minGuests": 5,
        "duration": 1,
        "templateType": "FreeScenic",
        "scheduleDays": "Saturday",
        "startLocation": "TP.HCM",
        "endLocation": "Tây Ninh",
        "specialRequirements": "Mang giày thể thao",
        "isActive": true,
        "createdAt": "2025-06-03T10:00:00Z",
        "updatedAt": "2025-06-03T10:00:00Z",
        "createdBy": {
          "id": "user-id",
          "fullName": "Nguyễn Văn A"
        },
        "images": [
          {
            "id": "image-id",
            "fileName": "nui-ba-den.jpg",
            "url": "/images/nui-ba-den.jpg"
          }
        ]
      }
    ],
    "pagination": {
      "totalCount": 1,
      "pageIndex": 1,
      "pageSize": 10,
      "totalPages": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  }
}
```

#### Error Responses

**Status Code**: `401 Unauthorized`
```json
{
  "statusCode": 401,
  "message": "Token không hợp lệ hoặc đã hết hạn"
}
```

**Status Code**: `403 Forbidden`
```json
{
  "statusCode": 403,
  "message": "Bạn không có quyền truy cập endpoint này"
}
```

---

### 2. Get Tour Template by ID

Lấy thông tin chi tiết của một tour template.

```http
GET /api/TourCompany/template/{id}
```

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string (UUID) | Yes | ID của tour template |

#### Response

**Status Code**: `200 OK`

```json
{
  "statusCode": 200,
  "message": "Lấy thông tin tour template thành công",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Tour Núi Bà Đen",
    "description": "Tour khám phá núi Bà Đen với cảnh đẹp thiên nhiên",
    "price": 0,
    "childPrice": null,
    "childMaxAge": null,
    "maxGuests": 20,
    "minGuests": 5,
    "duration": 1,
    "templateType": "FreeScenic",
    "scheduleDays": "Saturday",
    "startLocation": "TP.HCM",
    "endLocation": "Tây Ninh",
    "specialRequirements": "Mang giày thể thao",
    "isActive": true,
    "createdAt": "2025-06-03T10:00:00Z",
    "updatedAt": "2025-06-03T10:00:00Z",
    "createdBy": {
      "id": "user-id",
      "fullName": "Nguyễn Văn A"
    },
    "images": [
      {
        "id": "image-id",
        "fileName": "nui-ba-den.jpg",
        "url": "/images/nui-ba-den.jpg"
      }
    ],
    "tourDetails": [
      {
        "id": "detail-id",
        "timeSlot": "08:00",
        "location": "Núi Bà Đen",
        "description": "Khởi hành từ TP.HCM",
        "orderIndex": 1,
        "shop": null
      }
    ]
  }
}
```

#### Error Responses

**Status Code**: `404 Not Found`
```json
{
  "statusCode": 404,
  "message": "Không tìm thấy tour template"
}
```

---

### 3. Create Tour Template

Tạo tour template mới.

```http
POST /api/TourCompany/template
```

#### Request Body (Đã đơn giản hóa)

```json
{
  "title": "Tour Núi Bà Đen",
  "description": "Tour khám phá núi Bà Đen với cảnh đẹp thiên nhiên",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "month": 6,
  "year": 2025,
  "images": ["image1.jpg", "image2.jpg"]
}
```

**⚠️ Đã xóa**: `price`, `childPrice`, `childMaxAge`, `maxGuests`, `minGuests`, `duration`, `specialRequirements` (Các thông tin chi tiết sẽ được quản lý ở TourDetails level)

#### Request Body Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `title` | string | Yes | 1-200 chars | Tên tour template |
| `description` | string | No | max 1000 chars | Mô tả chi tiết |
| `templateType` | string | Yes | `FreeScenic`, `PaidAttraction` | Loại tour template |
| `scheduleDays` | string | Yes | `Saturday`, `Sunday` | Ngày trong tuần (chỉ 1 ngày) |
| `startLocation` | string | Yes | 1-500 chars | Điểm bắt đầu |
| `endLocation` | string | Yes | 1-500 chars | Điểm kết thúc |
| `month` | integer | Yes | 1-12 | Tháng áp dụng |
| `year` | integer | Yes | 2024-2030 | Năm áp dụng |
| `images` | array[string] | No | max 10 items | Danh sách URL ảnh |

**✨ Tính năng mới**: Tự động generate 4 slots cho tháng đã chọn sau khi tạo template thành công

#### Response

**Status Code**: `201 Created`

```json
{
  "statusCode": 201,
  "message": "Tạo tour template thành công",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Tour Núi Bà Đen",
    "templateType": "FreeScenic",
    "scheduleDays": "Saturday",
    "createdAt": "2025-06-03T10:00:00Z"
  }
}
```

#### Error Responses

**Status Code**: `400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "Dữ liệu không hợp lệ",
  "validationErrors": [
    "Chỉ được chọn Saturday hoặc Sunday",
    "Giá tour không được âm"
  ],
  "fieldErrors": {
    "scheduleDays": ["Chỉ được chọn một ngày duy nhất: Saturday hoặc Sunday"],
    "price": ["Giá phải >= 0"]
  }
}
```

---

### 4. Update Tour Template

Cập nhật tour template (partial update).

```http
PATCH /api/TourCompany/template/{id}
```

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string (UUID) | Yes | ID của tour template |

#### Request Body (Đã đơn giản hóa)

```json
{
  "title": "Tour Núi Bà Đen - Cập nhật",
  "description": "Mô tả mới",
  "templateType": "PaidAttraction",
  "images": ["new_image1.jpg"]
}
```

**Note**: Chỉ cần gửi các fields muốn update. Validation rules giống như Create.

#### Response

**Status Code**: `200 OK`

```json
{
  "statusCode": 200,
  "message": "Cập nhật tour template thành công",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Tour Núi Bà Đen - Cập nhật",
    "price": 50000,
    "updatedAt": "2025-06-03T11:00:00Z"
  }
}
```

#### Error Responses

**Status Code**: `403 Forbidden`
```json
{
  "statusCode": 403,
  "message": "Bạn không có quyền cập nhật tour template này"
}
```

**Status Code**: `409 Conflict**
```json
{
  "statusCode": 409,
  "message": "Không thể cập nhật tour template có slots đang active"
}
```

---

### 5. Delete Tour Template

Xóa tour template.

```http
DELETE /api/TourCompany/template/{id}
```

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string (UUID) | Yes | ID của tour template |

#### Response

**Status Code**: `200 OK`

```json
{
  "statusCode": 200,
  "message": "Xóa tour template thành công",
  "data": {
    "canDelete": true,
    "blockingReasons": []
  }
}
```

#### Error Responses

**Status Code**: `409 Conflict**
```json
{
  "statusCode": 409,
  "message": "Không thể xóa tour template",
  "data": {
    "canDelete": false,
    "blockingReasons": [
      "Có 3 tour slots đang active",
      "Có 2 bookings đang pending"
    ]
  }
}
```

---

### 6. Copy Tour Template

Tạo bản copy của tour template.

```http
POST /api/TourCompany/template/{id}/copy
```

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string (UUID) | Yes | ID của tour template gốc |

#### Request Body

```json
{
  "newTitle": "Tour Núi Bà Đen - Copy"
}
```

#### Request Body Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `newTitle` | string | Yes | 1-200 chars | Tiêu đề cho template mới |

#### Response

**Status Code**: `201 Created`

```json
{
  "statusCode": 201,
  "message": "Copy tour template thành công",
  "data": {
    "originalId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "newId": "4fb85f64-5717-4562-b3fc-2c963f66afa7",
    "newTitle": "Tour Núi Bà Đen - Copy",
    "copiedImages": 2,
    "copiedDetails": 5,
    "createdAt": "2025-06-03T12:00:00Z"
  }
}
```

---

## Data Models

### TourTemplateType Enum

| Value | Description |
|-------|-------------|
| `FreeScenic` | Tour danh lam thắng cảnh (miễn phí vào cửa) |
| `PaidAttraction` | Tour khu vui chơi (có phí vào cửa) |

### ScheduleDay Enum

| Value | Description |
|-------|-------------|
| `Saturday` | Thứ bảy |
| `Sunday` | Chủ nhật |

**Important**: Chỉ được chọn 1 ngày duy nhất, không được chọn cả Saturday và Sunday.

---

## Error Codes

| Status Code | Description | Common Causes |
|-------------|-------------|---------------|
| `400` | Bad Request | Invalid input data, validation errors |
| `401` | Unauthorized | Missing or invalid JWT token |
| `403` | Forbidden | Insufficient permissions, not owner |
| `404` | Not Found | Tour template not found |
| `409` | Conflict | Cannot delete/update due to dependencies |
| `500` | Internal Server Error | Server error |

---

## Rate Limiting

- **Rate Limit**: 100 requests per minute per user
- **Headers**: 
  - `X-RateLimit-Limit`: Request limit
  - `X-RateLimit-Remaining`: Remaining requests
  - `X-RateLimit-Reset`: Reset time (Unix timestamp)

---

## Examples

### cURL Examples

#### Create Tour Template (Đã đơn giản hóa)
```bash
curl -X POST "https://api.tayninhour.com/api/TourCompany/template" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Tour Núi Bà Đen",
    "description": "Tour khám phá núi Bà Đen",
    "templateType": "FreeScenic",
    "scheduleDays": "Saturday",
    "startLocation": "TP.HCM",
    "endLocation": "Tây Ninh",
    "month": 6,
    "year": 2025,
    "images": ["image1.jpg", "image2.jpg"]
  }'
```

#### Get Templates with Filter (Đã cập nhật)
```bash
curl -X GET "https://api.tayninhour.com/api/TourCompany/template?templateType=FreeScenic&startLocation=TP.HCM&pageSize=20" \
  -H "Authorization: Bearer your-jwt-token"
```

---

---

## Related APIs

### TourSlot API

#### Generate Tour Slots

```http
POST /api/TourSlot/generate
```

**Request Body**:
```json
{
  "tourTemplateId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "month": 6,
  "year": 2025,
  "scheduleDays": "Saturday",
  "overwriteExisting": false,
  "autoActivate": true
}
```

**Response**:
```json
{
  "statusCode": 200,
  "message": "Tạo tour slots thành công",
  "data": {
    "isSuccess": true,
    "createdSlotsCount": 4,
    "skippedSlotsCount": 0,
    "createdSlots": [
      {
        "id": "slot-id",
        "tourDate": "2025-06-07",
        "scheduleDay": "Saturday",
        "status": "Available"
      }
    ]
  }
}
```

#### Get Tour Slots by Template

```http
GET /api/TourSlot/template/{templateId}?month=6&year=2025
```

---

### TourDetails API

#### Get Timeline

```http
GET /api/TourDetails/timeline/{templateId}?includeShopInfo=true
```

**Response**:
```json
{
  "statusCode": 200,
  "data": {
    "tourTemplateId": "template-id",
    "timelineItems": [
      {
        "id": "detail-id",
        "timeSlot": "08:00",
        "location": "Núi Bà Đen",
        "description": "Khởi hành từ TP.HCM",
        "orderIndex": 1,
        "shop": null
      },
      {
        "id": "detail-id-2",
        "timeSlot": "12:00",
        "location": "Nhà hàng ABC",
        "description": "Ăn trưa",
        "orderIndex": 2,
        "shop": {
          "id": "shop-id",
          "name": "Nhà hàng ABC",
          "location": "Tây Ninh",
          "phoneNumber": "0123456789"
        }
      }
    ]
  }
}
```

#### Add Timeline Item

```http
POST /api/TourDetails
```

**Request Body**:
```json
{
  "tourTemplateId": "template-id",
  "timeSlot": "14:00",
  "location": "Chùa Cao Đài",
  "description": "Tham quan chùa",
  "shopId": "shop-id",
  "orderIndex": 3,
  "estimatedDuration": 60
}
```

---

### Migration API (Admin Only)

#### Preview Migration

```http
GET /api/TourMigration/preview
Authorization: Bearer {admin-token}
```

#### Execute Migration

```http
POST /api/TourMigration/execute?confirmMigration=true
Authorization: Bearer {admin-token}
```

---

**Last Updated**: June 3, 2025
**API Version**: v1.0
**Contact**: support@tayninhour.com
