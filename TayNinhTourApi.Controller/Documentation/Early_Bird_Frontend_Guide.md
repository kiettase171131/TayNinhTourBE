# Early Bird Pricing Information for Frontend

## T?ng quan
H? th?ng ?� ???c c?p nh?t ?? cung c?p th�ng tin early bird chi ti?t cho Frontend th�ng qua c�c API response.

## Logic Early Bird
- **Th?i gian �p d?ng**: 15 ng�y ??u sau khi tour ???c t?o
- **?i?u ki?n**: Tour ph?i kh?i h�nh sau �t nh?t 30 ng�y t? ng�y ??t
- **M?c gi?m gi�**: 25% tr�n gi� g?c

## API Endpoints v� Th�ng tin Early Bird

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

**Th�ng tin Early Bird:**
- `isEarlyBirdActive`: Tour c�n �p d?ng early bird kh�ng
- `earlyBirdPrice`: Gi� sau khi gi?m early bird
- `earlyBirdDiscountPercent`: Ph?n tr?m gi?m gi� (25%)
- `earlyBirdDiscountAmount`: S? ti?n ???c gi?m
- `earlyBirdEndDate`: Ng�y k?t th�c early bird
- `daysRemainingForEarlyBird`: S? ng�y c�n l?i c?a early bird
- `pricingType`: "Early Bird" ho?c "Standard"
- `finalPrice`: Gi� cu?i c�ng (computed property)
- `hasEarlyBirdDiscount`: C� ?ang gi?m gi� kh�ng (computed property)

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
    "description": "??t s?m ti?t ki?m 25% trong 5 ng�y c�n l?i!",
    "originalPrice": 1000000,
    "discountedPrice": 750000,
    "savingsAmount": 250000,
    "isExpiringSoon": true,
    "displayMessage": "Gi?m 25% - C�n 5 ng�y!"
  }
}
```

**Th�ng tin Early Bird:**
- `tourDates[]`: M?i slot c� th�ng tin gi� ri�ng
  - `originalPrice`: Gi� g?c
  - `finalPrice`: Gi� sau gi?m
  - `isEarlyBirdApplicable`: Slot n�y c� �p d?ng early bird kh�ng
  - `earlyBirdDiscountPercent`: % gi?m gi�

- `earlyBirdInfo`: Th�ng tin t?ng quan
  - `isActive`: Early bird c�n hi?u l?c
  - `discountPercent`: % gi?m gi�
  - `endDate`: Ng�y k?t th�c
  - `daysRemaining`: Ng�y c�n l?i
  - `description`: M� t? chi ti?t
  - `originalPrice`: Gi� g?c
  - `discountedPrice`: Gi� sau gi?m
  - `savingsAmount`: S? ti?n ti?t ki?m
  - `isExpiringSoon`: S?p h?t h?n (? 3 ng�y)
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

## C�ch FE s? d?ng th�ng tin Early Bird

### 1. Hi?n th? badge Early Bird
```javascript
if (tour.isEarlyBirdActive) {
  showEarlyBirdBadge(`Gi?m ${tour.earlyBirdDiscountPercent}%`);
}
```

### 2. Hi?n th? gi� v?i gi?m gi�
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
  showCountdown(`C�n ${tour.daysRemainingForEarlyBird} ng�y gi?m gi�!`);
}
```

### 4. C?nh b�o s?p h?t h?n
```javascript
if (tour.earlyBirdInfo?.isExpiringSoon) {
  showUrgentMessage("?? ?u ?�i s?p k?t th�c!");
}
```

### 5. T�nh t?ng ti?n v?i early bird
```javascript
const totalPrice = tour.finalPrice * numberOfGuests;
const savings = (tour.price - tour.finalPrice) * numberOfGuests;

if (savings > 0) {
  showSavingsMessage(`B?n ti?t ki?m ???c ${formatCurrency(savings)}!`);
}
```

## L?u � cho Developer

1. **Always check `isEarlyBirdActive`** tr??c khi hi?n th? th�ng tin early bird
2. **Use `finalPrice`** ?? t�nh t?ng ti?n, kh�ng ph?i `price`
3. **Show countdown** khi `daysRemainingForEarlyBird > 0`
4. **Highlight urgency** khi `isExpiringSoon = true`
5. **Display savings** ?? khuy?n kh�ch kh�ch h�ng ??t s?m

## Test Cases

### Early Bird Active
- Tour ???c t?o 5 ng�y tr??c
- Tour kh?i h�nh 45 ng�y n?a
- Expected: `isEarlyBirdActive = true`, `discountPercent = 25%`

### Early Bird Expired
- Tour ???c t?o 20 ng�y tr??c  
- Expected: `isEarlyBirdActive = false`, `pricingType = "Standard"`

### Tour Too Soon
- Tour kh?i h�nh 20 ng�y n?a
- Expected: `isEarlyBirdActive = false` (kh�ng ?? 30 ng�y)