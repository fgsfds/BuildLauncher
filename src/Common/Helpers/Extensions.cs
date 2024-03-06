namespace Common.Helpers
{
    public static class Extensions
    {
        public static string ToSizeString(this long size)
        {
            if (size < 1024)
            {
                return $"{size}B";
            }
            else if (size < 1024 * 1024)
            {
                return $"{size / 1024}Kb";
            }
            else if (size < 1024 * 1024 * 1024)
            {
                return $"{size / 1024 / 1024}Mb";
            }

            return $"{size / 1024 / 1024 / 1024}Gb";
        }
    }
}
