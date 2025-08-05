# Early Bird Pricing Information for Frontend

## T?ng quan
H? th?ng ?ã ???c c?p nh?t ?? cung c?p thông tin early bird chi ti?t cho Frontend thông qua các API response.

## Logic Early Bird
- **Th?i gian áp d?ng**: 15 ngày ??u sau khi tour ???c t?o
- **?i?u ki?n**: Tour ph?i kh?i hành sau ít nh?t 30 ngày t? ngày ??t
- **M?c gi?m giá**: 25% trên giá g?c

## API Endpoints và Thông tin Early Bird

### 1. GET `/api/UserTourBooking/available-tours`

**Response Structure:**
```json
{
  "items": [
    {
      "tourDetailsId": "guid",
      "title": "Tour Name",
      "price": 1000000,
      "isEarlyBirdActive": true,
      "earlyBirdPrice": 750000,
      "earlyBirdDiscountPercent": 25,
      "earlyBirdDiscountAmount": 250000,
      "earlyBirdEndDate": "2024-01-30T23:59:59",
      "daysRemainingForEarlyBird": 5,
      "pricingType": "Early Bird",
      "finalPrice": 750000,
      "hasEarlyBirdDiscount": true
    }
  ]
}
```

**Thông tin Early Bird:**
- `isEarlyBirdActive`: Tour còn áp d?ng early bird không
- `earlyBirdPrice`: Giá sau khi gi?m early bird
- `earlyBirdDiscountPercent`: Ph?n tr?m gi?m giá (25%)
- `earlyBirdDiscountAmount`: S? ti?n ???c gi?m
- `earlyBirdEndDate`: Ngày k?t thúc early bird
- `daysRemainingForEarlyBird`: S? ngày còn l?i c?a early bird
- `pricingType`: "Early Bird" ho?c "Standard"
- `finalPrice`: Giá cu?i cùng (computed property)
- `hasEarlyBirdDiscount`: Có ?ang gi?m giá không (computed property)

### 2. GET `/api/UserTourBooking/tour-details/{id}`

**Response Structure:**
```json
{
  "id": "guid",
  "title": "Tour Name",
  "tourDates": [
    {
      "tourSlotId": "guid",
      "tourDate": "2024-02-15T00:00:00",
      "originalPrice": 1000000,
      "finalPrice": 750000,
      "isEarlyBirdApplicable": true,
      "earlyBirdDiscountPercent": 25
    }
  ],
  "earlyBirdInfo": {
    "isActive": true,
    "discountPercent": 25,
    "endDate": "2024-01-30T23:59:59",
    "daysRemaining": 5,
    "description": "??t s?m ti?t ki?m 25% trong 5 ngày còn l?i!",
    "originalPrice": 1000000,
    "discountedPrice": 750000,
    "savingsAmount": 250000,
    "isExpiringSoon": true,
    "displayMessage": "Gi?m 25% - Còn 5 ngày!"
  }
}
```

**Thông tin Early Bird:**
- `tourDates[]`: M?i slot có thông tin giá riêng
  - `originalPrice`: Giá g?c
  - `finalPrice`: Giá sau gi?m
  - `isEarlyBirdApplicable`: Slot này có áp d?ng early bird không
  - `earlyBirdDiscountPercent`: % gi?m giá

- `earlyBirdInfo`: Thông tin t?ng quan
  - `isActive`: Early bird còn hi?u l?c
  - `discountPercent`: % gi?m giá
  - `endDate`: Ngày k?t thúc
  - `daysRemaining`: Ngày còn l?i
  - `description`: Mô t? chi ti?t
  - `originalPrice`: Giá g?c
  - `discountedPrice`: Giá sau gi?m
  - `savingsAmount`: S? ti?n ti?t ki?m
  - `isExpiringSoon`: S?p h?t h?n (? 3 ngày)
  - `displayMessage`: Message ?? hi?n th?

### 3. POST `/api/UserTourBooking/calculate-price`

**Response Structure:**
```json
{
  "tourOperationId": "guid",
  "numberOfGuests": 2,
  "originalPricePerGuest": 1000000,
  "totalOriginalPrice": 2000000,
  "discountPercent": 25,
  "discountAmount": 500000,
  "finalPrice": 1500000,
  "isEarlyBird": true,
  "pricingType": "Early Bird",
  "daysSinceCreated": 5,
  "daysUntilTour": 45,
  "isAvailable": true
}
```

## Cách FE s? d?ng thông tin Early Bird

### 1. Hi?n th? badge Early Bird
```javascript
if (tour.isEarlyBirdActive) {
  showEarlyBirdBadge(`Gi?m ${tour.earlyBirdDiscountPercent}%`);
}
```

### 2. Hi?n th? giá v?i gi?m giá
```javascript
if (tour.hasEarlyBirdDiscount) {
  showOriginalPrice(tour.price, { strikethrough: true });
  showDiscountedPrice(tour.finalPrice, { highlight: true });
  showSavings(tour.earlyBirdDiscountAmount);
}
```

### 3. Hi?n th? countdown
```javascript
if (tour.isEarlyBirdActive && tour.daysRemainingForEarlyBird > 0) {
  showCountdown(`Còn ${tour.daysRemainingForEarlyBird} ngày gi?m giá!`);
}
```

### 4. C?nh báo s?p h?t h?n
```javascript
if (tour.earlyBirdInfo?.isExpiringSoon) {
  showUrgentMessage("?? ?u ?ãi s?p k?t thúc!");
}
```

### 5. Tính t?ng ti?n v?i early bird
```javascript
const totalPrice = tour.finalPrice * numberOfGuests;
const savings = (tour.price - tour.finalPrice) * numberOfGuests;

if (savings > 0) {
  showSavingsMessage(`B?n ti?t ki?m ???c ${formatCurrency(savings)}!`);
}
```

## L?u ý cho Developer

1. **Always check `isEarlyBirdActive`** tr??c khi hi?n th? thông tin early bird
2. **Use `finalPrice`** ?? tính t?ng ti?n, không ph?i `price`
3. **Show countdown** khi `daysRemainingForEarlyBird > 0`
4. **Highlight urgency** khi `isExpiringSoon = true`
5. **Display savings** ?? khuy?n khích khách hàng ??t s?m

## Test Cases

### Early Bird Active
- Tour ???c t?o 5 ngày tr??c
- Tour kh?i hành 45 ngày n?a
- Expected: `isEarlyBirdActive = true`, `discountPercent = 25%`

### Early Bird Expired
- Tour ???c t?o 20 ngày tr??c  
- Expected: `isEarlyBirdActive = false`, `pricingType = "Standard"`

### Tour Too Soon
- Tour kh?i hành 20 ngày n?a
- Expected: `isEarlyBirdActive = false` (không ?? 30 ngày)