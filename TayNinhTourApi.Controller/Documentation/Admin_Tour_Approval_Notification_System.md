# ?? Admin Tour Approval/Rejection Notification System

## ?? **T?ng quan**

H? th?ng th�ng b�o t? ??ng g?i ??n **TourCompany** khi admin duy?t ho?c t? ch?i tour details, bao g?m c? **email notification** v� **in-app notification**.

---

## ?? **Th�ng b�o ???c th�m m?i**

### **1. ? Th�ng b�o DUY?T tour**

**Khi n�o trigger:**
- Admin duy?t (approve) tour details
- Status chuy?n t? `Pending` ? `Approved`

**N?i dung th�ng b�o:**

#### **?? In-App Notification:**
```json
{
  "title": "? Tour ???c duy?t",
  "message": "Tour 'T�n tour' ?� ???c admin duy?t v� c� th? b?t ??u nh?n booking!",
  "type": "Tour",
  "priority": "High",
  "icon": "?",
  "actionUrl": "/tours/approved"
}
```

#### **?? Email Notification:**
- **Subject:** `?? Ch�c m?ng! Tour 'T�n tour' ?� ???c duy?t`
- **N?i dung:** 
  - Th�ng b�o ch�c m?ng
  - Nh?n x�t t? admin (n?u c�)
  - H??ng d?n b??c ti?p theo:
    - Ki?m tra l?i m?i h??ng d?n vi�n
    - Theo d�i ph?n h?i
    - Chu?n b? tour
    - Marketing
  - Link ??n dashboard

### **2. ? Th�ng b�o T? CH?I tour**

**Khi n�o trigger:**
- Admin t? ch?i (reject) tour details  
- Status chuy?n t? `Pending` ? `Rejected`

**N?i dung th�ng b�o:**

#### **?? In-App Notification:**
```json
{
  "title": "? Tour b? t? ch?i",
  "message": "Tour 'T�n tour' ?� b? admin t? ch?i. Vui l�ng ki?m tra l� do v� ch?nh s?a l?i.",
  "type": "Warning",
  "priority": "High", 
  "icon": "?",
  "actionUrl": "/tours/rejected"
}
```

#### **?? Email Notification:**
- **Subject:** `? Tour 'T�n tour' c?n ch?nh s?a`
- **N?i dung:**
  - Th�ng b�o tour c?n ch?nh s?a
  - L� do t? ch?i c? th? t? admin
  - H??ng d?n h�nh ??ng:
    - ??c k? ph?n h?i
    - Ch?nh s?a tour
    - Ki?m tra l?i
    - G?i l?i duy?t
  - G?i � c?i thi?n
  - Th�ng tin li�n h? support

---

## ?? **Technical Implementation**

### **1. ??? Interface Methods Added**

**ITourCompanyNotificationService.cs:**
```csharp
/// <summary>
/// G?i th�ng b�o khi admin duy?t tour details
/// </summary>
Task<bool> NotifyTourApprovalAsync(
    Guid tourCompanyUserId,
    string tourDetailsTitle,
    string? adminComment = null);

/// <summary>
/// G?i th�ng b�o khi admin t? ch?i tour details
/// </summary>
Task<bool> NotifyTourRejectionAsync(
    Guid tourCompanyUserId,
    string tourDetailsTitle,
    string rejectionReason);
```

### **2. ?? Service Implementation**

**TourCompanyNotificationService.cs:**
- ? `NotifyTourApprovalAsync()` - G?i th�ng b�o duy?t tour
- ? `NotifyTourRejectionAsync()` - G?i th�ng b�o t? ch?i tour
- ?? T? ??ng t?o both email + in-app notifications
- ?? Rich HTML email templates v?i styling

### **3. ?? Workflow Integration**

**TourDetailsService.ApproveRejectTourDetailAsync():**
```csharp
// Sau khi update status trong database
if (request.IsApproved)
{
    // ? G?i th�ng b�o duy?t
    await notificationService.NotifyTourApprovalAsync(
        tourDetail.CreatedById,
        tourDetail.Title,
        request.Comment);
    
    // Trigger email invitations (existing)
    await TriggerApprovalEmailsAsync(tourDetail, adminId);
}
else
{
    // ? G?i th�ng b�o t? ch?i
    await notificationService.NotifyTourRejectionAsync(
        tourDetail.CreatedById,
        tourDetail.Title,
        request.Comment!);
}
```

---

## ?? **API Usage Examples**

### **1. Admin duy?t tour:**

```http
POST /api/Admin/tourdetails/{tourDetailsId}/approve
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "comment": "Tour r?t hay, ?� duy?t ?? public!"
}
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "?� duy?t tour detail th�nh c�ng. Th�ng b�o ?� ???c g?i ??n Tour Company.",
  "success": true
}
```

**Notifications sent:**
- ? In-app notification v?i title "? Tour ???c duy?t"
- ?? Email ch�c m?ng v?i subject "?? Ch�c m?ng! Tour 'T�n tour' ?� ???c duy?t"
- ?? Auto-trigger invitation emails to guides & shops

### **2. Admin t? ch?i tour:**

```http
POST /api/Admin/tourdetails/{tourDetailsId}/reject
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "comment": "M� t? tour ch?a ?? chi ti?t, vui l�ng b? sung th�m th�ng tin v? l?ch tr�nh v� ??a ?i?m tham quan."
}
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "?� t? ch?i tour detail th�nh c�ng. Th�ng b�o ?� ???c g?i ??n Tour Company.",
  "success": true
}
```

**Notifications sent:**
- ? In-app notification v?i title "? Tour b? t? ch?i"
- ?? Email h??ng d?n v?i subject "? Tour 'T�n tour' c?n ch?nh s?a"
- ?? Cancel pending invitations

---

## ?? **Benefits**

### **?? Cho TourCompany:**
- ? **Th�ng b�o ngay l?p t?c** khi admin x? l� tour
- ?? **C? email v� in-app** notification ??m b?o kh�ng b? l?
- ?? **Ph?n h?i chi ti?t** t? admin ?? c?i thi?n tour
- ?? **H??ng d?n c? th?** v? b??c ti?p theo c?n l�m
- ?? **Quick actions** th�ng qua action URLs

### **????? Cho Admin:**
- ? **T? ??ng h�a** vi?c th�ng b�o sau khi approve/reject
- ?? **Tracking** ???c feedback ?� g?i ??n tour company
- ?? **Workflow hi?u qu?** h?n v?i auto-trigger invitations

### **?? Cho H? th?ng:**
- ?? **T?ng engagement** v?i notification system
- ?? **Improve communication** gi?a admin v� tour company
- ?? **Streamlined approval process** v?i t? ??ng h�a
- ?? **Better UX** v?i rich HTML emails v� in-app notifications

---

## ?? **Notification Flow**

```mermaid
graph TD
    A[Admin Click Approve/Reject] --> B[Update TourDetails Status]
    B --> C{Is Approved?}
    
    C -->|Yes| D[Send Approval Notification]
    C -->|No| E[Send Rejection Notification]
    
    D --> F[Create In-App Notification ?]
    D --> G[Send Approval Email ??]
    D --> H[Trigger Guide/Shop Invitations ??]
    
    E --> I[Create In-App Notification ?]
    E --> J[Send Rejection Email ??]
    E --> K[Cancel Pending Invitations ??]
    
    F --> L[TourCompany sees notification]
    G --> L
    H --> M[Guides/Shops receive invitations]
    
    I --> N[TourCompany sees rejection]
    J --> N
    K --> O[Clean up workflow]
```

---

## ?? **Next Steps**

### **?? C� th? m? r?ng:**
1. **SMS notifications** cho nh?ng th�ng b�o quan tr?ng
2. **Push notifications** cho mobile app
3. **Slack/Discord integration** cho team notifications
4. **Analytics dashboard** ?? track approval rates
5. **Auto-reminder** n?u tour company ch?a ??c th�ng b�o

### **?? Metrics c� th? track:**
- T? l? m? email notifications
- Th?i gian ph?n h?i c?a tour company sau rejection
- S? l?n tour ???c approve/reject
- Conversion rate t? pending ? approved

---

## ? **Summary**

**?� ho�n th�nh:**
- ? Admin approval notification system
- ? Admin rejection notification system  
- ? Both email + in-app notifications
- ? Rich HTML email templates
- ? Integration v?i existing workflow
- ? Auto-trigger invitations sau approval
- ? Auto-cancel invitations sau rejection

**TourCompany gi? ?�y s? nh?n ???c th�ng b�o ngay l?p t?c** khi admin duy?t ho?c t? ch?i tour, gi�p h? c� th? h�nh ??ng k?p th?i v� hi?u qu? h?n! ??