# Tour Guide API Documentation

## Get Tour Guide Details by ID

### Endpoint
```
GET /api/Account/guides/{id}
```

### Description
L?y chi ti?t th�ng tin h??ng d?n vi�n theo ID ?? xem ???c detail c?a tour guide ?�.

### Authorization
**Required Roles:** Admin, Tour Company

### Parameters
- `id` (path parameter, required): GUID - ID c?a h??ng d?n vi�n

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
  "experience": "5 n?m kinh nghi?m d?n tour trong khu v?c T�y Ninh...",
  "skills": "Ti?ng Anh,L?ch s?,V?n h�a ??a ph??ng",
  "rating": 4.5,
  "totalToursGuided": 150,
  "isAvailable": true,
  "isActive": true,
  "notes": "H??ng d?n vi�n gi�u kinh nghi?m",
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
  "message": "Kh�ng t�m th?y h??ng d?n vi�n v?i ID ?� cho"
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
  "message": "L?i h? th?ng khi l?y th�ng tin chi ti?t h??ng d?n vi�n"
}
```

### Response Fields Description

#### Main Fields
- `id`: ID c?a h??ng d?n vi�n (TourGuide entity)
- `userId`: ID c?a User account li�n k?t
- `applicationId`: ID c?a ??n ??ng k� g?c
- `fullName`: H? t�n ??y ??
- `email`: Email li�n h?
- `phoneNumber`: S? ?i?n tho?i
- `experience`: M� t? kinh nghi?m chi ti?t
- `skills`: K? n?ng/chuy�n m�n (chu?i ph�n t�ch b?i d?u ph?y)
- `rating`: Rating trung b�nh t? kh�ch h�ng (0-5)
- `totalToursGuided`: T?ng s? tour ?� d?n
- `isAvailable`: C� available cho assignment kh�ng
- `isActive`: Tr?ng th�i ho?t ??ng
- `notes`: Ghi ch� b? sung
- `profileImageUrl`: URL ?nh ??i di?n
- `approvedAt`: Ng�y ???c duy?t
- `approvedById`: ID c?a admin ?� duy?t
- `approvedByName`: T�n admin ?� duy?t
- `createdAt`: Ng�y t?o record
- `updatedAt`: Ng�y c?p nh?t g?n nh?t

#### Statistics Object
- `activeInvitations`: S? l?i m?i ?ang ch? x? l�
- `completedTours`: S? tour ?� ho�n th�nh
- `lastTourDate`: Ng�y tour g?n nh?t

#### UserInfo Object
- `userName`: T�n ??ng nh?p (email)
- `displayName`: T�n hi?n th?
- `avatar`: URL avatar
- `joinedDate`: Ng�y tham gia h? th?ng
- `isUserActive`: Tr?ng th�i User account

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
  // Guide c� th? ???c assign tour m?i
  console.log(`Guide ${guideDetails.fullName} is available for new tours`);
}
```

### Related APIs
- `GET /api/Account/guides` - L?y danh s�ch t?t c? h??ng d?n vi�n
- `GET /api/Account/guides/available` - L?y danh s�ch h??ng d?n vi�n available cho ng�y c? th?