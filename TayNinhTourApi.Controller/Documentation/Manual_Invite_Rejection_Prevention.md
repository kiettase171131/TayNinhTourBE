# TourGuide Manual Invitation Enhanced Feature

## ?? Ch?n m?i l?i h??ng d?n viên ?ã t? ch?i

### **Tính n?ng m?i:**

H? th?ng ?ã ???c c?p nh?t ?? **ng?n ch?n vi?c m?i l?i** h??ng d?n viên ?ã t? ch?i l?i m?i cho cùng m?t tour tr??c ?ó.

---

## ? **Tính n?ng chi ti?t:**

### **1. Ki?m tra l?ch s? t? ch?i**
- Khi TourCompany m?i th? công m?t h??ng d?n viên
- H? th?ng s? ki?m tra xem h??ng d?n viên ?ó ?ã t? ch?i l?i m?i cho tour này tr??c ?ó ch?a
- N?u ?ã t? ch?i, s? hi?n th? message c?nh báo và **không cho phép m?i l?i**

### **2. Message c?nh báo**"H??ng d?n viên này ?ã t? ch?i l?i m?i cho tour này tr??c ?ó. Không th? m?i l?i."
### **3. API Endpoint ?nh h??ng**POST /api/TourDetails/{id:guid}/manual-invite-guide
---

## ?? **Implementation Details:**

### **A. Repository Layer:**

#### **ITourGuideInvitationRepository.cs**/// <summary>
/// Ki?m tra xem TourGuide ?ã t? ch?i l?i m?i cho TourDetails này tr??c ?ó ch?a
/// </summary>
/// <param name="tourDetailsId">ID c?a TourDetails</param>
/// <param name="guideId">ID c?a TourGuide</param>
/// <returns>True n?u ?ã có invitation b? reject</returns>
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
Trong method `CreateManualInvitationAsync`, thêm validation:
// 4. ? NEW: Check if guide has rejected invitation for this tour before
var hasRejected = await _unitOfWork.TourGuideInvitationRepository
    .HasRejectedInvitationAsync(tourDetailsId, guideId);
if (hasRejected)
{
    return new BaseResposeDto
    {
        StatusCode = 400,
        Message = "H??ng d?n viên này ?ã t? ch?i l?i m?i cho tour này tr??c ?ó. Không th? m?i l?i.",
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

### **Quy trình ki?m tra:**
1. **TourCompany** ch?n h??ng d?n viên ?? m?i th? công
2. **TourDetailsService** validate quy?n và tour
3. **TourGuideInvitationService** th?c hi?n ki?m tra các ?i?u ki?n:
   - ? TourDetails t?n t?i
   - ? TourGuide t?n t?i và available
   - ? Không có invitation pending
   - ? **KHÔNG có invitation rejected tr??c ?ó**
4. **N?u ?ã t? ch?i:** Hi?n th? message l?i và **không cho phép m?i**
5. **N?u ch?a t? ch?i:** T?o invitation m?i

### **L?i ích:**
- ? **Tránh spam invitation** ??n h??ng d?n viên
- ? **Tôn tr?ng quy?t ??nh** c?a h??ng d?n viên
- ? **Gi?m friction** trong h? th?ng
- ? **T?ng user experience** cho c? TourCompany và TourGuide
- ? **Centralized logic** trong TourGuideInvitationService

---

## ?? **Use Cases:**

### **Scenario 1: H??ng d?n viên ?ã t? ch?i**Input: TourCompany m?i Guide ?ã reject tour này
Flow: TourDetailsController ? TourDetailsService ? TourGuideInvitationService
Output: "H??ng d?n viên này ?ã t? ch?i l?i m?i cho tour này tr??c ?ó. Không th? m?i l?i."
Result: ? Không t?o invitation
### **Scenario 2: H??ng d?n viên ch?a t? ch?i**Input: TourCompany m?i Guide ch?a có l?ch s? reject
Flow: TourDetailsController ? TourDetailsService ? TourGuideInvitationService
Output: "?ã g?i l?i m?i thành công ??n h??ng d?n viên"
Result: ? T?o invitation thành công
### **Scenario 3: H??ng d?n viên ?ã reject tour khác**Input: TourCompany m?i Guide ?ã reject tour A cho tour B
Flow: TourDetailsController ? TourDetailsService ? TourGuideInvitationService
Output: "?ã g?i l?i m?i thành công ??n h??ng d?n viên"
Result: ? T?o invitation thành công (ch? check cùng tour)
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
   - Guide ch?a có l?ch s? ? M?i ? Should success
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
- ? **Authorization:** Ch? TourCompany có quy?n m?i
- ? **Data Integrity:** Ki?m tra IsDeleted flag
- ? **Business Logic:** Tôn tr?ng rejection decision
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

*Tài li?u này ???c c?p nh?t l?n cu?i: {{current_date}}*