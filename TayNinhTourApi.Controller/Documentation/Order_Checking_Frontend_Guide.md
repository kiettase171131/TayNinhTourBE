# Order Checking System - Frontend Integration Guide

## ?? T?ng quan

H? th?ng Order Checking cho phép các **Specialty Shop** ki?m tra và xác nh?n giao hàng ??n hàng c?a khách hàng. Thay vì s? d?ng QR code, h? th?ng s? d?ng **PayOS Order Code** ?? ??nh danh ??n hàng.

## ?? Thông tin c? b?n

### Authentication Required
T?t c? API endpoints yêu c?u JWT authentication v?i role **"Specialty Shop"**.

```http
Authorization: Bearer {jwt_token}
```

### Base URL
```
https://localhost:7205/api/Order
```

### ??nh danh ??n hàng
- **PayOS Order Code**: Format `TNDT{timestamp}{random}` (ví d?: `TNDT1234567890`)
- Thay th? cho OrderId ?? ??ng b? v?i h? th?ng thanh toán

---

## ?? Flow ho?t ??ng

### 1. Khách hàng mua hàng
1. Khách ch?n s?n ph?m ? Thanh toán
2. H? th?ng t?o **PayOS Order Code** 
3. Khách nh?n code và ??n shop

### 2. Shop check ??n hàng
1. Shop nh?p PayOS Order Code
2. H? th?ng validate:
   - Shop có bán s?n ph?m trong ??n không?
   - ??n hàng ?ã thanh toán ch?a?
   - ??n hàng ?ã check ch?a?
3. N?u h?p l? ? Mark as delivered

---

## ?? API Endpoints

### 1. Check ??n hàng (Giao hàng)

```http
POST /api/Order/check
Content-Type: application/json
Authorization: Bearer {jwt_token}
```

**Request Body:**
```json
{
    "payOsOrderCode": "TNDT1234567890"
}
```

**Success Response (200):**
```json
{
    "statusCode": 200,
    "message": "Check ??n hàng thành công - ?ã giao hàng cho khách",
    "success": true,
    "isProcessed": true,
    "wasAlreadyChecked": false,
    "checkedAt": "2025-01-17T10:30:00Z",
    "checkedByShopName": "Tay Ninh Traditional Handicrafts",
    "order": {
        "id": "guid-here",
        "payOsOrderCode": "TNDT1234567890",
        "totalAfterDiscount": 150000,
        "status": 1,
        "isChecked": true
    },
    "customer": {
        "userId": "guid-here",
        "name": "Nguy?n V?n A",
        "email": "customer@example.com",
        "phoneNumber": "0987654321"
    },
    "products": [
        {
            "productName": "Gi? tre truy?n th?ng",
            "quantity": 2,
            "unitPrice": 75000
        }
    ]
}
```

**Already Checked (409):**
```json
{
    "statusCode": 409,
    "message": "B?n ?ã check ??n hàng này r?i lúc 17/01/2025 10:30! Không th? check l?i l?n n?a.",
    "success": true,
    "isProcessed": false,
    "wasAlreadyChecked": true
}
```

**Error Responses:**
```json
// Không tìm th?y ??n hàng
{
    "statusCode": 404,
    "message": "Không tìm th?y ??n hàng v?i mã PayOS này",
    "success": false
}

// Shop không có quy?n check
{
    "statusCode": 403,
    "message": "Shop 'ABC' không có quy?n check ??n hàng này vì không bán các s?n ph?m trong ??n hàng",
    "success": false
}

// Ch?a thanh toán
{
    "statusCode": 400,
    "message": "??n hàng ch?a ???c thanh toán",
    "success": false
}
```

### 2. L?y danh sách ??n hàng có th? check

```http
GET /api/Order/checkable
Authorization: Bearer {jwt_token}
```

**Query Parameters:**
- `page` (optional): Trang hi?n t?i (default: 1)
- `pageSize` (optional): S? items per page (default: 10)
- `isChecked` (optional): Filter theo tr?ng thái check
- `fromDate` (optional): L?c t? ngày
- `toDate` (optional): L?c ??n ngày

**Response:**
```json
{
    "statusCode": 200,
    "message": "L?y danh sách ??n hàng thành công",
    "success": true,
    "orders": [
        {
            "id": "guid-here",
            "payOsOrderCode": "TNDT1234567890",
            "totalAfterDiscount": 150000,
            "status": 1,
            "isChecked": false,
            "createdAt": "2025-01-17T09:00:00Z",
            "orderDetails": [
                {
                    "productName": "Gi? tre truy?n th?ng",
                    "quantity": 2,
                    "unitPrice": 75000
                }
            ]
        }
    ],
    "totalCount": 5
}
```

### 3. L?y chi ti?t ??n hàng

```http
GET /api/Order/details/{payOsOrderCode}
Authorization: Bearer {jwt_token}
```

**Response:**
```json
{
    "statusCode": 200,
    "message": "L?y thông tin ??n hàng thành công",
    "success": true,
    "orderId": "guid-here",
    "payOsOrderCode": "TNDT1234567890",
    "isChecked": false,
    "checkedAt": null,
    "checkedByShopName": null,
    "order": {
        "id": "guid-here",
        "payOsOrderCode": "TNDT1234567890",
        "totalAfterDiscount": 150000,
        "status": 1,
        "orderDetails": [...]
    }
}
```

### 4. Debug - Thông tin shop hi?n t?i

```http
GET /api/Order/debug/shop-info
Authorization: Bearer {jwt_token}
```

### 5. Debug - Ki?m tra quy?n s? h?u ??n hàng

```http
GET /api/Order/debug/order-ownership/{payOsOrderCode}
Authorization: Bearer {jwt_token}
```

---

## ?? Frontend Implementation Guide

### 1. Order Check Form Component

```typescript
interface CheckOrderRequest {
    payOsOrderCode: string;
}

interface CheckOrderResponse {
    statusCode: number;
    message: string;
    success: boolean;
    isProcessed?: boolean;
    wasAlreadyChecked?: boolean;
    checkedAt?: string;
    checkedByShopName?: string;
    order?: OrderDto;
    customer?: CustomerInfoDto;
    products?: OrderDetailDto[];
}

const checkOrder = async (payOsOrderCode: string): Promise<CheckOrderResponse> => {
    const response = await fetch('/api/Order/check', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${getJwtToken()}`
        },
        body: JSON.stringify({ payOsOrderCode })
    });
    
    return await response.json();
};
```

### 2. Order List Component

```typescript
interface GetOrdersParams {
    page?: number;
    pageSize?: number;
    isChecked?: boolean;
    fromDate?: string;
    toDate?: string;
}

const getCheckableOrders = async (params: GetOrdersParams = {}) => {
    const queryString = new URLSearchParams(
        Object.entries(params).reduce((acc, [key, value]) => {
            if (value !== undefined) acc[key] = String(value);
            return acc;
        }, {} as Record<string, string>)
    ).toString();

    const response = await fetch(`/api/Order/checkable?${queryString}`, {
        headers: {
            'Authorization': `Bearer ${getJwtToken()}`
        }
    });
    
    return await response.json();
};
```

### 3. Status Handling

```typescript
const handleCheckOrder = async (payOsOrderCode: string) => {
    try {
        const result = await checkOrder(payOsOrderCode);
        
        switch (result.statusCode) {
            case 200:
                if (result.isProcessed) {
                    showSuccess('? ?ã giao hàng thành công!');
                } else {
                    showInfo('?? ' + result.message);
                }
                break;
                
            case 409:
                showWarning('?? ' + result.message);
                break;
                
            case 403:
                showError('?? B?n không có quy?n check ??n hàng này');
                break;
                
            case 404:
                showError('? Không tìm th?y ??n hàng');
                break;
                
            case 400:
                showError('?? ??n hàng ch?a ???c thanh toán');
                break;
                
            default:
                showError('? Có l?i x?y ra: ' + result.message);
        }
    } catch (error) {
        showError('? L?i k?t n?i: ' + error.message);
    }
};
```

---

## ?? UI/UX Recommendations

### 1. Check Order Form
```
???????????????????????????????????????
?  ?? Ki?m tra ??n hàng               ?
???????????????????????????????????????
?                                     ?
?  PayOS Order Code:                  ?
?  ??????????????????????????????????? ?
?  ? TNDT1234567890                  ? ?
?  ??????????????????????????????????? ?
?                                     ?
?  [?? Ki?m tra] [?? Quét QR]         ?
?                                     ?
???????????????????????????????????????
```

### 2. Success State
```
???????????????????????????????????????
?  ? Giao hàng thành công!           ?
???????????????????????????????????????
?  Mã ??n: TNDT1234567890             ?
?  Khách hàng: Nguy?n V?n A           ?
?  S?n ph?m: Gi? tre truy?n th?ng x2  ?
?  T?ng ti?n: 150,000 VN?             ?
?  Th?i gian: 17/01/2025 10:30        ?
???????????????????????????????????????
```

### 3. Already Checked State
```
???????????????????????????????????????
?  ?? ??n hàng ?ã ???c check          ?
???????????????????????????????????????
?  ?ã check lúc: 17/01/2025 10:30     ?
?  Shop check: Tay Ninh Handicrafts   ?
?                                     ?
?  Không th? check l?i!               ?
???????????????????????????????????????
```

---

## ?? Security & Validation

### Frontend Validation
```typescript
const validatePayOsOrderCode = (code: string): boolean => {
    // PayOS code format: TNDT + 10 digits
    const regex = /^TNDT\d{10}$/;
    return regex.test(code);
};

const formatPayOsOrderCode = (input: string): string => {
    // Remove spaces and convert to uppercase
    return input.replace(/\s/g, '').toUpperCase();
};
```

### Error Handling
```typescript
const getErrorMessage = (statusCode: number, message: string): string => {
    const errorMap = {
        400: '?? ??n hàng ch?a thanh toán',
        401: '?? Vui lòng ??ng nh?p l?i',
        403: '? Không có quy?n th?c hi?n',
        404: '?? Không tìm th?y ??n hàng',
        409: '?? ??n hàng ?ã ???c x? lý',
        500: '?? L?i h? th?ng'
    };
    
    return errorMap[statusCode] || message;
};
```

---

## ?? Mobile Considerations

### QR Code Integration
```typescript
// N?u mu?n thêm QR scanner
const scanQRCode = async (): Promise<string> => {
    try {
        const result = await BarcodeScanner.scan();
        if (result.text.includes('TNDT')) {
            return result.text;
        } else {
            throw new Error('QR code không h?p l?');
        }
    } catch (error) {
        throw new Error('L?i quét QR: ' + error.message);
    }
};
```

### Offline Support
```typescript
// Cache checked orders for offline viewing
const cacheCheckedOrder = (orderData: CheckOrderResponse) => {
    const cached = localStorage.getItem('checkedOrders') || '[]';
    const orders = JSON.parse(cached);
    orders.push({
        ...orderData,
        cachedAt: new Date().toISOString()
    });
    localStorage.setItem('checkedOrders', JSON.stringify(orders));
};
```

---

## ?? Testing Data

?? test ch?c n?ng, s? d?ng:

### Test Accounts
```
Email: specialtyshop1@example.com
Password: 12345678h@
Role: Specialty Shop
```

### Sample PayOS Codes
```
TNDT1234567890 - ??n hàng ?ã thanh toán, ch?a check
TNDT9876543210 - ??n hàng ?ã check
TNDT5555555555 - ??n hàng ch?a thanh toán
```

### Test Products
```
- Gi? tre truy?n th?ng Tây Ninh (150,000 VN?)
- G?m s? th? công Tây Ninh (280,000 VN?) 
- Th? c?m Tây Ninh (320,000 VN?)
```

---

## ?? Deployment Notes

### Environment Variables
```env
API_BASE_URL=https://localhost:7205
JWT_TOKEN_KEY=tay_ninh_tour_token
ORDER_CHECK_TIMEOUT=30000
```

### Error Monitoring
```typescript
// Log all order check attempts
const logOrderCheck = (payOsOrderCode: string, result: CheckOrderResponse) => {
    console.log('Order Check', {
        code: payOsOrderCode,
        statusCode: result.statusCode,
        success: result.success,
        timestamp: new Date().toISOString()
    });
};
```

---

## ?? Support

N?u g?p v?n ?? v?i API Order Checking:

1. **Debug Info**: S? d?ng `/api/Order/debug/shop-info` ?? ki?m tra thông tin shop
2. **Order Ownership**: S? d?ng `/api/Order/debug/order-ownership/{code}` ?? debug quy?n
3. **Logs**: Check console logs và network requests
4. **JWT Token**: ??m b?o token h?p l? và có role "Specialty Shop"

---

## ?? Changelog

### v1.0.0 (Current)
- ? Chuy?n t? OrderId sang PayOS Order Code
- ? Improved validation messages  
- ? Better error handling
- ? Debug endpoints
- ? Already checked detection
- ? Shop ownership validation