# ?? **QR Code Pricing Enhancement**

## ? **V?n ?? c?**
Tr??c ?�y, QR code ch? ch?a `TotalPrice` (gi� cu?i c�ng sau discount) m� kh�ng c� th�ng tin v?:
- Gi� g?c ban ??u
- Ph?n tr?m gi?m gi�  
- Lo?i pricing (Early Bird hay Standard)

## ? **Gi?i ph�p m?i**

### **QR Code Data Structure v2.0**
```json
{
  "bookingId": "guid-123",
  "bookingCode": "TB202501101234567",
  
  // ENHANCED PRICING INFORMATION
  "originalPrice": 1000000,     // Gi� g?c (tr??c discount)
  "discountPercent": 25,        // % gi?m gi� (Early Bird = 25%)
  "totalPrice": 750000,         // Gi� cu?i c�ng (sau discount)
  "priceType": "Early Bird",    // Lo?i pricing
  
  "numberOfGuests": 2,
  "status": "Confirmed",
  "generatedAt": "2025-01-10T10:30:00Z",
  "version": "2.0"              // Version tracking
}
```

### **So s�nh v1.0 vs v2.0**

| Field | v1.0 | v2.0 |
|-------|------|------|
| `totalPrice` | ? Gi� cu?i | ? Gi� cu?i |
| `originalPrice` | ? Kh�ng c� | ? Gi� g?c |
| `discountPercent` | ? Kh�ng c� | ? % gi?m |
| `priceType` | ? Kh�ng c� | ? Early Bird/Standard |
| `version` | ? Kh�ng c� | ? "2.0" |

## ?? **V� d? th?c t?**

### **Early Bird Booking**
```json
{
  "originalPrice": 1000000,
  "discountPercent": 25,
  "totalPrice": 750000,
  "priceType": "Early Bird"
}
```
?? *Kh�ch ???c gi?m 25% = ti?t ki?m 250,000 VN?*

### **Standard Booking**  
```json
{
  "originalPrice": 1000000,
  "discountPercent": 0,
  "totalPrice": 1000000,
  "priceType": "Standard"
}
```
?? *Kh�ch ??t mu?n = kh�ng c� discount*

## ?? **L?i �ch**

1. **Transparency**: Kh�ch h�ng th?y r� s? ti?n ti?t ki?m ???c
2. **Verification**: HDV c� th? verify gi� tr? booking ch�nh x�c
3. **Analytics**: C� th? ph�n t�ch hi?u qu? Early Bird pricing
4. **Backward Compatible**: V?n h? tr? QR codes v1.0 c?

## ?? **Implementation Details**

### **Files Updated:**
- ? `QRCodeService.cs` - Enhanced data generation
- ? `UserTourBookingService.cs` - Enhanced data generation  
- ? `QRCodeService.ValidateQRCodeData()` - Support v2.0 validation

### **API Impact:**
- ? **Kh�ng thay ??i API endpoint** 
- ? **Backward compatible** v?i QR codes c?
- ? **Enhanced data** trong QR code m?i

## ?? **Result**

**Tr??c**: QR code ch? c� `totalPrice: 750000`  
**Sau**: QR code c� ??y ?? th�ng tin pricing transparent

```json
// OLD v1.0
{
  "totalPrice": 750000
}

// NEW v2.0  
{
  "originalPrice": 1000000,
  "discountPercent": 25,
  "totalPrice": 750000,
  "priceType": "Early Bird"
}
```

**Gi? HDV v� kh�ch h�ng ??u th?y r� gi� tr? th?c s? c?a Early Bird discount!** ??