using Common.Providers;
using Web.Server.Database;
using Web.Server.Helpers;
using Web.Server.Providers;
using Web.Server.Tasks;

namespace Web.Server
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

            if (!builder.Environment.IsDevelopment())
            {
                builder.Services.AddHostedService<AppReleasesTask>();
                builder.Services.AddHostedService<PortsReleasesTask>();
                builder.Services.AddHostedService<ToolsReleasesTask>();
                builder.Services.AddHostedService<FileCheckTask>();
            }

            builder.Services.AddSingleton<AppReleasesProvider>();
            builder.Services.AddSingleton<AddonsProvider>();
            builder.Services.AddSingleton<PortsReleasesProvider>();
            builder.Services.AddSingleton<ToolsReleasesProvider>();
            builder.Services.AddSingleton<RepositoriesProvider>();

            builder.Services.AddSingleton<HttpClient>(CreateHttpClient);
            builder.Services.AddSingleton<S3Client>();

            builder.Services.AddSingleton<DatabaseContextFactory>();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                ServerProperties.IsDevMode = true;

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
