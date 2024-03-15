using Common.Enums;
using Ports.Providers;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// NotBlood port
    /// </summary>
    public sealed class NotBlood : NBlood
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.NotBlood;

        /// <inheritdoc/>
        public override string Exe => "notblood.exe";

        /// <inheritdoc/>
        public override string Name => "NotBlood";

        /// <inheritdoc/>
        public override string ConfigFile => "notblood.cfg";

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/clipmove/NotBlood/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.StartsWith("notblood-win64");
    }
}
