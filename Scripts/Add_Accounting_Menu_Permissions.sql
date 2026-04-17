-- Add menu codes for permission management
-- Permission code == menu name:
--   account_mapping
--   accounting_period

IF NOT EXISTS (SELECT 1 FROM [MenuNames] WHERE MenuName = 'account_mapping')
BEGIN
    INSERT INTO [MenuNames] (MenuID, MenuName, Title, MenuLink, Icon)
    VALUES (NEWID(), 'account_mapping', 'Account Mapping', '/account-mapping', 'account_balance');
END
GO

IF NOT EXISTS (SELECT 1 FROM [MenuNames] WHERE MenuName = 'accounting_period')
BEGIN
    INSERT INTO [MenuNames] (MenuID, MenuName, Title, MenuLink, Icon)
    VALUES (NEWID(), 'accounting_period', 'Accounting Period', '/accounting-period', 'date_range');
END
GO

-- Optional: grant all CRUD permissions to one user
-- Replace N'YOUR_USER_NAME' before running.
/*
DECLARE @UserName NVARCHAR(256) = N'YOUR_USER_NAME';
DECLARE @MenuId UNIQUEIDENTIFIER;

-- account_mapping
SELECT @MenuId = MenuID FROM [MenuNames] WHERE MenuName = 'account_mapping';
IF @MenuId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM [Permissions] WHERE UserName = @UserName AND MenuName = 'account_mapping')
BEGIN
    INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, Add)
    VALUES (NEWID(), @MenuId, 'account_mapping', @UserName, 1, 1, 1, 0, 1);
END

-- accounting_period
SELECT @MenuId = MenuID FROM [MenuNames] WHERE MenuName = 'accounting_period';
IF @MenuId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM [Permissions] WHERE UserName = @UserName AND MenuName = 'accounting_period')
BEGIN
    INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, Add)
    VALUES (NEWID(), @MenuId, 'accounting_period', @UserName, 1, 1, 1, 0, 1);
END
*/
