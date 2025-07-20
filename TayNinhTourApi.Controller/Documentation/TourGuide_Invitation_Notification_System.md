# TourGuide Invitation Notification System

## ?? H? th?ng th�ng b�o cho TourCompany v? TourGuide Invitation

H? th?ng ?� ???c c?p nh?t ?? t? ??ng g?i th�ng b�o email cho TourCompany trong c�c t�nh hu?ng sau:

### ?? **C�c lo?i th�ng b�o:**

#### 1. **?? Th�ng b�o khi TourGuide t? ch?i l?i m?i**
- **K�ch ho?t:** Ngay khi TourGuide reject invitation
- **N?i dung:** 
  - T�n h??ng d?n vi�n t? ch?i
  - L� do t? ch?i (n?u c�)
  - H??ng d?n h�nh ??ng ti?p theo

#### 2. **? Th�ng b�o c?n t�m guide th? c�ng (sau 24h)**
- **K�ch ho?t:** Khi TourDetails chuy?n t? `Pending` ? `AwaitingGuideAssignment`
- **N?i dung:**
  - S? l??ng l?i m?i ?� h?t h?n
  - H??ng d?n t�m guide th? c�ng
  - C?nh b�o v? deadline 5 ng�y

#### 3. **?? C?nh b�o tour s?p b? h?y (3 ng�y tr??c)**
- **K�ch ho?t:** Khi tour c�n 3 ng�y tr??c khi b? h?y t? ??ng
- **N?i dung:**
  - Th�ng b�o kh?n c?p
  - S? ng�y c�n l?i
  - H??ng d?n h�nh ??ng ngay l?p t?c

### ?? **Timeline th�ng b�o:**

```
TourGuide reject ? ?? Th�ng b�o ngay l?p t?c
    ?
? 24h: H?t h?n invitations ? ?? Th�ng b�o c?n manual selection
    ?
?? 3 ng�y tr??c h?y ? ?? C?nh b�o kh?n c?p
    ?
?? 5 ng�y: Tour b? h?y ? ?? Th�ng b�o h?y tour
```

### ??? **Technical Implementation:**

#### API Calls:
1. **TourGuide reject invitation:**
   ```
   POST /api/TourGuideInvitation/{invitationId}/reject
   ? Triggers: NotifyGuideRejectionAsync()
   ```

2. **Background job (daily):**
   ```
   TransitionToManualSelectionAsync()
   ? Triggers: NotifyManualGuideSelectionNeededAsync()
   ```

3. **Background job (daily):**
   ```
   CancelUnassignedTourDetailsAsync()
   ? Triggers: NotifyTourRiskCancellationAsync()
   ```

#### Service Methods:
- `NotifyGuideRejectionAsync()`
- `NotifyManualGuideSelectionNeededAsync()`
- `NotifyTourRiskCancellationAsync()`

### ?? **Email Templates:**

#### 1. **Guide Rejection Email:**
```
Subject: Th�ng b�o: H??ng d?n vi�n t? ch?i tour '{tourTitle}'

Content:
- T�n guide t? ch?i
- L� do t? ch?i (n?u c�)
- H�nh ??ng ti?p theo: m?i guide kh�c, ?i?u ch?nh y�u c?u
```

#### 2. **Manual Selection Email:**
```
Subject: C?n h�nh ??ng: Tour '{tourTitle}' ch?a c� h??ng d?n vi�n

Content:
- T�nh tr?ng: X l?i m?i ?� h?t h?n
- H�nh ??ng: ??ng nh?p h? th?ng, g?i l?i m?i th? c�ng
- C?nh b�o: 5 ng�y deadline
```

#### 3. **Risk Cancellation Email:**
```
Subject: ?? KH?N C?P: Tour '{tourTitle}' s?p b? h?y

Content:
- C?nh b�o kh?n c?p
- C�n X ng�y tr??c khi h?y
- H�nh ??ng ngay l?p t?c
- Hotline h? tr?
```

### ??? **Configuration:**

#### Email Settings (appsettings.json):
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "noreply@tayninhtor.com",
    "FromName": "Tay Ninh Tour",
    // ...
  }
}
```

#### Notification Timing:
- **Invitation expiry:** 24 hours (automatic), 3 days (manual)
- **Manual selection:** After 24 hours from TourDetails creation
- **Risk warning:** 3 days before cancellation
- **Auto cancellation:** After 5 days without guide

### ?? **Troubleshooting:**

#### Common Issues:
1. **Email kh�ng g?i ???c:**
   - Ki?m tra EmailSettings trong appsettings
   - Verify SMTP credentials
   - Check firewall/network settings

2. **Notification kh�ng trigger:**
   - Verify BackgroundJobService ?ang ch?y
   - Check logs for error messages
   - Ensure ITourCompanyNotificationService ???c inject ?�ng

#### Debug Commands:
```csharp
// Test notification manually
await _notificationService.NotifyGuideRejectionAsync(
    tourCompanyUserId, 
    "Test Tour", 
    "Test Guide", 
    "Test reason");
```

### ?? **Monitoring:**

#### Logs to Watch:
- `"Sending rejection notification to TourCompany"`
- `"Successfully sent rejection notification"`
- `"Failed to send rejection notification"`

#### Metrics:
- Notification delivery rate
- Email bounce rate  
- TourCompany response time after notification

---

## ?? **Next Steps:**

1. **Test trong staging environment**
2. **Monitor email delivery rates**
3. **Collect feedback t? TourCompany**
4. **Optimize email templates based on usage**

---

*T�i li?u n�y c?p nh?t l?n cu?i: {{current_date}}*