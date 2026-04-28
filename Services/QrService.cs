using QRCoder;

namespace UrlShortener.Services
{
    public class QrService
    {
        public byte[] GenerateQrCode(string url)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrData);
            return qrCode.GetGraphic(20);
        }
    }
}