# Order Check API - Quick Reference

## ?? Endpoints Summary

| Method | Endpoint | Purpose | Auth | Role Required |
|--------|----------|---------|------|---------------|
| `POST` | `/api/Order/check` | Check & deliver order | ? | Specialty Shop |
| `GET` | `/api/Order/checkable` | List checkable orders | ? | Specialty Shop |
| `GET` | `/api/Order/details/{payOsOrderCode}` | Get order details | ? | Any |
| `GET` | `/api/Order/debug/shop-info` | Debug shop info | ? | Specialty Shop |
| `GET` | `/api/Order/debug/order-ownership/{code}` | Debug order ownership | ? | Specialty Shop |

## ?? Request/Response Examples

### Check Order (Main Function)
```bash
curl -X POST "/api/Order/check" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"payOsOrderCode": "TNDT1234567890"}'
```

### Response Status Codes
- `200` - Success (checked successfully)
- `409` - Already checked
- `403` - No permission (shop doesn't sell products in order)
- `404` - Order not found
- `400` - Order not paid yet

## ?? Key Points for Frontend

### PayOS Order Code Format
- Pattern: `TNDT{10-digits}`
- Example: `TNDT1234567890`
- Case sensitive: Always uppercase

### Business Rules
1. Only shops that sell products in the order can check it
2. Order must be paid (`status = 1`) before checking
3. Each order can only be checked once
4. System shows who checked and when

### Error Handling
```typescript
switch (response.statusCode) {
    case 200: // Success or already checked
        if (response.isProcessed) {
            showSuccess("? Delivered successfully!");
        } else {
            showInfo("?? " + response.message);
        }
        break;
    case 409: 
        showWarning("?? Already checked");
        break;
    case 403:
        showError("?? No permission");
        break;
    case 404:
        showError("? Order not found");
        break;
    case 400:
        showError("?? Not paid yet");
        break;
}
```

## ?? Sample Test Data

### Test Account
```
Email: specialtyshop1@example.com
Password: 12345678h@
```

### Sample PayOS Codes
```
TNDT1234567890  # Valid, not checked
TNDT9876543210  # Already checked  
TNDT5555555555  # Not paid yet
```

## ?? TypeScript Interfaces

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
    order?: {
        id: string;
        payOsOrderCode: string;
        totalAfterDiscount: number;
        status: number;
        isChecked: boolean;
    };
    customer?: {
        userId: string;
        name: string;
        email: string;
        phoneNumber: string;
    };
    products?: Array<{
        productName: string;
        quantity: number;
        unitPrice: number;
    }>;
}
```

## ?? UI States

### Input Form
```
???????????????????????????????????
? PayOS Order Code:               ?
? ??????????????????????????????? ?
? ? TNDT1234567890              ? ?
? ??????????????????????????????? ?
? [Check Order] [Scan QR]         ?
???????????????????????????????????
```

### Success Result  
```
???????????????????????????????????
? ? Order delivered successfully ?
? Code: TNDT1234567890            ?
? Customer: Nguyen Van A          ?
? Total: 150,000 VN?              ?
? Time: 17/01/2025 10:30          ?
???????????????????????????????????
```

### Already Checked
```
???????????????????????????????????
? ?? Already checked              ?
? Checked by: ABC Shop            ?  
? At: 17/01/2025 10:30            ?
? Cannot check again!             ?
???????????????????????????????????
```

## ?? Debug Commands

```bash
# Check shop info
curl -H "Authorization: Bearer {token}" \
  "/api/Order/debug/shop-info"

# Check order ownership  
curl -H "Authorization: Bearer {token}" \
  "/api/Order/debug/order-ownership/TNDT1234567890"

# List checkable orders
curl -H "Authorization: Bearer {token}" \
  "/api/Order/checkable?page=1&pageSize=10"
```