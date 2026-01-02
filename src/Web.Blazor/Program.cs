using Common.All.Enums;
using Common.All.Interfaces;
using Common.All.Providers;
using Common.All.Serializable.Addon;
using Common.All.Serializable.Downloadable;
using Database.Server;
using Mediator;
using Web.Blazor.Helpers;
using Web.Blazor.Providers;
using Web.Blazor.Tasks;

namespace Web.Blazor;

internal sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        _ = builder.Services.AddRazorPages();
        _ = builder.Services.AddServerSideBlazor();

        _ = builder.Services.AddControllers().AddJsonOptions((jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;

            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(AddonManifestContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(DownloadableAddonJsonModelDictionaryContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(GeneralReleaseJsonModelContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(AddonManifestContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(DependencyDtoContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(DependantAddonJsonModelContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(MapFileJsonModelContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(MapSlotJsonModelContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(SupportedGameJsonModelContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(GitHubReleaseEntityContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(GitHubReleaseAssetContext.Default);
        }));

        // Don't run tasks in dev mode
        if (!builder.Environment.IsDevelopment())
        {
            _ = builder.Services.AddHostedService<FileCheckTask>();
            _ = builder.Services.AddHostedService<PortsReleasesTask>();
            _ = builder.Services.AddHostedService<AppReleasesTask>();
            _ = builder.Services.AddHostedService<ToolsReleasesTask>();
        }

        if (builder.Environment.IsDevelopment())
        {
            ServerProperties.IsDeveloperMode = true;
        }

        _ = builder.Services.AddSingleton<RepoAppReleasesProvider>();
        _ = builder.Services.AddSingleton<DatabaseAddonsRetriever>();
        _ = builder.Services.AddSingleton<IReleaseProvider<PortEnum>>();
        _ = builder.Services.AddSingleton<IReleaseProvider<ToolEnum>>();

        _ = builder.Services.AddSingleton(CreateHttpClient);
        _ = builder.Services.AddSingleton<S3Client>();

        _ = builder.Services.AddSingleton<DatabaseContextFactory>(_ => new(builder.Environment.IsDevelopment()));

        _ = builder.Services.AddMediator((MediatorOptions options) =>
        {
            options.Namespace = "SimpleConsole.Mediator";
            options.ServiceLifetime = ServiceLifetime.Singleton;
            options.GenerateTypesAsInternal = true;
            options.NotificationPublisherType = typeof(ForeachAwaitPublisher);
            options.Assemblies = [typeof(Program)];
        });

        var app = builder.Build();

        // Creating database
        var dbContext = app.Services.GetService<DatabaseContextFactory>()!.Get();
        dbContext.Dispose();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            _ = app.UseExceptionHandler("/Error");
            _ = app.UseHsts();
        }

        _ = app.MapControllers();
        _ = app.UseHttpsRedirection();
        _ = app.UseStaticFiles();
        _ = app.UseRouting();
        _ = app.MapBlazorHub();
        _ = app.MapFallbackToPage("/_Host");

        app.Run();
    }

    private static HttpClient CreateHttpClient(IServiceProvider provider)
    {
        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        return httpClient;
    }
}