# ?? In-App Notification API Documentation

## ?? **T?ng quan**

H? th?ng notification cung c?p thông báo trong ?ng d?ng (in-app notifications) cho users, bao g?m c? **email notifications** và **in-app notifications** có th? xem ???c trong ?ng d?ng.

## ?? **Các lo?i thông báo**

### **1. ?? Email + In-App Notifications:**
- **TourGuide t? ch?i l?i m?i**
- **C?n tìm guide th? công (sau 24h)**  
- **C?nh báo tour s?p b? h?y (3 ngày tr??c)**
- **Booking m?i**
- **Tour b? h?y**
- **Khách hàng h?y booking**

### **2. ?? Notification Types:**
```csharp
public enum NotificationType
{
    General = 0,     // Thông báo chung
    Booking = 1,     // Thông báo v? booking
    Tour = 2,        // Thông báo v? tour  
    TourGuide = 3,   // Thông báo v? h??ng d?n viên
    Payment = 4,     // Thông báo v? thanh toán
    Wallet = 5,      // Thông báo v? ví ti?n
    System = 6,      // Thông báo h? th?ng
    Promotion = 7,   // Thông báo khuy?n mãi
    Warning = 8,     // Thông báo c?nh báo
    Error = 9        // Thông báo l?i/v?n ??
}
```

### **3. ? Notification Priority:**
```csharp
public enum NotificationPriority
{
    Low = 0,      // ?? ?u tiên th?p
    Normal = 1,   // ?? ?u tiên bình th??ng  
    High = 2,     // ?? ?u tiên cao
    Urgent = 3    // ?? ?u tiên kh?n c?p
}
```

---

## ?? **API Endpoints**

### **1. ?? L?y danh sách thông báo**

```http
GET /api/Notification
```

**Query Parameters:**
- `pageIndex` (int, optional): Trang hi?n t?i (0-based, default: 0)
- `pageSize` (int, optional): Kích th??c trang (default: 20, max: 100)
- `isRead` (bool, optional): L?c theo tr?ng thái ??c (null = t?t c?)
- `type` (string, optional): L?c theo lo?i thông báo (null = t?t c?)

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y danh sách thông báo thành công",
  "success": true,
  "notifications": [
    {
      "id": "uuid",
      "title": "H??ng d?n viên t? ch?i",
      "message": "H??ng d?n viên John Doe ?ã t? ch?i tour 'Núi Bà ?en Adventure'",
      "type": "TourGuide",
      "priority": "High",
      "isRead": false,
      "createdAt": "2024-01-15T10:30:00Z",
      "readAt": null,
      "actionUrl": "/tours/123",
      "icon": "?",
      "timeAgo": "2 gi? tr??c",
      "priorityClass": "priority-high",
      "typeClass": "type-guide"
    }
  ],
  "totalCount": 15,
  "pageIndex": 0,
  "pageSize": 20,
  "totalPages": 1,
  "unreadCount": 5,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

### **2. ?? L?y s? l??ng thông báo ch?a ??c**

```http
GET /api/Notification/unread-count
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y s? thông báo ch?a ??c thành công",
  "success": true,
  "unreadCount": 5
}
```

### **3. ? ?ánh d?u thông báo ?ã ??c**

```http
PUT /api/Notification/{notificationId}/read
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "?ã ?ánh d?u thông báo ?ã ??c",
  "success": true
}
```

### **4. ?? ?ánh d?u t?t c? thông báo ?ã ??c**

```http
PUT /api/Notification/mark-all-read
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "?ã ?ánh d?u 5 thông báo ?ã ??c",
  "success": true
}
```

### **5. ??? Xóa thông báo**

```http
DELETE /api/Notification/{notificationId}
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "?ã xóa thông báo",
  "success": true
}
```

### **6. ?? L?y th?ng kê thông báo**

```http
GET /api/Notification/stats
```

**Response:**
```json
{
  "totalNotifications": 25,
  "unreadCount": 5,
  "readCount": 20,
  "highPriorityCount": 3,
  "urgentCount": 1,
  "latestNotification": {
    "id": "uuid",
    "title": "Booking m?i",
    "message": "B?n có booking m?i #BK001",
    "createdAt": "2024-01-15T14:30:00Z"
  }
}
```

### **7. ?? L?y thông báo m?i nh?t (Realtime)**

```http
GET /api/Notification/latest?lastCheckTime=2024-01-15T10:00:00Z
```

**Response:** Tr? v? các thông báo m?i t? th?i ?i?m `lastCheckTime`

### **8. ????? T?o thông báo (Admin only)**

```http
POST /api/Notification
```

**Headers:**
```
Authorization: Bearer {ADMIN_JWT_TOKEN}
Content-Type: application/json
```

**Request Body:**
```json
{
  "userId": "uuid",
  "title": "Thông báo khuy?n mãi",
  "message": "Gi?m giá 20% cho tour Tây Ninh cu?i tu?n này!",
  "type": "Promotion",
  "priority": "Normal",
  "icon": "??",
  "actionUrl": "/promotions/weekend-deal",
  "expiresAt": "2024-01-31T23:59:59Z"
}
```

---

## ?? **Frontend Integration Examples**

### **1. ?? Notification Badge Component**

```javascript
// L?y s? l??ng thông báo ch?a ??c
const getUnreadCount = async () => {
  try {
    const response = await fetch('/api/Notification/unread-count', {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    const data = await response.json();
    return data.unreadCount;
  } catch (error) {
    console.error('Error fetching unread count:', error);
    return 0;
  }
};

// Component hi?n th? badge
const NotificationBadge = () => {
  const [unreadCount, setUnreadCount] = useState(0);
  
  useEffect(() => {
    getUnreadCount().then(setUnreadCount);
    
    // Polling every 30 seconds
    const interval = setInterval(() => {
      getUnreadCount().then(setUnreadCount);
    }, 30000);
    
    return () => clearInterval(interval);
  }, []);
  
  return (
    <div className="notification-badge">
      ??
      {unreadCount > 0 && (
        <span className="badge">{unreadCount}</span>
      )}
    </div>
  );
};
```

### **2. ?? Notification List Component**

```javascript
const NotificationList = () => {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({
    pageIndex: 0,
    pageSize: 20,
    totalPages: 0
  });

  const fetchNotifications = async (page = 0) => {
    setLoading(true);
    try {
      const response = await fetch(
        `/api/Notification?pageIndex=${page}&pageSize=${pagination.pageSize}`,
        {
          headers: { 'Authorization': `Bearer ${token}` }
        }
      );
      const data = await response.json();
      
      setNotifications(data.notifications);
      setPagination({
        pageIndex: data.pageIndex,
        pageSize: data.pageSize,
        totalPages: data.totalPages
      });
    } catch (error) {
      console.error('Error fetching notifications:', error);
    } finally {
      setLoading(false);
    }
  };

  const markAsRead = async (notificationId) => {
    try {
      await fetch(`/api/Notification/${notificationId}/read`, {
        method: 'PUT',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      
      // Update local state
      setNotifications(notifications.map(n => 
        n.id === notificationId ? { ...n, isRead: true } : n
      ));
    } catch (error) {
      console.error('Error marking as read:', error);
    }
  };

  return (
    <div className="notification-list">
      {notifications.map(notification => (
        <div 
          key={notification.id}
          className={`notification-item ${notification.priorityClass} ${notification.typeClass} ${notification.isRead ? 'read' : 'unread'}`}
          onClick={() => markAsRead(notification.id)}
        >
          <div className="notification-icon">{notification.icon}</div>
          <div className="notification-content">
            <h4>{notification.title}</h4>
            <p>{notification.message}</p>
            <span className="time-ago">{notification.timeAgo}</span>
          </div>
        </div>
      ))}
    </div>
  );
};
```

### **3. ?? CSS Styling**

```css
.notification-badge {
  position: relative;
  display: inline-block;
}

.notification-badge .badge {
  position: absolute;
  top: -8px;
  right: -8px;
  background: #ff4757;
  color: white;
  border-radius: 50%;
  padding: 2px 6px;
  font-size: 12px;
  font-weight: bold;
}

.notification-item {
  display: flex;
  padding: 12px;
  border-bottom: 1px solid #eee;
  cursor: pointer;
  transition: background-color 0.2s;
}

.notification-item:hover {
  background-color: #f8f9fa;
}

.notification-item.unread {
  background-color: #e3f2fd;
  border-left: 4px solid #2196f3;
}

.priority-urgent {
  border-left-color: #f44336 !important;
  background-color: #ffebee;
}

.priority-high {
  border-left-color: #ff9800;
}

.priority-normal {
  border-left-color: #4caf50;
}

.type-warning {
  color: #ff9800;
}

.type-error {
  color: #f44336;
}

.type-guide {
  color: #9c27b0;
}
```

---

## ?? **Background Jobs & Automation**

### **1. ?? Notification Cleanup Job**

```csharp
// T? ??ng xóa thông báo c? h?n 30 ngày
var deletedCount = await notificationService.CleanupOldNotificationsAsync(30);
```

### **2. ?? Auto-notification Triggers**

**A. TourGuide Rejection:**
```csharp
// T? ??ng t?o notification khi guide reject
await notificationService.CreateGuideRejectionNotificationAsync(
    tourCompanyUserId, tourTitle, guideName, rejectionReason);
```

**B. Manual Selection Needed:**
```csharp
// T? ??ng t?o notification sau 24h không có guide accept
await notificationService.CreateManualGuideSelectionNotificationAsync(
    tourCompanyUserId, tourTitle, expiredCount);
```

**C. Tour Risk Cancellation:**
```csharp
// T? ??ng t?o notification 3 ngày tr??c khi h?y tour
await notificationService.CreateTourRiskCancellationNotificationAsync(
    tourCompanyUserId, tourTitle, daysUntilCancellation);
```

---

## ??? **Technical Implementation**

### **1. ?? Database Schema**

```sql
CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    Type INT NOT NULL,
    Priority INT NOT NULL DEFAULT 1,
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME2 NULL,
    AdditionalData NVARCHAR(2000) NULL,
    ActionUrl NVARCHAR(500) NULL,
    Icon NVARCHAR(50) NULL,
    ExpiresAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

### **2. ?? Indexes**

```sql
-- Index cho performance query
CREATE INDEX IX_Notifications_UserId_IsRead ON Notifications(UserId, IsRead);
CREATE INDEX IX_Notifications_UserId_Type ON Notifications(UserId, Type);
CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt);
CREATE INDEX IX_Notifications_ExpiresAt ON Notifications(ExpiresAt);
```

### **3. ?? Cleanup Procedure**

```sql
-- Stored procedure ?? cleanup notifications c?
CREATE PROCEDURE CleanupOldNotifications
    @OlderThanDays INT = 30
AS
BEGIN
    DELETE FROM Notifications 
    WHERE ExpiresAt IS NOT NULL 
    AND ExpiresAt <= DATEADD(day, -@OlderThanDays, GETUTCDATE())
END
```

---

## ?? **Next Steps & Features**

### **1. ?? Real-time with SignalR**
- Implement SignalR cho real-time notifications
- Push notifications ngay l?p t?c không c?n polling

### **2. ?? Smart Filtering**
- Filter theo tour specific
- Filter theo date range
- Advanced search trong notifications

### **3. ?? Analytics & Metrics**
- Notification open rates
- User engagement metrics
- Popular notification types

### **4. ?? Push Notifications**
- Browser push notifications
- Mobile app push notifications
- Email + SMS integration

---

## ?? **Summary**

H? th?ng notification hi?n t?i cung c?p:

? **In-app notifications** có th? xem trong ?ng d?ng  
? **Email notifications** g?i qua email  
? **API endpoints** ??y ?? cho frontend  
? **Phân lo?i** theo type và priority  
? **Phân trang** và filtering  
? **Background jobs** cho automation  
? **Admin tools** ?? qu?n lý  

Ng??i dùng có th? **xem thông báo trong app** thay vì ch? nh?n email! ??