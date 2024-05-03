namespace Common.Releases
{
    public sealed class CommonRelease
    {
        public readonly string Url;
        public readonly string Version;

        public CommonRelease(string url, string version)
        {
            Url = url;
            Version = version;
        }
    }
}
