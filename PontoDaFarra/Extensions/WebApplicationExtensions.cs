using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using PontoDaFarra.context;
using PontoDaFarra.services;

namespace PontoDaFarra.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseAppPipeline(this WebApplication app)
    {
        // As imagens de produto são servidas pela própria API em todos os ambientes.
        var uploadsPath = Path.GetFullPath(
            app.Configuration["Storage:Local:Path"] ?? "uploads");

        Directory.CreateDirectory(uploadsPath);

        // Restringe o que pode ser servido: apenas as extensões aceitas no upload.
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        contentTypeProvider.Mappings.Clear();
        contentTypeProvider.Mappings[".jpg"] = "image/jpeg";
        contentTypeProvider.Mappings[".jpeg"] = "image/jpeg";
        contentTypeProvider.Mappings[".png"] = "image/png";
        contentTypeProvider.Mappings[".webp"] = "image/webp";

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/uploads",
            ContentTypeProvider = contentTypeProvider,
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                // O nome do arquivo é um GUID, então o conteúdo nunca muda.
                ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
            }
        });

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    public static async Task SeedLegalDocumentsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<LegalDocumentSeeder>();

        try
        {
            await seeder.SeedAsync();
        }
        catch (PostgresException exception)
            when (exception.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            // Sem migrations, o cadastro público também não funciona; apenas avisamos.
            app.Logger.LogWarning(
                "Tabela de documentos legais ausente. Seed ignorado até que as migrations sejam aplicadas.");
        }
    }

    public static async Task SeedAdminAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();

        try
        {
            await seeder.SeedAsync();
        }
        catch (PostgresException exception)
            when (exception.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            app.Logger.LogWarning(
                "Tabela de usuários ausente. Seed do admin inicial ignorado até que as migrations sejam aplicadas.");
        }
    }
}
