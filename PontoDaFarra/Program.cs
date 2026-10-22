using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PontoDaFarra.context;
using PontoDaFarra.Middleware;
using PontoDaFarra.repositories;
using PontoDaFarra.repositories.Interface;
using PontoDaFarra.services;
using PontoDaFarra.Services;
using PontoDaFarra.services.Interface;
DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["JWT_KEY"]
             ?? throw new InvalidOperationException("JWT_KEY não configurada");
var issuer = builder.Configuration["ISSUER"]!;
var audience = builder.Configuration["AUDIENCE"]!;
var frontend = builder.Configuration["FRONTEND_URL"]!;

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services
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

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<UserServices>();
builder.Services.AddScoped<IUserService>(provider => provider.GetRequiredService<UserServices>());
builder.Services.AddScoped<AuthServices>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryServices, CategoryServices>();
builder.Services.AddScoped<AuthCookieService>();
builder.Services.AddHostedService<RefreshTokenCleanupService>();

builder.Services.AddAuthorization();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(frontend)
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        var problemDetails = context.ProblemDetails;

        problemDetails.Instance ??= context.HttpContext.Request.Path;
        problemDetails.Type ??= ProblemDetailsWriter.ProblemType(problemDetails.Status ?? StatusCodes.Status500InternalServerError);
        problemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
    };
});

builder.Services.AddSingleton<ProblemDetailsWriter>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseExceptionHandler();

app.UseMiddleware<ProblemDetailsStatusCodeMiddleware>();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
