-- Add menu codes for Courier permission management
-- Permission code == menu name:
--   LMS_Courier  (11.1 LMS Courier -> https://abcourier.nvocc.vn)
--   Rate_Hub     (11.2 Rate Hub    -> https://courier.nvocc.vn)

IF NOT EXISTS (SELECT 1 FROM [MenuNames] WHERE MenuName = 'LMS_Courier')
BEGIN
    INSERT INTO [MenuNames] (MenuID, MenuName, Title, MenuLink, Icon)
    VALUES (NEWID(), 'LMS_Courier', '11.1 LMS Courier', 'https://abcourier.nvocc.vn', 'local_shipping');
END
GO

IF NOT EXISTS (SELECT 1 FROM [MenuNames] WHERE MenuName = 'Rate_Hub')
BEGIN
    INSERT INTO [MenuNames] (MenuID, MenuName, Title, MenuLink, Icon)
    VALUES (NEWID(), 'Rate_Hub', '11.2 Rate Hub', 'https://courier.nvocc.vn', 'hub');
END
GO

-- Optional: grant View permission to one user
-- Replace N'YOUR_USER_NAME' before running.
/*
DECLARE @UserName NVARCHAR(256) = N'YOUR_USER_NAME';
DECLARE @MenuId UNIQUEIDENTIFIER;

SELECT @MenuId = MenuID FROM [MenuNames] WHERE MenuName = 'LMS_Courier';
IF @MenuId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM [Permissions] WHERE UserName = @UserName AND MenuName = 'LMS_Courier')
BEGIN
    INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, Add)
    VALUES (NEWID(), @MenuId, 'LMS_Courier', @UserName, 1, 0, 0, 0, 0);
END

SELECT @MenuId = MenuID FROM [MenuNames] WHERE MenuName = 'Rate_Hub';
IF @MenuId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM [Permissions] WHERE UserName = @UserName AND MenuName = 'Rate_Hub')
BEGIN
    INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, Add)
    VALUES (NEWID(), @MenuId, 'Rate_Hub', @UserName, 1, 0, 0, 0, 0);
END
*/
