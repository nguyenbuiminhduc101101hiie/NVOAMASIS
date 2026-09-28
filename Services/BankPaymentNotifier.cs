namespace NVOAMASIS.Services
{
    /// <summary>Pub/sub in-process: báo cho màn hình Phiếu thu đang mở khi có phiếu vừa được tick "Đã thanh toán".</summary>
    public sealed class BankPaymentNotifier(ILogger<BankPaymentNotifier> logger)
    {
        public event Func<Guid, string, Task>? ReceiptPaid;

        /// <summary>Hóa đơn (5.14) vừa được tick đã thanh toán: (số nội bộ, khách hàng, số hóa đơn điện tử).</summary>
        public event Func<string, Guid, string, Task>? InvoicePaid;

        /// <summary>Phiếu chi (10.2) vừa được tick "Đã thanh toán" — công ty vừa chuyển khoản cho khách.</summary>
        public event Func<Guid, string, Task>? PaymentPaid;

        public async Task PublishAsync(Guid phieuthuId, string? soPhieu)
        {
            var handlers = ReceiptPaid;
            if (handlers == null) return;

            foreach (var handler in handlers.GetInvocationList().Cast<Func<Guid, string, Task>>())
            {
                try
                {
                    await handler(phieuthuId, soPhieu ?? "");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "BankPaymentNotifier handler failed");
                }
            }
        }

        public async Task PublishInvoicePaidAsync(string noibo, Guid customerId, string? eInvoiceNo)
        {
            var handlers = InvoicePaid;
            if (handlers == null) return;

            foreach (var handler in handlers.GetInvocationList().Cast<Func<string, Guid, string, Task>>())
            {
                try
                {
                    await handler(noibo, customerId, eInvoiceNo ?? noibo);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "BankPaymentNotifier invoice handler failed");
                }
            }
        }

        public async Task PublishPaymentPaidAsync(Guid phieuchiId, string? soPhieu)
        {
            var handlers = PaymentPaid;
            if (handlers == null) return;

            foreach (var handler in handlers.GetInvocationList().Cast<Func<Guid, string, Task>>())
            {
                try
                {
                    await handler(phieuchiId, soPhieu ?? "");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "BankPaymentNotifier payment handler failed");
                }
            }
        }
    }
}
