using Common.Helpers;
using System.Text.Json;

namespace Common.Config
{
    public sealed class ConfigProvider
    {
        public ConfigProvider()
        {
            Config = ReadConfigFromFile();

            Config.NotifyConfigChanged += SaveConfigFile;
        }

        /// <summary>
        /// Current config
        /// </summary>
        public ConfigEntity Config { get; private set; }

        /// <summary>
        /// Read config from file or create new file if it doesn't exist
        /// </summary>
        private ConfigEntity ReadConfigFromFile()
        {
            if (!File.Exists(Consts.ConfigFile))
            {
                ConfigEntity newConfig = new();

                Config = newConfig;

                SaveConfigFile();

                return newConfig;
            }

            ConfigEntity? config;

            using (FileStream fs = new(Consts.ConfigFile, FileMode.OpenOrCreate))
            {
                config = JsonSerializer.Deserialize(fs, ConfigEntityContext.Default.ConfigEntity);
            }

            config.ThrowIfNull();

            return config;
        }

        /// <summary>
        /// Save current config to file
        /// </summary>
        private void SaveConfigFile()
        {
            using FileStream fs = new(Consts.ConfigFile, FileMode.Create);

            JsonSerializer.Serialize(
               fs,
               Config,
               ConfigEntityContext.Default.ConfigEntity
               );
        }
    }
}
