# API Documentation - Withdrawal Stats

## API Endpoint: GET /api/Admin/Withdrawal/stats

### M� t?
API n�y cho ph�p admin l?y th?ng k� t?ng quan v? c�c y�u c?u r�t ti?n v?i kh? n?ng l?c theo ng�y.

### URL
```
GET /api/Admin/Withdrawal/stats
```

### Authentication
- Y�u c?u JWT Token
- Role: Admin

### Parameters (Query String)

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| startDate | DateTime? | No | Ng�y b?t ??u l?c (format: yyyy-MM-dd) |
| endDate | DateTime? | No | Ng�y k?t th�c l?c (format: yyyy-MM-dd) |

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
  "message": "L?y th?ng k� th�nh c�ng",
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
- **totalRequests**: T?ng s? y�u c?u r�t ti?n
- **pendingRequests**: S? y�u c?u ?ang ch? duy?t
- **approvedRequests**: S? y�u c?u ?� ???c duy?t
- **rejectedRequests**: S? y�u c?u b? t? ch?i
- **cancelledRequests**: S? y�u c?u b? h?y

#### Amount Statistics  
- **totalAmountRequested**: T?ng s? ti?n ?� y�u c?u r�t (VN?)
- **pendingAmount**: T?ng s? ti?n ?ang ch? duy?t (VN?)
- **approvedAmount**: T?ng s? ti?n ?� ???c duy?t (VN?)  
- **rejectedAmount**: T?ng s? ti?n b? t? ch?i (VN?)

#### Processing Metrics
- **averageProcessingTimeHours**: Th?i gian x? l� trung b�nh (gi?)
- **approvalRate**: T? l? duy?t (%)

#### Monthly Comparison
- **currentMonth**: Th?ng k� th�ng hi?n t?i
- **previousMonth**: Th?ng k� th�ng tr??c

#### Timestamps
- **lastRequestDate**: Th?i gian y�u c?u g?n nh?t
- **lastApprovalDate**: Th?i gian duy?t g?n nh?t
- **startDate**: Ng�y b?t ??u l?c (n?u c�)
- **endDate**: Ng�y k?t th�c l?c (n?u c�)
- **generatedAt**: Th?i gian t?o b�o c�o

### Error Responses

#### 400 Bad Request
```json
{
  "isSuccess": false,
  "statusCode": 400,
  "message": "Ng�y b?t ??u kh�ng th? l?n h?n ng�y k?t th�c"
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
  "message": "L?i server khi l?y th?ng k�"
}
```

### Usage Examples

#### L?y th?ng k� t?t c? th?i gian
```http
GET /api/Admin/Withdrawal/stats
```

#### L?y th?ng k� th�ng hi?n t?i
```http
GET /api/Admin/Withdrawal/stats?startDate=2024-01-01&endDate=2024-01-31
```

#### L?y th?ng k� t? ng�y c? th?
```http
GET /api/Admin/Withdrawal/stats?startDate=2024-01-15
```

### Business Logic Notes

1. **Date Filtering**: N?u kh�ng cung c?p startDate v� endDate, API s? tr? v? th?ng k� c?a t?t c? th?i gian
2. **Processing Time**: Ch? t�nh cho c�c y�u c?u ?� ???c x? l� (approved/rejected)
3. **Approval Rate**: T�nh d?a tr�n t? l? approved/(approved + rejected + cancelled)
4. **Monthly Stats**: Lu�n t�nh cho th�ng hi?n t?i v� th�ng tr??c, kh�ng b? ?nh h??ng b?i date filter
5. **Amounts**: T?t c? s? ti?n ??u t�nh b?ng VN?

### Frontend Integration

Endpoint n�y ph?c v? cho dashboard admin ?? hi?n th?:
- Cards th?ng k� t?ng quan (nh? trong h�nh b?n g?i)
- Charts so s�nh th�ng hi?n t?i vs th�ng tr??c  
- Filters theo ng�y th�ng
- KPI metrics v? processing time v� approval rate