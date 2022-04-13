using JastUsaLibrary.Models;
using JastUsaLibrary.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.ViewModels
{
    public class GameDownloadsViewModel : ObservableObject
    {
        
        private JastUsaAccountClient accountClient;

        private int selectedTabItemIndex = 0;
        public int SelectedTabItemIndex
        {
            get => selectedTabItemIndex;
            set
            {
                selectedTabItemIndex = value;
                OnPropertyChanged();
            }
        }

        private GameTranslationsResponse gameTranslationsResponse;
        public GameTranslationsResponse GameTranslationsResponse
        {
            get => gameTranslationsResponse;
            set
            {
                gameTranslationsResponse = value;
                OnPropertyChanged();
            }
        }

        private Game game;
        public Game Game
        {
            get => game;
            set
            {
                game = value;
                OnPropertyChanged();
            }
        }

        public GameDownloadsViewModel(Game game, GameTranslationsResponse gameTranslationsResponse, JastUsaAccountClient accountClient)
        {
            GameTranslationsResponse = gameTranslationsResponse;
            Game = game;
            this.accountClient = accountClient;

            if (gameTranslationsResponse.GamePathLinks.HydraMember.Count > 0)
            {
                SelectedTabItemIndex = 0;
            }
            else if (gameTranslationsResponse.GamePatchLinks.HydraMember.Count > 0)
            {
                SelectedTabItemIndex = 1;
            }
            else if (gameTranslationsResponse.GameExtraLinks.HydraMember.Count > 0)
            {
                SelectedTabItemIndex = 2;
            }
        }

        public RelayCommand<HydraMember> GetAndOpenDownloadLinkCommand
        {
            get => new RelayCommand<HydraMember>((a) =>
            {
                GetAndOpenDownloadLink(a);
            });
        }

        private void GetAndOpenDownloadLink(HydraMember downloadAsset)
        {
            var url = accountClient.GetAssetDownloadLinkAsync(downloadAsset.GameId, downloadAsset.GameLinkId);
            if (url != null)
            {
                ProcessStarter.StartUrl(url);
            }
        }


    }
}