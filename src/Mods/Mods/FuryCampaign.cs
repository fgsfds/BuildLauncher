﻿namespace Mods.Mods
{
    /// <summary>
    /// Ion Fury campaign
    /// </summary>
    public sealed class FuryCampaign : Addon
    {
        /// <summary>
        /// Main .con file
        /// </summary>
        public required string? MainCon { get; init; }

        /// <summary>
        /// Additional .con files
        /// </summary>
        public required HashSet<string>? AdditionalCons { get; init; }

        /// <summary>
        /// Main .rts file
        /// </summary>
        public required string? RTS { get; init; }
    }
}
