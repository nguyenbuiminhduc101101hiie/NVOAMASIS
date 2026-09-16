-- Add menu code for Form Booking Information permission
-- Permission code == menu name: FormBooking

IF NOT EXISTS (SELECT 1 FROM [MenuNames] WHERE MenuName = 'FormBooking')
BEGIN
    INSERT INTO [MenuNames] (MenuID, MenuName, Title)
    VALUES (NEWID(), 'FormBooking', '2.6 Form Booking Information');
END
GO

-- Optional: grant all CRUD permissions to one user
-- Replace N'YOUR_USER_NAME' before running.
/*
DECLARE @UserName NVARCHAR(256) = N'YOUR_USER_NAME';
DECLARE @MenuId UNIQUEIDENTIFIER;

SELECT @MenuId = MenuID FROM [MenuNames] WHERE MenuName = 'FormBooking';
IF @MenuId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM [Permissions] WHERE UserName = @UserName AND MenuName = 'FormBooking')
BEGIN
    INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, Add)
    VALUES (NEWID(), @MenuId, 'FormBooking', @UserName, 1, 1, 1, 0, 1);
END
*/
