using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MonitorAuthServer.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Rewrite;

namespace MonitorAuthServer
{
    public class Startup
    {
        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<Model.AllegroServiceConfig>(Configuration.GetSection("AllegroService"));
            services.Configure<Model.MonitorAuthConfig>(Configuration.GetSection("MonitorAuth"));
            services.Configure<Model.KeyEncryptCertificateConfig>(Configuration.GetSection("KeyEncryptCertificate"));
            services.Configure<Model.SignCertificateConfig>(Configuration.GetSection("SignCertificate"));
            services.Configure<Model.AuthDocSchemaConfig>(Configuration.GetSection("AuthDocSchema"));


            var authConfig = Configuration.GetSection("Authority")?.Get<Model.AuthorityConfig>();
            if (authConfig?.IsValid() ?? false)
            {
                var currentAuthority = authConfig.GetCurrent();

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(o => {
                        o.Authority = currentAuthority.Url;
                        o.Audience = currentAuthority.ApiId;
                    });

                services.AddAuthorization(o => {
                    o.AddPolicy("api:auth", p => {
                        p.AddRequirements(new Model.HasScopeRequirement("api:auth", currentAuthority.Url));
                    });

                    o.AddPolicy("api:app", p => {
                        p.AddRequirements(new Model.HasScopeRequirement("api:app", currentAuthority.Url));
                    });
                });
            }
            else
            {
                throw new Exception("Authority config is missing.");
            }


            services.AddDbContext<Model.Database>(o => {
                o.UseSqlServer(Extensions.ConnectionString.ForCurrentEnvironment(Configuration));
            });


            services.AddDistributedMemoryCache();
            services.AddSession(o => {
                o.IdleTimeout = TimeSpan.FromSeconds(60);
            });

            services.AddScoped<Model.DatabaseService>();
            services.AddScoped<Model.DatabaseTaskService>();

            services.AddSingleton<Model.AllegroServiceFactory>(svc => {
                var config = svc.GetRequiredService<IOptions<Model.AllegroServiceConfig>>();
                return Model.AllegroServiceFactory.Create(config);
            });

            services.AddScoped<Model.IAllegroService>(svc => {
                var factory = svc.GetRequiredService<Model.AllegroServiceFactory>();
                return factory.Create();
            });

            services.AddScoped<Model.KeyEncryptCertificateService>();
            services.AddScoped<Model.SignCertificateService>();
            services.AddScoped<Model.AuthDocSchemaService>();

            services.AddScoped<Model.MonitorAuthService>();


            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //if (env.IsDevelopment())
            //{
            //    //app.UseStatusCodePagesWithReExecute("/Status/Code/{0}");

            //    app.UseDeveloperExceptionPage();
            //    app.UseBrowserLink();
            //}
            //else
            //{
                app.UseWhen(context => context.IsApiRequested() ?? false, appBuilder => {
                    appBuilder.UseExceptionHandler(new ExceptionHandlerOptions() {
                        ExceptionHandler = Model.ApiExceptionHandler.Handler
                    });
                });

                app.UseWhen(context => !context.IsApiRequested() ?? false, appBuilder => {
                    appBuilder.UseExceptionHandler("/Home/Error");
                });
            //}

            app.UseRewriter(new RewriteOptions().AddRewrite("^silent", "views/silent.html", true));

            app.UseStaticFiles();

            app.UseSession();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
