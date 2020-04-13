using Microsoft.FSharp.Core;
using PIM.Server.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    public class AsynchPlayers
    {
        private BlockingCollection<(int, TbAction)> _actionQueue;
        private BlockingCollection<TbVisible>[] _visibleQueues;
        private TbVisible[] _visibleCurrent;
        public Player<TbVisible, TbAction>[] Players { get; private set; }
        private string[] _playerNames;
        public AsynchPlayers(string[] playerNames)
        {
            _playerNames = playerNames;
            _actionQueue = new BlockingCollection<(int, TbAction)>();
            _visibleQueues = _playerNames.Select(n => new BlockingCollection<TbVisible>()).ToArray();
            _visibleCurrent = _playerNames.Select(n => (TbVisible)null).ToArray();
            Players = _playerNames.Select(n => CreatePlayer(n)).ToArray();
        }

        private int PlayerIndex(string playerName)
        {
            int side = System.Array.IndexOf(_playerNames, playerName);
            if (side < 0) throw new Exception("Death");
            return side;
        }

        public void AddPlayerAction(string playerName, TbAction action)
        {
            int pIndex = PlayerIndex(playerName);
            _actionQueue.Add((pIndex, action));
        }

        public TbAction GetPlayerAction(int pSide)
        {
            (int aSide, TbAction a) = _actionQueue.Take();
            while (pSide != aSide) (aSide, a) = _actionQueue.Take(); //Silently ignore actions from other players?!
            return a;
        }

        public void SetPlayerVisible(int pIndex, TbVisible v)
        {
            _visibleQueues[pIndex].Add(v);
        }

        public TbVisible GetPlayerVisible(int pIndex)
        {
            if (_visibleQueues[pIndex].TryTake(out TbVisible next))
            {
                return next;
            }
            else
                return _visibleCurrent[pIndex];
        }

        public TbVisible GetNextPlayerVisible(int pIndex)
        {
            return _visibleQueues[pIndex].Take();
        }


        private Player<TbVisible, TbAction> CreatePlayer(string playerName)
        {
            int pIndex = PlayerIndex(playerName);
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
    public class GameThread
    {
        
        Thread _thread;
        Game<TbState, TbAction, TbVisible> _game;
        Player<TbVisible, TbAction>[] _players;
        public static GameThread Example()
        {
            string[] playerNames = new string[]
            {
                "Alice",
                "Bob",
                "Carol",
                "Dan"
            };
            var asynchPlayers = new AsynchPlayers(playerNames).Players;
            int playerCount = asynchPlayers.Length;
            var spec = TbGameSpecModule.initClassicRules(playerCount);
            var game = TbGameModule.create(spec);
            return new GameThread(
                game,
                asynchPlayers
            );
        }
            
        public GameThread(Game<TbState,TbAction,TbVisible> game,Player<TbVisible,TbAction>[] players)
        {
            _game = game;
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
        public void StopGame()
        {
            _thread.Abort();
        }
    }
}
