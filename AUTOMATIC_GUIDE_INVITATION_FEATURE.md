# T�nh n?ng t? ??ng m?i h??ng d?n vi�n (HDV) ph� h?p

## ?? T?ng quan

H? th?ng **?� C�** ch?c n?ng t? ??ng m?i h??ng d?n vi�n ph� h?p, nh?ng tr??c ?�y **THI?U** vi?c k�ch ho?t t? ??ng khi admin duy?t tour. V?n ?? n�y ?� ???c **KH?C PH?C**.

## ? C�c t�nh n?ng ?� c� s?n

### 1. **Automatic Guide Matching & Invitation System**
- **File**: `TayNinhTourApi.BusinessLogicLayer\Services\TourGuideInvitationService.cs`
- **Method**: `CreateAutomaticInvitationsAsync()`

**Ch?c n?ng:**
- T�m ki?m t?t c? TourGuide c� s?n trong h? th?ng
- So kh?p skills c?a guide v?i y�u c?u c?a tour (`SkillsRequired`)
- S? d?ng `SkillsMatchingUtility.MatchSkillsEnhanced()` ?? t�nh ?i?m t??ng th�ch
- S?p x?p guides theo ?? ph� h?p (match score cao nh?t tr??c)
- T?o invitation t? ??ng cho t?t c? guides ph� h?p
- G?i email v� th�ng b�o in-app cho t?ng guide

### 2. **Skill Matching Algorithm**
- **Enhanced matching**: So s�nh skills m?t c�ch th�ng minh
- **Match scoring**: T�nh ?i?m t??ng th�ch t? 0-1
- **Priority ranking**: ?u ti�n guides c� ?i?m cao nh?t

### 3. **Email & Notification System**
- **Email invitations**: G?i email m?i cho guides
- **In-app notifications**: Th�ng b�o trong ?ng d?ng
- **Company notifications**: Th�ng b�o cho tour company v? t�nh tr?ng m?i guide

### 4. **Invitation Workflow Management**
- **Expiration handling**: X? l� invitation h?t h?n (24 gi?)
- **Status transitions**: Chuy?n ??i tr?ng th�i t? ??ng
- **Manual selection**: Chuy?n sang t�m guide th? c�ng sau 24h
- **Tour cancellation**: H?y tour n?u kh�ng t�m ???c guide sau 5 ng�y

## ?? V?n ?? ?� kh?c ph?c

### **Problem**: Missing Trigger
Tr??c ?�y, ch?c n?ng t? ??ng m?i guide t?n t?i nh?ng **KH�NG ???c k�ch ho?t** khi admin duy?t tour.

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

## ?? Workflow ho�n ch?nh

### **Khi admin DUY?T tour:**

1. **Update tour status** ? `Approved`
2. **Trigger automatic guide invitations**:
   - T�m t?t c? guides c� skills ph� h?p
   - T?o invitation cho t?ng guide ph� h?p
   - G?i email m?i cho guides
   - G?i th�ng b�o in-app cho guides
3. **Update tour status** ? `AwaitingGuideAssignment`
4. **Send notification to TourCompany**:
   - Tour ?� ???c duy?t
   - H? th?ng ?� t? ??ng g?i l?i m?i cho guides ph� h?p
   - Theo d�i ph?n h?i t? guides

### **X? l� ph?n h?i t? guides:**

**Khi guide CH?P NH?N:**
- Update invitation status ? `Accepted`
- Update TourDetails status ? `WaitToPublic`
- Update TourOperation v?i guide info
- Expire t?t c? pending invitations kh�c
- G?i th�ng b�o cho TourCompany v? guide ?� ch?p nh?n

**Khi guide T? CH?I:**
- Update invitation status ? `Rejected`
- G?i th�ng b�o cho TourCompany v? l� do t? ch?i
- Ti?p t?c ch? guides kh�c ph?n h?i

**Khi invitation H?T H?N (24h):**
- Auto-expire pending invitations
- Chuy?n tour sang ch? ?? t�m guide th? c�ng
- G?i th�ng b�o cho TourCompany c?n t�m guide th? c�ng

**Khi KH�NG T�M ???C guide (5 ng�y):**
- Auto-cancel tour
- G?i email th�ng b�o h?y tour
- Ho�n ti?n cho bookings (n?u c�)

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

**T�nh n?ng t? ??ng m?i HDV ph� h?p ?� ho�n ch?nh:**

? **Automatic skill matching**  
? **Smart guide selection**  
? **Email & notification system**  
? **Workflow automation**  
? **Background job processing**  
? **Error handling & graceful degradation**  
? **Admin approval trigger** ? **M?I TH�M**

**H? th?ng hi?n t?i ho?t ??ng nh? sau:**
1. TourCompany t?o tour ? Pending
2. Admin duy?t tour ? **T? ??NG** t�m v� m?i guides ph� h?p
3. Guides nh?n email/notification v� ph?n h?i
4. H? th?ng t? ??ng x? l� workflow d?a tr�n ph?n h?i
5. TourCompany nh?n th�ng b�o v? ti?n tr�nh

**Kh�ng c?n th�m code g� n?a** - t�nh n?ng ?� s?n s�ng s? d?ng! ??