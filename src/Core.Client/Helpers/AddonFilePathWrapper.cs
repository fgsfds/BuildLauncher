namespace Core.Client.Helpers;

/// <summary>
/// Wraps a folder or archive path together with a manifest file name.
/// </summary>
public sealed record AddonFilePathWrapper
{
    private readonly string _pathToAddonFileOrFolder;
    private readonly string _mainFileName;

    /// <summary>
    /// Initializes a new wrapper, normalizing path separators to the platform native character.
    /// </summary>
    /// <param name="pathToAddon">Path to the folder or zip file containing the addon.</param>
    /// <param name="mainFileName">Name of the manifest file (entry name inside zip for packed addons).</param>
    public AddonFilePathWrapper(string pathToAddon, string mainFileName)
    {
        _pathToAddonFileOrFolder = pathToAddon.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        _mainFileName = mainFileName;
    }

    /// <summary>
    /// True when the path is a folder (no file extension).
    /// </summary>
    public bool IsFolder => string.IsNullOrEmpty(Path.GetExtension(_pathToAddonFileOrFolder));

    /// <summary>
    /// True when the path ends with ".zip".
    /// </summary>
    public bool IsZip => _pathToAddonFileOrFolder.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// True when this is an unpacked folder addon with a ".json" manifest.
    /// </summary>
    public bool IsJson => IsFolder && _mainFileName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// True when this is a loose ".map" file.
    /// </summary>
    public bool IsMap => IsFolder && _mainFileName.EndsWith(".map", StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// True when this is a ".grpinfo" file.
    /// </summary>
    public bool IsGrpInfo => IsFolder && _mainFileName.EndsWith(".grpinfo", StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Returns the folder path. For zips, this is the parent directory.
    /// </summary>
    public string PathToFolder => IsFolder ? _pathToAddonFileOrFolder : Path.GetDirectoryName(_pathToAddonFileOrFolder);

    /// <summary>
    /// Returns the file path to pass to the port.
    /// For folders: combined folder and filename. For zips: the zip path alone.
    /// </summary>
    public string PathToFile => IsFolder ? Path.Combine(_pathToAddonFileOrFolder, _mainFileName) : _pathToAddonFileOrFolder;

    /// <summary>
    /// Returns the file name component. For zips, this is the zip filename.
    /// </summary>
    public string FileName => Path.GetFileName(PathToFile);

    /// <summary>
    /// Returns a new wrapper with the same filename but a different folder path.
    /// </summary>
    /// <param name="newFolderPath">New folder or zip path.</param>
    public AddonFilePathWrapper WithChangedFolder(string newFolderPath)
    {
        return new(newFolderPath, _mainFileName);
    }

    [Obsolete("Don't use ToString(), use properties instead.", true)]
    public override string ToString() => Path.Combine(_pathToAddonFileOrFolder, _mainFileName);
}
