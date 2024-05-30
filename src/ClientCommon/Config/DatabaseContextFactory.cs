namespace ClientCommon.Config;

public class DatabaseContextFactory
{
    public DatabaseContext Get() => new();
}
