using Microsoft.FSharp.Core;
using PIM.Server.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Taenkeboks;
using Taenkeboks.Async;

namespace PIM.Server.Game
{
    public enum PlayerType
    {
        Async,
        Cpu
    }
    public class PlayerSpec
    {
        public PlayerType PlayerType { get; set; }
        public string PlayerName { get; set; }
    }
    public class GameTask
    {
        static Random _rnd = new Random();
        PlayerSpec[] _playerSpecs;
        string[] _playerNames;
        TbGameSpec _spec;
        AsyncGame<TbState, TbAction, TbVisible> _game;
        public string Id { get; } = Guid.NewGuid().ToString();
        public static GameTask Default(string playerName)
        {
            PlayerSpec[] players = new PlayerSpec[]
            {
                new PlayerSpec { PlayerType = PlayerType.Async, PlayerName = playerName },
                new PlayerSpec { PlayerType = PlayerType.Cpu, PlayerName = "Bob" },
                new PlayerSpec { PlayerType = PlayerType.Cpu, PlayerName = "Alice" },
                new PlayerSpec { PlayerType = PlayerType.Cpu, PlayerName = "Carol" }
            };
            players = players.OrderBy(x => _rnd.Next()).ToArray();//shuffle players
            var spec = TbGameSpecModule.initClassicQuick(players.Length);
            return new GameTask(spec, players);
        }
        public GameTask(TbGameSpec spec, PlayerSpec[] playerSpecs)
        {
            _playerSpecs = playerSpecs;
            _spec = spec;
            _playerNames = playerSpecs.Select(ps => ps.PlayerName).ToArray();
        }

        public GameTask Duplicate()
        {
            return new GameTask(_spec, _playerSpecs);
        }

        public TbVisible GetCurrent(string playerName)
        {
            var player = _game.Players.First(p => p.Name == playerName);//TODO: Throw custom exception instead of InvalidOperationException
            return player.Current();
        }

        public async Task<TbVisible> GetNext(string playerName)
        {
            var player = _game.Players.First(p => p.Name == playerName);//TODO: Throw custom exception instead of InvalidOperationException
            return await player.Next();


        }
        public async Task PerformAction(string playerName,TbAction action)
        {
            var player = _game.Players.First(p => p.Name == playerName);//TODO: Throw custom exception instead of InvalidOperationException
            await player.PerformAction(action);
        }

        public async Task Start()
        {
            var playerNames = _playerSpecs.Select(p => p.PlayerName).ToArray();
            var game = TbGameModule.create(_spec);
            _game = new AsyncGame<TbState, TbAction, TbVisible>(game, playerNames);
            Task[] cpuPlayerTasks =
                _playerSpecs.Select((p, i) => {
                    if (p.PlayerType == PlayerType.Cpu)
                        return new AsyncCpuPlayer(_game.Players[i], _spec).Start();
                    else
                        return null;
            }).Where(p=>p != null).ToArray();
            await _game.Start();
            Task.WaitAll(cpuPlayerTasks);
        }

        public void Stop()
        {
            _game.Stop();// this kills game and player tasks
        }
    }
}
