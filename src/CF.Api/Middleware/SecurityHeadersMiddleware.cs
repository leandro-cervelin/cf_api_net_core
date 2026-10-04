namespace CF.Api.Middleware;

/// <summary>Adds browser-hardening headers to every response, including errors.</summary>
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            var path = context.Request.Path;

            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "no-referrer";

            // The API only serves JSON, so nothing may load or frame it. The Scalar docs page (Development only)
            // is HTML that pulls its own scripts, so it is left out.
            if (!path.StartsWithSegments("/scalar"))
                headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";

            // Customer data must never be stored by browsers or intermediaries.
            if (path.StartsWithSegments("/api") && string.IsNullOrEmpty(headers.CacheControl))
                headers.CacheControl = "no-store";

            return Task.CompletedTask;
        });

        return next(context);
    }
}
