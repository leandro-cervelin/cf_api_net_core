using Asp.Versioning;
using CF.Api.OpenApi;
using Scalar.AspNetCore;

namespace CF.Api.Startup;

/// <summary>
///     URL-based API versioning (/api/v1/...) and the OpenAPI document with the Scalar docs page, which is only
///     served in Development.
/// </summary>
public static class ApiDocumentationSetup
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddMvc().AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        }).AddOpenApi(options =>
        {
            options.Document.AddDocumentTransformer<BearerSecurityTransformer>();
            options.Document.AddOperationTransformer<BearerSecurityTransformer>();
        });

        return services;
    }

    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;

        app.MapOpenApi().WithDocumentPerVersion();
        app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes(BearerSecurityTransformer.SchemeName));

        return app;
    }
}
