-- Manual migration script để tạo TourBookingGuests table
-- Chạy script này trực tiếp trong MySQL nếu EF migration gặp vấn đề

-- Kiểm tra xem table đã tồn tại chưa
DROP TABLE IF EXISTS `TourBookingGuests`;

-- Tạo TourBookingGuests table
CREATE TABLE `TourBookingGuests` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `TourBookingId` char(36) COLLATE ascii_general_ci NOT NULL COMMENT 'ID của TourBooking chứa guest này',
    `GuestName` varchar(100) CHARACTER SET utf8mb4 NOT NULL COMMENT 'Họ và tên của khách hàng',
    `GuestEmail` varchar(100) CHARACTER SET utf8mb4 NOT NULL COMMENT 'Email của khách hàng (unique trong cùng booking)',
    `GuestPhone` varchar(20) CHARACTER SET utf8mb4 NULL COMMENT 'Số điện thoại của khách hàng (tùy chọn)',
    `QRCodeData` longtext CHARACTER SET utf8mb4 NULL COMMENT 'QR code data riêng cho khách hàng này',
    `IsCheckedIn` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Trạng thái check-in của khách hàng',
    `CheckInTime` datetime(6) NULL COMMENT 'Thời gian check-in thực tế',
    `CheckInNotes` varchar(500) CHARACTER SET utf8mb4 NULL COMMENT 'Ghi chú bổ sung khi check-in',
    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    `CreatedById` char(36) COLLATE ascii_general_ci NOT NULL,
    `UpdatedById` char(36) COLLATE ascii_general_ci NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `DeletedAt` datetime(6) NULL,

    CONSTRAINT `PK_TourBookingGuests` PRIMARY KEY (`Id`),

    -- Foreign Key Constraint
    CONSTRAINT `FK_TourBookingGuests_TourBooking`
        FOREIGN KEY (`TourBookingId`) REFERENCES `TourBookings` (`Id`) ON DELETE CASCADE,

    -- Check Constraints (MySQL compatible)
    CONSTRAINT `CK_TourBookingGuests_GuestName_NotEmpty`
        CHECK (LENGTH(TRIM(`GuestName`)) > 0),
    CONSTRAINT `CK_TourBookingGuests_GuestEmail_NotEmpty`
        CHECK (LENGTH(TRIM(`GuestEmail`)) > 0)

) CHARACTER SET=utf8mb4 COMMENT='Bảng lưu trữ thông tin từng khách hàng trong tour booking với QR code riêng';

-- Tạo Indexes
CREATE INDEX `IX_TourBookingGuests_TourBookingId` 
    ON `TourBookingGuests` (`TourBookingId`) 
    WHERE `IsDeleted` = 0;

CREATE INDEX `IX_TourBookingGuests_GuestEmail` 
    ON `TourBookingGuests` (`GuestEmail`) 
    WHERE `IsDeleted` = 0;

CREATE INDEX `IX_TourBookingGuests_QRCodeData` 
    ON `TourBookingGuests` (`QRCodeData`) 
    WHERE `QRCodeData` IS NOT NULL AND `IsDeleted` = 0;

CREATE INDEX `IX_TourBookingGuests_IsCheckedIn` 
    ON `TourBookingGuests` (`IsCheckedIn`) 
    WHERE `IsDeleted` = 0;

-- Tạo Unique Constraint
CREATE UNIQUE INDEX `UQ_TourBookingGuests_Email_Booking` 
    ON `TourBookingGuests` (`TourBookingId`, `GuestEmail`) 
    WHERE `IsDeleted` = 0;

-- Verify table creation
SELECT 'TourBookingGuests table created successfully' as Result;
DESCRIBE `TourBookingGuests`;
