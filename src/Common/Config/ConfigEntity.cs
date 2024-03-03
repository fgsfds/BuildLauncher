using Common.Enums;
using Common.Helpers;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Common.Config
{
    public sealed class ConfigEntity
    {
        public ConfigEntity()
        {
            _theme = ThemeEnum.System;
        }

        public delegate void ConfigChanged();
        public event ConfigChanged NotifyConfigChanged;

        public delegate void ParameterChanged(string parameterName);
        public event ParameterChanged NotifyParameterChanged;

        private ThemeEnum _theme;
        public ThemeEnum Theme
        {
            get => _theme;
            set => SetConfigParameter(ref _theme, value);
        }

        private string _gamePathBlood;
        public string GamePathBlood
        {
            get => _gamePathBlood;
            set => SetConfigParameter(ref _gamePathBlood, value);
        }

        private string _gamePathDuke;
        public string GamePathDuke3D
        {
            get => _gamePathDuke;
            set => SetConfigParameter(ref _gamePathDuke, value);
        }

        private string _gamePathDuke64;
        public string GamePathDuke64
        {
            get => _gamePathDuke64;
            set => SetConfigParameter(ref _gamePathDuke64, value);
        }

        private string _gamePathDukeWT;
        public string GamePathDukeWT
        {
            get => _gamePathDukeWT;
            set => SetConfigParameter(ref _gamePathDukeWT, value);
        }

        private string _gamePathWang;
        public string GamePathWang
        {
            get => _gamePathWang;
            set => SetConfigParameter(ref _gamePathWang, value);
        }

        /// <summary>
        /// Sets config parameter if changed and invokes notifier
        /// </summary>
        /// <param name="fieldName">Parameter field to change</param>
        /// <param name="value">New value</param>
        private void SetConfigParameter<T>(ref T fieldName, T value, [CallerMemberName] string callerName = "")
        {
            callerName.ThrowIfNullOrEmpty();

            if (fieldName is null || !fieldName.Equals(value))
            {
                fieldName = value;
                NotifyConfigChanged?.Invoke();
                NotifyParameterChanged?.Invoke(callerName);
            }
        }
    }

    [JsonSourceGenerationOptions(
        WriteIndented = true,
        Converters = [typeof(JsonStringEnumConverter<ThemeEnum>)]
        )]
    [JsonSerializable(typeof(ConfigEntity))]
    internal sealed partial class ConfigEntityContext : JsonSerializerContext { }
}
