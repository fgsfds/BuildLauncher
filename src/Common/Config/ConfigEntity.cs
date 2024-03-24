﻿using Common.Enums;
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

        private ThemeEnum _theme = ThemeEnum.System;
        public ThemeEnum Theme
        {
            get => _theme;
            set => SetConfigParameter(ref _theme, value);
        }

        private string _gamePathBlood = string.Empty;
        public string GamePathBlood
        {
            get => _gamePathBlood;
            set => SetConfigParameter(ref _gamePathBlood, value);
        }

        private string _gamePathDuke = string.Empty;
        public string GamePathDuke3D
        {
            get => _gamePathDuke;
            set => SetConfigParameter(ref _gamePathDuke, value);
        }

        private string _gamePathDuke64 = string.Empty;
        public string GamePathDuke64
        {
            get => _gamePathDuke64;
            set => SetConfigParameter(ref _gamePathDuke64, value);
        }

        private string _gamePathDukeWT = string.Empty;
        public string GamePathDukeWT
        {
            get => _gamePathDukeWT;
            set => SetConfigParameter(ref _gamePathDukeWT, value);
        }

        private string _gamePathWang = string.Empty;
        public string GamePathWang
        {
            get => _gamePathWang;
            set => SetConfigParameter(ref _gamePathWang, value);
        }

        private string _gamePathRedneck = string.Empty;
        public string GamePathRedneck
        {
            get => _gamePathRedneck;
            set => SetConfigParameter(ref _gamePathRedneck, value);
        }

        private string _gamePathAgain = string.Empty;
        public string GamePathAgain
        {
            get => _gamePathAgain;
            set => SetConfigParameter(ref _gamePathAgain, value);
        }

        private string _gamePathSlave = string.Empty;
        public string GamePathSlave
        {
            get => _gamePathSlave;
            set => SetConfigParameter(ref _gamePathSlave, value);
        }

        private string _gamePathFury = string.Empty;
        public string GamePathFury
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

        public HashSet<Guid> DisabledAutoloadMods { get; set; } = [];

        public void AddDisabledAutoloadMod(Guid guid)
        {
            DisabledAutoloadMods.Add(guid);
            NotifyConfigChanged?.Invoke();
            NotifyParameterChanged?.Invoke(nameof(DisabledAutoloadMods));
        }

        public void RemoveDisabledAutoloadMod(Guid guid)
        {
            DisabledAutoloadMods.Remove(guid);
            NotifyConfigChanged?.Invoke();
            NotifyParameterChanged?.Invoke(nameof(DisabledAutoloadMods));
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
