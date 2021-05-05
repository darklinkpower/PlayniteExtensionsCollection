using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.IO;

namespace SteamGameTransferUtility
{
    public class SteamGameTransferUtility : Plugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private SteamGameTransferUtilitySettings settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("c2dac2df-44c9-4f47-8555-c8d134c4f400");

        public SteamGameTransferUtility(IPlayniteAPI api) : base(api)
        {
            settings = new SteamGameTransferUtilitySettings(this);
        }
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return null;
        }

        public override List<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs menuArgs)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = "Steam Game Transfer Utility",
                    MenuSection = "@Steam Game Transfer Utility",
                    Action = args => {
                        WindowMethod();
                    }
                },
                new MainMenuItem
                {
                    Description = "Set installation drive tag in all games",
                    MenuSection = "@Steam Game Transfer Utility",
                    Action = args => {
                        SetInstallDirTags();
                    }
                }
            };
        }

        public void WindowMethod()
        {
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });

            window.Height = 250;
            window.Width = 600;
            window.Title = "Steam Game Transfer Utility";

            // Set content of a window. Can be loaded from xaml, loaded from UserControl or created from code behind
            WindowView windowView = new WindowView();
            windowView.PlayniteApi = PlayniteApi;
            window.Content = windowView;

            // Set owner if you need to create modal dialog window
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Use Show or ShowDialog to show the window
            window.ShowDialog();
        }

        public void SetInstallDirTags()
        {
            var progRes = PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var gameDatabase = PlayniteApi.Database.Games;
                string driveTagPrefix = "[Install Drive]";
                foreach (Game game in gameDatabase)
                {
                    string tagName = string.Empty;
                    if (!string.IsNullOrEmpty(game.InstallDirectory) && game.IsInstalled == true)
                    {
                        FileInfo s = new FileInfo(game.InstallDirectory);
                        string sourceDrive = System.IO.Path.GetPathRoot(s.FullName).ToUpper();
                        tagName = string.Format("{0} {1}", driveTagPrefix, sourceDrive);
                        Tag driveTag = PlayniteApi.Database.Tags.Add(tagName);
                        AddTag(game, driveTag);
                    }

                    if (game.Tags == null)
                    {
                        continue;
                    }

                    foreach (Tag tag in game.Tags.Where(x => x.Name.StartsWith(driveTagPrefix)))
                    {
                        if (!string.IsNullOrEmpty(tagName))
                        {
                            if (tag.Name != tagName)
                            {
                                RemoveTag(game, tag);
                            }
                        }
                        else
                        {
                            RemoveTag(game, tag);
                        }
                    }
                }
            }, new GlobalProgressOptions("Setting installation drive tags..."));

            PlayniteApi.Dialogs.ShowMessage("Finished setting installation drive tags.", "Steam Game Transfer Utility");
        }

        public bool RemoveTag(Game game, Tag tag)
        {
            if (game.TagIds != null)
            {
                if (game.TagIds.Contains(tag.Id))
                {
                    game.TagIds.Remove(tag.Id);
                    PlayniteApi.Database.Games.Update(game);
                    bool tagRemoved = true;
                    return tagRemoved;
                }
                else
                {
                    bool tagRemoved = false;
                    return tagRemoved;
                }
            }
            else
            {
                bool tagRemoved = false;
                return tagRemoved;
            }
        }

        public bool AddTag(Game game, Tag tag)
        {
            if (game.TagIds == null)
            {
                game.TagIds = new List<Guid> { tag.Id };
                PlayniteApi.Database.Games.Update(game);
                bool tagAdded = true;
                return tagAdded;
            }
            else if (game.TagIds.Contains(tag.Id) == false)
            {
                game.TagIds.AddMissing(tag.Id);
                PlayniteApi.Database.Games.Update(game);
                bool tagAdded = true;
                return tagAdded;
            }
            else
            {
                bool tagAdded = false;
                return tagAdded;
            }
        }
    }
}