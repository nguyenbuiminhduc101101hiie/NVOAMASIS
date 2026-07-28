/*
  Upsert localization resources for:
  - Components/Accounting/Pages/FixedAssetDepreciation_WithPosting.razor
  - NavMenu item 10.4.7

  Target table: dbo.LocalizationResources (ResourceKey, Culture, Value)
  Cultures: vi-VN, en-US
  Safe to re-run.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH src AS (
    SELECT N'FixedAssetDepreciationPosting.Title' AS ResourceKey, N'vi-VN' AS Culture, N'Ghi sổ khấu hao TSCĐ' AS Value UNION ALL
    SELECT N'FixedAssetDepreciationPosting.Title', N'en-US', N'Post Fixed Asset Depreciation' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.Heading', N'vi-VN', N'Ghi sổ khấu hao TSCĐ' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.Heading', N'en-US', N'Post Fixed Asset Depreciation' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.Subtitle', N'vi-VN', N'Tính, lưu nháp và ghi sổ khấu hao theo từng kỳ kế toán' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.Subtitle', N'en-US', N'Calculate, save draft and post depreciation by accounting period' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.Reload', N'vi-VN', N'Tải lại' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.Reload', N'en-US', N'Reload' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.FiscalYear', N'vi-VN', N'Năm tài chính' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.FiscalYear', N'en-US', N'Fiscal year' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.FiscalPeriod', N'vi-VN', N'Kỳ kế toán' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.FiscalPeriod', N'en-US', N'Accounting period' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.MonthItem', N'vi-VN', N'Tháng {0}' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.MonthItem', N'en-US', N'Month {0}' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ProrateByDay', N'vi-VN', N'Phân bổ theo ngày thực tế' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ProrateByDay', N'en-US', N'Prorate by actual days' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.Calculate', N'vi-VN', N'Tính khấu hao' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.Calculate', N'en-US', N'Calculate depreciation' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.SaveDraft', N'vi-VN', N'Lưu nháp' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.SaveDraft', N'en-US', N'Save draft' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.Post', N'vi-VN', N'Ghi sổ' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.Post', N'en-US', N'Post' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.DeleteDraft', N'vi-VN', N'Xóa nháp' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.DeleteDraft', N'en-US', N'Delete draft' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.SearchAsset', N'vi-VN', N'Tìm mã, tên tài sản' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.SearchAsset', N'en-US', N'Search asset code/name' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.FilterDepartment', N'vi-VN', N'Lọc bộ phận' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.FilterDepartment', N'en-US', N'Filter by department' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.Info', N'vi-VN', N'Bản này tính tự động phương pháp đường thẳng. Phương pháp số dư giảm dần và theo sản lượng được hiển thị để kiểm tra nhưng chưa tự động tính vì cần thêm hệ số hoặc dữ liệu sản lượng. Lưu nháp không làm thay đổi sổ cái. Chỉ nút Ghi sổ mới tạo chứng từ và GeneralLedgerEntries.' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.Info', N'en-US', N'This screen auto-calculates straight-line depreciation. Declining-balance and units-of-production methods are shown for review but not calculated automatically because factors or production data are required. Saving a draft does not change the ledger. Only Post creates the voucher and GeneralLedgerEntries.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.VisibleAssets', N'vi-VN', N'Tài sản hiển thị' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.VisibleAssets', N'en-US', N'Visible assets' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.SelectedToSave', N'vi-VN', N'Đang chọn lưu' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.SelectedToSave', N'en-US', N'Selected to save' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.SelectedOriginalCost', N'vi-VN', N'Tổng nguyên giá đã chọn' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.SelectedOriginalCost', N'en-US', N'Selected original cost' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.SelectedDepreciation', N'vi-VN', N'Khấu hao kỳ đã chọn' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.SelectedDepreciation', N'en-US', N'Selected period depreciation' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColSelect', N'vi-VN', N'Chọn' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColSelect', N'en-US', N'Select' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColAssetCode', N'vi-VN', N'Mã tài sản' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColAssetCode', N'en-US', N'Asset code' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColAssetName', N'vi-VN', N'Tên tài sản' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColAssetName', N'en-US', N'Asset name' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColDepartment', N'vi-VN', N'Bộ phận' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColDepartment', N'en-US', N'Department' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColDepStart', N'vi-VN', N'Bắt đầu KH' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColDepStart', N'en-US', N'Dep. start' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColOriginalCost', N'vi-VN', N'Nguyên giá' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColOriginalCost', N'en-US', N'Original cost' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColOpeningAccum', N'vi-VN', N'HM đầu kỳ' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColOpeningAccum', N'en-US', N'Opening accum. dep.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColMonthlyDep', N'vi-VN', N'KH tháng chuẩn' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColMonthlyDep', N'en-US', N'Standard monthly dep.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColDepDays', N'vi-VN', N'Ngày KH' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColDepDays', N'en-US', N'Dep. days' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColPeriodDep', N'vi-VN', N'KH kỳ này' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColPeriodDep', N'en-US', N'Period dep.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColClosingAccum', N'vi-VN', N'HM cuối kỳ' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColClosingAccum', N'en-US', N'Closing accum. dep.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColRemaining', N'vi-VN', N'Còn lại' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColRemaining', N'en-US', N'Remaining' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColDebitAccount', N'vi-VN', N'TK Nợ' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColDebitAccount', N'en-US', N'Debit account' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColCreditAccount', N'vi-VN', N'TK Có' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColCreditAccount', N'en-US', N'Credit account' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.ColStatus', N'vi-VN', N'Trạng thái' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.ColStatus', N'en-US', N'Status' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.NoData', N'vi-VN', N'Chưa có dữ liệu. Chọn kỳ rồi bấm “Tính khấu hao”.' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.NoData', N'en-US', N'No data yet. Select a period then click “Calculate depreciation”.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.NoEligibleAssets', N'vi-VN', N'Không có tài sản đang sử dụng đủ điều kiện tính khấu hao.' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.NoEligibleAssets', N'en-US', N'No in-use assets are eligible for depreciation calculation.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.NoAssetSelectedSave', N'vi-VN', N'Chưa chọn tài sản cần lưu khấu hao.' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.NoAssetSelectedSave', N'en-US', N'No assets selected to save depreciation.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.MissingAccounts', N'vi-VN', N'Tài sản {0} chưa đủ tài khoản Nợ/Có.' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.MissingAccounts', N'en-US', N'Asset {0} is missing debit/credit accounts.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.SaveDraftSuccess', N'vi-VN', N'Đã lưu nháp khấu hao cho {0} tài sản kỳ {1}/{2}.' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.SaveDraftSuccess', N'en-US', N'Saved depreciation draft for {0} assets in period {1}/{2}.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.NoDraftSelectedPost', N'vi-VN', N'Chưa chọn bản nháp khấu hao cần ghi sổ.' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.NoDraftSelectedPost', N'en-US', N'No depreciation drafts selected to post.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.PostConfirmTitle', N'vi-VN', N'Ghi sổ khấu hao' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.PostConfirmTitle', N'en-US', N'Post depreciation' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.PostConfirmBody', N'vi-VN', N'Ghi sổ {0} tài sản kỳ {1}/{2}, tổng khấu hao {3}? Sau khi ghi sổ không thể xóa nháp.' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.PostConfirmBody', N'en-US', N'Post {0} assets for period {1}/{2}, total depreciation {3}? After posting, drafts cannot be deleted.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.PostSuccess', N'vi-VN', N'Đã ghi sổ chứng từ {0}: {1} tài sản, {2} dòng sổ cái, tổng {3}.' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.PostSuccess', N'en-US', N'Posted voucher {0}: {1} assets, {2} ledger lines, total {3}.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.PostFailed', N'vi-VN', N'Không thể ghi sổ khấu hao: {0}' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.PostFailed', N'en-US', N'Unable to post depreciation: {0}' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.NoDraftSelectedDelete', N'vi-VN', N'Chưa chọn bản nháp khấu hao cần xóa.' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.NoDraftSelectedDelete', N'en-US', N'No depreciation drafts selected to delete.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.DeleteConfirmTitle', N'vi-VN', N'Xóa bản nháp khấu hao' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.DeleteConfirmTitle', N'en-US', N'Delete depreciation drafts' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.DeleteConfirmBody', N'vi-VN', N'Xóa {0} bản nháp kỳ {1}/{2}?' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.DeleteConfirmBody', N'en-US', N'Delete {0} drafts for period {1}/{2}?' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.Delete', N'vi-VN', N'Xóa' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.Delete', N'en-US', N'Delete' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.Cancel', N'vi-VN', N'Hủy' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.Cancel', N'en-US', N'Cancel' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.DeleteSuccess', N'vi-VN', N'Đã xóa {0} bản nháp khấu hao.' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.DeleteSuccess', N'en-US', N'Deleted {0} depreciation drafts.' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.StatusPosted', N'vi-VN', N'Đã ghi sổ' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.StatusPosted', N'en-US', N'Posted' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.StatusDraft', N'vi-VN', N'Đã lưu nháp' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.StatusDraft', N'en-US', N'Draft saved' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.StatusSkipped', N'vi-VN', N'Không tính' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.StatusSkipped', N'en-US', N'Skipped' UNION ALL

    SELECT N'FixedAssetDepreciationPosting.StatusUnsaved', N'vi-VN', N'Chưa lưu' UNION ALL
    SELECT N'FixedAssetDepreciationPosting.StatusUnsaved', N'en-US', N'Not saved'
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

PRINT N'FixedAssetDepreciationPosting localization resources imported successfully.';
