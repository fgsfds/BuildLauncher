namespace Updater
{
    public sealed class AppRelease
    {
        public readonly string Url;
        public readonly Version Version;

        public AppRelease(string url, Version version)
        {
            Url = url;
            Version = version;
        }
    }
}
