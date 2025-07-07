# 🏪 SPECIALTY SHOP REGISTRATION FLOW - FRONTEND DEVELOPER GUIDE

## 📋 OVERVIEW

Hệ thống đăng ký Specialty Shop cho phép User đăng ký trở thành chủ cửa hàng đặc sản. Flow bao gồm:
1. User nộp đơn đăng ký với file upload
2. Admin xem và phê duyệt/từ chối đơn
3. User được cấp role "Specialty Shop" sau khi được duyệt

---

## 🔐 AUTHENTICATION & ROLES

### Required Roles:
- **User**: Có thể nộp đơn và xem trạng thái đơn của mình
- **Admin**: Có thể xem tất cả đơn và phê duyệt/từ chối
- **Specialty Shop**: Role được cấp tự động sau khi đơn được duyệt

### JWT Token Format:
```json
{
  "role": "User|Admin|Specialty Shop",
  "Id": "user-guid",
  "email": "user@example.com",
  "exp": 1750889282
}
```

---

## 🚀 API ENDPOINTS

### 1. USER ENDPOINTS

#### 📤 Submit SpecialtyShop Application
```http
POST /api/Account/specialty-shop-application
Authorization: Bearer {USER_JWT_TOKEN}
Content-Type: multipart/form-data
```

**Form Data Fields:**
```javascript
const formData = new FormData();
formData.append('ShopName', 'Tay Ninh Specialty Store');
formData.append('ShopDescription', 'Specializing in Tay Ninh local products');
formData.append('BusinessLicense', 'GP-123456789');
formData.append('Location', '123 Nguyen Hue Street, Tay Ninh');
formData.append('PhoneNumber', '0987654321');
formData.append('Email', 'shop@example.com');
formData.append('Website', 'https://shop-website.com');
formData.append('ShopType', 'Food & Beverage');
formData.append('OpeningHours', '8:00 AM - 10:00 PM');
formData.append('RepresentativeName', 'Nguyen Van Owner');
formData.append('BusinessLicenseFile', businessLicenseFile); // PDF/DOCX file
formData.append('Logo', logoFile); // PNG/JPG/JPEG/WEBP file
```

**File Requirements:**
- **BusinessLicenseFile**: PDF, DOCX (max 10MB)
- **Logo**: PNG, JPG, JPEG, WEBP (max 10MB)

**Success Response (201):**
```json
{
  "data": {
    "id": "e4b25fd8-d2a9-44d8-965b-f597bae5956b",
    "shopName": "Tay Ninh Specialty Store",
    "status": 0,
    "submittedAt": "2025-06-24T22:13:02.173763"
  },
  "statusCode": 201,
  "message": "Specialty shop application submitted successfully",
  "success": true
}
```

**Error Response (400):**
```json
{
  "statusCode": 400,
  "message": "Business license file and logo are required",
  "success": false,
  "validationErrors": ["ShopName is required", "Email format is invalid"]
}
```

#### 📋 Get My Application Status
```http
GET /api/Account/my-specialty-shop-application
Authorization: Bearer {USER_JWT_TOKEN}
```

**Success Response (200):**
```json
{
  "data": {
    "id": "e4b25fd8-d2a9-44d8-965b-f597bae5956b",
    "shopName": "Tay Ninh Specialty Store",
    "shopDescription": "Specializing in Tay Ninh local products",
    "businessLicense": "GP-123456789",
    "location": "123 Nguyen Hue Street, Tay Ninh",
    "phoneNumber": "0987654321",
    "email": "shop@example.com",
    "website": "https://shop-website.com",
    "shopType": "Food & Beverage",
    "openingHours": "8:00 AM - 10:00 PM",
    "representativeName": "Nguyen Van Owner",
    "businessLicenseUrl": "http://localhost:5267/uploads/businesslicense/file.pdf",
    "logoUrl": "http://localhost:5267/uploads/shoplogo/logo.png",
    "status": 1,
    "rejectionReason": null,
    "submittedAt": "2025-06-24T22:13:02.173763",
    "processedAt": "2025-06-24T22:15:43.788649"
  },
  "statusCode": 200,
  "message": "Application retrieved successfully",
  "success": true
}
```

**Status Values:**
- `0`: Pending (Đang chờ duyệt)
- `1`: Approved (Đã duyệt)
- `2`: Rejected (Bị từ chối)

### 2. ADMIN ENDPOINTS

#### 📊 Get All Applications (with Pagination)
```http
GET /api/SpecialtyShopApplication?page=0&pageSize=10&status=0
Authorization: Bearer {ADMIN_JWT_TOKEN}
```

**Query Parameters:**
- `page`: Page number (0-based, default: 0)
- `pageSize`: Items per page (default: 10)
- `status`: Filter by status (optional: 0=Pending, 1=Approved, 2=Rejected)

**Success Response (200):**
```json
{
  "data": {
    "items": [
      {
        "id": "e4b25fd8-d2a9-44d8-965b-f597bae5956b",
        "shopName": "Tay Ninh Specialty Store",
        "representativeName": "Nguyen Van Owner",
        "email": "shop@example.com",
        "phoneNumber": "0987654321",
        "location": "123 Nguyen Hue Street, Tay Ninh",
        "status": 0,
        "submittedAt": "2025-06-24T22:13:02.173763",
        "userInfo": {
          "id": "c9d05465-76fe-4c93-a469-4e9d090da601",
          "name": "User",
          "email": "user@gmail.com"
        }
      }
    ],
    "totalCount": 1,
    "pageNumber": 0,
    "pageSize": 10,
    "totalPages": 1
  },
  "statusCode": 200,
  "message": "Applications retrieved successfully",
  "success": true
}
```

#### 📋 Get Application Detail
```http
GET /api/SpecialtyShopApplication/{applicationId}
Authorization: Bearer {ADMIN_JWT_TOKEN}
```

#### ✅ Approve Application
```http
POST /api/SpecialtyShopApplication/{applicationId}/approve
Authorization: Bearer {ADMIN_JWT_TOKEN}
```

**Success Response (200):**
```json
{
  "statusCode": 200,
  "message": "Specialty shop application approved successfully",
  "success": true
}
```

#### ❌ Reject Application
```http
POST /api/SpecialtyShopApplication/{applicationId}/reject
Authorization: Bearer {ADMIN_JWT_TOKEN}
Content-Type: application/json
```

**Request Body:**
```json
{
  "rejectionReason": "Giấy phép kinh doanh không hợp lệ. Vui lòng cung cấp giấy phép mới nhất."
}
```

---

## 💻 FRONTEND IMPLEMENTATION EXAMPLES

### React/JavaScript Example

#### 1. Submit Application Form
```javascript
const submitApplication = async (formData) => {
  try {
    const token = localStorage.getItem('authToken');
    
    const response = await fetch('/api/Account/specialty-shop-application', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`
      },
      body: formData // FormData object with files
    });
    
    const result = await response.json();
    
    if (result.success) {
      alert('Đơn đăng ký đã được nộp thành công!');
      // Redirect to status page
    } else {
      console.error('Validation errors:', result.validationErrors);
    }
  } catch (error) {
    console.error('Submit error:', error);
  }
};
```

#### 2. Check Application Status
```javascript
const checkApplicationStatus = async () => {
  try {
    const token = localStorage.getItem('authToken');
    
    const response = await fetch('/api/Account/my-specialty-shop-application', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    const result = await response.json();
    
    if (result.success) {
      const status = result.data.status;
      const statusText = status === 0 ? 'Đang chờ duyệt' : 
                        status === 1 ? 'Đã duyệt' : 'Bị từ chối';
      
      console.log('Application status:', statusText);
      return result.data;
    }
  } catch (error) {
    console.error('Status check error:', error);
  }
};
```

#### 3. Admin - Get Applications List
```javascript
const getApplicationsList = async (page = 0, pageSize = 10, status = null) => {
  try {
    const token = localStorage.getItem('adminToken');
    
    let url = `/api/SpecialtyShopApplication?page=${page}&pageSize=${pageSize}`;
    if (status !== null) {
      url += `&status=${status}`;
    }
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    const result = await response.json();
    return result.data; // Contains items, totalCount, pagination info
  } catch (error) {
    console.error('Get applications error:', error);
  }
};
```

#### 4. Admin - Approve/Reject Application
```javascript
const approveApplication = async (applicationId) => {
  try {
    const token = localStorage.getItem('adminToken');
    
    const response = await fetch(`/api/SpecialtyShopApplication/${applicationId}/approve`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    const result = await response.json();
    
    if (result.success) {
      alert('Đơn đăng ký đã được phê duyệt!');
      // Refresh applications list
    }
  } catch (error) {
    console.error('Approve error:', error);
  }
};

const rejectApplication = async (applicationId, reason) => {
  try {
    const token = localStorage.getItem('adminToken');
    
    const response = await fetch(`/api/SpecialtyShopApplication/${applicationId}/reject`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ rejectionReason: reason })
    });
    
    const result = await response.json();
    
    if (result.success) {
      alert('Đơn đăng ký đã bị từ chối!');
      // Refresh applications list
    }
  } catch (error) {
    console.error('Reject error:', error);
  }
};
```

---

## 🔄 COMPLETE USER FLOW

### User Journey:
1. **Login** → Get JWT token with "User" role
2. **Submit Application** → Upload files + form data
3. **Check Status** → Monitor application progress
4. **Get Approved** → Automatically receive "Specialty Shop" role
5. **Login Again** → New JWT token with "Specialty Shop" role

### Admin Journey:
1. **Login** → Get JWT token with "Admin" role
2. **View Applications** → See all pending/approved/rejected applications
3. **Review Details** → Check application info and uploaded files
4. **Approve/Reject** → Process applications with optional rejection reason

---

## ⚠️ IMPORTANT NOTES

### File Upload Considerations:
- Use `FormData` for multipart form submissions
- Validate file types and sizes on frontend before upload
- Show upload progress for better UX
- Handle file upload errors gracefully

### Error Handling:
- Always check `success` field in API responses
- Display `validationErrors` array for form validation
- Handle network errors and timeouts
- Provide user-friendly error messages

### Security:
- Always include JWT token in Authorization header
- Validate user roles before showing admin features
- Sanitize file uploads on frontend
- Use HTTPS in production

### Performance:
- Implement pagination for admin applications list
- Cache application status to reduce API calls
- Optimize file upload with compression
- Use loading states for better UX

---

## 🧪 TESTING

### Test Data:
```javascript
const testApplicationData = {
  shopName: "Test Specialty Store",
  shopDescription: "Test description for specialty store",
  businessLicense: "GP-TEST-123456",
  location: "123 Test Street, Test City",
  phoneNumber: "0987654321",
  email: "teststore@example.com",
  website: "https://teststore.com",
  shopType: "Food & Beverage",
  openingHours: "8:00 AM - 10:00 PM",
  representativeName: "Test Owner Name"
};
```

### Test Files:
- Create small PDF file for business license testing
- Create small PNG/JPG file for logo testing
- Test with invalid file types to verify validation

---

## 📞 SUPPORT

Nếu có vấn đề với API hoặc cần hỗ trợ thêm, vui lòng liên hệ Backend team với thông tin:
- API endpoint gặp lỗi
- Request/Response data
- Error messages
- Browser console logs

**Base URL:** `http://localhost:5267` (Development)
**API Documentation:** Available at `/swagger` endpoint

---

## 🎯 COMMON ERROR SCENARIOS & SOLUTIONS

### 1. File Upload Errors
```javascript
// Error: File too large
{
  "statusCode": 400,
  "message": "File size exceeds maximum limit of 10MB",
  "success": false
}

// Solution: Validate file size before upload
const validateFileSize = (file, maxSizeMB = 10) => {
  const maxSize = maxSizeMB * 1024 * 1024; // Convert to bytes
  if (file.size > maxSize) {
    alert(`File size must be less than ${maxSizeMB}MB`);
    return false;
  }
  return true;
};
```

### 2. Authentication Errors
```javascript
// Error: Token expired
{
  "statusCode": 401,
  "message": "Token has expired",
  "success": false
}

// Solution: Implement token refresh or redirect to login
const handleAuthError = (response) => {
  if (response.status === 401) {
    localStorage.removeItem('authToken');
    window.location.href = '/login';
  }
};
```

### 3. Validation Errors
```javascript
// Error: Missing required fields
{
  "statusCode": 400,
  "message": "Validation failed",
  "success": false,
  "validationErrors": [
    "ShopName is required",
    "Email format is invalid",
    "PhoneNumber must be 10 digits"
  ]
}

// Solution: Display validation errors to user
const displayValidationErrors = (errors) => {
  errors.forEach(error => {
    const field = error.split(' ')[0].toLowerCase();
    const errorElement = document.getElementById(`${field}-error`);
    if (errorElement) {
      errorElement.textContent = error;
      errorElement.style.display = 'block';
    }
  });
};
```

---

## 🔧 ADVANCED IMPLEMENTATION PATTERNS

### 1. React Hook for Application Management
```javascript
import { useState, useEffect } from 'react';

const useSpecialtyShopApplication = () => {
  const [application, setApplication] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const submitApplication = async (formData) => {
    setLoading(true);
    setError(null);

    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch('/api/Account/specialty-shop-application', {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
        body: formData
      });

      const result = await response.json();

      if (result.success) {
        setApplication(result.data);
        return { success: true, data: result.data };
      } else {
        setError(result.message);
        return { success: false, errors: result.validationErrors };
      }
    } catch (err) {
      setError('Network error occurred');
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  };

  const checkStatus = async () => {
    setLoading(true);
    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch('/api/Account/my-specialty-shop-application', {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      const result = await response.json();
      if (result.success) {
        setApplication(result.data);
      }
    } catch (err) {
      setError('Failed to check application status');
    } finally {
      setLoading(false);
    }
  };

  return {
    application,
    loading,
    error,
    submitApplication,
    checkStatus
  };
};

export default useSpecialtyShopApplication;
```

### 2. Vue.js Composition API Example
```javascript
import { ref, reactive } from 'vue';

export function useSpecialtyShopApplication() {
  const application = ref(null);
  const loading = ref(false);
  const errors = reactive({});

  const submitApplication = async (formData) => {
    loading.value = true;
    Object.keys(errors).forEach(key => delete errors[key]);

    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch('/api/Account/specialty-shop-application', {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
        body: formData
      });

      const result = await response.json();

      if (result.success) {
        application.value = result.data;
        return { success: true };
      } else {
        if (result.validationErrors) {
          result.validationErrors.forEach(error => {
            const field = error.split(' ')[0].toLowerCase();
            errors[field] = error;
          });
        }
        return { success: false };
      }
    } catch (error) {
      errors.general = 'Network error occurred';
      return { success: false };
    } finally {
      loading.value = false;
    }
  };

  return {
    application,
    loading,
    errors,
    submitApplication
  };
}
```

### 3. File Upload with Progress
```javascript
const uploadWithProgress = async (formData, onProgress) => {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();

    xhr.upload.addEventListener('progress', (event) => {
      if (event.lengthComputable) {
        const percentComplete = (event.loaded / event.total) * 100;
        onProgress(percentComplete);
      }
    });

    xhr.addEventListener('load', () => {
      if (xhr.status === 200 || xhr.status === 201) {
        resolve(JSON.parse(xhr.responseText));
      } else {
        reject(new Error(`Upload failed with status ${xhr.status}`));
      }
    });

    xhr.addEventListener('error', () => {
      reject(new Error('Upload failed'));
    });

    const token = localStorage.getItem('authToken');
    xhr.open('POST', '/api/Account/specialty-shop-application');
    xhr.setRequestHeader('Authorization', `Bearer ${token}`);
    xhr.send(formData);
  });
};

// Usage
const handleSubmit = async (formData) => {
  try {
    const result = await uploadWithProgress(formData, (progress) => {
      console.log(`Upload progress: ${progress.toFixed(2)}%`);
      // Update progress bar UI
    });

    console.log('Upload successful:', result);
  } catch (error) {
    console.error('Upload failed:', error);
  }
};
```

---

## 📱 MOBILE CONSIDERATIONS

### File Selection on Mobile
```javascript
// Handle camera/gallery selection on mobile
const handleFileSelect = (inputElement) => {
  // For mobile devices, allow camera capture
  if (/Android|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
    inputElement.setAttribute('capture', 'environment'); // Use rear camera
    inputElement.setAttribute('accept', 'image/*,.pdf,.docx'); // Accept images and documents
  }
};

// Example usage
<input
  type="file"
  accept=".pdf,.docx,.png,.jpg,.jpeg,.webp"
  capture="environment" // For mobile camera
  onChange={handleFileChange}
/>
```

### Responsive Design Tips
- Use appropriate file input styling for mobile
- Implement touch-friendly buttons for admin actions
- Consider swipe gestures for application lists
- Optimize image previews for smaller screens

---

## 🚀 PRODUCTION DEPLOYMENT CHECKLIST

### Environment Configuration
- [ ] Update base URL to production API endpoint
- [ ] Configure proper CORS settings
- [ ] Set up HTTPS for secure file uploads
- [ ] Configure CDN for uploaded files

### Security Checklist
- [ ] Validate JWT tokens on every request
- [ ] Implement rate limiting for file uploads
- [ ] Sanitize file names and content
- [ ] Use secure headers (CSP, HSTS, etc.)

### Performance Optimization
- [ ] Implement file compression before upload
- [ ] Add image optimization for logos
- [ ] Use lazy loading for application lists
- [ ] Cache API responses where appropriate

### Monitoring & Analytics
- [ ] Track application submission success rates
- [ ] Monitor file upload performance
- [ ] Log API errors for debugging
- [ ] Set up alerts for failed uploads

---

## 📋 TESTING CHECKLIST

### User Flow Testing
- [ ] Submit application with valid data and files
- [ ] Submit with missing required fields
- [ ] Submit with invalid file types/sizes
- [ ] Check application status after submission
- [ ] Verify role change after approval

### Admin Flow Testing
- [ ] View applications list with pagination
- [ ] Filter applications by status
- [ ] View application details
- [ ] Approve applications
- [ ] Reject applications with reasons

### Edge Cases
- [ ] Network interruption during upload
- [ ] Token expiration during submission
- [ ] Large file upload handling
- [ ] Concurrent admin actions
- [ ] Mobile device compatibility

---

## 🔗 RELATED APIS

### User Management
- `POST /api/Authentication/login` - User login
- `POST /api/Authentication/register` - User registration
- `GET /api/Account/profile` - Get user profile

### File Management
- `GET /uploads/businesslicense/{filename}` - Download business license
- `GET /uploads/shoplogo/{filename}` - Download shop logo

### Specialty Shop Operations (After Approval)
- `GET /api/SpecialtyShop/my-shop` - Get shop details
- `PUT /api/SpecialtyShop/update` - Update shop information
- `GET /api/SpecialtyShop/orders` - View shop orders

---

**📞 SUPPORT CONTACT:**
- Backend Team: backend@tayninhtour.com
- API Issues: api-support@tayninhtour.com
- Documentation: docs@tayninhtour.com
