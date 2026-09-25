SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.LocalizationResources', N'U') IS NULL
BEGIN
    THROW 50001, 'Table dbo.LocalizationResources does not exist.', 1;
END;
GO

DECLARE @Resources TABLE
(
    ResourceKey nvarchar(500) NOT NULL,
    Culture nvarchar(10) NOT NULL,
    Value nvarchar(max) NOT NULL,
    PRIMARY KEY (ResourceKey, Culture)
);

INSERT INTO @Resources (ResourceKey, Culture, Value)
VALUES
(N'Dathanhtoan', N'en-US', N'Paid'),
(N'Dathanhtoan', N'vi-VN', N'Đã thanh toán'),
(N'Dathanhtoan', N'zh-CN', N'已付款'),
(N'NgayThanhToan', N'en-US', N'Payment date'),
(N'NgayThanhToan', N'vi-VN', N'Ngày thanh toán'),
(N'NgayThanhToan', N'zh-CN', N'付款日期'),
(N'Paid', N'en-US', N'Paid'),
(N'Paid', N'vi-VN', N'Đã TT'),
(N'Paid', N'zh-CN', N'已付'),
(N'Unpaid', N'en-US', N'Unpaid'),
(N'Unpaid', N'vi-VN', N'Chưa TT'),
(N'Unpaid', N'zh-CN', N'未付'),
(N'WrongAmountChip', N'en-US', N'Wrong amount: received {0} / required {1}'),
(N'WrongAmountChip', N'vi-VN', N'Sai số tiền: nhận {0} / cần {1}'),
(N'WrongAmountChip', N'zh-CN', N'金额不符：已收 {0} / 应收 {1}'),
(N'ReceivedOfTotal', N'en-US', N'Received {0} / {1}'),
(N'ReceivedOfTotal', N'vi-VN', N'Đã nhận {0} / {1}'),
(N'ReceivedOfTotal', N'zh-CN', N'已收 {0} / {1}'),
(N'BankTransactions', N'en-US', N'Bank transactions'),
(N'BankTransactions', N'vi-VN', N'Giao dịch ngân hàng'),
(N'BankTransactions', N'zh-CN', N'银行交易'),
(N'LinkToReceipt', N'en-US', N'Link to receipt'),
(N'LinkToReceipt', N'vi-VN', N'Gắn phiếu thu'),
(N'LinkToReceipt', N'zh-CN', N'关联收款单'),
(N'PaymentReceivedMsg', N'en-US', N'Receipt {0} has received payment'),
(N'PaymentReceivedMsg', N'vi-VN', N'Phiếu thu {0} đã nhận tiền'),
(N'PaymentReceivedMsg', N'zh-CN', N'收款单 {0} 已收到款项'),
(N'OnlyNeedReview', N'en-US', N'Only need review'),
(N'OnlyNeedReview', N'vi-VN', N'Chỉ giao dịch cần xem'),
(N'OnlyNeedReview', N'zh-CN', N'仅需审核'),
(N'Unmatched', N'en-US', N'Unmatched'),
(N'Unmatched', N'vi-VN', N'Chưa khớp phiếu'),
(N'Unmatched', N'zh-CN', N'未匹配'),
(N'WrongAmount', N'en-US', N'Wrong amount'),
(N'WrongAmount', N'vi-VN', N'Sai số tiền'),
(N'WrongAmount', N'zh-CN', N'金额不符'),
(N'AlreadyPaid', N'en-US', N'Already paid'),
(N'AlreadyPaid', N'vi-VN', N'Phiếu đã thanh toán'),
(N'AlreadyPaid', N'zh-CN', N'已付款'),
(N'BankMatched', N'en-US', N'Matched'),
(N'BankMatched', N'vi-VN', N'Đã khớp'),
(N'BankMatched', N'zh-CN', N'已匹配'),
(N'BankManual', N'en-US', N'Linked manually'),
(N'BankManual', N'vi-VN', N'Gắn tay'),
(N'BankManual', N'zh-CN', N'手动关联'),
(N'ClickToMarkPaid', N'en-US', N'Click to mark as paid'),
(N'ClickToMarkPaid', N'vi-VN', N'Bấm để đánh dấu đã thanh toán'),
(N'ClickToMarkPaid', N'zh-CN', N'点击标记为已付款'),
(N'ClickToUnmarkPaid', N'en-US', N'Click to unmark paid'),
(N'ClickToUnmarkPaid', N'vi-VN', N'Bấm để bỏ đánh dấu đã thanh toán'),
(N'ClickToUnmarkPaid', N'zh-CN', N'点击取消已付款标记'),
(N'ReceivedFullNotConfirmed', N'en-US', N'Full amount received - confirm'),
(N'ReceivedFullNotConfirmed', N'vi-VN', N'Đã nhận đủ tiền - bấm xác nhận'),
(N'ReceivedFullNotConfirmed', N'zh-CN', N'已收足款 - 请确认');

UPDATE target
SET target.Value = source.Value
FROM dbo.LocalizationResources AS target
INNER JOIN @Resources AS source
    ON source.ResourceKey = target.ResourceKey
   AND source.Culture = target.Culture;

INSERT INTO dbo.LocalizationResources (ResourceKey, Culture, Value)
SELECT source.ResourceKey, source.Culture, source.Value
FROM @Resources AS source
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.LocalizationResources AS target
    WHERE target.ResourceKey = source.ResourceKey
      AND target.Culture = source.Culture
);
