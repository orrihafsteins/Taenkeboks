using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taenkeboks;

namespace PIM.Server.Game
{
    public class GameManager
    {
        public static GameManager Instance { get; } = new GameManager();

        public static string[] CpuPlayers = new string[] { "Bob", "Alice", "Carol", "Dan" };

        private Dictionary<string, GameTask> _games = new Dictionary<string, GameTask>();
        public GameTask GetGame(string id) => _games[id]; //TODO: Throw custom exception
        public GameManager()
        {
            //TODO: Clean up completed and abandoned games in task
            //when game completed: remove from _games
            //whend some amount of time since last move: remove from _games and call game.Stop()
        }

        public GameTask DuplicateGame(string gameId)
        {
            var oldGame = _games[gameId];
            var game = oldGame.Duplicate();
            _games[game.Id] = game;
            var gameTask = game.Start();//the task will end when game is completed or game.Stop() is called by cleanup task
            return game;
        }

        public GameTask CreateGame(GameSpec spec)
        {
            PlayerSpec[] playerSpecs = spec.Players.Select(pn => new PlayerSpec()
            {
                PlayerName = pn,
                PlayerType = CpuPlayers.Contains(pn) ? PlayerType.Cpu : PlayerType.Async
            }).ToArray();
            GameTask game;
            switch (spec.Game)
            {
                case GameType.Taenkeboks:
                    game = new GameTask(spec.Taenkeboks, playerSpecs);
                    break;
                default:
                    throw new Exception("Unknown game type");
            }
            _games[game.Id] = game;
            var gameTask = game.Start();//the task will end when game is completed or game.Stop() is called by cleanup task
            return game;
        }

        internal GameTask CreateDefaultGame(string playerName)
        {
            var game = GameTask.Default(playerName);
            _games[game.Id] = game;
            var gameTask = game.Start();//the task will end when game is completed or game.Stop() is called by cleanup task
            return game;
        }
    }



}
