using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Pds.Audit.Api.Client.Registrations;
using Pds.Core.ApiAuthentication;
using Pds.Core.Logging;
using Pds.Core.Telemetry.ApplicationInsights;
using Pds.FundingClaim.Api.DependencyInjection;
using Pds.FundingClaim.Api.Telemetry;
using Pds.FundingClaim.Repositories.Data;

namespace Pds.FundingClaim.Api
{
    /// <summary>
    /// The startup class.
    /// </summary>
    public class Startup
    {
        private const string RequireElevatedRightsPolicyName = "RequireElevatedRights";
        private const string CurrentApiVersion = "v1.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="environment">The environment.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        /// <summary>
        /// Gets the assembly name.
        /// </summary>
        public string AssemblyName { get => GetType().Assembly.GetName().Name; }

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// Configures the services for the container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(RequireElevatedRightsPolicyName, policy => policy.RequireRole("FundingClaimApiRole"));
            });

            services.AddControllers(cfg =>
            {
                cfg.Filters.Add(new AuthorizeFilter(RequireElevatedRightsPolicyName));
            });

            services.AddLoggerAdapter();
            services.AddPdsApplicationInsightsTelemetry(options => BuildAppInsightsConfiguration(options));
            services.AddSingleton<ITelemetryInitializer>(
                new CustomTelemetryInitializer(
                    Configuration["Environment"],
                    AssemblyName));

            services.AddAzureADAuthentication(Configuration);

            services.AddFundingClaimServices(Configuration);

            if (Environment.IsDevelopment())
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc(CurrentApiVersion, new OpenApiInfo { Title = AssemblyName, Version = CurrentApiVersion });
                    c.CustomSchemaIds(type => type.ToString());
                });
                services.DisableAuthentication(AssemblyName);
            }

            services.AddDbContext<PdsContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("fundingclaims"));
            });

            var policyRegistry = services.AddPolicyRegistry();
            services.AddAuditApiClient(Configuration, policyRegistry);
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/{CurrentApiVersion}/swagger.json", AssemblyName);
                });
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void BuildAppInsightsConfiguration(PdsApplicationInsightsConfiguration options)
        {
            Configuration.Bind("PdsApplicationInsights", options);
            options.Component = AssemblyName;
        }
    }
}