# Enhanced Early Bird Pricing Information for Frontend

## ?? T?ng quan C?p nh?t
H? th?ng Early Bird ?� ???c c?p nh?t v?i logic m?i ?? t?o ?i?u ki?n t?t h?n cho kh�ch h�ng.

## ?? Enhanced Early Bird Logic
- **Th?i gian �p d?ng**: 14 ng�y ??u sau khi tour ???c **C�NG KHAI** (thay v� ng�y t?o)
- **?i?u ki?n**: Tour ph?i kh?i h�nh sau �t nh?t 30 ng�y t? ng�y ??t (gi? nguy�n)
- **M?c gi?m gi�**: 25% tr�n gi� g?c (gi? nguy�n)
- **??c bi?t**: N?u t? ng�y c�ng khai ??n slot ??u ti�n < 14 ng�y ? Early Bird k�o d�i ??n t?n ng�y slot

## ?? Thay ??i quan tr?ng

### Logic C? vs Logic M?i
| Aspect | Logic C? | Logic M?i |
|--------|----------|-----------|
| Th?i gian t�nh | 15 ng�y t? ng�y **t?o** tour | 14 ng�y t? ng�y **c�ng khai** tour |
| ?i?u ki?n ??c bi?t | Kh�ng | K�o d�i ??n ng�y slot n?u < 14 ng�y |
| T�nh t? | `TourDetails.CreatedAt` | `TourDetails.UpdatedAt` (khi status = Public) |

### V� d? Logic M?iScenario 1: Tour b�nh th??ng
- Ng�y c�ng khai: 01/01/2024
- Slot ??u ti�n: 20/01/2024 (19 ng�y sau c�ng khai)
- Early Bird: 01/01 ? 15/01 (14 ng�y ??u)

Scenario 2: Tour c�ng khai mu?n
- Ng�y c�ng khai: 10/01/2024  
- Slot ??u ti�n: 20/01/2024 (10 ng�y sau c�ng khai)
- Early Bird: 10/01 ? 20/01 (k�o d�i h?t 10 ng�y)
## API Endpoints v� Th�ng tin Enhanced Early Bird

### 1. GET `/api/UserTourBooking/available-tours`

**Enhanced Response Structure:**{
  "items": [
    {
      "tourDetailsId": "guid",
      "title": "Tour Name",
      "price": 1000000,
      "isEarlyBirdActive": true,
      "earlyBirdPrice": 750000,
      "earlyBirdDiscountPercent": 25,
      "earlyBirdDiscountAmount": 250000,
      "earlyBirdEndDate": "2024-01-15T23:59:59",
      "daysRemainingForEarlyBird": 3,
      "pricingType": "Early Bird",
      "finalPrice": 750000,
      "hasEarlyBirdDiscount": true,
      
      // NEW: Enhanced fields
      "tourPublicDate": "2024-01-01T00:00:00",
      "earlyBirdWindowDays": 10,
      "earlyBirdDescription": "Early Bird k�o d�i ??n ng�y tour v� ch? c�n 10 ng�y"
    }
  ]
}
### 2. GET `/api/UserTourBooking/tour-details/{id}`

**Enhanced Response Structure:**{
  "id": "guid",
  "title": "Tour Name",
  "tourDates": [
    {
      "tourSlotId": "guid",
      "tourDate": "2024-01-20T00:00:00",
      "originalPrice": 1000000,
      "finalPrice": 750000,
      "isEarlyBirdApplicable": true,
      "earlyBirdDiscountPercent": 25
    }
  ],
  "earlyBirdInfo": {
    "isActive": true,
    "discountPercent": 25,
    "endDate": "2024-01-20T00:00:00",
    "daysRemaining": 3,
    "description": "??t s?m ti?t ki?m 25% trong 3 ng�y c�n l?i! (Early Bird t? 01/01/2024 Early Bird k�o d�i ??n ng�y tour v� ch? c�n 10 ng�y)",
    "originalPrice": 1000000,
    "discountedPrice": 750000,
    "savingsAmount": 250000,
    "isExpiringSoon": true,
    "displayMessage": "Gi?m 25% - C�n 3 ng�y!"
  }
}
### 3. POST `/api/UserTourBooking/calculate-price`

**Enhanced Response Structure:**{
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
  "isAvailable": true,
  
  // NEW: Enhanced pricing info
  "tourPublicDate": "2024-01-01T00:00:00",
  "earlyBirdWindowDays": 10,
  "daysFromPublicToTourStart": 10,
  "earlyBirdEndDate": "2024-01-20T00:00:00",
  "earlyBirdDescription": "Early Bird k�o d�i ??n ng�y tour v� ch? c�n 10 ng�y"
}
## ?? Testing Endpoints

### Enhanced Test EndpointsGET /api/Testing/early-bird-pricing-tests
GET /api/Testing/test-early-bird-pricing?daysSincePublic=5&daysUntilTour=45
GET /api/Testing/early-bird-performance-test
**Test Parameters Changed:**
- `daysSinceCreated` ? `daysSincePublic`
- Reflects new logic based on public date

## C�ch FE s? d?ng Enhanced Early Bird

### 1. Hi?n th? Enhanced Early Bird Badgeif (tour.isEarlyBirdActive) {
  const description = tour.earlyBirdDescription || `Gi?m ${tour.earlyBirdDiscountPercent}%`;
  showEarlyBirdBadge(description);
  
  // Show special message for extended Early Bird
  if (tour.earlyBirdWindowDays < 14) {
    showSpecialMessage(`?? Early Bird ??c bi?t: k�o d�i ??n ng�y tour!`);
  }
}
### 2. Enhanced Countdown Displayif (tour.isEarlyBirdActive && tour.daysRemainingForEarlyBird > 0) {
  const isExtended = tour.earlyBirdWindowDays < 14;
  const message = isExtended 
    ? `C�n ${tour.daysRemainingForEarlyBird} ng�y ??n ng�y tour!`
    : `C�n ${tour.daysRemainingForEarlyBird} ng�y Early Bird!`;
  
  showCountdown(message);
}
### 3. Enhanced Information Displayif (tour.earlyBirdInfo?.isActive) {
  showEarlyBirdInfo({
    discount: `${tour.earlyBirdInfo.discountPercent}%`,
    description: tour.earlyBirdInfo.description,
    timeRemaining: `${tour.earlyBirdInfo.daysRemaining} ng�y`,
    specialNote: tour.earlyBirdWindowDays < 14 
      ? "?u ?�i ??c bi?t k�o d�i ??n ng�y tour!"
      : null
  });
}
## ?? L?u � cho Developer

### 1. Backward Compatibility
- C�c field c? v?n ???c gi? ?? kh�ng ph� v? code hi?n t?i
- `daysSinceCreated` b�y gi? th?c ch?t l� `daysSincePublic`

### 2. New Fields to Use// Use these new fields for better UX
tour.earlyBirdWindowDays      // S? ng�y Early Bird th?c t?
tour.earlyBirdDescription     // M� t? logic ???c �p d?ng
tour.daysFromPublicToTourStart // ?? hi?n th? th�ng tin debug
### 3. Enhanced Logic Handling// Check if Early Bird is extended to tour start
const isExtendedEarlyBird = tour.earlyBirdWindowDays < 14;

if (isExtendedEarlyBird) {
  showSpecialPromotion("?? ?u ?�i ??c bi?t: Early Bird k�o d�i ??n ng�y tour!");
}
## ?? Enhanced Test Cases

### Test Case 1: Enhanced Early Bird Active// Tour c�ng khai: 01/01/2024
// Booking: 05/01/2024 (4 ng�y sau c�ng khai < 14)
// Tour start: 15/02/2024 (41 ng�y sau booking >= 30)
// Expected: Early Bird active, 25% discount
### Test Case 2: Extended Early Bird (< 14 days)// Tour c�ng khai: 10/01/2024
// Tour start: 20/01/2024 (10 ng�y sau c�ng khai)
// Booking: 15/01/2024 (5 ng�y sau c�ng khai)
// Expected: Early Bird active (extended to tour start)
### Test Case 3: Early Bird Expired// Tour c�ng khai: 01/01/2024
// Booking: 20/01/2024 (19 ng�y sau c�ng khai > 14)
// Expected: Standard pricing, no discount
## ?? Benefits c?a Enhanced Logic

1. **Kh�ch h�ng ???c l?i h?n**: C� Early Bird ngay c? khi tour c�ng khai mu?n
2. **Tour company linh ho?t h?n**: C� th? c�ng khai tour mu?n m� v?n c� Early Bird
3. **UX t?t h?n**: Th�ng tin Early Bird r� r�ng, minh b?ch
4. **Business logic h?p l�**: T�nh t? ng�y kh�ch c� th? ??t (c�ng khai) thay v� ng�y t?o

## ?? Migration Guide

### For existing frontend code:
1. **Kh�ng c?n thay ??i g�** - c�c field c? v?n ho?t ??ng
2. **Khuy?n ngh?**: S? d?ng `earlyBirdDescription` ?? hi?n th? th�ng tin chi ti?t
3. **Optional**: Implement special handling cho extended Early Bird cases

### For new frontend features:
- S? d?ng enhanced fields ?? t?o UX t?t h?n
- Hi?n th? th�ng tin v? logic Early Bird ???c �p d?ng
- Show special promotions cho extended Early Bird cases