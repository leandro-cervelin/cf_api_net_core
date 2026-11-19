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
    /// <summary>
    ///     One OpenAPI document per API version. The name must match the ApiExplorer group name ('v'VVV, so "v1"):
    ///     ASP.NET Core puts an endpoint in a document only when its group name equals the document name.
    ///     Add a name here when a new version ships.
    /// </summary>
    private static readonly string[] DocumentNames = ["v1"];

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
        });

        foreach (var documentName in DocumentNames)
            services.AddOpenApi(documentName, options =>
            {
                options.AddDocumentTransformer<BearerSecurityTransformer>();
                options.AddOperationTransformer<BearerSecurityTransformer>();
            });

        return services;
    }

    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;

        app.MapOpenApi();
        app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes(BearerSecurityTransformer.SchemeName));

        return app;
    }
}
