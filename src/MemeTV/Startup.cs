using System;
using MemeTV.BusinessLogic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MemeTV
{
    public class Startup
    {
        private readonly IHostingEnvironment env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            this.env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddSingleton<ILogger, ConsoleLogger>();
            services.AddSingleton<IClipManager, ClipManager>();
            services.AddSingleton<IClipProvider, ClipProvider>();
            services.AddSingleton<IVttTemplateRenderer, VttTemplateRenderer>();
            services.AddSingleton<IClipIdentifierProvider, ClipIdentifierProvider>();

            services.AddSingleton<ISqlConnectionProvider, SqlConnectionProvider>();
            services.AddSingleton<IDbConnectionStringProvider>(re => new DbConnectionStringProvider(re.GetService<IOptions<AppSettings>>().Value));

            services.AddSingleton<IHostingEnvironment>(env);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSession();
            app.MapWhen(ctx =>
            {
                var identifierProvider = app.ApplicationServices.GetService<IClipIdentifierProvider>();
                var val = ctx.Request.Path.Value;
                if (string.IsNullOrEmpty(val)) return false;
                var possibleId = val.Replace("/", "");
                return identifierProvider.IsValid(possibleId) || val.Contains("view/", StringComparison.OrdinalIgnoreCase);
            }, builder =>
            {
                builder.Run(async ctx =>
                {
                    var viewContent = System.IO.File.ReadAllText(System.IO.Path.Combine(env.WebRootPath, "view", "index.html"));
                    await ctx.Response.WriteAsync(
                        string.Format(viewContent, "<script>var id = '" + ctx.Request.Path.Value.Replace("/", "") + "';</script>"));
                });
            });

            app.UseCors();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
