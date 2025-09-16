using Microsoft.Extensions.FileProviders;
using TestProject.Services;

namespace TestProject {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddScoped<FileBrowsingService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            var homeDirectory = app.Configuration["HomeDirectory"];

            // This will only serve known file types. Files with unknown extensions won't be served by default. This can be fixed by
            // implementing a custom provider.
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(homeDirectory ?? throw new ArgumentNullException(nameof(homeDirectory))),
                RequestPath = "/files",
            });
            app.MapControllers();   

            app.Run();
        }
    }
}