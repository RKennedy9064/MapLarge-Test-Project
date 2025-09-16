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
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(homeDirectory ?? throw new ArgumentNullException(nameof(homeDirectory))),
                RequestPath = "/files",
                EnableDirectoryBrowsing = true
            });
            app.MapControllers();   

            app.Run();
        }
    }
}