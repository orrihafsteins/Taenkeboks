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

namespace PIM.Server.DataModel
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

    public class GameThread
    {
        PlayerSpec[] _playerSpecs;
        string[] _playerNames;
        TbGameSpec _spec;
        AsyncGame<TbState, TbAction, TbVisible> _game;
        public static GameThread Example()
        {
            PlayerSpec[] players = new PlayerSpec[]
            {
                new PlayerSpec { PlayerType = PlayerType.Async, PlayerName = "orrihafsteins@gmail.com" },
                new PlayerSpec { PlayerType = PlayerType.Cpu, PlayerName = "Bob" },
                new PlayerSpec { PlayerType = PlayerType.Cpu, PlayerName = "Alice" },
                new PlayerSpec { PlayerType = PlayerType.Cpu, PlayerName = "Carol" }
            };
            var spec = TbGameSpecModule.initClassicRules(players.Length);
            return new GameThread(spec, players);
        }

        //public GameThread(Game<TbState, TbAction, TbVisible> game, Player<TbVisible, TbAction>[] players)
        public GameThread(TbGameSpec spec, PlayerSpec[] playerSpecs)
        {
            if (spec.playerCount != playerSpecs.Length) throw new Exception("Death");
            _playerSpecs = playerSpecs;
            _spec = spec;
            _playerNames = playerSpecs.Select(ps => ps.PlayerName).ToArray();
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
    }
}
