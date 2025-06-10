using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PortfolioService.Core.Handlers;
using PortfolioService.Infrastructure.Integrations;
using PortfolioService.Infrastructure.Persistence;
using StockService;
using System;

namespace PortfolioService.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Portfolio Service", Version = "v1" });
            });

            AddSettings(services);
            RegisterInfrastructure(services);
            RegisterCore(services);
        }

        private void AddSettings(IServiceCollection services)
        {
            services.AddSingleton(Configuration.GetSection("CurrencyLayerSettings").Get<CurrencyLayerSettings>());
            services.AddSingleton(Configuration.GetSection("DataServiceSettings").Get<DataServiceSettings>());
        }

        private void RegisterInfrastructure(IServiceCollection services)
        {
            services.AddSingleton<ICurrencyLayerClient, CurrencyLayerClient>();

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development") {
                services.AddSingleton<IDataService, LocalDataService>();
            } else {
                services.AddSingleton<IDataService, ProdDataService>();
            }

            services.AddSingleton<IStockService, StockService.StockService>();
        }

        private void RegisterCore(IServiceCollection services)
        {
            services.AddScoped<PortfolioHandler>();
            services.AddScoped<CurrencyConversionHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio Service v1"));
            }

            app.UseMiddleware<ExceptionHandlerMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}