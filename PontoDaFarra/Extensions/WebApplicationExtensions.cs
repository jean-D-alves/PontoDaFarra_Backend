using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PontoDaFarra.context;

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
}
