using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PontoDaFarra.context;

namespace PontoDaFarra.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseDevelopmentTools(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.GetFullPath("uploads")),
                RequestPath = "/uploads",
                OnPrepareResponse = ctx =>
                    ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff"
            });
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}