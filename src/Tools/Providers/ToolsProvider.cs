using Common.Client.Helpers;
using Common.Enums;
using Common.Helpers;
using Games.Providers;
using Tools.Tools;

namespace Tools.Providers
{
    /// <summary>
    /// Class that provides singleton instances of port types
    /// </summary>
    public sealed class ToolsProvider
    {
        private readonly List<BaseTool> _tools;

        public Mapster32 Mapster32 { get; init; }
        public XMapEdit XMapEdit { get; init; }


        public ToolsProvider(GamesProvider gamesProvider)
        {
            if (!Directory.Exists(ClientProperties.ToolsFolderPath))
            {
                Directory.CreateDirectory(ClientProperties.ToolsFolderPath);
            }

            Mapster32 = new(gamesProvider);
            XMapEdit = new(gamesProvider);

            _tools = [Mapster32, XMapEdit];
        }


        /// <summary>
        /// Get list of all ports
        /// </summary>
        public IEnumerable<BaseTool> GetAllTools() => _tools;

        /// <summary>
        /// Get port by enum
        /// </summary>
        /// <param name="toolEnum">Port enum</param>
        public BaseTool GetTool(ToolEnum toolEnum)
        {
            return toolEnum switch
            {
                ToolEnum.Mapster32 => Mapster32,
                ToolEnum.XMapEdit => XMapEdit,
                _ => ThrowHelper.NotImplementedException<BaseTool>()
            };
        }
    }
}
