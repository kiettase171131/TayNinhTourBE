# H??NG D?N API LU?NG RÚT TI?N

## ?? T?ng quan lu?ng rút ti?n
1. **L?y danh sách ngân hàng h? tr?** (tùy ch?n)
2. **T?o/qu?n lý tài kho?n ngân hàng**
3. **T?o yêu c?u rút ti?n**
4. **Theo dõi tr?ng thái yêu c?u**
5. **Admin x? lý yêu c?u** (backend)

---

## ?? 1. L?y danh sách ngân hàng h? tr?

### `GET /api/BankAccount/supported-banks`
- **Method**: GET
- **Authentication**: Không c?n (AllowAnonymous)
- **Tham s?**: Không
- **Tác d?ng**: L?y danh sách 29+ ngân hàng h? tr? t?i Vi?t Nam + tùy ch?n "Ngân hàng khác"
- **Response**: Danh sách ngân hàng v?i tên hi?n th?, tên vi?t t?t

```json
{
  "isSuccess": true,
  "data": [
    {
      "value": 0,
      "name": "Vietcombank",
      "displayName": "Ngân hàng Ngo?i th??ng Vi?t Nam (Vietcombank)",
      "shortName": "VCB",
      "isActive": true
    },
    {
      "value": 999,
      "name": "Other",
      "displayName": "Ngân hàng khác",
      "shortName": "OTHER",
      "isActive": true
    }
  ]
}
```

---

## ?? 2. Qu?n lý tài kho?n ngân hàng

### 2.1 T?o tài kho?n ngân hàng
**`POST /api/BankAccount`**
- **Method**: POST
- **Authentication**: Bearer Token
- **Body** (Ch?n t? danh sách ngân hàng có s?n):
```json
{
  "supportedBankId": 0,
  "bankName": "Vietcombank",
  "accountNumber": "1234567890",
  "accountHolderName": "NGUYEN VAN A",
  "isDefault": true,
  "notes": "Tài kho?n chính"
}
```

- **Body** (Ch?n "Ngân hàng khác"):
```json
{
  "supportedBankId": 999,
  "bankName": "Vietcombank",
  "customBankName": "Ngân hàng ABC XYZ",
  "accountNumber": "1234567890",
  "accountHolderName": "NGUYEN VAN A",
  "isDefault": true,
  "notes": "Tài kho?n ngân hàng khác"
}
```

- **Body** (Backward compatibility - không dùng enum):
```json
{
  "bankName": "Tên ngân hàng t? do",
  "accountNumber": "1234567890",
  "accountHolderName": "NGUYEN VAN A",
  "isDefault": true,
  "notes": "Tài kho?n t? do"
}
```

- **Tác d?ng**: T?o tài kho?n ngân hàng ?? nh?n ti?n rút

### 2.2 L?y danh sách tài kho?n ngân hàng
**`GET /api/BankAccount/my-accounts`**
- **Method**: GET
- **Authentication**: Bearer Token
- **Tác d?ng**: L?y t?t c? tài kho?n ngân hàng c?a user hi?n t?i

### 2.3 L?y tài kho?n m?c ??nh
**`GET /api/BankAccount/default`**
- **Method**: GET
- **Authentication**: Bearer Token
- **Tác d?ng**: L?y tài kho?n ngân hàng m?c ??nh ?? pre-fill form

---

## ?? 3. T?o yêu c?u rút ti?n

### 3.1 Validate tr??c khi t?o yêu c?u
**`POST /api/WithdrawalRequest/validate`**
- **Method**: POST
- **Authentication**: Bearer Token
- **Body**:
```json
{
  "amount": 100000,
  "bankAccountId": "guid-id"
}
```
- **Tác d?ng**: Ki?m tra s? d? ví, tài kho?n ngân hàng h?p l?

### 3.2 Ki?m tra ?i?u ki?n t?o yêu c?u
**`GET /api/WithdrawalRequest/can-create`**
- **Method**: GET
- **Authentication**: Bearer Token
- **Tác d?ng**: Ki?m tra user có th? t?o yêu c?u m?i không (không có yêu c?u pending)

### 3.3 T?o yêu c?u rút ti?n
**`POST /api/WithdrawalRequest`**
- **Method**: POST
- **Authentication**: Bearer Token
- **Body**:
```json
{
  "bankAccountId": "guid-id",
  "amount": 100000,
  "userNotes": "Rút ti?n l??ng tháng 12"
}
```
- **Tác d?ng**: T?o yêu c?u rút ti?n v?i tr?ng thái Pending

---

## ?? 4. Theo dõi yêu c?u rút ti?n

### 4.1 L?y danh sách yêu c?u c?a user
**`GET /api/WithdrawalRequest/my-requests`**
- **Method**: GET
- **Authentication**: Bearer Token
- **Query Parameters**:
  - `status` (optional): 0=Pending, 1=Approved, 2=Rejected, 3=Cancelled
  - `pageNumber` (default: 1)
  - `pageSize` (default: 10)
- **Tác d?ng**: Xem l?ch s? và tr?ng thái các yêu c?u rút ti?n

### 4.2 L?y chi ti?t yêu c?u rút ti?n
**`GET /api/WithdrawalRequest/{id}`**
- **Method**: GET
- **Authentication**: Bearer Token
- **Tác d?ng**: Xem chi ti?t m?t yêu c?u rút ti?n c? th?

### 4.3 L?y yêu c?u g?n nh?t
**`GET /api/WithdrawalRequest/latest`**
- **Method**: GET
- **Authentication**: Bearer Token
- **Tác d?ng**: Xem yêu c?u rút ti?n g?n nh?t c?a user

### 4.4 H?y yêu c?u rút ti?n
**`PUT /api/WithdrawalRequest/{id}/cancel`**
- **Method**: PUT
- **Authentication**: Bearer Token
- **Body**:
```json
{
  "reason": "Lý do h?y yêu c?u"
}
```
- **Tác d?ng**: H?y yêu c?u rút ti?n ?ang ? tr?ng thái Pending

### 4.5 L?y th?ng kê rút ti?n
**`GET /api/WithdrawalRequest/stats`**
- **Method**: GET
- **Authentication**: Bearer Token
- **Tác d?ng**: Xem th?ng kê t?ng quan v? các yêu c?u rút ti?n

---

## ?? 5. Admin APIs (Backend x? lý)

### 5.1 L?y danh sách yêu c?u cho admin
**`GET /api/admin/withdrawals`**
- **Method**: GET
- **Authentication**: Admin Role
- **Tác d?ng**: Admin xem t?t c? yêu c?u rút ti?n

### 5.2 Duy?t yêu c?u rút ti?n
**`PUT /api/admin/withdrawals/{id}/approve`**
- **Method**: PUT
- **Authentication**: Admin Role
- **Tác d?ng**: Admin phê duy?t yêu c?u rút ti?n

### 5.3 T? ch?i yêu c?u rút ti?n
**`PUT /api/admin/withdrawals/{id}/reject`**
- **Method**: PUT
- **Authentication**: Admin Role
- **Tác d?ng**: Admin t? ch?i yêu c?u rút ti?n

---

## ?? Lu?ng Frontend khuy?n ngh?

### **1. Trang qu?n lý tài kho?n ngân hàng**: 
- G?i API 1 ?? hi?n th? dropdown ngân hàng
- G?i API 2.1, 2.2, 2.3

### **2. Form t?o tài kho?n ngân hàng**:
```javascript
// Step 1: L?y danh sách ngân hàng
const banksResponse = await fetch('/api/BankAccount/supported-banks');
const banks = banksResponse.data;

// Step 2: Hi?n th? dropdown
// - Các ngân hàng th??ng: banks.filter(b => b.value !== 999)
// - Ngân hàng khác: banks.find(b => b.value === 999)

// Step 3: X? lý form
if (selectedBankId === 999) {
  // Hi?n th? input cho customBankName
  payload = {
    supportedBankId: 999,
    bankName: "Other", // có th? ?? tr?ng
    customBankName: userInput,
    // ... other fields
  }
} else {
  // Dùng ngân hàng có s?n
  payload = {
    supportedBankId: selectedBankId,
    bankName: selectedBank.displayName,
    // ... other fields
  }
}
```

### **3. Trang t?o yêu c?u rút ti?n**: 
- G?i API 2.2 ?? ch?n tài kho?n ngân hàng
- G?i API 3.1 ?? validate
- G?i API 3.3 ?? t?o yêu c?u

### **4. Trang l?ch s? rút ti?n**: 
- G?i API 4.1, 4.2, 4.5

### **5. Real-time updates**: 
- S? d?ng SignalR ho?c polling API 4.3

---

## ?? L?u ý quan tr?ng

- **S? ti?n t?i thi?u**: 1,000 VN?
- **Tr?ng thái**: Pending ? Approved/Rejected/Cancelled
- **B?o m?t**: S? tài kho?n ???c mask khi hi?n th?
- **Validation**: Ki?m tra s? d? ví tr??c khi cho phép rút ti?n
- **Duplicate check**: Không cho phép tài kho?n ngân hàng trùng l?p
- **Tính n?ng m?i**: 
  - ? **H? tr? ch?n t? 29+ ngân hàng có s?n**
  - ? **Tùy ch?n "Ngân hàng khác" cho phép nh?p t? do**
  - ? **Backward compatibility v?i API c?**
  - ? **Validation tên ngân hàng t? do khi ch?n "Other"**

---

## ?? **Tính n?ng "Ngân hàng khác"**

### **Logic x? lý**:
1. **Ch?n ngân hàng t? danh sách**: `supportedBankId` != 999 ? Dùng tên t? enum
2. **Ch?n "Ngân hàng khác"**: `supportedBankId` = 999 ? B?t bu?c có `customBankName`
3. **Không ch?n enum**: `supportedBankId` = null ? Dùng `bankName` (backward compatibility)

### **Validation**:
- Khi `supportedBankId` = 999: `customBankName` là b?t bu?c
- `customBankName` t?i ?a 100 ký t?
- Duplicate check áp d?ng cho tên cu?i cùng (sau khi x? lý)