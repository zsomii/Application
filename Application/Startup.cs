#region

using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;
using System.Text;
using Application.Data;
using Application.Data.Entity.Authorization;
using Application.Data.Entity.User;
using Application.Domain.Util;
using Application.Infrastructure;
using Application.Infrastructure.Filter;
using Application.Infrastructure.Middleware;
using Application.Infrastructure.Token;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

#endregion

namespace Application
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public const string PASSWORD_EXPIRATION_IN_DAYS = "PASSWORD_EXPIRATION_IN_DAYS";
        public const string USER_INACTIVITY_TIME_IN_MINUTES = "USER_INACTIVITY_TIME_IN_MINUTES";
        public const string LOCKOUT_USER = "LOCKOUT_USER";

        // DB Connections
        private const string MSSQLLOCALDB = "MSSQLLOCALDB";
        private const string REMOTE = "REMOTE";

        // Cross-Origin Resource Sharing (CORS)
        private const string CORS_POLICY = "CORSPolicy";

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureDatabase(services);
            ConfigureIdentity(services);
            ConfigureInfrastructure(services);

            #region Swagger

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Sample API",
                    Description = "This is the description of your new API",
                    TermsOfService = "None"
                });

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    Description = "Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = "header"
                });


                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.DocumentFilter<SwaggerSecurityRequirementsDocumentFilter>();
            });

            #endregion

            services.Configure<SecurityStampValidatorOptions>(options =>
                options.ValidationInterval = TimeSpan.FromSeconds(10));
        }

        private static void ConfigureInfrastructure(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy(CORS_POLICY, builder =>
            {
                builder.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }));

            Installer.ConfigureServices(services);

            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile(new MappingProfile()); });
            var mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            // MVC
            services.AddMvc(config => { config.Filters.Add(new ModelStateCheckFilter()); }).AddJsonOptions(
                options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd'T'HH:mm:ssZ";
                    options.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects;
                    options.SerializerSettings.SerializationBinder = new TypeNameSerializationBinder();
                    options.SerializerSettings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
                    options.SerializerSettings.MetadataPropertyHandling =
                        Newtonsoft.Json.MetadataPropertyHandling.ReadAhead;
                }
            );

            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region Swagger

            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "PharmaL3 API"); });

            #endregion

            app.UseMiddleware<HttpHandlerMiddleware>();
            app.UseCors(CORS_POLICY);
            app.UseAuthentication();
            app.UseSpaStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller}/{action=Index}/{id?}");
            });
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp/dist";

                if (env.IsDevelopment())
                {
                    spa.Options.StartupTimeout = new TimeSpan(0, 3, 0);
                    spa.UseAngularCliServer("start");
                }
            });

           // autoMapper.AssertConfigurationIsValid();
        }

        public virtual void ConfigureDatabase(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options
                    .ReplaceService<Microsoft.EntityFrameworkCore.Metadata.Internal.IEntityMaterializerSource,
                        DateTimeMaterializerSource>();
                options.EnableSensitiveDataLogging();
                options.UseSqlServer(_configuration.GetConnectionString(MSSQLLOCALDB),
                    sqlServerOptions =>
                    {
                        sqlServerOptions.CommandTimeout(60);
                        sqlServerOptions.EnableRetryOnFailure(
                            10,
                            TimeSpan.FromSeconds(30),
                            null);
                    });
            });
        }

        private static void ConfigureJwtAuth(AuthenticationOptions options)
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }

        private void ConfigureJwtBearer(JwtBearerOptions cfg)
        {
            cfg.RequireHttpsMetadata = false;
            cfg.SaveToken = true;
            cfg.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = _configuration["JwtIssuer"],
                ValidAudience = _configuration["JwtAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"])),
                ClockSkew = TimeSpan.Zero
            };
            cfg.SecurityTokenValidators.Clear();
            cfg.SecurityTokenValidators.Add(L3JwtSecurityTokenHandler.INSTANCE);
        }

        private void ConfigureIdentity(IServiceCollection services)
        {
            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IISOptions>(options => { options.ForwardClientCertificate = false; });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(ConfigureJwtAuth).AddJwtBearer(ConfigureJwtBearer);

            services.Configure<IdentityOptions>(ConfigureIdentity);
        }

        private static void ConfigureIdentity(IdentityOptions options)
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = false;
            options.Lockout.MaxFailedAccessAttempts = 3;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(365000);
        }
    }
}