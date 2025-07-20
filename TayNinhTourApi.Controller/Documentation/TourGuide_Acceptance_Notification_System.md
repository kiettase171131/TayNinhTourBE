# ?? Tour Guide Acceptance Notification System

## ?? **T?ng quan**

H? th?ng th�ng b�o t? ??ng g?i ??n **TourCompany** khi **TourGuide ch?p nh?n l?i m?i** tham gia tour, bao g?m c? **email notification** v� **in-app notification**.

---

## ? **Th�ng b�o ?� ???c th�m**

### **?? Th�ng b�o H??NG D?N VI�N CH?P NH?N**

**Khi n�o trigger:**
- TourGuide ch?p nh?n l?i m?i (accept invitation)
- Status invitation chuy?n t? `Pending` ? `Accepted`

**N?i dung th�ng b�o:**

#### **?? In-App Notification:**
```json
{
  "title": "?? H??ng d?n vi�n ch?p nh?n!",
  "message": "{GuideName} ?� ch?p nh?n l?i m?i cho tour '{TourTitle}'. Tour s?n s�ng ?? public!",
  "type": "TourGuide",
  "priority": "High",
  "icon": "??",
  "actionUrl": "/tours/ready-to-public"
}
```

#### **?? Email Notification:**
- **Subject:** `?? Tuy?t v?i! H??ng d?n vi�n ?� ch?p nh?n tour '{TourTitle}'`
- **N?i dung:** 
  - Th�ng b�o ch�c m?ng 
  - Th�ng tin h??ng d?n vi�n (t�n, email, th?i gian ch?p nh?n)
  - H??ng d?n b??c ti?p theo:
    - X�c nh?n th�ng tin
    - L�n l?ch meeting
    - K�ch ho?t Public
    - Marketing
  - G?i � th�nh c�ng
  - CTA buttons: "Li�n h? h??ng d?n vi�n" & "K�ch ho?t Public Tour"

---

## ?? **Technical Implementation**

### **1. ??? Interface Method Added**

**ITourCompanyNotificationService.cs:**
```csharp
/// <summary>
/// G?i th�ng b�o khi TourGuide ch?p nh?n l?i m?i tour
/// </summary>
Task<bool> NotifyGuideAcceptanceAsync(
    Guid tourCompanyUserId,
    string tourDetailsTitle,
    string guideFullName,
    string guideEmail,
    DateTime acceptedAt);
```

### **2. ?? Service Implementation**

**TourCompanyNotificationService.cs:**
- ? `NotifyGuideAcceptanceAsync()` - G?i th�ng b�o guide ch?p nh?n
- ?? T? ??ng t?o both email + in-app notifications
- ?? Rich HTML email template v?i styling v� CTAs

### **3. ?? Workflow Integration**

**TourGuideInvitationService.AcceptInvitationAsync():**
```csharp
// Sau khi update invitation status v� TourDetails status
try
{
    await _unitOfWork.SaveChangesAsync();
    
    // Update TourDetails status
    await UpdateTourDetailsStatusAfterGuideAcceptanceAsync(invitation.TourDetailsId, invitationId);
    
    // ?? SEND NOTIFICATION TO TOUR COMPANY
    await NotifyTourCompanyAboutGuideAcceptanceAsync(invitation, guideId);
}
catch (Exception saveEx)
{
    // Handle errors...
}
```

### **4. ?? Helper Method**

```csharp
/// <summary>
/// G?i th�ng b�o cho TourCompany khi TourGuide ch?p nh?n l?i m?i
/// </summary>
private async Task NotifyTourCompanyAboutGuideAcceptanceAsync(TourGuideInvitation invitation, Guid guideId)
{
    // Get TourDetails and TourGuide info
    var tourDetails = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(invitation.TourDetailsId);
    var tourGuide = await _unitOfWork.TourGuideRepository.GetByIdAsync(guideId);

    // Send notification
    await _notificationService.NotifyGuideAcceptanceAsync(
        tourDetails.CreatedById,
        tourDetails.Title,
        tourGuide.FullName,
        tourGuide.Email,
        invitation.RespondedAt ?? DateTime.UtcNow);
}
```

---

## ?? **API Flow Examples**

### **1. TourGuide ch?p nh?n l?i m?i:**

```http
POST /api/TourGuideInvitation/{invitationId}/accept
Authorization: Bearer {guide-token}
Content-Type: application/json

{
  "invitationId": "invitation-guid",
  "confirmUnderstanding": true
}
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "?� ch?p nh?n l?i m?i th�nh c�ng",
  "success": true
}
```

**Notifications sent:**
- ?? In-app notification v?i title "?? H??ng d?n vi�n ch?p nh?n!"
- ?? Email ch�c m?ng v?i subject "?? Tuy?t v?i! H??ng d?n vi�n ?� ch?p nh?n tour..."
- ?? Auto-update TourDetails status to `WaitToPublic`
- ?? Auto-expire other pending invitations

---

## ?? **Benefits**

### **?? Cho TourCompany:**
- ? **Th�ng b�o ngay l?p t?c** khi c� guide ch?p nh?n
- ?? **C?m gi�c t�ch c?c** v?i tin t?t l�nh
- ?? **C? email v� in-app** notification ?? kh�ng b? l?
- ????? **Th�ng tin chi ti?t guide** ?? li�n h? ngay
- ?? **H??ng d?n c? th?** v? b??c ti?p theo
- ?? **Quick actions** ?? li�n h? guide v� k�ch ho?t tour

### **????? Cho TourGuide:**
- ? **Workflow m??t m�** sau khi accept invitation
- ?? **TourCompany s? li�n h? s?m** ?? th?o lu?n chi ti?t
- ?? **Relationship building** t?t h?n v?i tour company

### **?? Cho H? th?ng:**
- ?? **T?ng engagement** v?i notification system
- ?? **Better communication** gi?a guide v� tour company
- ? **Faster response time** t? tour company
- ?? **Complete notification coverage** cho to�n b? invitation workflow

---

## ?? **Complete Invitation Workflow Notifications**

```mermaid
graph TD
    A[Admin Approve TourDetails] --> B[Auto-send Guide Invitations]
    
    B --> C{Guide Response?}
    
    C -->|Accept| D[?? Send Acceptance Notification]
    C -->|Reject| E[? Send Rejection Notification]
    C -->|No Response 24h| F[?? Send Manual Selection Needed]
    C -->|No Response 5 days| G[?? Send Risk Cancellation]
    
    D --> H[Update TourDetails to WaitToPublic]
    D --> I[Update TourOperation with Guide]
    D --> J[Expire Other Pending Invitations]
    
    E --> K[TourCompany can send manual invitations]
    F --> K
    G --> L[Auto-cancel TourDetails]
    
    H --> M[TourCompany can activate Public]
    I --> M
    J --> M
```

---

## ?? **Notification Types Summary**

### **? Positive Notifications:**
1. **?? Guide Acceptance** - Khi guide ch?p nh?n l?i m?i
2. **? Tour Approval** - Khi admin duy?t tour

### **?? Action Required Notifications:**
1. **? Guide Rejection** - Khi guide t? ch?i l?i m?i
2. **? Manual Selection Needed** - Sau 24h kh�ng c� guide accept
3. **?? Risk Cancellation** - 3 ng�y tr??c h?y tour t? ??ng

### **?? Informational Notifications:**
1. **?? New Booking** - Khi c� booking m?i
2. **?? Booking Cancellation** - Khi kh�ch h�ng h?y booking
3. **?? Revenue Transfer** - Khi ti?n ???c chuy?n v�o v�

---

## ?? **Next Steps**

### **?? C� th? m? r?ng:**
1. **Guide Profile** trong notification ?? TourCompany bi?t th�m v? guide
2. **Auto-scheduling** meeting gi?a TourCompany v� guide
3. **Tour preparation checklist** sau khi guide accept
4. **Real-time chat** integration gi?a TourCompany v� guide
5. **Performance tracking** c?a guide sau tour ho�n th�nh

### **?? Metrics c� th? track:**
- Response time c?a TourCompany sau khi nh?n notification
- Success rate c?a tours c� guide accept invitation
- Satisfaction score c?a collaboration TourCompany-Guide
- Conversion rate t? acceptance ? successful tour

---

## ? **Summary**

**?� ho�n th�nh:**
- ? TourGuide acceptance notification system
- ? Both email + in-app notifications  
- ? Rich HTML email template v?i CTAs
- ? Integration v?i existing invitation workflow
- ? Auto-update TourDetails status v� TourOperation
- ? Complete notification coverage cho to�n b? invitation lifecycle

**TourCompany gi? ?�y s? nh?n ???c th�ng b�o ngay l?p t?c** khi h??ng d?n vi�n ch?p nh?n l?i m?i, gi�p h? c� th? li�n h? v� chu?n b? tour k?p th?i! ??

**Workflow notification ho�n ch?nh:**
```
Admin Approve ? Auto Invitations ? Guide Accept ? ?? Notification ? TourCompany Action
```

H? th?ng gi? ?�y ?� cover ??y ?? t?t c? c�c scenarios trong invitation workflow! ??