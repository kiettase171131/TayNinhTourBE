# ? Test Checklist - No Suitable Guides Notification

## ?? Pre-test Setup

- [ ] Ensure EmailSender is configured properly in `appsettings.json`
- [ ] Database connection is working  
- [ ] Admin user token is available
- [ ] At least one TourDetails exists in database
- [ ] At least one TourCompany user exists

## ?? Test Cases

### Test Case 1: Debug Endpoint - Valid TourDetails
**Objective:** Verify debug endpoint works with valid TourDetails

**Steps:**
1. [ ] Get a valid TourDetails ID from database
2. [ ] Call debug endpoint: `POST /api/admin/debug/test-no-suitable-guides-notification/{tourDetailsId}`
3. [ ] Use valid admin token

**Expected Results:**
- [ ] HTTP 200 response
- [ ] Success message: "Debug: ?ã g?i thông báo test thành công"
- [ ] New notification record in database
- [ ] Email sent (check logs)

**Verification:**
```sql
-- Check notification was created
SELECT * FROM Notifications 
WHERE UserId = (SELECT CreatedById FROM TourDetails WHERE Id = '{tourDetailsId}')
AND CreatedAt > NOW() - INTERVAL 5 MINUTE
ORDER BY CreatedAt DESC;
```

### Test Case 2: Debug Endpoint - Invalid TourDetails
**Objective:** Verify proper error handling for non-existent TourDetails

**Steps:**
1. [ ] Use a random GUID that doesn't exist: `123e4567-e89b-12d3-a456-426614174000`
2. [ ] Call debug endpoint with invalid ID
3. [ ] Use valid admin token

**Expected Results:**
- [ ] HTTP 404 response
- [ ] Error message: "TourDetails không t?n t?i"
- [ ] No notification created
- [ ] No email sent

### Test Case 3: Debug Endpoint - Unauthorized Access
**Objective:** Verify admin-only access restriction

**Steps:**
1. [ ] Use valid TourDetails ID
2. [ ] Call debug endpoint without token OR with non-admin token
3. [ ] Check response

**Expected Results:**
- [ ] HTTP 401 (no token) or 403 (non-admin token)
- [ ] Appropriate error message
- [ ] No notification created

### Test Case 4: Natural Flow - Auto Invite with No Matching Guides
**Objective:** Test notification through natural approval flow

**Steps:**
1. [ ] Create TourDetails with very specific/rare skills:
   ```json
   {
     "title": "Test Tour - Rare Skills",
     "skillsRequired": "UnicornRiding,DragonTaming,PhoenixFeatherCollection"
   }
   ```
2. [ ] Admin approve the tour: `POST /api/admin/tourdetails/{id}/approve`
3. [ ] System auto-invites guides ? finds none ? sends notification

**Expected Results:**
- [ ] HTTP 200 approval response
- [ ] Auto-invite process triggered
- [ ] No matching guides found
- [ ] Notification sent to TourCompany
- [ ] Log message: "No tour guides found with matching skills"

### Test Case 5: Verify Email Content
**Objective:** Check email template renders correctly

**Manual Check (if email service configured):**
- [ ] Recipient receives email
- [ ] Subject line correct: "C?n ch?n h??ng d?n viên: Tour '{TourTitle}'"
- [ ] HTML formatting displays properly
- [ ] All placeholders filled (no {TourTitle} text)
- [ ] Links work (if any)
- [ ] Mobile-friendly layout

### Test Case 6: Verify In-App Notification
**Objective:** Check in-app notification appears correctly

**Steps:**
1. [ ] Call debug endpoint successfully
2. [ ] Get notifications for TourCompany user: `GET /api/notifications/user/{userId}`
3. [ ] Check notification details

**Expected Results:**
- [ ] Notification appears in user's notification list
- [ ] Title: "?? Không tìm th?y h??ng d?n viên phù h?p"
- [ ] Priority: "High"
- [ ] Icon: "??"
- [ ] ActionUrl: "/guides/list"
- [ ] IsRead: false
- [ ] CreatedAt within last few minutes

## ?? Debugging Steps

### If notification not received:

1. **Check Application Logs:**
   ```
   Search for these log messages:
   - "Sending no suitable guides notification to TourCompany"
   - "Successfully created in-app notification"
   - "Successfully sent email notification"
   - "Error sending no suitable guides notification"
   ```

2. **Check Database:**
   ```sql
   -- Verify notification record
   SELECT Id, UserId, Title, Message, CreatedAt 
   FROM Notifications 
   WHERE Title LIKE '%Không tìm th?y h??ng d?n viên%'
   ORDER BY CreatedAt DESC;
   
   -- Verify TourDetails owner
   SELECT Id, CreatedById, Title, CreatedAt
   FROM TourDetails 
   WHERE Id = '{your-test-tour-id}';
   
   -- Check if user exists
   SELECT Id, Name, Email, IsActive 
   FROM Users 
   WHERE Id = '{createdById-from-above}';
   ```

3. **Check Email Configuration:**
   ```json
   // In appsettings.json
   {
     "EmailSettings": {
       "SmtpHost": "smtp.gmail.com",
       "SmtpPort": 587,
       "FromEmail": "noreply@tayninhour.com",
       "FromName": "Tay Ninh Tour",
       "Username": "your-email@gmail.com",
       "Password": "your-app-password"
     }
   }
   ```

### If API returns error:

1. **Check Request Format:**
   - Valid GUID format for tourDetailsId
   - Bearer token in Authorization header
   - Admin role for token

2. **Check Server Logs:**
   - Look for exception stack traces
   - Check database connection errors
   - Verify service registrations

## ? Success Criteria

The test is successful when:

- [ ] Debug endpoint works for valid inputs
- [ ] Proper error handling for invalid inputs
- [ ] Security restrictions enforced (admin-only)
- [ ] Notifications created in database
- [ ] Emails sent (if email service configured)
- [ ] No errors in application logs
- [ ] Natural flow triggers notification correctly

## ?? Test Results Record

**Test Date:** ________________  
**Tester:** ____________________  
**Environment:** _______________  

| Test Case | Status | Notes |
|-----------|--------|-------|
| Debug Endpoint - Valid | ? Pass ? Fail | |
| Debug Endpoint - Invalid | ? Pass ? Fail | |
| Unauthorized Access | ? Pass ? Fail | |
| Natural Flow | ? Pass ? Fail | |
| Email Content | ? Pass ? Fail | |
| In-App Notification | ? Pass ? Fail | |

**Overall Result:** ? All Tests Pass ? Issues Found

**Issues Found:**
_________________________________
_________________________________
_________________________________

**Sign-off:** ____________________