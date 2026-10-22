using Microsoft.AspNetCore.Http.Features;
using PontoDaFarra.Extensions;
using PontoDaFarra.Middleware;
using PontoDaFarra.Services;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["JWT_KEY"]
             ?? throw new InvalidOperationException("JWT_KEY não configurada");
var issuer = builder.Configuration["ISSUER"]!;
var audience = builder.Configuration["AUDIENCE"]!;
var frontend = builder.Configuration["FRONTEND_URL"]!;

var maxUploadMb = builder.Configuration.GetValue("UploadImagem:TamanhoMaximoMB", 5);
var maxRequestBytes = maxUploadMb * 1024L * 1024L + 256 * 1024;

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxRequestBytes;
    options.ValueLengthLimit = 64 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = maxRequestBytes);

builder.Services.AddForwardedHeadersConfiguration();
builder.Services.AddRateLimiting();
builder.Services.AddJwtAuthentication(jwtKey, issuer, audience);
builder.Services.AddApplicationServices(builder.Environment);
builder.Services.AddAuthorization();
builder.Services.AddCorsPolicy(frontend);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddOptions();
builder.Services.AddErrorHandling();
builder.Services.AddDatabase(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

app.UseMiddleware<ProblemDetailsStatusCodeMiddleware>();

app.UseDevelopmentTools();

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
