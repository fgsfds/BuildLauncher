// using Core.Client.Interfaces;
// using Games.Games;
// using Games.Providers;
// using Moq;
//
// namespace Tests.Unit.Helpers;
//
// internal sealed class TestInstalledGamesProvider : InstalledGamesProvider
// {
//     private readonly IReadOnlyList<BaseGame> _games;
//
//     public TestInstalledGamesProvider(IReadOnlyList<BaseGame> games)
//         : base(Mock.Of<IConfigProvider>())
//     {
//         _games = games;
//     }
//
//     public TestInstalledGamesProvider(IConfigProvider config, IReadOnlyList<BaseGame> games)
//         : base(config)
//     {
//         _games = games;
//     }
//
//     public override IReadOnlyList<BaseGame> GetGames() => _games;
// }



