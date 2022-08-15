using GamesSizeCalculator.GOG;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GamesSizeCalculator.Tests.GOG
{
    public class GogSizeCalculatorTests
    {
        private GogSizeCalculator SetupGogSizeCalculator(string slug, string id = null)
        {
            var urlFiles = new Dictionary<string, string> { { $"https://www.gog.com/game/{slug}", $"./GOG/{slug}.html" } };
            if (id != null)
            {
                urlFiles.Add($"https://api.gog.com/products/{id}?expand=description", $"./GOG/{id}.json");
            }
            var downloader = new FakeHttpDownloader(urlFiles);
            var calc = new GogSizeCalculator(downloader);
            return calc;
        }

        private Game SetupGame(string slug, string id)
        {
            var game = new Game();
            if (slug != null)
            {
                game.Links = new System.Collections.ObjectModel.ObservableCollection<Link> { new Link("GOG", $"https://www.gog.com/game/{slug}") };
            }

            if (id != null)
            {
                game.PluginId = GogSizeCalculator.GogLibraryId;
                game.GameId = id;
            }

            return game;
        }

        [Fact]
        public async Task Project_Eden_Slug_Returns_391MB()
        {
            var calc = SetupGogSizeCalculator("project_eden");
            var game = SetupGame("project_eden", null);
            var size = await calc.GetInstallSizeAsync(game);
            Assert.Equal((ulong)(391 * 1024), size);
        }

        [Fact]
        public async Task Project_Eden_Id_Returns_391MB()
        {
            var calc = SetupGogSizeCalculator("project_eden", "1207659235");
            var game = SetupGame(null, "1207659235");
            var size = await calc.GetInstallSizeAsync(game);
            Assert.Equal((ulong)(391 * 1024), size);
        }

        [Fact]
        public async Task Serial_Cleaners_Slug_Returns_0MB()
        {
            var calc = SetupGogSizeCalculator("serial_cleaners");
            var game = SetupGame("serial_cleaners", "1676910822");
            var size = await calc.GetInstallSizeAsync(game);
            Assert.Equal(0UL, size);
        }

        [Fact]
        public async Task Serial_Cleaners_Id_Returns_0MB()
        {
            var calc = SetupGogSizeCalculator("serial_cleaners", "1676910822");
            var game = SetupGame(null, "1676910822");
            var size = await calc.GetInstallSizeAsync(game);
            Assert.Equal(0UL, size);
        }
    }
}
