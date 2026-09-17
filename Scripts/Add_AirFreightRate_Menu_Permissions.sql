-- Add menu code for the new 4.8 Bao gia hang Air feature
-- Permission code == menu name: AirFreightRate

IF NOT EXISTS (SELECT 1 FROM [MenuNames] WHERE MenuName = 'AirFreightRate')
BEGIN
    INSERT INTO [MenuNames] (MenuID, MenuName, Title)
    VALUES (NEWID(), 'AirFreightRate', '4.8 Bao gia hang Air');
END
GO

-- Optional: grant all CRUD permissions to one user
-- Replace N'YOUR_USER_NAME' before running.
/*
DECLARE @UserName NVARCHAR(256) = N'YOUR_USER_NAME';
DECLARE @MenuId UNIQUEIDENTIFIER;

SELECT @MenuId = MenuID FROM [MenuNames] WHERE MenuName = 'AirFreightRate';
IF @MenuId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM [Permissions] WHERE UserName = @UserName AND MenuName = 'AirFreightRate')
BEGIN
    INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, Add)
    VALUES (NEWID(), @MenuId, 'AirFreightRate', @UserName, 1, 1, 1, 0, 1);
END
*/
