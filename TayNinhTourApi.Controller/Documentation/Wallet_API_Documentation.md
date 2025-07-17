# Wallet API Documentation

## Overview
API ?? qu?n l� v� ti?n cho Tour Company v� Specialty Shop. API t? ??ng ph�t hi?n role c?a user v� tr? v? th�ng tin v� t??ng ?ng.

## Endpoints

### 1. Get My Wallet (Universal)
**GET** `/api/wallet/my-wallet`

**Authentication:** Required (JWT Token)
**Roles:** Tour Company, Specialty Shop

**Description:** L?y th�ng tin v� c?a user hi?n t?i. T? ??ng detect role v� tr? v? v� t??ng ?ng.

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y th�ng tin v� c�ng ty tour th�nh c�ng",
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

**Description:** L?y th�ng tin v� chi ti?t cho Tour Company.

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y th�ng tin v� c�ng ty tour th�nh c�ng",
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

**Description:** L?y th�ng tin v� cho Specialty Shop.

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y th�ng tin v� shop th�nh c�ng",
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

**Description:** Ki?m tra user c� v� ti?n kh�ng.

**Response:**
```json
{
  "statusCode": 200,
  "message": "User c� v� ti?n",
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

**Description:** L?y lo?i v� c?a user.

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y lo?i v� th�nh c�ng",
  "success": true,
  "data": {
    "walletType": "SpecialtyShop"
  }
}
```

## Wallet Types

### Tour Company Wallet
- **wallet**: S? ti?n c� th? r�t ngay
- **revenueHold**: S? ti?n ?ang hold (ch? 3 ng�y sau khi tour k?t th�c)
- **totalBalance**: T?ng s? ti?n (wallet + revenueHold)

### Specialty Shop Wallet
- **wallet**: S? ti?n t? vi?c b�n s?n ph?m
- Kh�ng c� hold balance v� thanh to�n tr?c ti?p

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
  "message": "Kh�ng t�m th?y th�ng tin v�",
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
      // User c� v�, c� th? hi?n th? wallet UI
      console.log('User has wallet:', data.data.walletType);
    } else {
      // User ch?a c� v�, hi?n th? message ??ng k�
      console.log('User needs to register for wallet');
    }
  } catch (error) {
    console.error('Error checking wallet:', error);
  }
};
```

## Notes
- T?t c? endpoints y�u c?u JWT authentication
- API t? ??ng detect role d?a tr�n database records
- S? ti?n ???c tr? v? d?ng decimal (VND)
- Timestamps theo format ISO 8601 UTC