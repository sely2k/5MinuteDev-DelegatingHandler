using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace MyHealthCheckMonitor
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

            services.AddRazorPages();

            #region
            services.AddHealthChecksUI()
                .AddInMemoryStorage();
            #endregion

            services.AddHealthChecks()
                .AddCheck<ExampleHealthCheck>("example_health_check", HealthStatus.Unhealthy, new string[] { "examplecheck" })



            #region 
            .AddSqlServer(connectionString: Configuration["ConnectionStrings:ConnectionString"])
            .AddApplicationInsightsPublisher()
            .AddDatadogPublisher("myservice.healthchecks");
            #endregion

            //.AddPrometheusGatewayPublisher()

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                #region
                app.UseHealthChecksUI(config => config.UIPath = "/hc-ui");
                #endregion

                endpoints.MapHealthChecks("/hcbase");

                #region
                endpoints.MapHealthChecks("/hc", options: new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                #endregion
            });
        }
    }
}
