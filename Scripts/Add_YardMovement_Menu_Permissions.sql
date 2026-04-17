-- Add menu code for Yard Movement permission
-- Permission code == menu name: YardMovement

IF NOT EXISTS (SELECT 1 FROM [MenuNames] WHERE MenuName = 'YardMovement')
BEGIN
    INSERT INTO [MenuNames] (MenuID, MenuName, Title, MenuLink, Icon)
    VALUES (NEWID(), 'YardMovement', '5.8.9 Yard Movement', '/ExcelImportCrud', 'move_down');
END
GO

-- Optional: grant all CRUD permissions to one user
-- Replace N'YOUR_USER_NAME' before running.
/*
DECLARE @UserName NVARCHAR(256) = N'YOUR_USER_NAME';
DECLARE @MenuId UNIQUEIDENTIFIER;

SELECT @MenuId = MenuID FROM [MenuNames] WHERE MenuName = 'YardMovement';
IF @MenuId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM [Permissions] WHERE UserName = @UserName AND MenuName = 'YardMovement')
BEGIN
    INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, Add)
    VALUES (NEWID(), @MenuId, 'YardMovement', @UserName, 1, 1, 1, 0, 1);
END
*/
