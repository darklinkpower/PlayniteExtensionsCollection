using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Entities
{
    public class JastGameWrapper : ObservableObject
    {
        private Game _game;
        public Game Game { get => _game; set => SetValue(ref _game, value); }

        private ObservableCollection<JastAssetWrapper> _assets;
        public ObservableCollection<JastAssetWrapper> Assets { get => _assets; set => SetValue(ref _assets, value); }

        public JastGameWrapper(Game game, ObservableCollection<JastAssetWrapper> jastAssetWrappers)
        {
            Game = game;
            Assets = jastAssetWrappers;
        }
    }
}
