using Common.Providers;
using Database.Server;
using Web.Blazor.Helpers;
using Web.Blazor.Providers;
using Web.Blazor.Tasks;

namespace Web.Blazor;

public class Program
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
        });

        // Don't run tasks in dev mode
        if (!builder.Environment.IsDevelopment())
        {
            _ = builder.Services.AddHostedService<AppReleasesTask>();
            _ = builder.Services.AddHostedService<PortsReleasesTask>();
            //_ = builder.Services.AddHostedService<ToolsReleasesTask>();
            _ = builder.Services.AddHostedService<FileCheckTask>();
        }
        
        if (builder.Environment.IsDevelopment())
        {
            ServerProperties.IsDevMode = true;
        }

        _ = builder.Services.AddSingleton<AppReleasesProvider>();
        _ = builder.Services.AddSingleton<AddonsProvider>();
        _ = builder.Services.AddSingleton<PortsReleasesProvider>();
        _ = builder.Services.AddSingleton<ToolsReleasesProvider>();
        _ = builder.Services.AddSingleton<RepositoriesProvider>();

        _ = builder.Services.AddSingleton<HttpClient>(CreateHttpClient);
        _ = builder.Services.AddSingleton<S3Client>();

        _ = builder.Services.AddSingleton<DatabaseContextFactory>(x => new(builder.Environment.IsDevelopment()));


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
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        return httpClient;
    }
}