using System.IO.Compression;
using System.Linq;
using AutoMapper;
using CF.Api.Middleware;
using CF.Customer.Application.Facades;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Domain.Helpers.PasswordHasher;
using CF.Customer.Domain.Repositories;
using CF.Customer.Domain.Services;
using CF.Customer.Domain.Services.Interfaces;
using CF.Customer.Infrastructure.DbContext;
using CF.Customer.Infrastructure.Mappers;
using CF.Customer.Infrastructure.Repositories;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace CF.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ICustomerFacade, CustomerFacade>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<ICustomerRepository, CustomerRepository>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "CF API", Version = "v1"}); });
            services.AddDefaultCorrelationId();
            services.AddControllers();
            services.AddAutoMapper(typeof(CustomerProfile));

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
            services.AddResponseCompression(options => { options.Providers.Add<GzipCompressionProvider>(); });

            services.AddDbContext<CustomerContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DbConnection"),
                    a => { a.MigrationsAssembly("CF.Api"); });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMapper autoMapper)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                autoMapper.ConfigurationProvider.AssertConfigurationIsValid();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCorrelationId();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<LogRequestMiddleware>();
            app.UseMiddleware<LogResponseMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "CF API V1"); });
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            RunMigration(app);
        }

        private static void RunMigration(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetService<CustomerContext>();
            if (context == null) return;
            if (context.Database.GetPendingMigrations().Any())
                context.Database.Migrate();
        }
    }
}