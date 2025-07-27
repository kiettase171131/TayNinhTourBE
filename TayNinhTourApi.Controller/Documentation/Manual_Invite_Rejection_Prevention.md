# TourGuide Manual Invitation Enhanced Feature

## ?? Ch?n m?i l?i h??ng d?n vi�n ?� t? ch?i

### **T�nh n?ng m?i:**

H? th?ng ?� ???c c?p nh?t ?? **ng?n ch?n vi?c m?i l?i** h??ng d?n vi�n ?� t? ch?i l?i m?i cho c�ng m?t tour tr??c ?�.

---

## ? **T�nh n?ng chi ti?t:**

### **1. Ki?m tra l?ch s? t? ch?i**
- Khi TourCompany m?i th? c�ng m?t h??ng d?n vi�n
- H? th?ng s? ki?m tra xem h??ng d?n vi�n ?� ?� t? ch?i l?i m?i cho tour n�y tr??c ?� ch?a
- N?u ?� t? ch?i, s? hi?n th? message c?nh b�o v� **kh�ng cho ph�p m?i l?i**

### **2. Message c?nh b�o**"H??ng d?n vi�n n�y ?� t? ch?i l?i m?i cho tour n�y tr??c ?�. Kh�ng th? m?i l?i."
### **3. API Endpoint ?nh h??ng**POST /api/TourDetails/{id:guid}/manual-invite-guide
---

## ?? **Implementation Details:**

### **A. Repository Layer:**

#### **ITourGuideInvitationRepository.cs**/// <summary>
/// Ki?m tra xem TourGuide ?� t? ch?i l?i m?i cho TourDetails n�y tr??c ?� ch?a
/// </summary>
/// <param name="tourDetailsId">ID c?a TourDetails</param>
/// <param name="guideId">ID c?a TourGuide</param>
/// <returns>True n?u ?� c� invitation b? reject</returns>
Task<bool> HasRejectedInvitationAsync(Guid tourDetailsId, Guid guideId);
#### **TourGuideInvitationRepository.cs**public async Task<bool> HasRejectedInvitationAsync(Guid tourDetailsId, Guid guideId)
{
    return await _context.TourGuideInvitations
        .AnyAsync(i => i.TourDetailsId == tourDetailsId
                      && i.GuideId == guideId
                      && i.Status == InvitationStatus.Rejected
                      && !i.IsDeleted);
}
### **B. Service Layer:**

#### **TourGuideInvitationService.cs**
Trong method `CreateManualInvitationAsync`, th�m validation:
// 4. ? NEW: Check if guide has rejected invitation for this tour before
var hasRejected = await _unitOfWork.TourGuideInvitationRepository
    .HasRejectedInvitationAsync(tourDetailsId, guideId);
if (hasRejected)
{
    return new BaseResposeDto
    {
        StatusCode = 400,
        Message = "H??ng d?n vi�n n�y ?� t? ch?i l?i m?i cho tour n�y tr??c ?�. Kh�ng th? m?i l?i.",
        success = false
    };
}
#### **TourDetailsService.cs**
C?p nh?t method `ManualInviteGuideAsync` ?? s? d?ng `TourGuideInvitationService`:
// ? Use TourGuideInvitationService instead of directly creating invitation
using var scope = _serviceProvider.CreateScope();
var invitationService = scope.ServiceProvider.GetRequiredService<ITourGuideInvitationService>();

var result = await invitationService.CreateManualInvitationAsync(tourDetailsId, guideId, companyId);
return result;
---

## ?? **Business Logic:**

### **Quy tr�nh ki?m tra:**
1. **TourCompany** ch?n h??ng d?n vi�n ?? m?i th? c�ng
2. **TourDetailsService** validate quy?n v� tour
3. **TourGuideInvitationService** th?c hi?n ki?m tra c�c ?i?u ki?n:
   - ? TourDetails t?n t?i
   - ? TourGuide t?n t?i v� available
   - ? Kh�ng c� invitation pending
   - ? **KH�NG c� invitation rejected tr??c ?�**
4. **N?u ?� t? ch?i:** Hi?n th? message l?i v� **kh�ng cho ph�p m?i**
5. **N?u ch?a t? ch?i:** T?o invitation m?i

### **L?i �ch:**
- ? **Tr�nh spam invitation** ??n h??ng d?n vi�n
- ? **T�n tr?ng quy?t ??nh** c?a h??ng d?n vi�n
- ? **Gi?m friction** trong h? th?ng
- ? **T?ng user experience** cho c? TourCompany v� TourGuide
- ? **Centralized logic** trong TourGuideInvitationService

---

## ?? **Use Cases:**

### **Scenario 1: H??ng d?n vi�n ?� t? ch?i**Input: TourCompany m?i Guide ?� reject tour n�y
Flow: TourDetailsController ? TourDetailsService ? TourGuideInvitationService
Output: "H??ng d?n vi�n n�y ?� t? ch?i l?i m?i cho tour n�y tr??c ?�. Kh�ng th? m?i l?i."
Result: ? Kh�ng t?o invitation
### **Scenario 2: H??ng d?n vi�n ch?a t? ch?i**Input: TourCompany m?i Guide ch?a c� l?ch s? reject
Flow: TourDetailsController ? TourDetailsService ? TourGuideInvitationService
Output: "?� g?i l?i m?i th�nh c�ng ??n h??ng d?n vi�n"
Result: ? T?o invitation th�nh c�ng
### **Scenario 3: H??ng d?n vi�n ?� reject tour kh�c**Input: TourCompany m?i Guide ?� reject tour A cho tour B
Flow: TourDetailsController ? TourDetailsService ? TourGuideInvitationService
Output: "?� g?i l?i m?i th�nh c�ng ??n h??ng d?n vi�n"
Result: ? T?o invitation th�nh c�ng (ch? check c�ng tour)
---

## ?? **Testing:**

### **Test Cases:**
1. **Test rejection check:**
   - T?o invitation ? Guide reject ? Th? m?i l?i ? Should fail
2. **Test cross-tour:**
   - Guide reject tour A ? M?i cho tour B ? Should success
3. **Test pending check:**
   - T?o invitation pending ? Th? m?i l?i ? Should fail
4. **Test normal flow:**
   - Guide ch?a c� l?ch s? ? M?i ? Should success
5. **Test service integration:**
   - API call ? TourDetailsService ? TourGuideInvitationService ? Database

---

## ?? **Performance Impact:**

- **Database Query:** 1 additional `AnyAsync` query per manual invitation
- **Performance:** Minimal impact (indexed fields)
- **Memory:** No additional memory usage
- **Latency:** < 1ms additional latency
- **Service Layer:** Centralized logic, better maintainability

---

## ?? **Architecture Improvements:**

### **Before:**TourDetailsController 
    ? TourDetailsService (direct DB operations)
        ? Manual invitation creation
### **After:**TourDetailsController 
    ? TourDetailsService (business validation)
        ? TourGuideInvitationService (invitation logic + rejection check)
            ? Repository (database operations)
### **Benefits:**
- ? **Separation of concerns**
- ? **Reusable invitation logic**
- ? **Consistent validation across all invitation flows**
- ? **Easier testing and maintenance**

---

## ?? **Security Considerations:**

- ? **Validation:** Check ownership c?a TourDetails
- ? **Authorization:** Ch? TourCompany c� quy?n m?i
- ? **Data Integrity:** Ki?m tra IsDeleted flag
- ? **Business Logic:** T�n tr?ng rejection decision
- ? **Service Layer Security:** All validation in one place

---

## ?? **Future Enhancements:**

### **Possible improvements:**
1. **Time-based override:** Allow re-invite after N months
2. **Reason-based override:** Allow re-invite if rejection reason was temporary
3. **Admin override:** Allow admin to override rejection
4. **Notification to guide:** Inform guide that company tried to re-invite
5. **Rejection analytics:** Track rejection patterns for insights

---

## ??? **Debugging:**

### **Logs to Check:**
- `"TourCompany {CompanyId} manually inviting Guide {GuideId} for TourDetails {TourDetailsId}"`
- `"Manual invitation result for TourDetails {TourDetailsId}, Guide {GuideId}: Status={StatusCode}"`
- `"Creating manual invitation for TourDetails {TourDetailsId} to Guide {GuideId}"`
- `"Check if guide has rejected invitation for this tour before"`

### **Common Issues:**
1. **Service not injected properly** ? Check DI configuration
2. **Invitation still created** ? Check if correct service method is called
3. **Wrong guideId type** ? Ensure using TourGuide.Id not User.Id

---

*T�i li?u n�y ???c c?p nh?t l?n cu?i: {{current_date}}*