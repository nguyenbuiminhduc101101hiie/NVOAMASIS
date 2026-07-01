using QRCoder;

namespace NVOAMASIS.Services
{
    public class HblQrService
    {
        public string? GenerateQrCodeBase64(string? hblNo, int pixelsPerModule = 8)
        {
            if (string.IsNullOrWhiteSpace(hblNo))
                return null;

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(hblNo.Trim(), QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var pngBytes = qrCode.GetGraphic(pixelsPerModule);
            return Convert.ToBase64String(pngBytes);
        }
    }
}
