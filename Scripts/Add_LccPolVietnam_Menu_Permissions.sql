-- Add menu code for "4.1.1 LCC POL VIETNAM" permission management
-- Permission code == menu name: LCC_POL_Vietnam

IF NOT EXISTS (SELECT 1 FROM [MenuNames] WHERE MenuName = 'LCC_POL_Vietnam')
BEGIN
    INSERT INTO [MenuNames] (MenuID, MenuName, Title)
    VALUES (NEWID(), 'LCC_POL_Vietnam', '4.1.1 LCC POL VIETNAM');
END
GO

-- Optional: grant all CRUD permissions (View/Add/Edit/Delete) to one user
-- Replace N'YOUR_USER_NAME' before running.
/*
DECLARE @UserName NVARCHAR(256) = N'YOUR_USER_NAME';
DECLARE @MenuId UNIQUEIDENTIFIER;

SELECT @MenuId = MenuID FROM [MenuNames] WHERE MenuName = 'LCC_POL_Vietnam';
IF @MenuId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM [Permissions] WHERE UserName = @UserName AND MenuName = 'LCC_POL_Vietnam')
BEGIN
    INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, Add)
    VALUES (NEWID(), @MenuId, 'LCC_POL_Vietnam', @UserName, 1, 1, 1, 0, 1);
END
*/
