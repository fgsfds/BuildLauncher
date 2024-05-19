using Common.Enums;
using Games.Providers;

namespace Tools.Tools
{
    public sealed class XMapEdit : BaseTool
    {
        private readonly GamesProvider _gamesProvider;

        /// <inheritdoc/>
        public override string Exe => "xmapedit.exe";

        /// <inheritdoc/>
        public override string Name => "XMAPEDIT";

        /// <inheritdoc/>
        public override ToolEnum ToolEnum => ToolEnum.XMapEdit;

        /// <inheritdoc/>
        public override string PathToExecutableFolder => _gamesProvider.GetGame(GameEnum.Blood).GameInstallFolder ?? string.Empty;

        /// <inheritdoc/>
        public override bool CanBeLaunched => _gamesProvider.IsBloodInstalled;


        public XMapEdit(GamesProvider gamesProvider)
        {
            _gamesProvider = gamesProvider;
        }


        /// <inheritdoc/>
        public override string GetStartToolArgs() => string.Empty;
    }
}
