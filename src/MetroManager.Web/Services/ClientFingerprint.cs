using System;
using Microsoft.AspNetCore.Http;

namespace MetroManager.Web.Services
{
    public class ClientFingerprint
    {
        private const string CookieName = "mm-fp";
        public string EnsureFingerprint(HttpContext ctx)
        {
            if (ctx.Request.Cookies.TryGetValue(CookieName, out var val) && !string.IsNullOrWhiteSpace(val))
                return val;

            var fp = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", string.Empty)
                .Replace("+", string.Empty)
                .Replace("/", string.Empty);

            ctx.Response.Cookies.Append(CookieName, fp, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = ctx.Request.IsHttps,
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });

            return fp;
        }
    }
}

