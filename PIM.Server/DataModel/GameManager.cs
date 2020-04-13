using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Server.DataModel
{
    public abstract class GameManager<A, V, H, S>
    {
        Dictionary<int, GameThread<A, V, H, S>> _games = new Dictionary<int, GameThread<A, V, H, S>>();
        int _nextID = 2;
        
        public int AddGame(IPartialInfoMultiplayer<A, V, H, S> game, string[] playerTypes, string[] playerIDs)
        {
            lock (this)
            {
                int id = _nextID;
                _nextID++;
                var gameThread = new GameThread<A, V, H, S>(game, playerTypes, playerIDs);
                _games[id] = gameThread;
                gameThread.StartGame();
                return id;
            }
        }

        public GameThread<A, V, H, S> GetGame(int id)
        {
            lock (this)
            {
                return _games.GetValueOrDefault(id);
            }
        }
    }
}
