# Migration Summary: Convert from TourCompany.Id to User.Id

## Overview
This migration converts the system from using TourCompany.Id to User.Id for CreatedById and UpdatedById fields in tour-related entities. This simplifies the authorization logic and removes the need for TourCompany lookup operations.

## Changes Made

### 1. Entity Models Updated
- **TourDetails.cs**: Changed CreatedBy and UpdatedBy navigation properties from TourCompany to User
- **TourTemplate.cs**: Changed CreatedBy and UpdatedBy navigation properties from TourCompany to User  
- **TourOperation.cs**: Changed CreatedBy and UpdatedBy navigation properties from TourCompany to User
- **User.cs**: Added new navigation properties for tour-related entities

### 2. Entity Configurations Updated
- **TourDetailsConfiguration.cs**: Updated foreign key relationships to reference Users table
- **TourTemplateConfiguration.cs**: Updated foreign key relationships to reference Users table
- **TourOperationConfiguration.cs**: Updated foreign key relationships to reference Users table
- **TourCompanyConfiguration.cs**: Removed tour-related navigation properties

### 3. Service Layer Updates
- **TourDetailsService.cs**: 
  - Removed TourCompany lookup logic in CreateTourDetailAsync
  - Simplified DeleteTourDetailAsync to use User.Id directly
  - Updated ManualInviteGuideAsync parameter from companyId to userId
- **TourOperationService.cs**: Removed TourCompany lookup logic in CreateOperationAsync

### 4. Database Migration
- **ConvertToUserIdMigration.sql**: Script to update existing data and foreign key constraints

## Benefits

### 1. Simplified Authorization
- No more TourCompany.Id lookup from User.Id
- Direct permission checking using User.Id
- Reduced complexity in service methods

### 2. Better Performance
- Eliminated extra database queries for TourCompany lookup
- Faster authorization checks
- Reduced join operations

### 3. Cleaner Code
- Removed conditional logic for TourCompany existence
- Simplified service method signatures
- More consistent with existing User-based entities

## Migration Steps

### 1. Code Changes (Completed)
✅ Updated entity models
✅ Updated entity configurations  
✅ Updated service methods
✅ Removed TourCompany dependencies

### 2. Database Migration (Required)
⚠️ **IMPORTANT**: Run the migration script before deploying code changes

```sql
-- Execute this script in your database
-- File: TayNinhTourApi.DataAccessLayer/Migrations/ConvertToUserIdMigration.sql
```

### 3. Testing (Recommended)
- Test tour template creation/update/delete
- Test tour details creation/update/delete  
- Test tour operation creation/update
- Verify authorization still works correctly
- Check that existing data displays properly

## Rollback Plan

If issues occur, you can rollback by:

1. Revert code changes to previous commit
2. Run rollback SQL script (create if needed)
3. Restore TourCompany.Id relationships

## Notes

### TourCompany Entity Still Exists
- TourCompany entity is still used for business logic (wallet, revenue)
- Only the CreatedBy/UpdatedBy relationships were changed
- TourCompany.UserId relationship remains intact

### Authorization Logic
- Role-based authorization still works the same way
- User.Role determines permissions
- TourCompany information accessed via User.TourCompany navigation property when needed

### Frontend Impact
- No changes required in frontend code
- API responses remain the same structure
- Authentication tokens still contain User.Id

## Verification Queries

After migration, run these queries to verify data integrity:

```sql
-- Check TourDetails
SELECT COUNT(*) FROM TourDetails WHERE CreatedById NOT IN (SELECT Id FROM Users);

-- Check TourTemplate  
SELECT COUNT(*) FROM TourTemplate WHERE CreatedById NOT IN (SELECT Id FROM Users);

-- Check TourOperation
SELECT COUNT(*) FROM TourOperation WHERE CreatedById NOT IN (SELECT Id FROM Users);
```

All queries should return 0 for successful migration.
