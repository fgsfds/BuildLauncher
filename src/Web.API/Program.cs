using Common.All.Serializable.Addon;
using Common.All.Serializable.Downloadable;
using Database.Server;
using Web.API.Helpers;

namespace Web.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        _ = builder.Services.AddControllers().AddJsonOptions(jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;

            //jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(AddonManifestContext.Default);
            //jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(DownloadableAddonJsonModelDictionaryContext.Default);
            //jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(GeneralReleaseJsonModelContext.Default);
            //jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(AddonManifestContext.Default);
            //jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(DependencyDtoContext.Default);
            //jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(DependantAddonJsonModelContext.Default);
            //jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(MapFileJsonModelContext.Default);
            //jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(MapSlotJsonModelContext.Default);
            //jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(SupportedGameJsonModelContext.Default);
            //jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(GitHubReleaseEntityContext.Default);
            //jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(GitHubReleaseAssetContext.Default);
        });

        // Don't run tasks in dev mode
        if (!builder.Environment.IsDevelopment())
        {
            //_ = builder.Services.AddHostedService<FileCheckTask>();
            //_ = builder.Services.AddHostedService<PortsReleasesTask>();
            //_ = builder.Services.AddHostedService<AppReleasesTask>();
            //_ = builder.Services.AddHostedService<ToolsReleasesTask>();
        }

        if (builder.Environment.IsDevelopment())
        {
            ServerProperties.IsDeveloperMode = true;
        }

        _ = builder.Services.AddSingleton(CreateHttpClient);
        _ = builder.Services.AddSingleton<S3Client>();

        _ = builder.Services.AddSingleton<DatabaseContextFactory>(_ => new(builder.Environment.IsDevelopment()));

        builder.Services.AddOpenApi();


        var app = builder.Build();

        app.UseDefaultFiles();
        app.MapStaticAssets();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.MapFallbackToFile("/index.html");

        app.Run();
    }

    private static HttpClient CreateHttpClient(IServiceProvider provider)
    {
        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        return httpClient;
    }
}
