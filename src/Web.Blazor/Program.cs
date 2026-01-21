using Common.All.Enums;
using Common.All.Interfaces;
using Common.All.Providers;
using Common.All.Serializable;
using Common.All.Serializable.Addon;
using Common.All.Serializable.Downloadable;
using Database.Server;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ports.Providers;
using Tools.Providers;
using Web.Blazor.Helpers;
using Web.Blazor.Providers;
using Web.Blazor.Tasks;

namespace Web.Blazor;

internal sealed class Program
{
    private const string OpenApiJsonPath = "/openapi/v1.json";
    private static IConfiguration _config = null!;
    private static bool _isDevMode;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        _config = builder.Configuration;
        _isDevMode = builder.Environment.IsDevelopment();

        var services = builder.Services;
        var controllers = services.AddControllers();

        _ = builder.Logging.AddConsole();
        _ = builder.Configuration.AddEnvironmentVariables();

        _ = services.AddRazorPages();
        _ = services.AddHttpClient();
        _ = services.AddMiddlewareAnalysis();
        _ = services.AddPooledDbContextFactory<DatabaseContext>(GetConnectionString);
        _ = services.AddEndpointsApiExplorer();
        _ = services.AddOpenApi();
        _ = services.AddProblemDetails();

        _ = services.AddMediator(options =>
        {
            options.Namespace = "SimpleConsole.Mediator";
            options.ServiceLifetime = ServiceLifetime.Singleton;
            options.GenerateTypesAsInternal = true;
            options.NotificationPublisherType = typeof(ForeachAwaitPublisher);
            options.Assemblies = [typeof(Program)];
        });

        _ = controllers.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.PropertyNamingPolicy = null;

            options.JsonSerializerOptions.TypeInfoResolverChain.Add(AddonManifestContext.Default);
            options.JsonSerializerOptions.TypeInfoResolverChain.Add(MapFileJsonModelContext.Default);
            options.JsonSerializerOptions.TypeInfoResolverChain.Add(MapSlotJsonModelContext.Default);
            options.JsonSerializerOptions.TypeInfoResolverChain.Add(SupportedGameJsonModelContext.Default);
            options.JsonSerializerOptions.TypeInfoResolverChain.Add(DataJsonModelContext.Default);
            options.JsonSerializerOptions.TypeInfoResolverChain.Add(DownloadableAddonJsonModelDictionaryContext.Default);
            options.JsonSerializerOptions.TypeInfoResolverChain.Add(GitHubReleaseEntityContext.Default);
        });

        if (!_isDevMode)
        {
            _ = services.AddHostedService<FileCheckTask>();
            _ = services.AddHostedService<PortsReleasesTask>();
            _ = services.AddHostedService<AppReleasesTask>();
            _ = services.AddHostedService<ToolsReleasesTask>();
        }

        _ = services.AddSingleton<RepoAppReleasesProvider>();
        _ = services.AddSingleton<DatabaseAddonsRetriever>();
        _ = services.AddSingleton<IReleaseProvider<PortEnum>, PortsReleasesProvider>();
        _ = services.AddSingleton<IReleaseProvider<ToolEnum>, ToolsReleasesProvider>();
        _ = services.AddSingleton<S3Client>();


        var app = builder.Build();

        using (var dbContext = app.Services.GetRequiredService<IDbContextFactory<DatabaseContext>>().CreateDbContext())
        {
            dbContext.Database.Migrate();
        }

        if (!_isDevMode)
        {
            _ = app.UseExceptionHandler("/Error");
            _ = app.UseHsts();
        }

        if (_isDevMode)
        {
            _ = app.MapOpenApi(OpenApiJsonPath);
            _ = app.UseSwaggerUI(x => x.SwaggerEndpoint(OpenApiJsonPath, "main"));
        }

        _ = app.MapControllers();
        _ = app.MapFallbackToPage("/_Host");

        _ = app.UseStaticFiles();
        _ = app.UseStatusCodePages();

        app.Run();
    }

    private static void GetConnectionString(DbContextOptionsBuilder builder)
    {
        if (_isDevMode)
        {
            _ = builder.UseNpgsql("Host=localhost;Port=5432;Database=buildlauncher;Username=postgres;Password=123;Include Error Detail=True");
            return;
        }

        var dbIp = _config.GetValue<string>("DbIp") ?? throw new NullReferenceException("dbIp");
        var dbPort = _config.GetValue<string>("DbPort") ?? throw new NullReferenceException("dbPort");
        var dbUser = _config.GetValue<string>("DbUser") ?? throw new NullReferenceException("dbUser");
        var dbPass = _config.GetValue<string>("DbPass") ?? throw new NullReferenceException("dbPass");
        var dbName = _config.GetValue<string>("DbName") ?? throw new NullReferenceException("dbName");

        _ = builder.UseNpgsql($"Host={dbIp};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass}");
    }
}
