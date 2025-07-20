# ?? In-App Notification API Documentation

## ?? **T?ng quan**

H? th?ng notification cung c?p th�ng b�o trong ?ng d?ng (in-app notifications) cho users, bao g?m c? **email notifications** v� **in-app notifications** c� th? xem ???c trong ?ng d?ng.

## ?? **C�c lo?i th�ng b�o**

### **1. ?? Email + In-App Notifications:**
- **TourGuide t? ch?i l?i m?i**
- **C?n t�m guide th? c�ng (sau 24h)**  
- **C?nh b�o tour s?p b? h?y (3 ng�y tr??c)**
- **Booking m?i**
- **Tour b? h?y**
- **Kh�ch h�ng h?y booking**

### **2. ?? Notification Types:**
```csharp
public enum NotificationType
{
    General = 0,     // Th�ng b�o chung
    Booking = 1,     // Th�ng b�o v? booking
    Tour = 2,        // Th�ng b�o v? tour  
    TourGuide = 3,   // Th�ng b�o v? h??ng d?n vi�n
    Payment = 4,     // Th�ng b�o v? thanh to�n
    Wallet = 5,      // Th�ng b�o v? v� ti?n
    System = 6,      // Th�ng b�o h? th?ng
    Promotion = 7,   // Th�ng b�o khuy?n m�i
    Warning = 8,     // Th�ng b�o c?nh b�o
    Error = 9        // Th�ng b�o l?i/v?n ??
}
```

### **3. ? Notification Priority:**
```csharp
public enum NotificationPriority
{
    Low = 0,      // ?? ?u ti�n th?p
    Normal = 1,   // ?? ?u ti�n b�nh th??ng  
    High = 2,     // ?? ?u ti�n cao
    Urgent = 3    // ?? ?u ti�n kh?n c?p
}
```

---

## ?? **API Endpoints**

### **1. ?? L?y danh s�ch th�ng b�o**

```http
GET /api/Notification
```

**Query Parameters:**
- `pageIndex` (int, optional): Trang hi?n t?i (0-based, default: 0)
- `pageSize` (int, optional): K�ch th??c trang (default: 20, max: 100)
- `isRead` (bool, optional): L?c theo tr?ng th�i ??c (null = t?t c?)
- `type` (string, optional): L?c theo lo?i th�ng b�o (null = t?t c?)

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y danh s�ch th�ng b�o th�nh c�ng",
  "success": true,
  "notifications": [
    {
      "id": "uuid",
      "title": "H??ng d?n vi�n t? ch?i",
      "message": "H??ng d?n vi�n John Doe ?� t? ch?i tour 'N�i B� ?en Adventure'",
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

### **2. ?? L?y s? l??ng th�ng b�o ch?a ??c**

```http
GET /api/Notification/unread-count
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "L?y s? th�ng b�o ch?a ??c th�nh c�ng",
  "success": true,
  "unreadCount": 5
}
```

### **3. ? ?�nh d?u th�ng b�o ?� ??c**

```http
PUT /api/Notification/{notificationId}/read
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "?� ?�nh d?u th�ng b�o ?� ??c",
  "success": true
}
```

### **4. ?? ?�nh d?u t?t c? th�ng b�o ?� ??c**

```http
PUT /api/Notification/mark-all-read
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "?� ?�nh d?u 5 th�ng b�o ?� ??c",
  "success": true
}
```

### **5. ??? X�a th�ng b�o**

```http
DELETE /api/Notification/{notificationId}
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "?� x�a th�ng b�o",
  "success": true
}
```

### **6. ?? L?y th?ng k� th�ng b�o**

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
    "message": "B?n c� booking m?i #BK001",
    "createdAt": "2024-01-15T14:30:00Z"
  }
}
```

### **7. ?? L?y th�ng b�o m?i nh?t (Realtime)**

```http
GET /api/Notification/latest?lastCheckTime=2024-01-15T10:00:00Z
```

**Response:** Tr? v? c�c th�ng b�o m?i t? th?i ?i?m `lastCheckTime`

### **8. ????? T?o th�ng b�o (Admin only)**

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
  "title": "Th�ng b�o khuy?n m�i",
  "message": "Gi?m gi� 20% cho tour T�y Ninh cu?i tu?n n�y!",
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
// L?y s? l??ng th�ng b�o ch?a ??c
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
// T? ??ng x�a th�ng b�o c? h?n 30 ng�y
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
// T? ??ng t?o notification sau 24h kh�ng c� guide accept
await notificationService.CreateManualGuideSelectionNotificationAsync(
    tourCompanyUserId, tourTitle, expiredCount);
```

**C. Tour Risk Cancellation:**
```csharp
// T? ??ng t?o notification 3 ng�y tr??c khi h?y tour
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
- Push notifications ngay l?p t?c kh�ng c?n polling

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

? **In-app notifications** c� th? xem trong ?ng d?ng  
? **Email notifications** g?i qua email  
? **API endpoints** ??y ?? cho frontend  
? **Ph�n lo?i** theo type v� priority  
? **Ph�n trang** v� filtering  
? **Background jobs** cho automation  
? **Admin tools** ?? qu?n l�  

Ng??i d�ng c� th? **xem th�ng b�o trong app** thay v� ch? nh?n email! ??