# TourGuide Invitation Notification System

## ?? H? th?ng thông báo cho TourCompany v? TourGuide Invitation

H? th?ng ?ã ???c c?p nh?t ?? t? ??ng g?i thông báo email cho TourCompany trong các tình hu?ng sau:

### ?? **Các lo?i thông báo:**

#### 1. **?? Thông báo khi TourGuide t? ch?i l?i m?i**
- **Kích ho?t:** Ngay khi TourGuide reject invitation
- **N?i dung:** 
  - Tên h??ng d?n viên t? ch?i
  - Lý do t? ch?i (n?u có)
  - H??ng d?n hành ??ng ti?p theo

#### 2. **? Thông báo c?n tìm guide th? công (sau 24h)**
- **Kích ho?t:** Khi TourDetails chuy?n t? `Pending` ? `AwaitingGuideAssignment`
- **N?i dung:**
  - S? l??ng l?i m?i ?ã h?t h?n
  - H??ng d?n tìm guide th? công
  - C?nh báo v? deadline 5 ngày

#### 3. **?? C?nh báo tour s?p b? h?y (3 ngày tr??c)**
- **Kích ho?t:** Khi tour còn 3 ngày tr??c khi b? h?y t? ??ng
- **N?i dung:**
  - Thông báo kh?n c?p
  - S? ngày còn l?i
  - H??ng d?n hành ??ng ngay l?p t?c

### ?? **Timeline thông báo:**

```
TourGuide reject ? ?? Thông báo ngay l?p t?c
    ?
? 24h: H?t h?n invitations ? ?? Thông báo c?n manual selection
    ?
?? 3 ngày tr??c h?y ? ?? C?nh báo kh?n c?p
    ?
?? 5 ngày: Tour b? h?y ? ?? Thông báo h?y tour
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
Subject: Thông báo: H??ng d?n viên t? ch?i tour '{tourTitle}'

Content:
- Tên guide t? ch?i
- Lý do t? ch?i (n?u có)
- Hành ??ng ti?p theo: m?i guide khác, ?i?u ch?nh yêu c?u
```

#### 2. **Manual Selection Email:**
```
Subject: C?n hành ??ng: Tour '{tourTitle}' ch?a có h??ng d?n viên

Content:
- Tình tr?ng: X l?i m?i ?ã h?t h?n
- Hành ??ng: ??ng nh?p h? th?ng, g?i l?i m?i th? công
- C?nh báo: 5 ngày deadline
```

#### 3. **Risk Cancellation Email:**
```
Subject: ?? KH?N C?P: Tour '{tourTitle}' s?p b? h?y

Content:
- C?nh báo kh?n c?p
- Còn X ngày tr??c khi h?y
- Hành ??ng ngay l?p t?c
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
1. **Email không g?i ???c:**
   - Ki?m tra EmailSettings trong appsettings
   - Verify SMTP credentials
   - Check firewall/network settings

2. **Notification không trigger:**
   - Verify BackgroundJobService ?ang ch?y
   - Check logs for error messages
   - Ensure ITourCompanyNotificationService ???c inject ?úng

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

*Tài li?u này c?p nh?t l?n cu?i: {{current_date}}*