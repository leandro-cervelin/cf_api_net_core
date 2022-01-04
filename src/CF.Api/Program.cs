using System.IO.Compression;
using CF.Api.Middleware;
using CF.Customer.Application.Facades;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Domain.Repositories;
using CF.Customer.Domain.Services;
using CF.Customer.Domain.Services.Interfaces;
using CF.Customer.Infrastructure.DbContext;
using CF.Customer.Infrastructure.Mappers;
using CF.Customer.Infrastructure.Repositories;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();

builder.Services.AddControllers();
builder.Services.AddDefaultCorrelationId();
builder.Services.AddTransient<ICustomerFacade, CustomerFacade>();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<ICustomerRepository, CustomerRepository>();
builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddAutoMapper(typeof(CustomerProfile));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SetupSwagger());
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
builder.Services.AddResponseCompression(options => { options.Providers.Add<GzipCompressionProvider>(); });
builder.Services.AddDbContext<CustomerContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"),
        a => { a.MigrationsAssembly("CF.Api"); });
});

AddNLog();

await using var app = builder.Build();

app.UseCorrelationId();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CF Api"));
    app.UseMiddleware<LogRequestMiddleware>();
    app.UseMiddleware<LogResponseMiddleware>();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();

static Action<SwaggerGenOptions> SetupSwagger()
{
    return c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "CF API", Version = "v1" });

        c.CustomSchemaIds(x => x.FullName);

        c.AddSecurityDefinition("Authorization", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            Description = "JWT Authorization header using the Bearer scheme",
            In = ParameterLocation.Header
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Authorization"
                    }
                },
                new List<string>()
            }
        });
    };
}

void AddNLog()
{
    if (builder.Environment.EnvironmentName.Contains("Test")) return;
    LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
}

public partial class Program { }
