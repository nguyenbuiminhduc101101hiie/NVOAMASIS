-- Store imported Excel file name on 8.3.4 / 8.3.5 rows so operators can delete by file + date.

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.[8_3_4_Import_Yard_Movement_CatLai_Current_In_Yard]', N'SourceFile') IS NULL
    ALTER TABLE dbo.[8_3_4_Import_Yard_Movement_CatLai_Current_In_Yard] ADD [SourceFile] NVARCHAR(260) NULL;
GO

IF COL_LENGTH(N'dbo.[8_3_4_Import_Yard_Movement_CatLai_In_Out_Yard]', N'SourceFile') IS NULL
    ALTER TABLE dbo.[8_3_4_Import_Yard_Movement_CatLai_In_Out_Yard] ADD [SourceFile] NVARCHAR(260) NULL;
GO

IF COL_LENGTH(N'dbo.[8_3_5_Cang_VICT]', N'SourceFile') IS NULL
    ALTER TABLE dbo.[8_3_5_Cang_VICT] ADD [SourceFile] NVARCHAR(260) NULL;
GO
