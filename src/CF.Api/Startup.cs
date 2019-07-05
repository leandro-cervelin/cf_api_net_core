using System;
using System.IO.Compression;
using AutoMapper;
using CF.Api.Middlewares;
using CF.CustomerMngt.Application.Facades;
using CF.CustomerMngt.Application.Facades.Interfaces;
using CF.CustomerMngt.Application.Mappers;
using CF.CustomerMngt.Domain.Helpers.PasswordHasher;
using CF.CustomerMngt.Domain.Repositories;
using CF.CustomerMngt.Domain.Services;
using CF.CustomerMngt.Domain.Services.Interfaces;
using CF.CustomerMngt.Infrastructure.DbContext;
using CF.CustomerMngt.Infrastructure.Repositories;
using CorrelationId;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

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
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "CF API", Version = "v1"}); });
            services.AddCorrelationId();
            services.AddAutoMapper(typeof(CustomerMngtProfile));
            services.AddDbContext<CustomerMngtContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DbConnection"),
                    a => { a.MigrationsAssembly("CF.Api"); });
            });
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
            services.AddResponseCompression(options => { options.Providers.Add<GzipCompressionProvider>(); });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IMapper autoMapper)
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
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}");
            });

            RunMigration(app);
        }

        private static void RunMigration(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                if (serviceScope.ServiceProvider.GetService<CustomerMngtContext>().Database.GetPendingMigrations().Any())
                    serviceScope.ServiceProvider.GetService<CustomerMngtContext>().Database.Migrate();
            }
        }
    }
}