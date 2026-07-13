namespace Core.Client.Helpers;

public static class Ensure
{
    public static void DirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
