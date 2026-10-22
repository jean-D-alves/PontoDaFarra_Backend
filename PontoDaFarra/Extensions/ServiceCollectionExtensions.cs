using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PontoDaFarra.context;
using PontoDaFarra.Middleware;
using PontoDaFarra.repositories;
using PontoDaFarra.repositories.Interface;
using PontoDaFarra.services;
using PontoDaFarra.Services;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddForwardedHeadersConfiguration(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    private static string GetPartitionKey(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                context => RateLimitPartition.GetFixedWindowLimiter(
                    GetPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
                GetPartitionKey(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                var problemDetailsWriter = context.HttpContext.RequestServices
                    .GetRequiredService<ProblemDetailsWriter>();

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);

                var statusCode = StatusCodes.Status429TooManyRequests;

                await problemDetailsWriter.WriteAsync(
                    context.HttpContext,
                    statusCode,
                    ProblemDetailsWriter.Title(statusCode),
                    ProblemDetailsWriter.Detail(statusCode),
                    cancellationToken);
            };
        });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        string jwtKey,
        string issuer,
        string audience)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        ctx.Token = ctx.Request.Cookies[AuthCookieService.AccessTokenCookie];

                        if (string.IsNullOrEmpty(ctx.Token))
                        {
                            var header = ctx.Request.Headers.Authorization.ToString();
                            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                                ctx.Token = header["Bearer ".Length..];
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<UserServices>();
        services.AddScoped<IUserService>(provider => provider.GetRequiredService<UserServices>());
        services.AddScoped<AuthServices>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<ICategoryServices, CategoryServices>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<AuthCookieService>();
        services.AddHostedService<RefreshTokenCleanupService>();

        if (environment.IsDevelopment())
        {
            services.AddSingleton<IImageStorage, LocalImageStorage>();
        }
        else
        {
            services.AddSingleton<IImageStorage, R2ImageStorage>();
        }

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, string frontend)
    {
        services.AddCors(o => o.AddDefaultPolicy(p =>
            p.WithOrigins(frontend)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials()));

        return services;
    }

    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var problemDetails = context.ProblemDetails;

                problemDetails.Instance ??= context.HttpContext.Request.Path;
                problemDetails.Type ??= ProblemDetailsWriter.ProblemType(problemDetails.Status ?? StatusCodes.Status500InternalServerError);
                problemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
            };
        });

        services.AddSingleton<ProblemDetailsWriter>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var statusCode = StatusCodes.Status400BadRequest;

                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Status = statusCode,
                    Title = ProblemDetailsWriter.Title(statusCode),
                    Detail = ProblemDetailsWriter.Detail(statusCode),
                    Instance = context.HttpContext.Request.Path,
                    Type = ProblemDetailsWriter.ProblemType(statusCode)
                };

                problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { ProblemDetailsWriter.ProblemJsonContentType }
                };
            };
        });

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}