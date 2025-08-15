# Database Migration Instructions

## Group Booking Support Migration

### File: `20241213_AddGroupBookingSupport.sql`

This migration adds support for Group Representative bookings to the TayNinhTour database.

### What this migration does:
1. Adds `BookingType` column to `TourBookings` table (default: 'Individual')
2. Adds `GroupName` column to `TourBookings` table (nullable)
3. Adds `GroupDescription` column to `TourBookings` table (nullable)
4. Adds `GroupQRCodeData` column to `TourBookings` table (nullable)
5. Adds `IsGroupRepresentative` column to `TourBookingGuests` table (default: false)
6. Creates indexes for better query performance

### How to run this migration:

#### Option 1: Using SQL Server Management Studio (SSMS)
1. Open SQL Server Management Studio
2. Connect to your TayNinhTour database
3. Open the file `20241213_AddGroupBookingSupport.sql`
4. Execute the script (F5)
5. Check the Messages tab for "Migration completed: Group Booking Support added successfully"

#### Option 2: Using Command Line (sqlcmd)
```bash
sqlcmd -S [server_name] -d TayNinhTourDB -U [username] -P [password] -i 20241213_AddGroupBookingSupport.sql
```

#### Option 3: Using Entity Framework Migration (if configured)
```bash
dotnet ef database update
```

### Verification:
After running the migration, verify the changes:

```sql
-- Check if columns were added to TourBookings
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TourBookings' 
    AND COLUMN_NAME IN ('BookingType', 'GroupName', 'GroupDescription', 'GroupQRCodeData');

-- Check if column was added to TourBookingGuests
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TourBookingGuests' 
    AND COLUMN_NAME = 'IsGroupRepresentative';

-- Check indexes
SELECT name FROM sys.indexes 
WHERE name IN ('IX_TourBookings_BookingType', 'IX_TourBookingGuests_IsGroupRepresentative');
```

### Rollback (if needed):
```sql
-- Remove indexes
DROP INDEX IF EXISTS IX_TourBookings_BookingType ON [dbo].[TourBookings];
DROP INDEX IF EXISTS IX_TourBookingGuests_IsGroupRepresentative ON [dbo].[TourBookingGuests];

-- Remove columns from TourBookings
ALTER TABLE [dbo].[TourBookings] DROP COLUMN IF EXISTS BookingType;
ALTER TABLE [dbo].[TourBookings] DROP COLUMN IF EXISTS GroupName;
ALTER TABLE [dbo].[TourBookings] DROP COLUMN IF EXISTS GroupDescription;
ALTER TABLE [dbo].[TourBookings] DROP COLUMN IF EXISTS GroupQRCodeData;

-- Remove column from TourBookingGuests
ALTER TABLE [dbo].[TourBookingGuests] DROP COLUMN IF EXISTS IsGroupRepresentative;
```

### Important Notes:
- This migration is safe to run multiple times (idempotent)
- It checks for existence before adding columns
- Default values are set for existing records
- No data loss will occur