-- Add menu code for the new 4.7 Pricing Ocean Freight Rate feature
-- Permission code == menu name: PricingOceanFreightRate

IF NOT EXISTS (SELECT 1 FROM [MenuNames] WHERE MenuName = 'PricingOceanFreightRate')
BEGIN
    INSERT INTO [MenuNames] (MenuID, MenuName, Title)
    VALUES (NEWID(), 'PricingOceanFreightRate', '4.7 Pricing Ocean Freight Rate');
END
GO

-- Optional: grant all CRUD permissions to one user
-- Replace N'YOUR_USER_NAME' before running.
/*
DECLARE @UserName NVARCHAR(256) = N'YOUR_USER_NAME';
DECLARE @MenuId UNIQUEIDENTIFIER;

SELECT @MenuId = MenuID FROM [MenuNames] WHERE MenuName = 'PricingOceanFreightRate';
IF @MenuId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM [Permissions] WHERE UserName = @UserName AND MenuName = 'PricingOceanFreightRate')
BEGIN
    INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, Add)
    VALUES (NEWID(), @MenuId, 'PricingOceanFreightRate', @UserName, 1, 1, 1, 0, 1);
END
*/
