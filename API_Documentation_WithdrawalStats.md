# API Documentation - Withdrawal Stats

## API Endpoint: GET /api/Admin/Withdrawal/stats

### Mô t?
API này cho phép admin l?y th?ng kê t?ng quan v? các yêu c?u rút ti?n v?i kh? n?ng l?c theo ngày.

### URL
```
GET /api/Admin/Withdrawal/stats
```

### Authentication
- Yêu c?u JWT Token
- Role: Admin

### Parameters (Query String)

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| startDate | DateTime? | No | Ngày b?t ??u l?c (format: yyyy-MM-dd) |
| endDate | DateTime? | No | Ngày k?t thúc l?c (format: yyyy-MM-dd) |

### Request Example

```http
GET /api/Admin/Withdrawal/stats?startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer <admin_jwt_token>
```

### Response Format

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "L?y th?ng kê thành công",
  "data": {
    "totalRequests": 150,
    "pendingRequests": 25,
    "approvedRequests": 100,
    "rejectedRequests": 20,
    "cancelledRequests": 5,
    "totalAmountRequested": 50000000,
    "pendingAmount": 12500000,
    "approvedAmount": 30000000,
    "rejectedAmount": 6000000,
    "averageProcessingTimeHours": 24.5,
    "approvalRate": 80.0,
    "currentMonth": {
      "month": "01/2024",
      "requestCount": 45,
      "totalAmount": 15000000,
      "approvedCount": 35,
      "approvedAmount": 12000000,
      "approvalRate": 77.8
    },
    "previousMonth": {
      "month": "12/2023",
      "requestCount": 38,
      "totalAmount": 12000000,
      "approvedCount": 30,
      "approvedAmount": 9500000,
      "approvalRate": 78.9
    },
    "lastRequestDate": "2024-01-31T14:30:00Z",
    "lastApprovalDate": "2024-01-31T16:45:00Z",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "generatedAt": "2024-02-01T10:00:00Z"
  }
}
```

### Response Fields Description

#### Main Statistics
- **totalRequests**: T?ng s? yêu c?u rút ti?n
- **pendingRequests**: S? yêu c?u ?ang ch? duy?t
- **approvedRequests**: S? yêu c?u ?ã ???c duy?t
- **rejectedRequests**: S? yêu c?u b? t? ch?i
- **cancelledRequests**: S? yêu c?u b? h?y

#### Amount Statistics  
- **totalAmountRequested**: T?ng s? ti?n ?ã yêu c?u rút (VN?)
- **pendingAmount**: T?ng s? ti?n ?ang ch? duy?t (VN?)
- **approvedAmount**: T?ng s? ti?n ?ã ???c duy?t (VN?)  
- **rejectedAmount**: T?ng s? ti?n b? t? ch?i (VN?)

#### Processing Metrics
- **averageProcessingTimeHours**: Th?i gian x? lý trung bình (gi?)
- **approvalRate**: T? l? duy?t (%)

#### Monthly Comparison
- **currentMonth**: Th?ng kê tháng hi?n t?i
- **previousMonth**: Th?ng kê tháng tr??c

#### Timestamps
- **lastRequestDate**: Th?i gian yêu c?u g?n nh?t
- **lastApprovalDate**: Th?i gian duy?t g?n nh?t
- **startDate**: Ngày b?t ??u l?c (n?u có)
- **endDate**: Ngày k?t thúc l?c (n?u có)
- **generatedAt**: Th?i gian t?o báo cáo

### Error Responses

#### 400 Bad Request
```json
{
  "isSuccess": false,
  "statusCode": 400,
  "message": "Ngày b?t ??u không th? l?n h?n ngày k?t thúc"
}
```

#### 401 Unauthorized
```json
{
  "isSuccess": false,
  "statusCode": 401,
  "message": "Unauthorized"
}
```

#### 403 Forbidden
```json
{
  "isSuccess": false,
  "statusCode": 403,
  "message": "Forbidden - Admin role required"
}
```

#### 500 Internal Server Error
```json
{
  "isSuccess": false,
  "statusCode": 500,
  "message": "L?i server khi l?y th?ng kê"
}
```

### Usage Examples

#### L?y th?ng kê t?t c? th?i gian
```http
GET /api/Admin/Withdrawal/stats
```

#### L?y th?ng kê tháng hi?n t?i
```http
GET /api/Admin/Withdrawal/stats?startDate=2024-01-01&endDate=2024-01-31
```

#### L?y th?ng kê t? ngày c? th?
```http
GET /api/Admin/Withdrawal/stats?startDate=2024-01-15
```

### Business Logic Notes

1. **Date Filtering**: N?u không cung c?p startDate và endDate, API s? tr? v? th?ng kê c?a t?t c? th?i gian
2. **Processing Time**: Ch? tính cho các yêu c?u ?ã ???c x? lý (approved/rejected)
3. **Approval Rate**: Tính d?a trên t? l? approved/(approved + rejected + cancelled)
4. **Monthly Stats**: Luôn tính cho tháng hi?n t?i và tháng tr??c, không b? ?nh h??ng b?i date filter
5. **Amounts**: T?t c? s? ti?n ??u tính b?ng VN?

### Frontend Integration

Endpoint này ph?c v? cho dashboard admin ?? hi?n th?:
- Cards th?ng kê t?ng quan (nh? trong hình b?n g?i)
- Charts so sánh tháng hi?n t?i vs tháng tr??c  
- Filters theo ngày tháng
- KPI metrics v? processing time và approval rate