using Common.Enums;
using Common.Helpers;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ClientCommon.Config
{
    public sealed class ConfigEntity
    {
        public ConfigEntity()
        {
            _theme = ThemeEnum.System;
        }

        public delegate void ParameterChanged(string? parameterName);
        public event ParameterChanged ParameterChangedEvent;

        private ThemeEnum _theme = ThemeEnum.System;
        public ThemeEnum Theme
        {
            get => _theme;
            set => SetConfigParameter(ref _theme, value);
        }

        private string? _gamePathBlood = null;
        public string? GamePathBlood
        {
            get => _gamePathBlood;
            set => SetConfigParameter(ref _gamePathBlood, value);
        }

        private string? _gamePathDuke = null;
        public string? GamePathDuke3D
        {
            get => _gamePathDuke;
            set => SetConfigParameter(ref _gamePathDuke, value);
        }

        private string? _gamePathDuke64 = null;
        public string? GamePathDuke64
        {
            get => _gamePathDuke64;
            set => SetConfigParameter(ref _gamePathDuke64, value);
        }

        private string? _gamePathDukeWT = null;
        public string? GamePathDukeWT
        {
            get => _gamePathDukeWT;
            set => SetConfigParameter(ref _gamePathDukeWT, value);
        }

        private string? _gamePathWang = null;
        public string? GamePathWang
        {
            get => _gamePathWang;
            set => SetConfigParameter(ref _gamePathWang, value);
        }

        private string? _gamePathRedneck = null;
        public string? GamePathRedneck
        {
            get => _gamePathRedneck;
            set => SetConfigParameter(ref _gamePathRedneck, value);
        }

        private string? _gamePathAgain = null;
        public string? GamePathAgain
        {
            get => _gamePathAgain;
            set => SetConfigParameter(ref _gamePathAgain, value);
        }

        private string? _gamePathSlave = null;
        public string? GamePathSlave
        {
            get => _gamePathSlave;
            set => SetConfigParameter(ref _gamePathSlave, value);
        }

        private string? _gamePathFury = null;
        public string? GamePathFury
        {
            get => _gamePathFury;
            set => SetConfigParameter(ref _gamePathFury, value);
        }

        private bool _skipIntro = false;
        public bool SkipIntro
        {
            get => _skipIntro;
            set => SetConfigParameter(ref _skipIntro, value);
        }

        private bool _skipStartup = true;
        public bool SkipStartup
        {
            get => _skipStartup;
            set => SetConfigParameter(ref _skipStartup, value);
        }

        private bool _useLocalApi = false;
        public bool UseLocalApi
        {
            get => _useLocalApi;
            set => SetConfigParameter(ref _useLocalApi, value);
        }

        public HashSet<string> DisabledAutoloadMods { get; set; } = [];

        public void AddDisabledAutoloadMod(string id)
        {
            DisabledAutoloadMods.Add(id);
            ParameterChangedEvent?.Invoke(nameof(DisabledAutoloadMods));
        }

        public void RemoveDisabledAutoloadMod(string id)
        {
            DisabledAutoloadMods.Remove(id);
            ParameterChangedEvent?.Invoke(nameof(DisabledAutoloadMods));
        }

        private Dictionary<string, TimeSpan> _playtimes = [];
        public Dictionary<string, TimeSpan> Playtimes
        {
            get => _playtimes;
            set => SetConfigParameter(ref _playtimes, value);
        }

        private Dictionary<string, bool> _upvotes = [];
        public Dictionary<string, bool> Upvotes
        {
            get => _upvotes;
            set => SetConfigParameter(ref _upvotes, value);
        }

        private string _apiPassword = string.Empty;
        public string ApiPassword
        {
            get => _apiPassword;
            set => SetConfigParameter(ref _apiPassword, value);
        }

        public void ForceUpdateConfig()
        {
            ParameterChangedEvent?.Invoke(null);
        }


        /// <summary>
        /// Sets config parameter if changed and invokes notifier
        /// </summary>
        /// <param name="field">Parameter field to change</param>
        /// <param name="value">New value</param>
        private void SetConfigParameter<T>(ref T? field, T value, [CallerMemberName] string callerName = "")
        {
            callerName.ThrowIfNullOrEmpty();

            if (field is null || !field.Equals(value))
            {
                if (value is string str && string.IsNullOrWhiteSpace(str))
                {
                    field = default;
                }
                else
                {
                    field = value;
                }

                ParameterChangedEvent?.Invoke(callerName);
            }
        }
    }

    [JsonSourceGenerationOptions(
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = [typeof(JsonStringEnumConverter<ThemeEnum>)]
        )]
    [JsonSerializable(typeof(ConfigEntity))]
    internal sealed partial class ConfigEntityContext : JsonSerializerContext;
}
