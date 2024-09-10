namespace Database.Server;

public class DatabaseContextFactory
{
    private readonly bool _isDevMode;

    public DatabaseContextFactory(bool isDevMode)
    {
        _isDevMode = isDevMode;
    }

    public DatabaseContext Get() => new(_isDevMode);
}
