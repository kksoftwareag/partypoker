using Blazored.LocalStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbelt.Extensions.DependencyInjection;

namespace PlanningPoker.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddBlazoredLocalStorage(config => config.JsonSerializerOptions.WriteIndented = true);
            services.AddScoped<UserState>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    // Cache unchanged static file for 1 year
                    if (String.IsNullOrEmpty(context.Context.Request.Query["v"]) == false ||
                        context.Context.Request.Path.ToString().Contains("css", StringComparison.OrdinalIgnoreCase) ||
                        context.Context.Request.Path.ToString().Contains("woff", StringComparison.OrdinalIgnoreCase) ||
                        context.Context.Request.Path.ToString().Contains("ico", StringComparison.OrdinalIgnoreCase) ||
                        context.Context.Request.Path.ToString().Contains("png", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Context.Response.Headers.Add("cache-control", new[] { "public,max-age=31536000" });
                        context.Context.Response.Headers.Add("Expires", new[] { DateTime.UtcNow.AddYears(1).ToString("R") }); // Format RFC1123
                    }
                }
            });
            // call .UseStaticFiles() twice to keep blazor working
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCssLiveReload();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
