# 🏦 HƯỚNG DẪN FRONTEND API BANK ACCOUNT - QUẢN LÝ TÀI KHOẢN NGÂN HÀNG

## 📋 Tổng quan

API BankAccount hỗ trợ **2 cách chọn ngân hàng** cho Frontend:

1. **🏛️ Ngân hàng có sẵn**: Chọn từ 29+ ngân hàng phổ biến Việt Nam
2. **🆕 Ngân hàng khác**: Nhập tên ngân hàng tự do

---

## 🎯 WORKFLOW FRONTEND KHUYẾN NGHỊ

### **Step 1: Lấy danh sách ngân hàng**// Gọi API để lấy danh sách ngân hàng hỗ trợ
const fetchSupportedBanks = async () => {
  const response = await fetch('/api/BankAccount/supported-banks');
  const result = await response.json();
  return result.data; // Array of supported banks
};
### **Step 2: Render UI với 2 options**const renderBankSelection = (banks) => {
  const regularBanks = banks.filter(b => b.value !== 999);
  const otherBank = banks.find(b => b.value === 999);
  
  return (
    <div>
      <select onChange={handleBankChange}>
        <option value="">-- Chọn ngân hàng --</option>
        
        {/* Render ngân hàng có sẵn */}
        {regularBanks.map(bank => (
          <option key={bank.value} value={bank.value}>
            {bank.displayName}
          </option>
        ))}
        
        {/* Option "Ngân hàng khác" */}
        <option value="999">{otherBank.displayName}</option>
      </select>
      
      {/* Input cho ngân hàng tự do */}
      {selectedBankId === 999 && (
        <input 
          type="text"
          placeholder="Nhập tên ngân hàng của bạn..."
          onChange={handleCustomBankNameChange}
          required
        />
      )}
    </div>
  );
};
### **Step 3: Xử lý submit form**const createBankAccount = async (formData) => {
  let payload;
  
  if (formData.selectedBankId === 999) {
    // Case: Ngân hàng khác
    payload = {
      supportedBankId: 999,
      bankName: "Other",
      customBankName: formData.customBankName, // ⚠️ BẮT BUỘC
      accountNumber: formData.accountNumber,
      accountHolderName: formData.accountHolderName.toUpperCase(),
      isDefault: formData.isDefault,
      notes: formData.notes
    };
  } else {
    // Case: Ngân hàng có sẵn
    const selectedBank = banks.find(b => b.value === formData.selectedBankId);
    payload = {
      supportedBankId: formData.selectedBankId,
      bankName: selectedBank.displayName, // Tự động từ enum
      accountNumber: formData.accountNumber,
      accountHolderName: formData.accountHolderName.toUpperCase(),
      isDefault: formData.isDefault,
      notes: formData.notes
    };
  }

  const response = await fetch('/api/BankAccount', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${authToken}`
    },
    body: JSON.stringify(payload)
  });

  return await response.json();
};
---

## 🏦 BẢNG ENUM NGÂN HÀNG CÓ SẴN

### **📋 Danh sách đầy đủ 29 ngân hàng + Other**

| **ID** | **Tên enum** | **Tên hiển thị** | **Tên viết tắt** | **Ghi chú** |
|--------|-------------|------------------|------------------|-------------|
| **0** | Vietcombank | Ngân hàng Ngoại thương Việt Nam (Vietcombank) | VCB | ⭐ #1 VN |
| **1** | VietinBank | Ngân hàng Công thương Việt Nam (VietinBank) | CTG | 🏛️ Nhà nước |
| **2** | BIDV | Ngân hàng Đầu tư và Phát triển Việt Nam (BIDV) | BIDV | 💼 Đầu tư |
| **3** | Techcombank | Ngân hàng Kỹ thương Việt Nam (Techcombank) | TCB | 🚀 Tech |
| **4** | Sacombank | Ngân hàng Sài Gòn Thương tín (Sacombank) | STB | 🌆 Sài Gòn |
| **5** | ACB | Ngân hàng Á Châu (ACB) | ACB | 🌟 Á Châu |
| **6** | MBBank | Ngân hàng Quân đội (MBBank) | MB | ⚡ Quân đội |
| **7** | TPBank | Ngân hàng Tiên Phong (TPBank) | TPB | 🌅 Tiên phong |
| **8** | VPBank | Ngân hàng Việt Nam Thịnh vượng (VPBank) | VPB | 💎 Thịnh vượng |
| **9** | SHB | Ngân hàng Sài Gòn - Hà Nội (SHB) | SHB | 🏢 SG-HN |
| **10** | HDBank | Ngân hàng Phát triển Nhà TP.HCM (HDBank) | HDB | 🏠 Phát triển |
| **11** | VIB | Ngân hàng Quốc tế Việt Nam (VIB) | VIB | 🌍 Quốc tế |
| **12** | Eximbank | Ngân hàng Xuất nhập khẩu Việt Nam (Eximbank) | EIB | 📦 XNK |
| **13** | SeABank | Ngân hàng Đông Nam Á (SeABank) | SEAB | 🌏 ĐNA |
| **14** | OCB | Ngân hàng Phương Đông (OCB) | OCB | 🧭 Phương Đông |
| **15** | MSB | Ngân hàng Hàng hải (MSB) | MSB | ⚓ Hàng hải |
| **16** | SCB | Ngân hàng Sài Gòn (SCB) | SCB | 🏙️ Sài Gòn |
| **17** | DongABank | Ngân hàng Đông Á (DongA Bank) | DAB | 🌸 Đông Á |
| **18** | LienVietPostBank | Ngân hàng Bưu điện Liên Việt (LienVietPostBank) | LPB | 📮 Bưu điện |
| **19** | ABBANK | Ngân hàng An Bình (ABBANK) | ABB | ☮️ An Bình |
| **20** | PVcomBank | Ngân hàng Đại chúng Việt Nam (PVcomBank) | PVCB | ⛽ Petro |
| **21** | NamABank | Ngân hàng Nam Á (Nam A Bank) | NAB | 🏔️ Nam Á |
| **22** | BacABank | Ngân hàng Bắc Á (Bac A Bank) | BAB | 🏔️ Bắc Á |
| **23** | Saigonbank | Ngân hàng Sài Gòn Công thương (Saigonbank) | SGB | 🏭 SG Công thương |
| **24** | VietBank | Ngân hàng Việt Nam Thương tín (VietBank) | VBB | 🇻🇳 VN Thương tín |
| **25** | Kienlongbank | Ngân hàng Kiên Long (Kienlongbank) | KLB | 🐉 Kiên Long |
| **26** | PGBank | Ngân hàng Xăng dầu Petrolimex (PGBank) | PGB | ⛽ Xăng dầu |
| **27** | OceanBank | Ngân hàng Đại Dương (OceanBank) | OJB | 🌊 Đại Dương |
| **28** | CoopBank | Ngân hàng Hợp tác xã Việt Nam (Co-opBank) | COOP | 🤝 Hợp tác xã |
| **999** | Other | Ngân hàng khác | OTHER | 🆕 Tự nhập |

---

## 🔄 API ENDPOINTS CHO FRONTEND

### **1. 📋 Lấy danh sách ngân hàng**GET /api/BankAccount/supported-banks
Authorization: Không cần
**Response:**
{
  "isSuccess": true,
  "data": [
    {
      "value": 0,
      "name": "Vietcombank",
      "displayName": "Ngân hàng Ngoại thương Việt Nam (Vietcombank)",
      "shortName": "VCB",
      "logoUrl": null,
      "isActive": true
    },
    {
      "value": 3,
      "name": "Techcombank", 
      "displayName": "Ngân hàng Kỹ thương Việt Nam (Techcombank)",
      "shortName": "TCB",
      "logoUrl": null,
      "isActive": true
    },
    // ... 27 ngân hàng khác
    {
      "value": 999,
      "name": "Other",
      "displayName": "Ngân hàng khác",
      "shortName": "OTHER",
      "logoUrl": null,
      "isActive": true
    }
  ]
}
### **2. 💳 Tạo tài khoản ngân hàng**POST /api/BankAccount
Authorization: Bearer {token}
Content-Type: application/json
### **3. 📄 Lấy danh sách tài khoản của user**GET /api/BankAccount/my-accounts
Authorization: Bearer {token}
### **4. 🔍 Lấy tài khoản mặc định**GET /api/BankAccount/default
Authorization: Bearer {token}
---

## 📝 2 CASE STUDY CHO FRONTEND

### **🏛️ CASE 1: Chọn ngân hàng có sẵn (Vietcombank)**

**Request body:**{
  "supportedBankId": 0,
  "bankName": "Vietcombank",
  "accountNumber": "1234567890",
  "accountHolderName": "NGUYEN VAN A",
  "isDefault": true,
  "notes": "Tài khoản Vietcombank chính"
}
**Logic Frontend:**// User chọn Vietcombank từ dropdown
const selectedBank = banks.find(b => b.value === 0);

const payload = {
  supportedBankId: 0,
  bankName: selectedBank.displayName, // Auto fill
  accountNumber: userInput.accountNumber,
  accountHolderName: userInput.accountHolderName.toUpperCase(),
  isDefault: userInput.isDefault,
  notes: userInput.notes
};
### **🆕 CASE 2: Chọn "Ngân hàng khác"**

**Request body:**{
  "supportedBankId": 999,
  "bankName": "Other",
  "customBankName": "Ngân hàng ABC xyz",
  "accountNumber": "9876543210",
  "accountHolderName": "TRAN THI B",
  "isDefault": false,
  "notes": "Ngân hàng địa phương"
}
**Logic Frontend:**// User chọn "Ngân hàng khác" từ dropdown
// → Hiện input field cho customBankName

const payload = {
  supportedBankId: 999,
  bankName: "Other",
  customBankName: userInput.customBankName, // ⚠️ REQUIRED!
  accountNumber: userInput.accountNumber,
  accountHolderName: userInput.accountHolderName.toUpperCase(),
  isDefault: userInput.isDefault,
  notes: userInput.notes
};
---

## ⚠️ VALIDATION RULES CHO FRONTEND

### **✅ Các rule bắt buộc:**

1. **`accountNumber`**: 
   - ✅ Chỉ chứa số (0-9)
   - ✅ Tối đa 50 ký tự
   - ❌ Không chứa chữ cái, ký tự đặc biệt

2. **`accountHolderName`**:
   - ✅ Bắt buộc
   - ✅ Tối đa 100 ký tự
   - 💡 Khuyến nghị: Viết HOA

3. **`customBankName`** (khi `supportedBankId = 999`):
   - ✅ BẮT BUỘC khi chọn "Ngân hàng khác"
   - ✅ Tối đa 100 ký tự
   - ❌ Không được để trống

4. **`notes`**:
   - ⭕ Tùy chọn
   - ✅ Tối đa 500 ký tự

### **🚨 Error handling:**const validateForm = (formData) => {
  const errors = [];
  
  // Validate account number
  if (!/^[0-9]+$/.test(formData.accountNumber)) {
    errors.push("Số tài khoản chỉ được chứa số");
  }
  
  if (formData.accountNumber.length > 50) {
    errors.push("Số tài khoản không quá 50 ký tự");
  }
  
  // Validate custom bank name for "Other" option
  if (formData.supportedBankId === 999 && !formData.customBankName?.trim()) {
    errors.push("Tên ngân hàng là bắt buộc khi chọn 'Ngân hàng khác'");
  }
  
  return errors;
};
---

## 🎨 UI/UX RECOMMENDATIONS

### **🖼️ Giao diện gợi ý:**
const BankAccountForm = () => {
  const [selectedBankId, setSelectedBankId] = useState(null);
  const [banks, setBanks] = useState([]);
  
  return (
    <form>
      {/* Bank Selection */}
      <div className="form-group">
        <label>Chọn ngân hàng *</label>
        <select 
          value={selectedBankId} 
          onChange={(e) => setSelectedBankId(Number(e.target.value))}
          required
        >
          <option value="">-- Chọn ngân hàng --</option>
          
          {/* Nhóm ngân hàng phổ biến */}
          <optgroup label="🏦 Ngân hàng phổ biến">
            {banks.filter(b => [0, 1, 2, 3, 5].includes(b.value)).map(bank => (
              <option key={bank.value} value={bank.value}>
                {bank.shortName} - {bank.displayName}
              </option>
            ))}
          </optgroup>
          
          {/* Nhóm ngân hàng khác */}
          <optgroup label="🏛️ Ngân hàng khác">
            {banks.filter(b => ![0, 1, 2, 3, 5, 999].includes(b.value)).map(bank => (
              <option key={bank.value} value={bank.value}>
                {bank.shortName} - {bank.displayName}
              </option>
            ))}
          </optgroup>
          
          {/* Option "Ngân hàng khác" */}
          <optgroup label="🆕 Tùy chọn khác">
            <option value="999">🆕 Ngân hàng khác (nhập tên tự do)</option>
          </optgroup>
        </select>
      </div>
      
      {/* Custom Bank Name Input (chỉ hiện khi chọn "Other") */}
      {selectedBankId === 999 && (
        <div className="form-group">
          <label>Tên ngân hàng *</label>
          <input 
            type="text"
            placeholder="VD: Ngân hàng Hợp tác xã ABC..."
            maxLength={100}
            required
          />
          <small className="text-muted">
            💡 Nhập tên đầy đủ của ngân hàng của bạn
          </small>
        </div>
      )}
      
      {/* Account Number */}
      <div className="form-group">
        <label>Số tài khoản *</label>
        <input 
          type="text"
          pattern="[0-9]*"
          maxLength={50}
          placeholder="VD: 1234567890"
          required
        />
      </div>
      
      {/* Account Holder Name */}
      <div className="form-group">
        <label>Tên chủ tài khoản *</label>
        <input 
          type="text"
          maxLength={100}
          placeholder="VD: NGUYEN VAN A"
          style={{ textTransform: 'uppercase' }}
          required
        />
        <small className="text-muted">
          💡 Nhập đúng tên trên thẻ ngân hàng (viết HOA)
        </small>
      </div>
    </form>
  );
};
---

## 🎯 CHECKLIST CHO FRONTEND DEVELOPER

### **✅ Triển khai Frontend:**

- [ ] **API Integration**:
  - [ ] Gọi `/api/BankAccount/supported-banks` để lấy danh sách
  - [ ] Handle authentication với Bearer token
  - [ ] Error handling cho response

- [ ] **UI Components**:
  - [ ] Dropdown/Select cho chọn ngân hàng
  - [ ] Conditional input field cho `customBankName`
  - [ ] Form validation real-time
  - [ ] Loading states & error messages

- [ ] **Logic Processing**:
  - [ ] Phân biệt 2 cases: ngân hàng có sẵn vs ngân hàng khác
  - [ ] Auto-fill `bankName` khi chọn từ enum
  - [ ] Validate `customBankName` required khi `supportedBankId = 999`
  - [ ] Format `accountHolderName` thành uppercase

- [ ] **UX Enhancements**:
  - [ ] Nhóm ngân hàng phổ biến vs ngân hàng khác
  - [ ] Placeholder text hợp lý
  - [ ] Tooltip/hint cho user
  - [ ] Success/error feedback

**🚀 Ready to implement! Happy coding! 🎉**