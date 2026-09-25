namespace NVOAMASIS.Services
{
    /// <summary>Pub/sub in-process: báo cho màn hình Phiếu thu đang mở khi có phiếu vừa được tick "Đã thanh toán".</summary>
    public sealed class BankPaymentNotifier(ILogger<BankPaymentNotifier> logger)
    {
        public event Func<Guid, string, Task>? ReceiptPaid;

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
    }
}
