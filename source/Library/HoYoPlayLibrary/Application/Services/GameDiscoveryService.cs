using HoYoPlayLibrary.Domain.Entities;
using HoYoPlayLibrary.Domain.Interfaces;
using HoYoPlayLibrary.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoYoPlayLibrary.Application.Services
{
    internal class GameDiscoveryService
    {
        private readonly IHoyoPlayGameRepository _gameRepository;

        public GameDiscoveryService(IHoyoPlayGameRepository gameRepository)
        {
            _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        }

        public IEnumerable<HoyoPlayGame> GetInstalledGames()
        {
            return _gameRepository.GetInstalledGames();
        }
    }
}
