# Order Checking System - Frontend Integration Guide

## ?? T?ng quan

H? th?ng Order Checking cho ph�p c�c **Specialty Shop** ki?m tra v� x�c nh?n giao h�ng ??n h�ng c?a kh�ch h�ng. Thay v� s? d?ng QR code, h? th?ng s? d?ng **PayOS Order Code** ?? ??nh danh ??n h�ng.

## ?? Th�ng tin c? b?n

### Authentication Required
T?t c? API endpoints y�u c?u JWT authentication v?i role **"Specialty Shop"**.

```http
Authorization: Bearer {jwt_token}
```

### Base URL
```
https://localhost:7205/api/Order
```

### ??nh danh ??n h�ng
- **PayOS Order Code**: Format `TNDT{timestamp}{random}` (v� d?: `TNDT1234567890`)
- Thay th? cho OrderId ?? ??ng b? v?i h? th?ng thanh to�n

---

## ?? Flow ho?t ??ng

### 1. Kh�ch h�ng mua h�ng
1. Kh�ch ch?n s?n ph?m ? Thanh to�n
2. H? th?ng t?o **PayOS Order Code** 
3. Kh�ch nh?n code v� ??n shop

### 2. Shop check ??n h�ng
1. Shop nh?p PayOS Order Code
2. H? th?ng validate:
   - Shop c� b�n s?n ph?m trong ??n kh�ng?
   - ??n h�ng ?� thanh to�n ch?a?
   - ??n h�ng ?� check ch?a?
3. N?u h?p l? ? Mark as delivered

---

## ?? API Endpoints

### 1. Check ??n h�ng (Giao h�ng)

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
    "message": "Check ??n h�ng th�nh c�ng - ?� giao h�ng cho kh�ch",
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
    "message": "B?n ?� check ??n h�ng n�y r?i l�c 17/01/2025 10:30! Kh�ng th? check l?i l?n n?a.",
    "success": true,
    "isProcessed": false,
    "wasAlreadyChecked": true
}
```

**Error Responses:**
```json
// Kh�ng t�m th?y ??n h�ng
{
    "statusCode": 404,
    "message": "Kh�ng t�m th?y ??n h�ng v?i m� PayOS n�y",
    "success": false
}

// Shop kh�ng c� quy?n check
{
    "statusCode": 403,
    "message": "Shop 'ABC' kh�ng c� quy?n check ??n h�ng n�y v� kh�ng b�n c�c s?n ph?m trong ??n h�ng",
    "success": false
}

// Ch?a thanh to�n
{
    "statusCode": 400,
    "message": "??n h�ng ch?a ???c thanh to�n",
    "success": false
}
```

### 2. L?y danh s�ch ??n h�ng c� th? check

```http
GET /api/Order/checkable
Authorization: Bearer {jwt_token}
```

**Query Parameters:**
- `page` (optional): Trang hi?n t?i (default: 1)
- `pageSize` (optional): S? items per page (default: 10)
- `isChecked` (optional): Filter theo tr?ng th�i check
- `fromDate` (optional): L?c t? ng�y
- `toDate` (optional): L?c ??n ng�y

**Response:**
```json
{
    "statusCode": 200,
    "message": "L?y danh s�ch ??n h�ng th�nh c�ng",
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

### 3. L?y chi ti?t ??n h�ng

```http
GET /api/Order/details/{payOsOrderCode}
Authorization: Bearer {jwt_token}
```

**Response:**
```json
{
    "statusCode": 200,
    "message": "L?y th�ng tin ??n h�ng th�nh c�ng",
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

### 4. Debug - Th�ng tin shop hi?n t?i

```http
GET /api/Order/debug/shop-info
Authorization: Bearer {jwt_token}
```

### 5. Debug - Ki?m tra quy?n s? h?u ??n h�ng

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
                    showSuccess('? ?� giao h�ng th�nh c�ng!');
                } else {
                    showInfo('?? ' + result.message);
                }
                break;
                
            case 409:
                showWarning('?? ' + result.message);
                break;
                
            case 403:
                showError('?? B?n kh�ng c� quy?n check ??n h�ng n�y');
                break;
                
            case 404:
                showError('? Kh�ng t�m th?y ??n h�ng');
                break;
                
            case 400:
                showError('?? ??n h�ng ch?a ???c thanh to�n');
                break;
                
            default:
                showError('? C� l?i x?y ra: ' + result.message);
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
?  ?? Ki?m tra ??n h�ng               ?
???????????????????????????????????????
?                                     ?
?  PayOS Order Code:                  ?
?  ??????????????????????????????????? ?
?  ? TNDT1234567890                  ? ?
?  ??????????????????????????????????? ?
?                                     ?
?  [?? Ki?m tra] [?? Qu�t QR]         ?
?                                     ?
???????????????????????????????????????
```

### 2. Success State
```
???????????????????????????????????????
?  ? Giao h�ng th�nh c�ng!           ?
???????????????????????????????????????
?  M� ??n: TNDT1234567890             ?
?  Kh�ch h�ng: Nguy?n V?n A           ?
?  S?n ph?m: Gi? tre truy?n th?ng x2  ?
?  T?ng ti?n: 150,000 VN?             ?
?  Th?i gian: 17/01/2025 10:30        ?
???????????????????????????????????????
```

### 3. Already Checked State
```
???????????????????????????????????????
?  ?? ??n h�ng ?� ???c check          ?
???????????????????????????????????????
?  ?� check l�c: 17/01/2025 10:30     ?
?  Shop check: Tay Ninh Handicrafts   ?
?                                     ?
?  Kh�ng th? check l?i!               ?
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
        400: '?? ??n h�ng ch?a thanh to�n',
        401: '?? Vui l�ng ??ng nh?p l?i',
        403: '? Kh�ng c� quy?n th?c hi?n',
        404: '?? Kh�ng t�m th?y ??n h�ng',
        409: '?? ??n h�ng ?� ???c x? l�',
        500: '?? L?i h? th?ng'
    };
    
    return errorMap[statusCode] || message;
};
```

---

## ?? Mobile Considerations

### QR Code Integration
```typescript
// N?u mu?n th�m QR scanner
const scanQRCode = async (): Promise<string> => {
    try {
        const result = await BarcodeScanner.scan();
        if (result.text.includes('TNDT')) {
            return result.text;
        } else {
            throw new Error('QR code kh�ng h?p l?');
        }
    } catch (error) {
        throw new Error('L?i qu�t QR: ' + error.message);
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
TNDT1234567890 - ??n h�ng ?� thanh to�n, ch?a check
TNDT9876543210 - ??n h�ng ?� check
TNDT5555555555 - ??n h�ng ch?a thanh to�n
```

### Test Products
```
- Gi? tre truy?n th?ng T�y Ninh (150,000 VN?)
- G?m s? th? c�ng T�y Ninh (280,000 VN?) 
- Th? c?m T�y Ninh (320,000 VN?)
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

1. **Debug Info**: S? d?ng `/api/Order/debug/shop-info` ?? ki?m tra th�ng tin shop
2. **Order Ownership**: S? d?ng `/api/Order/debug/order-ownership/{code}` ?? debug quy?n
3. **Logs**: Check console logs v� network requests
4. **JWT Token**: ??m b?o token h?p l? v� c� role "Specialty Shop"

---

## ?? Changelog

### v1.0.0 (Current)
- ? Chuy?n t? OrderId sang PayOS Order Code
- ? Improved validation messages  
- ? Better error handling
- ? Debug endpoints
- ? Already checked detection
- ? Shop ownership validation