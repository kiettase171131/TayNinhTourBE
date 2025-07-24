# ?? H??NG D?N TEST VOUCHER SYSTEM (C� CH?C N?NG CLAIM)

## ?? OVERVIEW
H? th?ng voucher ?� ???c n�ng c?p v?i ch?c n?ng claiming:
- **Admin t?o voucher** template v?i t�n, s? l??ng v� ph?n tr?m gi?m gi�
- **H? th?ng t? ??ng t?o** c�c m� voucher ng?u nhi�n duy nh?t
- **Kh�ch h�ng claim/nh?n** voucher codes v� l?u v�o kho c� nh�n
- **Kh�ch h�ng s? d?ng** voucher t? kho c� nh�n khi checkout
- **Theo d�i vi?c claiming v� usage** voucher

---

## ?? C�I ??T TR??C KHI TEST

### 1. Chu?n b? Database-- Ki?m tra tables ?� ???c t?o v?i fields m?i
SELECT * FROM Vouchers;
SELECT * FROM VoucherCodes;

-- Ki?m tra structure c?a VoucherCodes
DESCRIBE VoucherCodes;
-- C?n c� c�c fields: IsClaimed, ClaimedByUserId, ClaimedAt, IsUsed, UsedByUserId, UsedAt

-- Reset data n?u c?n
DELETE FROM VoucherCodes;
DELETE FROM Vouchers;
### 2. Chu?n b? Token Authentication
- **Admin Token**: C?n role "Admin" ?? t?o/s?a/x�a voucher
- **Customer Token**: ?? claim, xem voucher codes v� s? d?ng trong checkout

### 3. Chu?n b? Test Data
- �t nh?t 1 product c� trong h? th?ng
- Cart items ?? test checkout v?i voucher
- Nhi?u user accounts ?? test claiming

---

## ?? TEST CASES M?I - VOUCHER CLAIMING SYSTEM

### **LU?NG 1: ADMIN T?O VOUCHER (Kh�ng ??i)**

#### Test Case 1.1: T?o voucher v?i nhi?u codesPOST /api/Product/Create-Voucher
Authorization: Bearer {admin-token}
Content-Type: application/json

{
    "name": "T?t 2025 Sale",
    "quantity": 10,
    "discountAmount": 0,
    "discountPercent": 25,
    "startDate": "2024-12-25T00:00:00Z",
    "endDate": "2025-01-15T23:59:59Z"
}
**Expected Result:**
- StatusCode: 200
- GeneratedCodes: Array v?i 10 m� codes kh�c nhau
- T?t c? codes c� IsClaimed = false, ClaimedByUserId = null

**Verify Database:**SELECT * FROM Vouchers WHERE Name = 'T?t 2025 Sale';
SELECT * FROM VoucherCodes WHERE VoucherId = '{voucher-id}';
-- T?t c? codes ph?i c� IsClaimed = 0, ClaimedByUserId = NULL
---

### **LU?NG 2: CUSTOMER XEM V� CLAIM VOUCHER**

#### Test Case 2.1: Xem voucher codes c� th? claimGET /api/Product/GetAvailable-VoucherCodes?pageIndex=1&pageSize=20
**Expected Result:**
- StatusCode: 200
- Data: Array c�c voucher codes ch?a ???c claim
- M?i item g?m:
  - `VoucherCodeId`: Guid c?a voucher code
  - `VoucherName`: "T?t 2025 Sale"
  - `Code`: M� voucher (VD: "TET2-ABC123-4567")
  - `DiscountPercent`: 25
  - `CanClaim`: true (n?u ch?a h?t h?n)

**Verify:** Ch? hi?n th? codes c� IsClaimed = false

#### Test Case 2.2: Claim voucher code th�nh c�ngPOST /api/Product/Claim-VoucherCode/{voucher-code-id}
Authorization: Bearer {customer-token}
**Expected Result:**
- StatusCode: 200
- Message: "Nh?n m� voucher th�nh c�ng!"
- VoucherCode: Object ch?a th�ng tin voucher v?a claim

**Verify Database:**SELECT * FROM VoucherCodes WHERE Id = '{voucher-code-id}';
-- IsClaimed = 1, ClaimedByUserId = {user-id}, ClaimedAt = current time
#### Test Case 2.3: Claim voucher ?� ???c claim b?i user kh�cPOST /api/Product/Claim-VoucherCode/{already-claimed-voucher-code-id}
Authorization: Bearer {another-customer-token}
**Expected Result:**
- StatusCode: 404
- Message: "M� voucher kh�ng t?n t?i ho?c ?� ???c nh?n b?i ng??i kh�c."

#### Test Case 2.4: User claim multiple codes t? c�ng voucherPOST /api/Product/Claim-VoucherCode/{another-code-from-same-voucher}
Authorization: Bearer {customer-token}
**Expected Result:**
- StatusCode: 400
- Message: "B?n ?� nh?n m?t m� voucher t? ch??ng tr�nh n�y r?i."

---

### **LU?NG 3: CUSTOMER QU?N L� KHO VOUCHER C� NH�N**

#### Test Case 3.1: Xem t?t c? voucher c?a m�nhGET /api/Product/My-Vouchers?pageIndex=1&pageSize=10
Authorization: Bearer {customer-token}
**Expected Result:**
- StatusCode: 200
- Data: Array c�c voucher ?� claim
- ActiveCount: S? voucher ch?a s? d?ng v� ch?a h?t h?n
- UsedCount: S? voucher ?� s? d?ng
- ExpiredCount: S? voucher h?t h?n ch?a s? d?ng

#### Test Case 3.2: Filter voucher theo statusGET /api/Product/My-Vouchers?status=active
GET /api/Product/My-Vouchers?status=used
GET /api/Product/My-Vouchers?status=expired
Authorization: Bearer {customer-token}
**Expected Results:**
- **active**: Ch? voucher IsUsed=false v� EndDate>=now
- **used**: Ch? voucher IsUsed=true
- **expired**: Ch? voucher IsUsed=false v� EndDate<now

#### Test Case 3.3: Voucher status logic
**Verify c�c computed properties:**
- `IsExpired`: true n?u DateTime.UtcNow > EndDate
- `IsActive`: true n?u !IsExpired && !IsUsed
- `Status`: "?� s? d?ng" | "?� h?t h?n" | "C� th? s? d?ng"

---

### **LU?NG 4: CUSTOMER S? D?NG VOUCHER T? KHO C� NH�N**

#### Test Case 4.1: Checkout v?i voucher t? kho c� nh�nPOST /api/Product/CheckoutCart
Authorization: Bearer {customer-token}
Content-Type: application/json

{
    "cartItemIds": ["{cart-item-id}"],
    "myVoucherCodeId": "{voucher-code-id-from-my-vouchers}"
}
**Expected Result:**
- StatusCode: 200
- CheckoutUrl: URL PayOS
- TotalOriginal: Gi� g?c
- DiscountAmount: S? ti?n ???c gi?m (25%)
- TotalAfterDiscount: Gi� sau khi gi?m

#### Test Case 4.2: Checkout v?i voucher code tr?c ti?p (backward compatibility)POST /api/Product/CheckoutCart
Authorization: Bearer {customer-token}
Content-Type: application/json

{
    "cartItemIds": ["{cart-item-id}"],
    "voucherCode": "TET2-ABC123-4567"
}
**Expected Result:** V?n ho?t ??ng nh? c?

#### Test Case 4.3: ?u ti�n MyVoucherCodeId over VoucherCodePOST /api/Product/CheckoutCart
Authorization: Bearer {customer-token}
Content-Type: application/json

{
    "cartItemIds": ["{cart-item-id}"],
    "voucherCode": "INVALID-CODE",
    "myVoucherCodeId": "{valid-voucher-code-id}"
}
**Expected Result:** S? d?ng MyVoucherCodeId, ignore VoucherCode

#### Test Case 4.4: Checkout v?i voucher kh�ng thu?c v? userPOST /api/Product/CheckoutCart
Authorization: Bearer {customer-token}
Content-Type: application/json

{
    "cartItemIds": ["{cart-item-id}"],
    "myVoucherCodeId": "{other-user-voucher-code-id}"
}
**Expected Result:**
- StatusCode: 400
- Message: "Kh�ng t�m th?y m� voucher trong kho voucher c?a b?n."

#### Test Case 4.5: Checkout v?i voucher ?� s? d?ngPOST /api/Product/CheckoutCart
Authorization: Bearer {customer-token}
Content-Type: application/json

{
    "cartItemIds": ["{cart-item-id}"],
    "myVoucherCodeId": "{used-voucher-code-id}"
}
**Expected Result:**
- StatusCode: 400
- Message: "M� voucher n�y ?� ???c s? d?ng."

---

### **LU?NG 5: PAYMENT V� USAGE TRACKING**

#### Test Case 5.1: Ho�n th�nh payment v?i claimed voucher
1. Th?c hi?n checkout v?i myVoucherCodeId
2. M� ph?ng payment success t? PayOS
3. Trigger `ClearCartAndUpdateInventoryAsync`

**Verify sau khi payment:**-- Voucher code ?� ???c ?�nh d?u s? d?ng
SELECT * FROM VoucherCodes WHERE Id = '{voucher-code-id}';
-- IsUsed = 1, UsedByUserId = {user-id}, UsedAt = current time
-- ClaimedByUserId v?n gi? nguy�n

-- Order c� voucher code
SELECT * FROM Orders WHERE VoucherCode = '{voucher-code}';
#### Test Case 5.2: Ki?m tra statistics sau usageGET /api/Product/My-Vouchers
Authorization: Bearer {customer-token}
**Verify:**
- UsedCount t?ng l�n 1
- ActiveCount gi?m ?i 1
GET /api/Product/GetVoucher/{voucher-id}
Authorization: Bearer {admin-token}
**Verify:**
- ClaimedCount: S? codes ?� ???c claim
- UsedCount: S? codes ?� ???c s? d?ng
- RemainingCount: S? codes ch?a ???c claim

---

## ?? ADVANCED TEST CASES

### 1. Concurrent Claiming
**Scenario:** 2 users c�ng claim 1 voucher code// User A v� User B c�ng POST claim request
// Ch? 1 user th�nh c�ng, user c�n l?i nh?n l?i 404
### 2. Voucher Expiry During Claim Process
**Setup:** T?o voucher v?i EndDate g?n h?t h?n
**Test:** Claim voucher khi voucher ?ang h?t h?n
**Expected:** Kh�ng cho ph�p claim

### 3. User Experience Flows
**Test Case 3.1: Full Customer Journey**
1. User xem available vouchers
2. User claim m?t voucher
3. User xem My Vouchers (voucher xu?t hi?n)
4. User checkout v?i voucher
5. Payment success
6. User xem My Vouchers (voucher status = "?� s? d?ng")

### 4. Admin Management v?i Claimed Vouchers
**Test Case 4.1: Admin update voucher c� codes ?� claim**PUT /api/Product/Update-Voucher/{voucher-id}
{
    "discountPercent": 30,
    "isActive": false
}**Verify:** Claimed codes v?n ho?t ??ng v?i discount c?

**Test Case 4.2: Admin delete voucher c� codes ?� claim**DELETE /api/Product/Voucher/{voucher-id}**Verify:** Claimed codes v?n s? d?ng ???c

### 5. Data Integrity Tests
**Test Case 5.1: Orphaned voucher codes**
- Verify: Kh�ng c� VoucherCode n�o c� VoucherId kh�ng t?n t?i

**Test Case 5.2: Claiming validation**
- User kh�ng th? claim > 1 code t? c�ng voucher
- Code ?� claim kh�ng xu?t hi?n trong available list

---

## ?? PERFORMANCE TESTS

### 1. Load Test Claiming
- 100 users ??ng th?i claim t? pool 1000 voucher codes
- Measure: Response time, success rate, data consistency

### 2. My Vouchers Pagination
- User c� 500+ claimed vouchers
- Test pagination performance v?i filters

---

## ?? CRITICAL SCENARIOS

### 1. Race Conditions
- Multiple users claiming same code
- User claiming while admin deactivates voucher
- Payment processing while voucher expires

### 2. Business Logic Integrity
- Claimed voucher count consistency
- Usage statistics accuracy
- Voucher lifecycle management

---

## ?? FRONTEND INTEGRATION SCENARIOS

### 1. Voucher Discovery Flow// 1. User browse available vouchers
GET /api/Product/GetAvailable-VoucherCodes

// 2. User claim voucher
POST /api/Product/Claim-VoucherCode/{id}

// 3. Show success message v� redirect to My Vouchers
### 2. Checkout Flow// 1. User ? cart page, xem My Vouchers
GET /api/Product/My-Vouchers?status=active

// 2. User ch?n voucher ?? apply
POST /api/Product/CheckoutCart
{
    "cartItemIds": [...],
    "myVoucherCodeId": "selected-voucher-id"
}
### 3. Voucher Management Flow// Dashboard hi?n th? voucher statistics
GET /api/Product/My-Vouchers // ?? l?y ActiveCount, UsedCount, ExpiredCount

// Filter vouchers by status
GET /api/Product/My-Vouchers?status=active
GET /api/Product/My-Vouchers?status=used
GET /api/Product/My-Vouchers?status=expired
---

## ?? SUCCESS CRITERIA

### Functional Requirements
- ? Users c� th? claim voucher codes
- ? Users c� th? xem kho voucher c� nh�n
- ? Users c� th? s? d?ng voucher t? kho c� nh�n
- ? Statistics claiming/usage ch�nh x�c
- ? Backward compatibility v?i voucher code tr?c ti?p

### Technical Requirements
- ? Race condition handling
- ? Data consistency
- ? Performance acceptable (< 1s cho claim, < 2s cho My Vouchers)
- ? Database integrity constraints

### Business Requirements
- ? User ch? claim ???c 1 code per voucher template
- ? Claimed vouchers kh�ng th? claim b?i user kh�c
- ? Used vouchers kh�ng th? reuse
- ? Expired vouchers kh�ng th? claim/use

---

## ??? AUTOMATION TEST EXAMPLES

### Postman Collection Setup// Pre-request Script
pm.globals.set("baseUrl", "https://api.tayninhtrip.com");
pm.globals.set("adminToken", "admin-jwt-token");
pm.globals.set("customerToken", "customer-jwt-token");

// Test Script cho Claim Voucher
pm.test("Claim voucher successful", function () {
    pm.response.to.have.status(200);
    const response = pm.response.json();
    pm.expect(response.success).to.be.true;
    pm.expect(response.voucherCode).to.not.be.null;
    
    // Store claimed voucher ID for subsequent tests
    pm.globals.set("claimedVoucherId", response.voucherCode.voucherCodeId);
});
### Database Verification Queries-- Verify claim statistics
SELECT 
    v.Name,
    COUNT(vc.Id) as TotalCodes,
    COUNT(CASE WHEN vc.IsClaimed = 1 THEN 1 END) as ClaimedCount,
    COUNT(CASE WHEN vc.IsUsed = 1 THEN 1 END) as UsedCount
FROM Vouchers v
LEFT JOIN VoucherCodes vc ON v.Id = vc.VoucherId
WHERE v.Name = 'T?t 2025 Sale'
GROUP BY v.Id, v.Name;

-- Verify user voucher ownership
SELECT 
    u.Name as UserName,
    v.Name as VoucherName,
    vc.Code,
    vc.IsClaimed,
    vc.IsUsed,
    vc.ClaimedAt,
    vc.UsedAt
FROM Users u
JOIN VoucherCodes vc ON u.Id = vc.ClaimedByUserId
JOIN Vouchers v ON vc.VoucherId = v.Id
WHERE u.Id = '{user-id}';
---

H? th?ng voucher claiming ?� ???c thi?t k? ?? mang l?i tr?i nghi?m t??ng t? nh? c�c ?ng d?ng th??ng m?i ?i?n t? l?n, cho ph�p users t�ch tr? v� qu?n l� vouchers c� nh�n m?t c�ch hi?u qu?!