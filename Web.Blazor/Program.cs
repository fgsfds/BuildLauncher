using Common.Providers;
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
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        builder.Services.AddControllers().AddJsonOptions(jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
        });

        // Don't run tasks in dev mode
        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddHostedService<AppReleasesTask>();
            builder.Services.AddHostedService<PortsReleasesTask>();
            builder.Services.AddHostedService<ToolsReleasesTask>();
            builder.Services.AddHostedService<FileCheckTask>();
        }
        
        if (builder.Environment.IsDevelopment())
        {
            ServerProperties.IsDevMode = true;
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


        // Creating database
        var dbContext = app.Services.GetService<DatabaseContextFactory>()!.Get();
        dbContext.Dispose();


        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }


        app.MapControllers();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");


        app.Run();
    }

    private static HttpClient CreateHttpClient(IServiceProvider provider)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        return httpClient;
    }
}