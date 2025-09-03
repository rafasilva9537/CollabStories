using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using api.Constants;
using api.Data;
using api.Interfaces;
using api.Middlewares;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace api.Startup;

public static class DependenciesConfig
{
    public static IServiceCollection AddDependencyInjectionServices(this IServiceCollection services)
    {
        services.AddScoped<IStoryService, StoryService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IImageService, ImageService>();
        
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IStorySessionService, StorySessionService>();
        
        services.AddHostedService<TimerBackgroundService>();
        
        return services;
    }
    
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
                    },
                    new string[] {}
                }
            });
        });

        return services;
    }
    
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options => 
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options => 
        {
            string? secret = configuration["JwtConfig:Secret"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
            string? issuer = configuration["JwtConfig:ValidIssuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER");
            string? audience = configuration["JwtConfig:ValidAudiences"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE");

            if(secret is null || issuer is null || audience is null)
            {
                throw new ApplicationException("Jwt is not set in the configuration");
            }

            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // TODO: change back to true in deploy
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                RequireAudience = true,
                ValidAudience = audience,
                ValidIssuer = issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    StringValues accessToken = context.HttpContext.Request.Query["access_token"];
                    PathString path = context.HttpContext.Request.Path;

                    if(!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/story-hub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
        
        return services;
    }

    public static IServiceCollection AddConfiguredIdentityCore(this IServiceCollection services)
    {
        services.AddIdentityCore<AppUser>()
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        // TODO: change to more secure configs after
        services.Configure<IdentityOptions>(options => {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
        });
        
        return services;
    }

    public static IServiceCollection AddPolicyBasedAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyConstants.RequiredAdminRole, policyBuilder =>
                policyBuilder.RequireRole(RoleConstants.Admin)
            );
        });
        return services;
    }
    
    public static IServiceCollection AddConfiguredControllers(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
        return services;
    }

    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
            context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        return services;
    }

    public static IServiceCollection AddSqlServerDbContext(this IServiceCollection service, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection") ?? Environment.GetEnvironmentVariable("MSSQL_CONNECTION_STRING");
        service.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        return service;
    }

    public static IServiceCollection AddConfiguredCors(this IServiceCollection services)
    {
        //TODO: improve security
        CorsPolicy corsPolicy = new CorsPolicyBuilder()
            .WithOrigins(
                "http://localhost:5500", 
                "http://localhost:3001", 
                "http://localhost:3000",
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .Build();
        services.AddCors(options => {
            options.AddPolicy(CorsPolicyConstants.AllowSpecificOrigins, corsPolicy);
        });
        
        return services;
    }
}