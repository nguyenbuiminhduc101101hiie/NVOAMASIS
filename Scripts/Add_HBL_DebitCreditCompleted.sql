-- Thêm cờ xác nhận hoàn thành Debit / Credit trên bảng HBL
-- Chạy trên SQL Server trước khi deploy code mới.

IF COL_LENGTH('dbo.HBL', 'debitCompleted') IS NULL
BEGIN
    ALTER TABLE dbo.HBL ADD debitCompleted bit NULL;
END
GO

IF COL_LENGTH('dbo.HBL', 'creditCompleted') IS NULL
BEGIN
    ALTER TABLE dbo.HBL ADD creditCompleted bit NULL;
END
GO

UPDATE dbo.HBL SET debitCompleted = 0 WHERE debitCompleted IS NULL;
UPDATE dbo.HBL SET creditCompleted = 0 WHERE creditCompleted IS NULL;
GO
