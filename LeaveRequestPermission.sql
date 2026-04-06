-- Add LeaveRequests menu to MenuNames table for permission management
-- Run this script to enable permission management for Leave Requests feature

-- Check if LeaveRequests menu already exists
IF NOT EXISTS (SELECT 1 FROM [MenuNames] WHERE MenuName = 'LeaveRequests')
BEGIN
    INSERT INTO [MenuNames] (MenuID, MenuName, Title, MenuLink, Icon)
    VALUES (
        NEWID(),
        'LeaveRequests',
        'Quản lý nghỉ phép',
        '/leave-requests',
        'event_available'
    )
    PRINT 'LeaveRequests menu added successfully'
END
ELSE
BEGIN
    PRINT 'LeaveRequests menu already exists'
END
GO

-- Example: Grant permissions to a specific user
-- Replace 'your_username' with actual username
/*
DECLARE @MenuId UNIQUEIDENTIFIER
SELECT @MenuId = MenuID FROM [MenuNames] WHERE MenuName = 'LeaveRequests'

-- Grant all permissions to admin user
INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, Add)
VALUES (
    NEWID(),
    @MenuId,
    'LeaveRequests',
    'your_username',  -- Replace with actual username
    1,  -- See: Can view the leave request list
    0,  -- Edit: Not used for approval page
    0,  -- Del: Not used for approval page
    1,  -- Approve: Can approve/reject leave requests
    0   -- Add: Not used for approval page
)
*/
