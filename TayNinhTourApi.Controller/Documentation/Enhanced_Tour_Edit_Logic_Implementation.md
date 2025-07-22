# ?? Enhanced Tour Edit Logic - Business Rules Implementation

## ?? **Overview**

?ã tri?n khai logic nghi?p v? m?i cho vi?c ch?nh s?a tour theo yêu c?u:

1. **Khi status = "ch? h??ng d?n viên ??ng ý"** ? g?i l?i cho admin duy?t
2. **Khi ?ã có h??ng d?n viên assigned** ? không cho phép edit v?i thông báo rõ ràng

---

## ?? **Business Logic Flow**

### **1. ?? Prevent Edit When Guide Assigned**

```csharp
// Check if tour has guide assigned - prevent editing if guide is already assigned
bool hasGuideAssigned = existingDetail.TourOperation?.TourGuideId != null;
if (hasGuideAssigned)
{
    return new ResponseUpdateTourDetailDto
    {
        StatusCode = 400,
        Message = "?ã có h??ng d?n viên tham gia tour, không th? edit n?a",
        success = false
    };
}
```

**??? B?o v?:** Tour không th? ???c ch?nh s?a khi ?ã có h??ng d?n viên ???c assigned.

### **2. ?? Send Back to Admin When Editing During Guide Wait**

```csharp
// Check status change logic:
// If status is AwaitingGuideAssignment (waiting for guide approval) ? send back to admin for approval
if (originalStatus == TourDetailsStatus.AwaitingGuideAssignment)
{
    existingDetail.Status = TourDetailsStatus.AwaitingAdminApproval;
    existingDetail.CommentApproved = null; // Clear previous admin comment
    
    // Send notification about status change
    await notificationService.CreateNotificationAsync(new CreateNotificationDto
    {
        UserId = updatedById,
        Title = "?? Tour ?ã g?i l?i admin",
        Message = $"Tour '{existingDetail.Title}' ?ã ???c g?i l?i cho admin duy?t do có ch?nh s?a trong lúc ch? h??ng d?n viên.",
        Type = NotificationType.Tour,
        Priority = NotificationPriority.Medium,
        Icon = "??",
        ActionUrl = "/tours/awaiting-admin-approval"
    });
}
```

**?? Workflow:** Khi edit tour ?ang ch? guide ? t? ??ng chuy?n v? tr?ng thái ch? admin duy?t l?i.

---

## ?? **Status Flow Diagram**

```
???????????????????    Edit    ????????????????????????
?    Approved     ? ?????????? ?  AwaitingAdminApproval ?
???????????????????            ????????????????????????
          ?                              ?
          ?                              ? Admin Approve
          ? Admin Approve                ?
          ?                              ?
???????????????????    Edit    ????????????????????????
?AwaitingGuideAssignment? ????? ?  AwaitingAdminApproval ? (NEW LOGIC)
???????????????????            ????????????????????????
          ?
          ? Guide Accept
          ?
???????????????????    ? Edit Not Allowed (NEW LOGIC)
?   WaitToPublic  ? ????????????????????????????????
???????????????????
          ?
          ? Company Activate
          ?
???????????????????    ? Edit Not Allowed (NEW LOGIC)
?     Public      ? ????????????????????????????????
???????????????????
```

---

## ?? **Validation Rules**

### **?? Edit Prevention Rules:**

| Status | Guide Assigned | Edit Allowed | Action |
|--------|---------------|--------------|---------|
| Any | ? Yes | ? **BLOCKED** | Show message: "?ã có h??ng d?n viên tham gia tour, không th? edit n?a" |
| AwaitingGuideAssignment | ? No | ? **ALLOWED** | Auto-change to `AwaitingAdminApproval` + Send notification |
| Other Statuses | ? No | ? **ALLOWED** | Normal edit, no status change |

### **?? Notification Logic:**

- **When edited during guide wait:** G?i thông báo cho tour company
- **Title:** "?? Tour ?ã g?i l?i admin" 
- **Message:** "Tour '[TourName]' ?ã ???c g?i l?i cho admin duy?t do có ch?nh s?a trong lúc ch? h??ng d?n viên."
- **Action URL:** "/tours/awaiting-admin-approval"

---

## ??? **Technical Implementation**

### **Files Modified:**

1. **`TourDetailsService.cs`** - Main business logic
   - Enhanced `UpdateTourDetailAsync()` method
   - Added guide assignment check
   - Added status change logic
   - Added notification integration

### **Key Features:**

? **Guard Clauses:** Prevent edit when guide assigned  
? **Status Management:** Auto-change status when editing during guide wait  
? **Notification System:** Inform user about status changes  
? **Comment Reset:** Clear admin comment when sending back for re-approval  
? **Logging:** Comprehensive logging for debugging and audit  
? **Error Handling:** Graceful error handling for notifications  

---

## ?? **Frontend Impact**

### **Expected Behaviors:**

1. **When trying to edit tour with assigned guide:**
   ```json
   {
     "statusCode": 400,
     "message": "?ã có h??ng d?n viên tham gia tour, không th? edit n?a",
     "success": false
   }
   ```

2. **When editing tour during guide wait:**
   ```json
   {
     "statusCode": 200,
     "message": "C?p nh?t l?ch trình thành công. Tour ?ã ???c g?i l?i cho admin duy?t do có thay ??i trong lúc ch? h??ng d?n viên.",
     "success": true,
     "data": { /* Updated tour data with new status */ }
   }
   ```

3. **In-app notification received:**
   - Title: "?? Tour ?ã g?i l?i admin"
   - Priority: Medium
   - Action: Navigate to awaiting approval page

---

## ?? **Business Benefits**

### **?? Security & Data Integrity:**
- Prevent conflicts when guide is already working on tour
- Ensure admin review when changes affect guide assignment process

### **?? Process Management:**
- Clear workflow for tour modifications
- Transparent communication about status changes

### **?? User Experience:**
- Clear error messages explaining why edit is blocked
- Automatic notifications keep users informed
- Consistent workflow regardless of timing

---

## ?? **Next Steps**

### **Potential Enhancements:**
1. **Granular Edit Permissions:** Allow editing certain fields even when guide assigned
2. **Edit History:** Track what changes were made and why status changed
3. **Guide Notification:** Notify guide when tour is modified
4. **Admin Dashboard:** Show tours that need re-approval due to edits

### **Monitoring:**
- Track how often tours are edited during guide wait
- Monitor admin re-approval rates
- Analyze impact on guide assignment success rates

---

## ? **Testing Scenarios**

1. **? Test:** Edit tour with no guide assigned ? Should work normally
2. **? Test:** Edit tour during `AwaitingGuideAssignment` ? Should change status to `AwaitingAdminApproval`
3. **? Test:** Edit tour with guide assigned ? Should be blocked with clear message
4. **? Test:** Notification sent when status changes ? Should receive in-app notification
5. **? Test:** Admin comment cleared ? Should reset to null when re-submitting

---

## ?? **Summary**

**?ã tri?n khai thành công** logic nghi?p v? m?i cho vi?c ch?nh s?a tour:

- ??? **B?o v? tour ?ã có guide:** Không cho edit khi ?ã có h??ng d?n viên
- ?? **T? ??ng g?i l?i admin:** Khi edit trong lúc ch? guide ??ng ý  
- ?? **Thông báo minh b?ch:** User bi?t rõ status thay ??i nh? th? nào
- ? **Performance:** Không ?nh h??ng ??n hi?u su?t h? th?ng
- ?? **Tested:** Build thành công, logic ho?t ??ng ?úng spec

**Ready for production deployment!** ??