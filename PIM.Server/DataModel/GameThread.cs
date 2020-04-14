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

namespace PIM.Server.DataModel
{

    public static class FSharpUtil
    {
        public static Func<T, Unit> ToFunc<T>(this Action<T> action)
        {
            return x => { action(x); return (Unit)Activator.CreateInstance(typeof(Unit), true); };
        }

        public static FSharpFunc<I, Unit> ToFSharp<I>(this Action<I> a)
        {
            var unit = (Unit)Activator.CreateInstance(typeof(Unit), true);
            var funcA = a.ToFunc();
            return FSharpFunc<I, Unit>.FromConverter(new Converter<I, Unit>(funcA));
        }

        public static FSharpFunc<I, O> ToFsharp<I, O>(this Func<I, O> a)
        {
            return FSharpFunc<I, O>.FromConverter(new Converter<I, O>(a));
        }


    }


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
        
        Thread _thread;
        Game<TbState, TbAction, TbVisible> _game;
        private Channel<(int, TbAction)> _actionQueue; //Game controller adds actions to this queue on behalf of players, game thread processes
        private Channel<TbVisible>[] _visibleQueues; //Game thread adds visible information to these queues, picked up and delivered to players by GameControllar
        private TbVisible[] _visibleCurrent; //Contains last return visible information for each player
        private string[] _playerNames;
        private Player<TbVisible,TbAction>[] _players;

        public static GameThread Example()
        {
            int playerCount = 4;
            var spec = TbGameSpecModule.initClassicRules(playerCount);
            PlayerSpec[] players = new PlayerSpec[]
            {
                new PlayerSpec { PlayerType = PlayerType.Async, PlayerName = "orrihafsteins@gmail.com" },
                new PlayerSpec { PlayerType = PlayerType.Cpu, PlayerName = "Bob" },
                new PlayerSpec { PlayerType = PlayerType.Cpu, PlayerName = "Alice" },
                new PlayerSpec { PlayerType = PlayerType.Cpu, PlayerName = "Carol" }
            };
            return new GameThread(spec, players);
        }

        //public GameThread(Game<TbState, TbAction, TbVisible> game, Player<TbVisible, TbAction>[] players)
        public GameThread(TbGameSpec spec, PlayerSpec[] playerSpecs)
        {
            if (spec.playerCount >= playerSpecs.Length) throw new Exception("Death");
            _actionQueue = Channel.CreateUnbounded<(int, TbAction)>();
            _visibleQueues = playerSpecs.Select(n => Channel.CreateUnbounded<TbVisible>()).ToArray();
            _visibleCurrent = playerSpecs.Select(n => (TbVisible)null).ToArray();
            _playerNames = playerSpecs.Select(ps => ps.PlayerName).ToArray();
            var players = playerSpecs.Select( (ps,i) =>
            {
                switch (ps.PlayerType)
                {
                    case PlayerType.Async:
                        return TbAiPlayerModule.createPlayer(spec, ps.PlayerName);
                    case PlayerType.Cpu:
                        return CreatePAsynclayer(i,ps.PlayerName);
                    default:
                        throw new Exception("Death");
                }
            }).ToArray();
            _game = TbGameModule.create(spec);
            _players = players;
        }



        private void GameLoop()
        {
            Game.play<TbState, TbAction, TbVisible>(_game,_players);
        }

        public void StartGame()
        {
            if (_thread != null) throw new Exception("already started");
            _thread = new Thread(new System.Threading.ThreadStart(GameLoop));
            _thread.Start();
        }
        //public void StopGame()
        //{
        //    _thread.Abort();
        //}

        private int PlayerIndex(string playerName)
        {
            int side = System.Array.IndexOf(_playerNames, playerName);
            if (side < 0) throw new Exception("Death");
            return side;
        }

        public async Task AddPlayerAction(string playerName, TbAction action)
        {
            int pIndex = PlayerIndex(playerName);
            await _actionQueue.Writer.WriteAsync((pIndex, action));
        }

        private async Task<TbAction> GetPlayerAction(int pSide)
        {
            (int aSide, TbAction a) = await _actionQueue.Reader.ReadAsync();
            while (pSide != aSide) (aSide, a) = await _actionQueue.Reader.ReadAsync(); //Silently ignore actions from other players?!
            return a;
        }

        private async Task SetPlayerVisible(int pIndex, TbVisible v)
        {
            await _visibleQueues[pIndex].Writer.WriteAsync(v);
        }

        public TbVisible GetPlayerVisible(int pIndex)//Immediately returns with the next or current state
        {
            if (_visibleQueues[pIndex].Reader.TryRead(out TbVisible next))
            {
                return next;
            }
            else
                return _visibleCurrent[pIndex];
        }

        public async Task<TbVisible> GetNextPlayerVisible(int pIndex)//awaits until a new visible state for player is available
        {
            return await _visibleQueues[pIndex].Reader.ReadAsync();
        }

        private Player<TbVisible, TbAction> CreatePAsynclayer(int pIndex,string playerName)
        {
            Unit unit = (Unit)Activator.CreateInstance(typeof(Unit), true);
            Func<TbVisible, TbAction> policy = (a) =>
            {
                return GetPlayerAction(pIndex);
            };
            Func<TbVisible, Unit> update = (v) =>
            {
                SetPlayerVisible(pIndex, v);
                return unit;
            };
            return new Player<TbVisible, TbAction>(
                playerName,
                policy.ToFsharp(),
                update.ToFsharp()
            );
        }
    }
}
