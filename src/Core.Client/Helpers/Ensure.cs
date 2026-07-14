namespace Core.Client.Helpers;

/// <summary>Helper methods for precondition checks and directory creation.</summary>
public static class Ensure
{
    /// <summary>Creates the specified directory if it does not already exist.</summary>
    /// <param name="path">Directory path to ensure exists.</param>
    public static void DirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
