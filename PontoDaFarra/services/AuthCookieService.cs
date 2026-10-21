using Microsoft.AspNetCore.Http;

namespace PontoDaFarra.services;

public class AuthCookieService(IConfiguration config)
{
    public const string AccessTokenCookie = "access_token";
    public const string RefreshTokenCookie = "refresh_token";
    public const string RefreshTokenPath = "/auth";

    public void Set(HttpContext context, string accessToken, string refreshToken)
    {
        var accessMinutes = config.GetValue("EXPIRES_MINUTES", 15);
        var refreshDays = config.GetValue("REFRESH_EXPIRES_DAYS", 7);

        context.Response.Cookies.Append(AccessTokenCookie, accessToken, BuildCookieOptions(context, accessMinutes));
        context.Response.Cookies.Append(RefreshTokenCookie, refreshToken, BuildCookieOptions(context, refreshDays * 24 * 60, RefreshTokenPath));
    }

    public void Delete(HttpContext context)
    {
        context.Response.Cookies.Delete(AccessTokenCookie, BuildCookieOptions(context));
        context.Response.Cookies.Delete(RefreshTokenCookie, BuildCookieOptions(context, path: RefreshTokenPath));
    }

    private static CookieOptions BuildCookieOptions(HttpContext context, int? minutes = null, string path = "/") => new()
    {
        HttpOnly = true,
        Secure = context.Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Path = path,
        Expires = minutes is null ? null : DateTimeOffset.UtcNow.AddMinutes(minutes.Value)
    };
}
