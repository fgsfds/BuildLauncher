namespace Common.Helpers
{
    public static class Consts
    {
        /// <summary>
        /// Path to main downloadable mods manifests file
        /// </summary>
        public const string Manifests = "https://github.com/fgsfds/Build-Mods-Repo/raw/master/manifests.json";

        /// <summary>
        /// GirtHub releases Url
        /// </summary>
        public const string GitHubReleases = "https://api.github.com/repos/fgsfds/BuildLauncher/releases";

        /// <summary>
        /// Config file
        /// </summary>
        public const string ConfigFile = "config.json";

        /// <summary>
        /// World Tour Stopgap folder
        /// </summary>
        public const string WTStopgap = "WTStopgap";

        /// <summary>
        /// Blood ini
        /// </summary>
        public const string BloodIni = "BLOOD.INI";

        /// <summary>
        /// Blood Cryptic Passage ini
        /// </summary>
        public const string CrypticIni = "CRYPTIC.INI";

        /// <summary>
        /// Combined mod folder
        /// </summary>
        public const string CombinedModFolder = "combined";

        /// <summary>
        /// Combined def file name
        /// </summary>
        public const string CombinedDef = "z_combined.def";

        public const string UpdateFile = ".update";

        public const string UpdateFolder = "update";

        public static readonly Guid BloodGuid = new("a611ba93-7a99-4bf5-aa20-c70a3c6f3937");
        public static readonly Guid CrypticGuid = new("618d788b-7222-42c0-bddb-241321a84e00");
        public static readonly Guid WangGuid = new("178ee8b7-05e2-4155-a5c8-b476411e1759");
        public static readonly Guid WantonGuid = new("d2280c69-7fe7-4145-a87e-dc89b1592106");
        public static readonly Guid TwinDragonGuid = new("95c95b09-2af5-4283-947d-3bc9ba95fa12");
        public static readonly Guid Duke3dGuid = new("6831c611-3e9a-458d-82de-a84630c2f0af");
        public static readonly Guid DukeDCGuid = new("01d0c219-79b4-4ff4-9347-71356772bae8");
        public static readonly Guid CaribbeanGuid = new("149f94a8-67c5-46fc-94d9-5979981ed3f5");
        public static readonly Guid NuclearWinterGuid = new("b66de608-5563-4ff4-9180-eafa3e5c05b8");
        public static readonly Guid WorldTourGuid = new("bc84eec9-f0b7-49e8-8445-2c53959f9dad");
        public static readonly Guid Duke64Guid = new("a0166e53-2918-4a74-9a1b-c6229103d13c");
        public static readonly Guid FuryGuid = new("16e17d93-dcaa-469f-8a42-07fb0e39dd32");
        public static readonly Guid SlaveGuid = new("b6430ca7-ac76-420a-aff8-75b916aacfdd");
        public static readonly Guid RedneckGuid = new("4580f0db-cc7c-4bbe-8ecc-00cc52469c07");
        public static readonly Guid AgainGuid = new("f292db2a-8aba-4262-9ff1-3f17ef0607c7");
        public static readonly Guid Route66Guid = new("2f13d194-c74d-4f91-944b-62e87b561398");
    }
}
