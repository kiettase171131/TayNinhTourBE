# 🚀 Enhanced TourOperation Edit Logic - Business Rules Implementation

## 📋 **Overview**

Triển khai logic nghiệp vụ cho việc chỉnh sửa TourOperation theo yêu cầu:

1. **🚫 Khi đã có hướng dẫn viên tham gia** → KHÔNG cho phép edit với thông báo rõ ràng
2. **🔄 Khi TourDetails status = "đang chờ phân công hdv" (AwaitingGuideAssignment)** → cho phép edit và **đổi TourDetails status** thành AwaitingAdminApproval để admin duyệt lại
3. **✅ Các trạng thái khác** → cho phép edit bình thường

---

## 🔧 **Business Logic Flow**

### **Priority 1: 🛡️ Block Edit When Guide is ASSIGNED (Strongest Rule)**

// BUSINESS RULE 1: Check if tour guide is ASSIGNED - prevent editing if guide is already assigned
// This is the STRONGEST rule - once a guide is assigned and working, NO EDITS allowed
bool hasGuideAssigned = operation.TourGuideId != null;
if (hasGuideAssigned)
{
    return new ResponseUpdateOperationDto
    {
        success = false,
        Message = "Đã có hướng dẫn viên tham gia tour operation, không thể edit nữa"
    };
}
**🛡️ Quy tắc tối cao:** Khi đã có guide được assign (`TourGuideId != null`), TUYỆT ĐỐI không được phép edit.

### **Priority 2: 🔄 Reset TourDetails Status When Edit During Guide Wait**
// BUSINESS RULE 2: Check TourDetails status and update if needed
// Get the related TourDetails to check its status
var relatedTourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(operation.TourDetailsId);

// If TourDetails status is AwaitingGuideAssignment (waiting for guide assignment) → send back to admin for approval
if (originalTourDetailsStatus == TourDetailsStatus.AwaitingGuideAssignment)
{
    relatedTourDetails.Status = TourDetailsStatus.AwaitingAdminApproval; // Reset to "pending admin approval"
    relatedTourDetails.CommentApproved = null; // Clear previous admin comment
    relatedTourDetails.UpdatedAt = DateTime.UtcNow;
    
    // Update TourDetails in database
    await _unitOfWork.TourDetailsRepository.UpdateAsync(relatedTourDetails);
    
    // Send notification about status change
    await notificationService.CreateNotificationAsync(new CreateNotificationDto
    {
        UserId = operation.CreatedById,
        Title = "📝 Tour đã gửi lại admin",
        Message = $"Tour '{relatedTourDetails.Title}' đã được gửi lại cho admin duyệt do có chỉnh sửa trong lúc chờ hướng dẫn viên được phân công.",
        Type = NotificationType.Tour,
        Priority = NotificationPriority.Medium,
        Icon = "📝",
        ActionUrl = "/tours/awaiting-admin-approval"
    });
}
**🔄 Logic thứ 2:** Khi đang chờ guide assignment mà có edit → reset **TourDetails.Status** về AwaitingAdminApproval (pending admin).

---

## 📊 **Corrected Status Flow Diagram**TourDetails Status Flow (QUAN TRỌNG - Đây là status admin duyệt):

┌─────────────────────┐    Edit     ┌─────────────────────────┐
│     Approved        │ ──────────► │      Approved           │
└─────────────────────┘   ✅ OK     └─────────────────────────┘
          │                                      │
          │ Admin Approve                        │ Admin Approve  
          │                                      │
          ▼                                      ▼
┌─────────────────────┐    Edit     ┌─────────────────────────┐
│ AwaitingGuide       │ ──────────► │ AwaitingAdminApproval   │
│ Assignment          │   🔄 RESET  │ (Gửi lại Admin)       │
│ (Chờ phân công HDV) │             └─────────────────────────┘
└─────────────────────┘                      │
          │                                  │ Admin Approve
          │ Guide Accept                     │
          │ (TourOperation.TourGuideId       ▼
          │  assigned)               ┌─────────────────────────┐
          ▼                          │ AwaitingGuideAssignment │
┌─────────────────────┐              │ (Chờ phân công HDV)    │
│   InProgress        │              └─────────────────────────┘
│ (Has Guide)         │                      │
│                     │                      │ Guide Accept
│ ❌ BLOCK EDIT       │                      ▼
└─────────────────────┘              ┌─────────────────────────┐
                                     │     InProgress          │
                                     │                         │
                                     │ ❌ BLOCK EDIT           │
                                     └─────────────────────────┘

TourOperation Status: Không quan trọng cho admin approval workflow
---

## 🎯 **Key Features**

### **✅ Business Rules Priority Order**

| Priority | Rule | Condition | Action |
|----------|------|-----------|---------|
| **1** | **Block Edit** | `operation.TourGuideId != null` | ❌ **KHÔNG CHO EDIT** |
| **2** | **Reset TourDetails Status** | `tourDetails.Status == AwaitingGuideAssignment` | 🔄 **TourDetails.Status = AwaitingAdminApproval** |
| **3** | **Normal Edit** | Other statuses | ✅ **Allow Edit** |

### **📋 Detailed Logic Flow**

// Step 1: Check guide assignment (STRONGEST RULE)
if (operation.TourGuideId != null) 
{
    return ERROR("Đã có hướng dẫn viên tham gia, không thể edit");
}

// Step 2: Check TourDetails status for reset logic
var tourDetails = await GetTourDetails(operation.TourDetailsId);
if (tourDetails.Status == AwaitingGuideAssignment) 
{
    tourDetails.Status = AwaitingAdminApproval; // Reset to pending admin
    await UpdateTourDetails(tourDetails);
    // Send notification
}

// Step 3: Continue with normal operation update

### **🔔 Notification Details**{
  "title": "📝 Tour đã gửi lại admin",
  "message": "Tour '{tourTitle}' đã được gửi lại cho admin duyệt do có chỉnh sửa trong lúc chờ hướng dẫn viên được phân công.",
  "type": "Tour",
  "priority": "Medium",
  "icon": "📝",
  "actionUrl": "/tours/awaiting-admin-approval"
}
---

## 🚀 **API Usage**

### **PATCH TourOperation Update**
PATCH /api/TourOperation/{operationId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "price": 500000
}
### **Response Examples**

#### ✅ **Success (Normal Update)**{
  "success": true,
  "message": "Cập nhật operation thành công",
  "operation": {
    "id": "...",
    "price": 500000,
    "status": "Scheduled",
    "tourGuideId": null
  }
}
#### ✅ **Success (TourDetails Status Reset)**{
  "success": true,
  "message": "Cập nhật operation thành công. Tour đã được gửi lại cho admin duyệt do có thay đổi trong lúc chờ hướng dẫn viên được phân công.",
  "operation": {
    "id": "...",
    "price": 500000,
    "status": "Scheduled",
    "tourGuideId": null
  }
}
#### ❌ **Error (Guide Already Assigned)**{
  "success": false,
  "message": "Đã có hướng dẫn viên tham gia tour operation, không thể edit nữa"
}
---

## 🎯 **Scenarios Summary**

| TourDetails Status | Guide Assigned? | Edit Allowed? | Result |
|-------------------|----------------|---------------|---------|
| **Approved** | ❌ No | ✅ Yes | Normal update |
| **Approved** | ✅ Yes | ❌ No | Block with error |
| **AwaitingGuideAssignment** | ❌ No | ✅ Yes | Update + Reset TourDetails to AwaitingAdminApproval |
| **AwaitingGuideAssignment** | ✅ Yes | ❌ No | Block with error |
| **Any Status** | ✅ Yes (always) | ❌ No | Block with error |

---

## 🐛 **MAJOR CORRECTION**

### **❌ Sai trước đây**
- Cố gắng thay đổi `TourOperation.Status`
- Không hiểu rằng admin approval dựa trên `TourDetails.Status`

### **✅ Đã sửa đúng**
- **Thay đổi `TourDetails.Status`** khi cần admin duyệt lại
- **TourOperation.Status** không liên quan đến admin approval workflow
- **Logic chính xác**: Edit operation → Update TourDetails status → Admin approval

---

## 📈 **Technical Benefits**

### **🛡️ Data Integrity**
- **Absolute protection** khi guide đã làm việc
- **Correct status flow** cho admin approval process thông qua TourDetails
- **Proper relationship** giữa TourOperation và TourDetails

### **👥 User Experience**
- **Clear error messages** cho từng scenario
- **Smart notifications** khi TourDetails status thay đổi
- **Predictable behavior** cho tour companies

### **🔧 Maintainability**
- **Correct domain logic** với proper entity relationships
- **Clear separation** giữa TourOperation management và admin approval
- **Robust implementation** theo đúng business requirements

---

## 🎯 **Debug Endpoint**
GET /api/TourOperation/debug/{operationId}
**Response:**
{
  "success": true,
  "debugInfo": {
    "operationId": "...",
    "currentPrice": 500000,
    "hasGuideAssigned": false
  },
  "tourDetailsInfo": {
    "message": "TourDetails status là status quan trọng cho admin approval",
    "note": "Khi edit operation → TourDetails status sẽ thay đổi (không phải operation status)"
  },
  "businessRuleChecks": {
    "canEdit": "✅ Có thể edit (chưa có guide)",
    "importantNote": "⚠️ QUAN TRỌNG: Status để admin duyệt là TourDetails.Status, không phải TourOperation.Status!"
  }
}
---

**📅 Implementation Date:** {Current Date}  
**👨‍💻 Implemented By:** GitHub Copilot  
**🔄 Version:** 2.0 (MAJOR CORRECTION - Fixed Domain Logic)  
**🐛 Major Fix:** Corrected to update TourDetails.Status instead of TourOperation.Status  
**📚 Reference:** Proper domain logic với TourDetails làm entity chính cho admin approval