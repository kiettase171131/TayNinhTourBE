# Tour Guide API Documentation

## Get Tour Guide Details by ID

### Endpoint
```
GET /api/Account/guides/{id}
```

### Description
L?y chi ti?t thông tin h??ng d?n viên theo ID ?? xem ???c detail c?a tour guide ?ó.

### Authorization
**Required Roles:** Admin, Tour Company

### Parameters
- `id` (path parameter, required): GUID - ID c?a h??ng d?n viên

### Request Example
```http
GET /api/Account/guides/123e4567-e89b-12d3-a456-426614174000
Authorization: Bearer {your-jwt-token}
```

### Response Format

#### Success Response (200 OK)
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "userId": "987fcdeb-51a2-43d7-8765-123456789012",
  "applicationId": "456e7890-a12b-34c5-d678-901234567890",
  "fullName": "Nguy?n V?n A",
  "email": "nguyenvana@email.com",
  "phoneNumber": "0123456789",
  "experience": "5 n?m kinh nghi?m d?n tour trong khu v?c Tây Ninh...",
  "skills": "Ti?ng Anh,L?ch s?,V?n hóa ??a ph??ng",
  "rating": 4.5,
  "totalToursGuided": 150,
  "isAvailable": true,
  "isActive": true,
  "notes": "H??ng d?n viên giàu kinh nghi?m",
  "profileImageUrl": "https://example.com/profile.jpg",
  "approvedAt": "2024-01-15T10:30:00Z",
  "approvedById": "abc12345-def6-7890-abcd-123456789012",
  "approvedByName": "Admin User",
  "createdAt": "2024-01-15T10:00:00Z",
  "updatedAt": "2024-03-10T14:20:00Z",
  "statistics": {
    "activeInvitations": 3,
    "completedTours": 145,
    "lastTourDate": "2024-03-08T09:00:00Z"
  },
  "userInfo": {
    "userName": "nguyenvana@email.com",
    "displayName": "Nguy?n V?n A",
    "avatar": "https://example.com/avatar.jpg",
    "joinedDate": "2024-01-15T09:00:00Z",
    "isUserActive": true
  }
}
```

#### Error Response - Not Found (404)
```json
{
  "success": false,
  "statusCode": 404,
  "message": "Không tìm th?y h??ng d?n viên v?i ID ?ã cho"
}
```

#### Error Response - Unauthorized (401/403)
```json
{
  "success": false,
  "statusCode": 401,
  "message": "Unauthorized"
}
```

#### Error Response - Server Error (500)
```json
{
  "success": false,
  "statusCode": 500,
  "message": "L?i h? th?ng khi l?y thông tin chi ti?t h??ng d?n viên"
}
```

### Response Fields Description

#### Main Fields
- `id`: ID c?a h??ng d?n viên (TourGuide entity)
- `userId`: ID c?a User account liên k?t
- `applicationId`: ID c?a ??n ??ng ký g?c
- `fullName`: H? tên ??y ??
- `email`: Email liên h?
- `phoneNumber`: S? ?i?n tho?i
- `experience`: Mô t? kinh nghi?m chi ti?t
- `skills`: K? n?ng/chuyên môn (chu?i phân tách b?i d?u ph?y)
- `rating`: Rating trung bình t? khách hàng (0-5)
- `totalToursGuided`: T?ng s? tour ?ã d?n
- `isAvailable`: Có available cho assignment không
- `isActive`: Tr?ng thái ho?t ??ng
- `notes`: Ghi chú b? sung
- `profileImageUrl`: URL ?nh ??i di?n
- `approvedAt`: Ngày ???c duy?t
- `approvedById`: ID c?a admin ?ã duy?t
- `approvedByName`: Tên admin ?ã duy?t
- `createdAt`: Ngày t?o record
- `updatedAt`: Ngày c?p nh?t g?n nh?t

#### Statistics Object
- `activeInvitations`: S? l?i m?i ?ang ch? x? lý
- `completedTours`: S? tour ?ã hoàn thành
- `lastTourDate`: Ngày tour g?n nh?t

#### UserInfo Object
- `userName`: Tên ??ng nh?p (email)
- `displayName`: Tên hi?n th?
- `avatar`: URL avatar
- `joinedDate`: Ngày tham gia h? th?ng
- `isUserActive`: Tr?ng thái User account

### Usage Examples

#### L?y chi ti?t tour guide ?? hi?n th? trong admin panel:
```javascript
const guideId = "123e4567-e89b-12d3-a456-426614174000";
const response = await fetch(`/api/Account/guides/${guideId}`, {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
const guideDetails = await response.json();
```

#### Ki?m tra availability c?a guide:
```javascript
if (guideDetails.isAvailable && guideDetails.isActive) {
  // Guide có th? ???c assign tour m?i
  console.log(`Guide ${guideDetails.fullName} is available for new tours`);
}
```

### Related APIs
- `GET /api/Account/guides` - L?y danh sách t?t c? h??ng d?n viên
- `GET /api/Account/guides/available` - L?y danh sách h??ng d?n viên available cho ngày c? th?