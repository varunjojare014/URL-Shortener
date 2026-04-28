using Microsoft.AspNetCore.Mvc;
using UrlShortener.Services;
using UrlShortener.DTOs;

namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("")]
    public class UrlController : ControllerBase
    {
        private readonly UrlService _service;
        private readonly QrService _qrService;

        public UrlController(UrlService service, QrService qrService)
        {
            _service = service;
            _qrService = qrService;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> Shorten([FromBody] CreateUrlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OriginalUrl))
                return BadRequest(new { message = "Original URL is required" });

            try
            {
                var code = await _service.CreateShortUrl(request.OriginalUrl, request.CustomCode);
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var shortUrl = $"{baseUrl}/{code}";

                var qrBytes = _qrService.GenerateQrCode(shortUrl);
                var qrBase64 = Convert.ToBase64String(qrBytes);

                return Ok(new
                {
                    shortUrl,
                    qrCode = $"data:image/png;base64,{qrBase64}"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            var url = await _service.GetOriginalUrl(code);

            if (url == null)
                return NotFound();

            return RedirectPermanent(url); // better than Redirect()
        }
    }
}