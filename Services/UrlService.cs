using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Models;

namespace UrlShortener.Services
{
    public class UrlService
    {
        private readonly AppDbContext _context;
        private readonly RedisService _redis;

        public UrlService(AppDbContext context, RedisService redis)
        {
            _context = context;
            _redis = redis;
        }

        public async Task<string> CreateShortUrl(string originalUrl, string? customCode)
        {
            string shortCode;

            if (!string.IsNullOrWhiteSpace(customCode))
            {
                var exists = await _context.UrlMappings
                    .AnyAsync(x => x.ShortCode == customCode);

                if (exists)
                    throw new InvalidOperationException("Custom URL already taken");

                shortCode = customCode;
            }
            else
            {
                shortCode = await GenerateUniqueShortCode();
            }

            var url = new UrlMapping
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode
            };

            _context.UrlMappings.Add(url);
            await _context.SaveChangesAsync();

            await _redis.SetAsync(shortCode, originalUrl);

            return shortCode;
        }

        public async Task<string?> GetOriginalUrl(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                return null;

            var cached = await _redis.GetAsync(shortCode);
            if (cached != null)
                return cached;

            var url = await _context.UrlMappings
                .FirstOrDefaultAsync(x => x.ShortCode == shortCode);

            if (url == null)
                return null;

            url.ClickCount++;
            await _context.SaveChangesAsync();

            await _redis.SetAsync(shortCode, url.OriginalUrl);

            return url.OriginalUrl;
        }

        private async Task<string> GenerateUniqueShortCode()
        {
            string code;

            do
            {
                code = Guid.NewGuid().ToString("N").Substring(0, 6);
            }
            while (await _context.UrlMappings.AnyAsync(x => x.ShortCode == code));

            return code;
        }
    }
}