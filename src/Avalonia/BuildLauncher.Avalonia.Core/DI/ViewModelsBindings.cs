using BuildLauncher.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BuildLauncher.DI
{
    public static class ViewModelsBindings
    {
        public static void Load(ServiceCollection container)
        {
            container.AddSingleton<ViewModelsFactory>();

            container.AddSingleton<MainViewModel>();
            container.AddSingleton<SettingsViewModel>();
            container.AddSingleton<AboutViewModel>();
        }
    }
}
