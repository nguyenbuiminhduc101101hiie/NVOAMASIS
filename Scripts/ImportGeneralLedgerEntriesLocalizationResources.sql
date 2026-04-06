/*
  Upsert localization resources for General Ledger Entries pages:
  - GeneralLedgerEntries_5_8_6_Index.razor
  - GeneralLedgerEntry_AddEditDialog.razor

  Target table: dbo.LocalizationResources (ResourceKey, Culture, Value)
  Safe to re-run.
*/

SET NOCOUNT ON;

BEGIN TRANSACTION;

;WITH src AS (
    SELECT N'GeneralLedgerEntries.VoucherNo' AS ResourceKey, N'vi-VN' AS Culture, N'Số chứng từ' AS Value UNION ALL
    SELECT N'GeneralLedgerEntries.VoucherNo', N'en-US', N'Voucher no' UNION ALL
    SELECT N'GeneralLedgerEntries.PostingDate', N'vi-VN', N'Ngày ghi sổ' UNION ALL
    SELECT N'GeneralLedgerEntries.PostingDate', N'en-US', N'Posting date' UNION ALL
    SELECT N'GeneralLedgerEntries.VoucherDate', N'vi-VN', N'Ngày chứng từ' UNION ALL
    SELECT N'GeneralLedgerEntries.VoucherDate', N'en-US', N'Voucher date' UNION ALL
    SELECT N'GeneralLedgerEntries.AccountCode', N'vi-VN', N'Tài khoản' UNION ALL
    SELECT N'GeneralLedgerEntries.AccountCode', N'en-US', N'Account code' UNION ALL
    SELECT N'GeneralLedgerEntries.FiscalYear', N'vi-VN', N'Năm tài chính' UNION ALL
    SELECT N'GeneralLedgerEntries.FiscalYear', N'en-US', N'Fiscal year' UNION ALL
    SELECT N'GeneralLedgerEntries.FiscalPeriod', N'vi-VN', N'Kỳ tài chính' UNION ALL
    SELECT N'GeneralLedgerEntries.FiscalPeriod', N'en-US', N'Fiscal period' UNION ALL
    SELECT N'GeneralLedgerEntries.Debit', N'vi-VN', N'Nợ' UNION ALL
    SELECT N'GeneralLedgerEntries.Debit', N'en-US', N'Debit' UNION ALL
    SELECT N'GeneralLedgerEntries.Credit', N'vi-VN', N'Có' UNION ALL
    SELECT N'GeneralLedgerEntries.Credit', N'en-US', N'Credit' UNION ALL
    SELECT N'GeneralLedgerEntries.DebitFC', N'vi-VN', N'Nợ ngoại tệ' UNION ALL
    SELECT N'GeneralLedgerEntries.DebitFC', N'en-US', N'Debit FC' UNION ALL
    SELECT N'GeneralLedgerEntries.CreditFC', N'vi-VN', N'Có ngoại tệ' UNION ALL
    SELECT N'GeneralLedgerEntries.CreditFC', N'en-US', N'Credit FC' UNION ALL
    SELECT N'GeneralLedgerEntries.CurrencyCode', N'vi-VN', N'Loại tiền' UNION ALL
    SELECT N'GeneralLedgerEntries.CurrencyCode', N'en-US', N'Currency code' UNION ALL
    SELECT N'GeneralLedgerEntries.ExchangeRate', N'vi-VN', N'Tỷ giá' UNION ALL
    SELECT N'GeneralLedgerEntries.ExchangeRate', N'en-US', N'Exchange rate' UNION ALL
    SELECT N'GeneralLedgerEntries.SourceModule', N'vi-VN', N'Phân hệ nguồn' UNION ALL
    SELECT N'GeneralLedgerEntries.SourceModule', N'en-US', N'Source module' UNION ALL
    SELECT N'GeneralLedgerEntries.SourceId', N'vi-VN', N'Mã nguồn' UNION ALL
    SELECT N'GeneralLedgerEntries.SourceId', N'en-US', N'Source ID' UNION ALL
    SELECT N'GeneralLedgerEntries.CreatedBy', N'vi-VN', N'Người tạo' UNION ALL
    SELECT N'GeneralLedgerEntries.CreatedBy', N'en-US', N'Created by' UNION ALL
    SELECT N'GeneralLedgerEntries.CreatedAt', N'vi-VN', N'Ngày tạo' UNION ALL
    SELECT N'GeneralLedgerEntries.CreatedAt', N'en-US', N'Created at' UNION ALL
    SELECT N'GeneralLedgerEntries.Company' AS ResourceKey, N'vi-VN' AS Culture, N'Công ty' AS Value UNION ALL
    SELECT N'GeneralLedgerEntries.Company', N'en-US', N'Company' UNION ALL
    SELECT N'GeneralLedgerEntries.Voucher', N'vi-VN', N'Chứng từ' UNION ALL
    SELECT N'GeneralLedgerEntries.Voucher', N'en-US', N'Voucher' UNION ALL
    SELECT N'GeneralLedgerEntries.VoucherLine', N'vi-VN', N'Dòng chứng từ' UNION ALL
    SELECT N'GeneralLedgerEntries.VoucherLine', N'en-US', N'Voucher line' UNION ALL
    SELECT N'GeneralLedgerEntries.Customer', N'vi-VN', N'Khách hàng' UNION ALL
    SELECT N'GeneralLedgerEntries.Customer', N'en-US', N'Customer' UNION ALL
    SELECT N'GeneralLedgerEntries.Shipment', N'vi-VN', N'Lô hàng (HBL)' UNION ALL
    SELECT N'GeneralLedgerEntries.Shipment', N'en-US', N'Shipment (HBL)' UNION ALL
    SELECT N'GeneralLedgerEntries.ContractId', N'vi-VN', N'Hợp đồng' UNION ALL
    SELECT N'GeneralLedgerEntries.ContractId', N'en-US', N'Contract' UNION ALL
    SELECT N'GeneralLedgerEntries.BranchCode', N'vi-VN', N'Mã chi nhánh' UNION ALL
    SELECT N'GeneralLedgerEntries.BranchCode', N'en-US', N'Branch code' UNION ALL
    SELECT N'GeneralLedgerEntries.IsTaxBook', N'vi-VN', N'Sổ thuế' UNION ALL
    SELECT N'GeneralLedgerEntries.IsTaxBook', N'en-US', N'Tax book' UNION ALL
    SELECT N'GeneralLedgerEntries.IsManagementBook', N'vi-VN', N'Sổ quản trị' UNION ALL
    SELECT N'GeneralLedgerEntries.IsManagementBook', N'en-US', N'Management book' UNION ALL
    SELECT N'GeneralLedgerEntries.LedgerType', N'vi-VN', N'Loại sổ' UNION ALL
    SELECT N'GeneralLedgerEntries.LedgerType', N'en-US', N'Ledger type'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

COMMIT TRANSACTION;
