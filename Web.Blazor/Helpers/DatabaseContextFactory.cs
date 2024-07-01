using Web.Blazor.Database;

namespace Web.Blazor.Helpers
{
    public class DatabaseContextFactory
    {
        public DatabaseContext Get() => new();
    }
}
