using Core.All.Enums;

namespace Core.All.Helpers;

/// <summary>
///     Represents the method that handles addon change events.
/// </summary>
/// <param name="gameEnum">The game that was changed.</param>
/// <param name="addonType">Optional addon type that was changed.</param>
public delegate void AddonChanged(GameEnum gameEnum, AddonTypeEnum? addonType);
