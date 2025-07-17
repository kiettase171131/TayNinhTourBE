# Wallet API Documentation

## Overview
API ?? qu?n lý ví ti?n cho Tour Company và Specialty Shop. API t? ??ng phát hi?n role c?a user và tr? v? thông tin ví t??ng ?ng.

## Endpoints

### 1. Get My Wallet (Universal)
**GET** `/api/wallet/my-wallet`

**Authentication:** Required (JWT Token)
**Roles:** Tour Company, Specialty Shop

**Description:** L?y thông tin ví c?a user hi?n t?i. T? ??ng detect role và tr? v? ví t??ng ?ng.

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y thông tin ví công ty tour thành công",
  "success": true,
  "data": {
    "walletType": "TourCompany",
    "ownerName": "ABC Tour Company",
    "availableBalance": 1500000,
    "holdBalance": 500000,
    "totalBalance": 2000000,
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

### 2. Get Tour Company Wallet
**GET** `/api/wallet/tour-company`

**Authentication:** Required (JWT Token)
**Roles:** Tour Company only

**Description:** L?y thông tin ví chi ti?t cho Tour Company.

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y thông tin ví công ty tour thành công",
  "success": true,
  "data": {
    "id": "guid",
    "userId": "guid",
    "companyName": "ABC Tour Company",
    "wallet": 1500000,
    "revenueHold": 500000,
    "totalBalance": 2000000,
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

### 3. Get Specialty Shop Wallet
**GET** `/api/wallet/specialty-shop`

**Authentication:** Required (JWT Token)
**Roles:** Specialty Shop only

**Description:** L?y thông tin ví cho Specialty Shop.

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y thông tin ví shop thành công",
  "success": true,
  "data": {
    "id": "guid",
    "userId": "guid",
    "shopName": "XYZ Souvenir Shop",
    "wallet": 800000,
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

### 4. Check Has Wallet
**GET** `/api/wallet/has-wallet`

**Authentication:** Required (JWT Token)
**Roles:** Any authenticated user

**Description:** Ki?m tra user có ví ti?n không.

**Response:**
```json
{
  "statusCode": 200,
  "message": "User có ví ti?n",
  "success": true,
  "data": {
    "hasWallet": true,
    "walletType": "TourCompany"
  }
}
```

### 5. Get Wallet Type
**GET** `/api/wallet/wallet-type`

**Authentication:** Required (JWT Token)
**Roles:** Any authenticated user

**Description:** L?y lo?i ví c?a user.

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y lo?i ví thành công",
  "success": true,
  "data": {
    "walletType": "SpecialtyShop"
  }
}
```

## Wallet Types

### Tour Company Wallet
- **wallet**: S? ti?n có th? rút ngay
- **revenueHold**: S? ti?n ?ang hold (ch? 3 ngày sau khi tour k?t thúc)
- **totalBalance**: T?ng s? ti?n (wallet + revenueHold)

### Specialty Shop Wallet
- **wallet**: S? ti?n t? vi?c bán s?n ph?m
- Không có hold balance vì thanh toán tr?c ti?p

## Error Responses

### 401 Unauthorized
```json
{
  "statusCode": 401,
  "message": "Unauthorized access",
  "success": false
}
```

### 403 Forbidden
```json
{
  "statusCode": 403,
  "message": "Access forbidden",
  "success": false
}
```

### 404 Not Found
```json
{
  "statusCode": 404,
  "message": "Không tìm th?y thông tin ví",
  "success": false
}
```

## Usage Examples

### Frontend Integration
```javascript
// Get current user's wallet
const getMyWallet = async () => {
  try {
    const response = await fetch('/api/wallet/my-wallet', {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    const data = await response.json();
    
    if (data.success) {
      console.log('Wallet Type:', data.data.walletType);
      console.log('Available Balance:', data.data.availableBalance);
      console.log('Total Balance:', data.data.totalBalance);
    }
  } catch (error) {
    console.error('Error fetching wallet:', error);
  }
};

// Check if user has wallet
const checkWallet = async () => {
  try {
    const response = await fetch('/api/wallet/has-wallet', {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    const data = await response.json();
    
    if (data.data.hasWallet) {
      // User có ví, có th? hi?n th? wallet UI
      console.log('User has wallet:', data.data.walletType);
    } else {
      // User ch?a có ví, hi?n th? message ??ng ký
      console.log('User needs to register for wallet');
    }
  } catch (error) {
    console.error('Error checking wallet:', error);
  }
};
```

## Notes
- T?t c? endpoints yêu c?u JWT authentication
- API t? ??ng detect role d?a trên database records
- S? ti?n ???c tr? v? d?ng decimal (VND)
- Timestamps theo format ISO 8601 UTC