using Microsoft.Extensions.DependencyInjection;

namespace Core.Client.DI;

public static class BindingsManager
{
    private static ServiceCollection? _instance;
    private static ServiceProvider? _provider;
    private static readonly Lock _lock = new();

    public static ServiceCollection Instance
    {
        get
        {
            if (_instance is null)
            {
                using (_lock.EnterScope())
                {
                    _instance = new ServiceCollection();
                }
            }

            return _instance;
        }
    }

    public static ServiceProvider Provider
    {
        get
        {
            if (_provider is null)
            {
                using (_lock.EnterScope())
                {
                    _provider = Instance.BuildServiceProvider(new ServiceProviderOptions
                    {
                        ValidateOnBuild = true,
                        ValidateScopes = true
                    });
                }
            }

            return _provider;
        }
    }

    public static void Reset()
    {
        _instance = null;
        _provider?.Dispose();
        _provider = null;
    }
}
