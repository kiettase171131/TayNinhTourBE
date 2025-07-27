# ?? Admin Debug API - Test No Suitable Guides Notification

## Endpoint Information

**URL:** `/api/admin/debug/test-no-suitable-guides-notification/{tourDetailsId}`  
**Method:** `POST`  
**Authorization:** Required (Admin role only)  
**Content-Type:** `application/json`  

## Purpose

Test th�ng b�o g?i cho TourCompany khi kh�ng t�m th?y h??ng d?n vi�n c� k? n?ng ph� h?p v?i tour.

## Parameters

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `tourDetailsId` | `GUID` | ? | ID c?a TourDetails c?n test notification |

### Headers

| Header | Type | Required | Description |
|--------|------|----------|-------------|
| `Authorization` | `string` | ? | `Bearer {admin_jwt_token}` |

## Request Example

```http
POST /api/admin/debug/test-no-suitable-guides-notification/123e4567-e89b-12d3-a456-426614174000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

## Response Examples

### ? Success Response (200)

```json
{
  "statusCode": 200,
  "message": "Debug: ?� g?i th�ng b�o test th�nh c�ng",
  "success": true
}
```

### ? Error Responses

#### TourDetails Not Found (404)
```json
{
  "statusCode": 404,
  "message": "TourDetails kh�ng t?n t?i",
  "success": false
}
```

#### Unauthorized (401)
```json
{
  "statusCode": 401,
  "message": "Unauthorized"
}
```

#### Forbidden (403)
```json
{
  "statusCode": 403,
  "message": "Access denied. Admin role required."
}
```

#### Internal Server Error (500)
```json
{
  "statusCode": 500,
  "message": "C� l?i x?y ra khi test th�ng b�o debug"
}
```

## What This Endpoint Does

1. **Validates TourDetails exists** - Ki?m tra tour c� t?n t?i kh�ng
2. **Triggers notification logic** - G?i `NotifyTourCompanyAboutNoSuitableGuidesAsync`
3. **Creates in-app notification** - T?o th�ng b�o trong app
4. **Sends email notification** - G?i email chi ti?t cho TourCompany
5. **Returns success/error status** - Tr? v? k?t qu?

## Notification Content Generated

### ?? In-App Notification
- **Title:** "?? Kh�ng t�m th?y h??ng d?n vi�n ph� h?p"
- **Message:** H??ng d?n h�nh ??ng c?n thi?t
- **Priority:** `High`
- **Icon:** "??"
- **Action URL:** "/guides/list"

### ?? Email Notification  
- **Subject:** "C?n ch?n h??ng d?n vi�n: Tour '{TourTitle}'"
- **Content:** HTML email v?i:
  - Gi?i th�ch v?n ??
  - H�nh ??ng c?n th?c hi?n
  - G?i � t�m h??ng d?n vi�n
  - K? n?ng tour y�u c?u
  - C?nh b�o th?i h?n
  - Call-to-action buttons

## Usage Examples

### Using cURL
```bash
curl -X POST \
  'https://api.tayninhtrour.com/api/admin/debug/test-no-suitable-guides-notification/123e4567-e89b-12d3-a456-426614174000' \
  -H 'Authorization: Bearer YOUR_ADMIN_TOKEN' \
  -H 'Content-Type: application/json'
```

### Using JavaScript (Fetch)
```javascript
const response = await fetch('/api/admin/debug/test-no-suitable-guides-notification/123e4567-e89b-12d3-a456-426614174000', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${adminToken}`,
    'Content-Type': 'application/json'
  }
});

const result = await response.json();
console.log('Notification test result:', result);
```

### Using Postman
1. Set method to `POST`
2. URL: `{{baseUrl}}/api/admin/debug/test-no-suitable-guides-notification/{{tourDetailsId}}`
3. Headers:
   - `Authorization: Bearer {{adminToken}}`
   - `Content-Type: application/json`
4. Send request

## Testing Scenarios

### Scenario 1: Valid TourDetails
```
Input: Existing TourDetails ID
Expected: Success response + notifications sent
Check: Database notifications table + email logs
```

### Scenario 2: Invalid TourDetails ID
```
Input: Non-existent GUID
Expected: 404 error response
Check: Error message in response
```

### Scenario 3: Invalid GUID Format
```
Input: "invalid-guid-format"
Expected: 400 Bad Request
Check: Model validation error
```

## Verification Steps

After calling this endpoint, verify:

1. **Check database:**
   ```sql
   SELECT * FROM Notifications 
   WHERE UserId = (SELECT CreatedById FROM TourDetails WHERE Id = '{tourDetailsId}')
   ORDER BY CreatedAt DESC LIMIT 1;
   ```

2. **Check application logs:**
   ```
   Sending no suitable guides notification to TourCompany for TourDetails {TourDetailsId}
   Successfully created in-app notification for TourDetails {TourDetailsId}
   Successfully sent email notification for no suitable guides to {Email}
   ```

3. **Check email delivery** (if email service is configured)

## Notes

- ?? **Debug endpoint only** - Kh�ng s? d?ng trong production workflow
- ?? **Admin access required** - Ch? admin m?i c� th? g?i
- ?? **Real notifications sent** - Email th?t s? ???c g?i ??n TourCompany
- ?? **Idempotent** - C� th? g?i nhi?u l?n an to�n
- ?? **Logging enabled** - T?t c? actions ???c log chi ti?t

## Related Endpoints

- `POST /api/admin/tourdetails/{id}/approve` - Admin approve tour (triggers natural flow)
- `GET /api/admin/tourdetails/{id}/review` - Get tour details for review
- `GET /api/notifications/user/{userId}` - Get user notifications

---

**Created:** {{current_date}}  
**Version:** 1.0  
**Status:** ? Active