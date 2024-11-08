using Common.Common.Providers;
using Common.Entities;
using Common.Serializable.Addon;
using Database.Server;
using Web.Blazor.Helpers;
using Web.Blazor.Providers;
using Web.Blazor.Tasks;

namespace Web.Blazor;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        _ = builder.Services.AddRazorPages();
        _ = builder.Services.AddServerSideBlazor();

        _ = builder.Services.AddControllers().AddJsonOptions(jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;

            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(AddonsJsonEntityListContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(DownloadableAddonEntityListContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(GeneralReleaseEntityContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(GeneralReleaseEntityObsoleteContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(AddonManifestContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(DependencyDtoContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(DependantAddonDtoContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(MapFileDtoContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(MapSlotDtoContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(SupportedGameDtoContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(GitHubReleaseEntityContext.Default);
            jsonOptions.JsonSerializerOptions.TypeInfoResolverChain.Add(GitHubReleaseAssetContext.Default);
        });

        // Don't run tasks in dev mode
        if (!builder.Environment.IsDevelopment())
        {
            _ = builder.Services.AddHostedService<FileCheckTask>();
            _ = builder.Services.AddHostedService<PortsReleasesTask>();
            _ = builder.Services.AddHostedService<AppReleasesTask>();
            //_ = builder.Services.AddHostedService<ToolsReleasesTask>();
        }

        if (builder.Environment.IsDevelopment())
        {
            ServerProperties.IsDeveloperMode = true;
        }

        _ = builder.Services.AddSingleton<AppReleasesProvider>();
        _ = builder.Services.AddSingleton<AddonsProvider>();
        _ = builder.Services.AddSingleton<PortsReleasesProvider>();
        _ = builder.Services.AddSingleton<ToolsReleasesProvider>();
        _ = builder.Services.AddSingleton<RepositoriesProvider>();

        _ = builder.Services.AddSingleton<HttpClient>(CreateHttpClient);
        _ = builder.Services.AddSingleton<S3Client>();

        _ = builder.Services.AddSingleton<DatabaseContextFactory>(x => new(builder.Environment.IsDevelopment()));

        _ = builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

        var app = builder.Build();


        // Creating database
        var dbContext = app.Services.GetService<DatabaseContextFactory>()!.Get();
        dbContext.Dispose();


        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            _ = app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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