namespace Common.Helpers
{
    public static class CommonProperties
    {
        public static readonly string ExeFolderPath = Directory.GetCurrentDirectory();

        public static readonly string DataFolderPath = Path.Combine(ExeFolderPath, "Data");

        public static readonly string PortsFolderPath = Path.Combine(DataFolderPath, "Ports");

        /// <summary>
        /// Is app running in the developer mode
        /// </summary>
        public static bool IsDevMode { get; set; } = false;
    }
}
