# Enhanced Early Bird Pricing Information for Frontend

## ?? T?ng quan C?p nh?t
H? th?ng Early Bird ?ã ???c c?p nh?t v?i logic m?i ?? t?o ?i?u ki?n t?t h?n cho khách hàng.

## ?? Enhanced Early Bird Logic
- **Th?i gian áp d?ng**: 14 ngày ??u sau khi tour ???c **CÔNG KHAI** (thay vì ngày t?o)
- **?i?u ki?n**: Tour ph?i kh?i hành sau ít nh?t 30 ngày t? ngày ??t (gi? nguyên)
- **M?c gi?m giá**: 25% trên giá g?c (gi? nguyên)
- **??c bi?t**: N?u t? ngày công khai ??n slot ??u tiên < 14 ngày ? Early Bird kéo dài ??n t?n ngày slot

## ?? Thay ??i quan tr?ng

### Logic C? vs Logic M?i
| Aspect | Logic C? | Logic M?i |
|--------|----------|-----------|
| Th?i gian tính | 15 ngày t? ngày **t?o** tour | 14 ngày t? ngày **công khai** tour |
| ?i?u ki?n ??c bi?t | Không | Kéo dài ??n ngày slot n?u < 14 ngày |
| Tính t? | `TourDetails.CreatedAt` | `TourDetails.UpdatedAt` (khi status = Public) |

### Ví d? Logic M?iScenario 1: Tour bình th??ng
- Ngày công khai: 01/01/2024
- Slot ??u tiên: 20/01/2024 (19 ngày sau công khai)
- Early Bird: 01/01 ? 15/01 (14 ngày ??u)

Scenario 2: Tour công khai mu?n
- Ngày công khai: 10/01/2024  
- Slot ??u tiên: 20/01/2024 (10 ngày sau công khai)
- Early Bird: 10/01 ? 20/01 (kéo dài h?t 10 ngày)
## API Endpoints và Thông tin Enhanced Early Bird

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
      "earlyBirdDescription": "Early Bird kéo dài ??n ngày tour vì ch? còn 10 ngày"
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
    "description": "??t s?m ti?t ki?m 25% trong 3 ngày còn l?i! (Early Bird t? 01/01/2024 Early Bird kéo dài ??n ngày tour vì ch? còn 10 ngày)",
    "originalPrice": 1000000,
    "discountedPrice": 750000,
    "savingsAmount": 250000,
    "isExpiringSoon": true,
    "displayMessage": "Gi?m 25% - Còn 3 ngày!"
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
  "earlyBirdDescription": "Early Bird kéo dài ??n ngày tour vì ch? còn 10 ngày"
}
## ?? Testing Endpoints

### Enhanced Test EndpointsGET /api/Testing/early-bird-pricing-tests
GET /api/Testing/test-early-bird-pricing?daysSincePublic=5&daysUntilTour=45
GET /api/Testing/early-bird-performance-test
**Test Parameters Changed:**
- `daysSinceCreated` ? `daysSincePublic`
- Reflects new logic based on public date

## Cách FE s? d?ng Enhanced Early Bird

### 1. Hi?n th? Enhanced Early Bird Badgeif (tour.isEarlyBirdActive) {
  const description = tour.earlyBirdDescription || `Gi?m ${tour.earlyBirdDiscountPercent}%`;
  showEarlyBirdBadge(description);
  
  // Show special message for extended Early Bird
  if (tour.earlyBirdWindowDays < 14) {
    showSpecialMessage(`?? Early Bird ??c bi?t: kéo dài ??n ngày tour!`);
  }
}
### 2. Enhanced Countdown Displayif (tour.isEarlyBirdActive && tour.daysRemainingForEarlyBird > 0) {
  const isExtended = tour.earlyBirdWindowDays < 14;
  const message = isExtended 
    ? `Còn ${tour.daysRemainingForEarlyBird} ngày ??n ngày tour!`
    : `Còn ${tour.daysRemainingForEarlyBird} ngày Early Bird!`;
  
  showCountdown(message);
}
### 3. Enhanced Information Displayif (tour.earlyBirdInfo?.isActive) {
  showEarlyBirdInfo({
    discount: `${tour.earlyBirdInfo.discountPercent}%`,
    description: tour.earlyBirdInfo.description,
    timeRemaining: `${tour.earlyBirdInfo.daysRemaining} ngày`,
    specialNote: tour.earlyBirdWindowDays < 14 
      ? "?u ?ãi ??c bi?t kéo dài ??n ngày tour!"
      : null
  });
}
## ?? L?u ý cho Developer

### 1. Backward Compatibility
- Các field c? v?n ???c gi? ?? không phá v? code hi?n t?i
- `daysSinceCreated` bây gi? th?c ch?t là `daysSincePublic`

### 2. New Fields to Use// Use these new fields for better UX
tour.earlyBirdWindowDays      // S? ngày Early Bird th?c t?
tour.earlyBirdDescription     // Mô t? logic ???c áp d?ng
tour.daysFromPublicToTourStart // ?? hi?n th? thông tin debug
### 3. Enhanced Logic Handling// Check if Early Bird is extended to tour start
const isExtendedEarlyBird = tour.earlyBirdWindowDays < 14;

if (isExtendedEarlyBird) {
  showSpecialPromotion("?? ?u ?ãi ??c bi?t: Early Bird kéo dài ??n ngày tour!");
}
## ?? Enhanced Test Cases

### Test Case 1: Enhanced Early Bird Active// Tour công khai: 01/01/2024
// Booking: 05/01/2024 (4 ngày sau công khai < 14)
// Tour start: 15/02/2024 (41 ngày sau booking >= 30)
// Expected: Early Bird active, 25% discount
### Test Case 2: Extended Early Bird (< 14 days)// Tour công khai: 10/01/2024
// Tour start: 20/01/2024 (10 ngày sau công khai)
// Booking: 15/01/2024 (5 ngày sau công khai)
// Expected: Early Bird active (extended to tour start)
### Test Case 3: Early Bird Expired// Tour công khai: 01/01/2024
// Booking: 20/01/2024 (19 ngày sau công khai > 14)
// Expected: Standard pricing, no discount
## ?? Benefits c?a Enhanced Logic

1. **Khách hàng ???c l?i h?n**: Có Early Bird ngay c? khi tour công khai mu?n
2. **Tour company linh ho?t h?n**: Có th? công khai tour mu?n mà v?n có Early Bird
3. **UX t?t h?n**: Thông tin Early Bird rõ ràng, minh b?ch
4. **Business logic h?p lý**: Tính t? ngày khách có th? ??t (công khai) thay vì ngày t?o

## ?? Migration Guide

### For existing frontend code:
1. **Không c?n thay ??i gì** - các field c? v?n ho?t ??ng
2. **Khuy?n ngh?**: S? d?ng `earlyBirdDescription` ?? hi?n th? thông tin chi ti?t
3. **Optional**: Implement special handling cho extended Early Bird cases

### For new frontend features:
- S? d?ng enhanced fields ?? t?o UX t?t h?n
- Hi?n th? thông tin v? logic Early Bird ???c áp d?ng
- Show special promotions cho extended Early Bird cases