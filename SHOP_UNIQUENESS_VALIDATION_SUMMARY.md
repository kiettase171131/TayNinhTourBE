# Shop Uniqueness Validation for Timeline Items

## Overview
Added validation to ensure that a specialty shop can only appear once in the entire timeline of a TourDetails. This prevents duplicate shop assignments and ensures each shop gets a fair chance to participate.

## Business Rules Implemented
- **One Shop, One Appearance**: Each specialty shop can only be used once per timeline
- **Cross-Timeline Validation**: Validation checks existing timeline items before allowing new ones
- **Update Prevention**: When updating a timeline item, prevents assigning a shop that's already used elsewhere
- **Bulk Creation Support**: Validates multiple timeline items in a single request to prevent conflicts

## Files Modified

### 1. Repository Interface Enhancement
**File**: `TayNinhTourApi.DataAccessLayer\Repositories\Interface\ITimelineItemRepository.cs`

Added two new methods:
- `IsSpecialtyShopUsedInTimelineAsync()`: Checks if a specific shop is already used in the timeline
- `GetUsedSpecialtyShopIdsAsync()`: Returns all shop IDs currently used in the timeline

### 2. Repository Implementation
**File**: `TayNinhTourApi.DataAccessLayer\Repositories\TimelineItemRepository.cs`

Implemented the validation methods:
```csharp
public async Task<bool> IsSpecialtyShopUsedInTimelineAsync(Guid tourDetailsId, Guid specialtyShopId, Guid? excludeTimelineItemId = null)
{
    var query = _context.TimelineItems
        .Where(ti => ti.TourDetailsId == tourDetailsId &&
                    ti.SpecialtyShopId == specialtyShopId &&
                    ti.IsActive &&
                    !ti.IsDeleted);

    if (excludeTimelineItemId.HasValue)
    {
        query = query.Where(ti => ti.Id != excludeTimelineItemId.Value);
    }

    return await query.AnyAsync();
}
```

### 3. Service Layer Validation
**File**: `TayNinhTourApi.BusinessLogicLayer\Services\TourDetailsService.cs`

Enhanced three key methods with shop uniqueness validation:

#### CreateTimelineItemsAsync (Bulk Creation)
- Validates no duplicate shops within the request itself
- Checks for conflicts with existing timeline items
- Provides detailed error messages with shop names

#### CreateTimelineItemAsync (Single Creation)
- Validates the shop isn't already used in the timeline
- Returns clear error message with shop name and ID

#### UpdateTimelineItemAsync (Timeline Item Update)
- Only validates if SpecialtyShopId is being changed
- Excludes current timeline item from conflict check
- Provides helpful error messages

## Validation Features

### Error Messages
- **Vietnamese Language**: All error messages are in Vietnamese for better user experience
- **Shop Name Inclusion**: Error messages include both shop name and ID for clarity
- **Context-Specific**: Different messages for creation vs update scenarios

### Performance Considerations
- **Efficient Queries**: Uses database-level checks rather than loading all items
- **Conditional Validation**: Only validates when SpecialtyShopId is provided/changed
- **Batch Operations**: Supports bulk validation for multiple items

### Edge Cases Handled
- **Null Shop IDs**: Validation only runs when SpecialtyShopId is provided
- **Update Exclusions**: When updating, excludes the current item from conflict checks
- **Soft Deleted Items**: Only considers active, non-deleted timeline items
- **Request Duplicates**: Validates for duplicates within a single bulk request

## Example Scenarios

### Scenario 1: Creating Timeline Items
```
Timeline: 
- 8:00 - Kh?i hành
- 10:00 - Ghé shop bánh tráng (Shop A)
- 12:00 - ?n tr?a (Shop B)

Attempt to add: 14:00 - Mua ??c s?n (Shop A)
Result: ? Error - "Shop 'Bánh tráng Tây Ninh' ?ã ???c s? d?ng trong timeline"
```

### Scenario 2: Updating Timeline Items
```
Current: 10:00 - Ghé shop bánh tráng (Shop A)
Update to: 10:00 - Ghé shop bánh tráng (Shop C - already used at 14:00)
Result: ? Error - "Shop '??c s?n Tây Ninh' ?ã ???c s? d?ng trong timeline"
```

### Scenario 3: Bulk Creation
```
Request to create:
- 16:00 - Ghé shop A
- 18:00 - Ghé shop A (duplicate in same request)
Result: ? Error - "Các shop b? trùng l?p trong request: Shop A"
```

## Integration Points
- Works with existing timeline validation (time chronological order)
- Maintains compatibility with existing API endpoints
- Doesn't affect timeline items without specialty shops
- Preserves all existing functionality while adding new validation

## Benefits
1. **Fair Distribution**: Each shop gets equal opportunity to participate
2. **Conflict Prevention**: Eliminates double-booking of shops
3. **Better UX**: Clear error messages help users choose alternatives  
4. **Data Integrity**: Ensures timeline consistency across the application
5. **Business Logic**: Enforces the one-shop-per-timeline business rule