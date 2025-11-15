using Microsoft.Extensions.DependencyInjection;
using Tools.Tools;

namespace Tools.DI;

public static class ToolsBindings
{
    public static void Load(ServiceCollection container)
    {
        _ = container.AddSingleton<BaseTool, Mapster32>();
        _ = container.AddSingleton<BaseTool, XMapEdit>();
        _ = container.AddSingleton<BaseTool, DOSBlood>();
    }
}
