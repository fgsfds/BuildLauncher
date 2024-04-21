//using Common.Enums;
//using Common.Enums.Addons;
//using Games.Games;
//using Mods.Mods;
//using Mods.Providers;
//using Ports.Ports;

//namespace Tests
//{
//    public class UnitTest1
//    {
//        [Fact]
//        public void Test1()
//        {
//            var currentDir = Directory.GetCurrentDirectory();

//            InstalledModsProvider provider = new(new());

//            Raze razePort = new();
//            DukeGame dukeGame = new(provider)
//            {
//                Duke64RomPath = string.Empty,
//                DukeWTInstallPath = string.Empty,
//                GameInstallFolder = Path.Combine(currentDir, "Game")
//            };
//            DukeCampaign dukeCamp = new()
//            {
//                Guid = Guid.NewGuid(),
//                ModType = ModTypeEnum.Campaign,
//                DisplayName = "Main Campaign",
//                SupportedPorts = null,
//                Description = string.Empty,
//                PathToFile = @"C:\Games\Duke3D\MOD.ZIP",
//                StartupFile = "MOD.CON",
//                Image = null,
//                Version = 1,
//                Url = null,
//                Author = null,
//                IsOfficial = false,
//                AddonEnum = DukeAddonEnum.Duke3D
//            };

//            var args = razePort.GetStartGameArgs(dukeGame, dukeCamp, true);

//            var expected = $@" -nosetup -savedir ""{currentDir}\Data\Ports\Raze\Save\Main_Campaign"" -addon 0 -file ""{currentDir}\Data\Duke3D\Campaigns\MOD.ZIP"" -con ""MOD.CON"" -quick";

//            Assert.Equal(expected, args);
//        }
//    }
//}