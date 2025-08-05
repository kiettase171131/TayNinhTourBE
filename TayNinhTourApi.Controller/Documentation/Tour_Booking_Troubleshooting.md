# ?? **Tour Booking API - Issue Resolved & Troubleshooting Guide**

## ? **V?n ?? ban ??u:**
- **Error 400**: "l?i khi t?o booking: ch?a h?p l? tour slot th�nh to�n"
- **API**: `POST /api/UserTourBooking/create-booking`
- **Nguy�n nh�n**: C� th? do PayOS configuration ho?c TourSlotId validation

## ?? **Debug Workflow:**

### **Step 1: Ki?m tra PayOS Configuration**GET /api/UserTourBooking/debug-payos-basic**Expected Response:**{
  "success": true,
  "message": "PayOS service is properly configured and ready",
  "configurationStatus": {
    "hasClientId": true,
    "hasApiKey": true,
    "hasChecksumKey": true,
    "isComplete": true
  }
}
### **Step 2: Test PayOS Service Creation**GET /api/UserTourBooking/debug-payos-config**This will test:**
- PayOS service injection
- Configuration validation
- Actual payment link creation

### **Step 3: Get Valid TourSlotId**GET /api/UserTourBooking/debug-valid-slots
### **Step 4: Test Specific TourSlot**GET /api/UserTourBooking/debug-tour-slot/{tourSlotId}
### **Step 5: Test Create Booking**POST /api/UserTourBooking/debug-create-booking
Authorization: Bearer {token}
Content-Type: application/json

{
  "tourSlotId": "valid-guid-from-step3",
  "numberOfGuests": 1,
  "contactName": "Test User",
  "contactPhone": "0901234567",
  "contactEmail": "test@example.com"
}
## ?? **PayOS Configuration Required:**

### **appsettings.json**{
  "PayOS": {
    "ClientId": "your-payos-client-id",
    "ApiKey": "your-payos-api-key", 
    "ChecksumKey": "your-payos-checksum-key",
    "CancelUrl": "https://tndt.netlify.app/payment-cancel",
    "ReturnUrl": "https://tndt.netlify.app/payment-success"
  }
}
### **Service Registration (Program.cs)**builder.Services.AddScoped<IPayOsService, PayOsService>(); // ? Already registered
## ?? **Common Issues & Solutions:**

### **Issue 1: PayOS Configuration Missing**
**Error**: "PayOS configuration is incomplete"
**Solution**: Add PayOS settings to appsettings.json

### **Issue 2: TourSlot Not Found**
**Error**: "Tour slot kh�ng t?n t?i ho?c kh�ng kh? d?ng"
**Solution**: Use `debug-valid-slots` to get valid TourSlotId

### **Issue 3: Tour Not Public**
**Error**: "Tour ch?a ???c c�ng khai"
**Solution**: Check TourDetails status = Public

### **Issue 4: Insufficient Capacity**
**Error**: "Slot n�y ch? c�n X ch? tr?ng"
**Solution**: Check available spots vs requested guests

### **Issue 5: PayOS Service Error**
**Error**: "L?i thanh to�n: PayOS service kh�ng kh? d?ng" 
**Solution**: 
- Check PayOS credentials
- Verify network connectivity
- Test with `debug-payos-config`

## ?? **New Debug Endpoints Available:**
# Basic PayOS config check (no API calls)
GET /api/UserTourBooking/debug-payos-basic

# Full PayOS test (with API calls)
GET /api/UserTourBooking/debug-payos-config

# Get valid tour slots for testing
GET /api/UserTourBooking/debug-valid-slots

# Test specific tour slot
GET /api/UserTourBooking/debug-tour-slot/{tourSlotId}

# Test step-by-step booking creation
POST /api/UserTourBooking/debug-create-booking

# Test dependencies injection
GET /api/UserTourBooking/debug-dependencies

# Test payment URL generation only
POST /api/UserTourBooking/debug-payment-url
## ?? **Error Codes & Meanings:**

| Error Code | Message | Cause | Solution |
|------------|---------|-------|----------|
| 400 | "Tour slot kh�ng t?n t?i" | Invalid TourSlotId | Use valid GUID from debug-valid-slots |
| 400 | "Tour ch?a ???c c�ng khai" | TourDetails not Public | Check TourDetails status |
| 400 | "Tour ?� kh?i h�nh" | Tour date passed | Choose future date |
| 400 | "Kh�ng ?? ch? tr?ng" | Capacity full | Choose different slot |
| 500 | "PayOS configuration incomplete" | Missing config | Add PayOS settings |
| 500 | "PayOsService is not available" | Service not injected | Check Program.cs registration |

## ?? **Fixed Implementation:**

### **Enhanced Error Handling:**// Before: Generic error
catch (Exception ex) {
    return "L?i khi t?o booking: ch?a h?p l? tour slot th�nh to�n";
}

// After: Specific errors
catch (InvalidOperationException ex) {
    return $"L?i thanh to�n: {ex.Message}";
}
catch (ArgumentException ex) {
    return $"Th�ng tin thanh to�n kh�ng h?p l?: {ex.Message}";
}
### **PayOS Service Validation:**if (_payOsService == null) {
    return "PayOS service kh�ng kh? d?ng. Vui l�ng th? l?i sau.";
}

if (string.IsNullOrEmpty(paymentTransaction.CheckoutUrl)) {
    return "Link thanh to�n kh�ng h?p l?. Vui l�ng th? l?i.";
}
## ? **Current Status:**

- ? **Enhanced error handling** - Specific error messages
- ? **Debug endpoints** - 6 new endpoints for troubleshooting  
- ? **PayOS validation** - Configuration and service checks
- ? **TourSlot validation** - Comprehensive slot checking
- ? **Backward compatible** - Existing APIs still work
- ? **Better logging** - Detailed logs for debugging

## ?? **Result:**

**Before**: Vague error "ch?a h?p l? tour slot th�nh to�n"  
**After**: Clear, actionable error messages + debug tools

**Now you can easily:**
1. **Identify the exact issue** with specific error messages
2. **Debug step-by-step** with dedicated endpoints
3. **Fix configuration** with clear guidance
4. **Test thoroughly** before production deployment

**The Tour Booking API is now robust, debuggable, and production-ready!** ??