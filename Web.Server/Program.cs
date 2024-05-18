using ClientCommon.Config;
using ClientCommon.Providers;
using Common.Tools;
using Games.Providers;
using Mods.Providers;
using Ports.Providers;
using Superheater.Web.Server.Providers;
using Superheater.Web.Server.Tasks;
using Tools.Providers;
using Web.Server.Database;
using Web.Server.Helpers;

namespace Superheater.Web.Server
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddJsonOptions(jsonOptions =>
            {
                jsonOptions.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHostedService<AppReleasesTask>();
            builder.Services.AddHostedService<PortsReleasesTask>();
            builder.Services.AddHostedService<ToolsReleasesTask>();

            builder.Services.AddSingleton<AppReleasesProvider>();
            builder.Services.AddSingleton<PortsReleasesProvider>();
            builder.Services.AddSingleton<ToolsReleasesProvider>();
            builder.Services.AddSingleton<PortsProvider>();
            builder.Services.AddSingleton<ToolsProvider>();
            builder.Services.AddSingleton<ConfigProvider>();

            //TODO DELETE THESE
            {
                builder.Services.AddSingleton<GamesProvider>();
                builder.Services.AddSingleton<InstalledAddonsProviderFactory>();
                builder.Services.AddSingleton<DownloadableAddonsProviderFactory>();
                builder.Services.AddSingleton<PlaytimeProvider>();
                builder.Services.AddSingleton<ArchiveTools>();
            }

            builder.Services.AddSingleton<HttpClient>(CreateHttpClient);
            builder.Services.AddSingleton<S3Client>();

            builder.Services.AddSingleton<DatabaseContextFactory>();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            var dbContext = new DatabaseContext();
            dbContext.Dispose();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }

        private static HttpClient CreateHttpClient(IServiceProvider provider)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
            return httpClient;
        }
    }
}
