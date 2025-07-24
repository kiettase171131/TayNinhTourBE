# Tính n?ng t? ??ng m?i h??ng d?n viên (HDV) phù h?p

## ?? T?ng quan

H? th?ng **?Ã CÓ** ch?c n?ng t? ??ng m?i h??ng d?n viên phù h?p, nh?ng tr??c ?ây **THI?U** vi?c kích ho?t t? ??ng khi admin duy?t tour. V?n ?? này ?ã ???c **KH?C PH?C**.

## ? Các tính n?ng ?ã có s?n

### 1. **Automatic Guide Matching & Invitation System**
- **File**: `TayNinhTourApi.BusinessLogicLayer\Services\TourGuideInvitationService.cs`
- **Method**: `CreateAutomaticInvitationsAsync()`

**Ch?c n?ng:**
- Tìm ki?m t?t c? TourGuide có s?n trong h? th?ng
- So kh?p skills c?a guide v?i yêu c?u c?a tour (`SkillsRequired`)
- S? d?ng `SkillsMatchingUtility.MatchSkillsEnhanced()` ?? tính ?i?m t??ng thích
- S?p x?p guides theo ?? phù h?p (match score cao nh?t tr??c)
- T?o invitation t? ??ng cho t?t c? guides phù h?p
- G?i email và thông báo in-app cho t?ng guide

### 2. **Skill Matching Algorithm**
- **Enhanced matching**: So sánh skills m?t cách thông minh
- **Match scoring**: Tính ?i?m t??ng thích t? 0-1
- **Priority ranking**: ?u tiên guides có ?i?m cao nh?t

### 3. **Email & Notification System**
- **Email invitations**: G?i email m?i cho guides
- **In-app notifications**: Thông báo trong ?ng d?ng
- **Company notifications**: Thông báo cho tour company v? tình tr?ng m?i guide

### 4. **Invitation Workflow Management**
- **Expiration handling**: X? lý invitation h?t h?n (24 gi?)
- **Status transitions**: Chuy?n ??i tr?ng thái t? ??ng
- **Manual selection**: Chuy?n sang tìm guide th? công sau 24h
- **Tour cancellation**: H?y tour n?u không tìm ???c guide sau 5 ngày

## ?? V?n ?? ?ã kh?c ph?c

### **Problem**: Missing Trigger
Tr??c ?ây, ch?c n?ng t? ??ng m?i guide t?n t?i nh?ng **KHÔNG ???c kích ho?t** khi admin duy?t tour.

### **Solution**: Added Automatic Trigger
**File**: `TayNinhTourApi.BusinessLogicLayer\Services\TourDetailsService.cs`

**Changes Made:**

1. **Updated `ApproveRejectTourDetailAsync()`**:
```csharp
// TRIGGER EMAIL INVITATIONS: G?i email m?i khi admin approve TourDetails
if (request.IsApproved)
{
    await TriggerApprovalEmailsAsync(tourDetail, adminId);
    
    // ?? NEW: TRIGGER AUTOMATIC GUIDE INVITATIONS when approved
    await TriggerAutomaticGuideInvitationsAsync(tourDetail, adminId);
}
```

2. **Added `TriggerAutomaticGuideInvitationsAsync()` method**:
```csharp
private async Task TriggerAutomaticGuideInvitationsAsync(TourDetails tourDetails, Guid adminId)
{
    // Get TourGuideInvitationService using DI
    var invitationService = scope.ServiceProvider.GetRequiredService<ITourGuideInvitationService>();
    
    // Create automatic invitations for suitable guides
    var result = await invitationService.CreateAutomaticInvitationsAsync(tourDetails.Id, adminId);
    
    // Send notifications to TourCompany about approval status
    // Handle both success and error cases gracefully
}
```

## ?? Workflow hoàn ch?nh

### **Khi admin DUY?T tour:**

1. **Update tour status** ? `Approved`
2. **Trigger automatic guide invitations**:
   - Tìm t?t c? guides có skills phù h?p
   - T?o invitation cho t?ng guide phù h?p
   - G?i email m?i cho guides
   - G?i thông báo in-app cho guides
3. **Update tour status** ? `AwaitingGuideAssignment`
4. **Send notification to TourCompany**:
   - Tour ?ã ???c duy?t
   - H? th?ng ?ã t? ??ng g?i l?i m?i cho guides phù h?p
   - Theo dõi ph?n h?i t? guides

### **X? lý ph?n h?i t? guides:**

**Khi guide CH?P NH?N:**
- Update invitation status ? `Accepted`
- Update TourDetails status ? `WaitToPublic`
- Update TourOperation v?i guide info
- Expire t?t c? pending invitations khác
- G?i thông báo cho TourCompany v? guide ?ã ch?p nh?n

**Khi guide T? CH?I:**
- Update invitation status ? `Rejected`
- G?i thông báo cho TourCompany v? lý do t? ch?i
- Ti?p t?c ch? guides khác ph?n h?i

**Khi invitation H?T H?N (24h):**
- Auto-expire pending invitations
- Chuy?n tour sang ch? ?? tìm guide th? công
- G?i thông báo cho TourCompany c?n tìm guide th? công

**Khi KHÔNG TÌM ???C guide (5 ngày):**
- Auto-cancel tour
- G?i email thông báo h?y tour
- Hoàn ti?n cho bookings (n?u có)

## ?? Monitoring & Background Jobs

**Background Service**: `BackgroundJobService.cs`
- **Hourly**: Expire h?t h?n invitations
- **Daily**: Transition to manual selection, Cancel unassigned tours

## ?? Notification System

**TourCompany Notifications**:
- Tour approval v?i automatic invitations
- Guide acceptance/rejection
- Manual selection needed
- Tour risk cancellation
- Tour auto-cancellation

**TourGuide Notifications**:
- New invitation received
- Invitation expiring soon

## ?? Email System

**Email Templates**:
- Guide invitation emails
- Tour approval notifications
- Tour cancellation notifications
- Manual selection reminders

## ?? K?t lu?n

**Tính n?ng t? ??ng m?i HDV phù h?p ?ã hoàn ch?nh:**

? **Automatic skill matching**  
? **Smart guide selection**  
? **Email & notification system**  
? **Workflow automation**  
? **Background job processing**  
? **Error handling & graceful degradation**  
? **Admin approval trigger** ? **M?I THÊM**

**H? th?ng hi?n t?i ho?t ??ng nh? sau:**
1. TourCompany t?o tour ? Pending
2. Admin duy?t tour ? **T? ??NG** tìm và m?i guides phù h?p
3. Guides nh?n email/notification và ph?n h?i
4. H? th?ng t? ??ng x? lý workflow d?a trên ph?n h?i
5. TourCompany nh?n thông báo v? ti?n trình

**Không c?n thêm code gì n?a** - tính n?ng ?ã s?n sàng s? d?ng! ??