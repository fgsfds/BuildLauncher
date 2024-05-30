﻿using ClientCommon.Config;
using ClientCommon.Providers;
using Common.Enums;
using Common.Interfaces;

namespace Mods.Providers
{
    public class InstalledAddonsProviderFactory(
        ConfigProvider config,
        PlaytimeProvider playtimeProvider
        )
    {
        private readonly Dictionary<GameEnum, InstalledAddonsProvider> _list = [];
        private readonly ConfigProvider _config = config;
        private readonly PlaytimeProvider _playtimeProvider = playtimeProvider;

        /// <summary>
        /// Get or create singleton instance of the provider
        /// </summary>
        /// <param name="game">Game</param>
        public InstalledAddonsProvider GetSingleton(IGame game)
        {
            if (_list.TryGetValue(game.GameEnum, out var value))
            {
                return value;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            InstalledAddonsProvider newProvider = new(game, _config, _playtimeProvider);
#pragma warning restore CS0618 // Type or member is obsolete
            _list.Add(game.GameEnum, newProvider);

            return newProvider;
        }
    }
}
