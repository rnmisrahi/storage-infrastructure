﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JwtSample.Server.Data;
using Microsoft.IdentityModel.Tokens;
using JwtSample.Server.Controllers;
using JwtSample.Server.LogProvider;

namespace JwtSample.Server
{
    public class Startup
    {
        private IHostingEnvironment _env;
        private string _testSecret = null;
        public static ILoggerFactory _loggerFactory;
        private string connectionString = null;

        public Startup(IHostingEnvironment env)
        {
            _env = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            connectionString = _env.IsDevelopment() ? "LocalConnection" : "DefaultConnection";

            _testSecret = Configuration["JwtSecret"];

            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString(connectionString)));

            services.Configure<AppOptions>(options => Configuration.Bind(options));

            //services
            //    .AddIdentity<ApplicationUser, IdentityRole>(options =>
            //{
            //    options.Password.RequireDigit = false;
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequireLowercase = false;
            //    options.Password.RequireUppercase = false;
            //    options.Password.RequireNonAlphanumeric = false;
            //})
            //.AddEntityFrameworkStores<ApplicationDbContext>()
            //.AddDefaultTokenProviders();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ApplicationDbContext context)
        {
            _loggerFactory = loggerFactory;
            _loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddAzureWebAppDiagnostics();
            //TODO: The LogLevel needs to be Warning on release
            loggerFactory.AddContext(LogLevel.Information, Configuration.GetConnectionString(connectionString));
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.Issuer,

                    ValidateAudience = false,
                    ValidAudience = AuthOptions.Audience,

                    ValidateLifetime = true,

                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true

                    //ClockSkew = TimeSpan.Zero
                }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "api/v1/{controller=Home}/{action=Index}/{id?}");
            });

            DBInitializer.Initialize(context);
        }
    }
}
