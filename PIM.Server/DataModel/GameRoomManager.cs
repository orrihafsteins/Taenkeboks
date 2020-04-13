using PIM.Server.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Server.DataModel
{
    
    
    public class GameRoom
    {
        List<BlockingCollection<GameRoomEventView>> _responseQueues;

        List<string> _players = new List<string>();
        public string Name { get; }
        public int GameRoomID { get; }
        public GameRoom(int gameRoomID, string name)
        {
            _responseQueues = new List<BlockingCollection<GameRoomEventView>>();
            GameRoomID = gameRoomID;
            Name = name;
        }

        public void AddPlayer(string playerID)
        {
            lock (this)
            {
                if (_players.Contains(playerID))
                    return; 
                _players.Add(playerID);
                _responseQueues.Add(new BlockingCollection<GameRoomEventView>());
                var gre = GameRoomEventView.PlayerAdded(playerID);
                foreach (var p in _responseQueues)
                    p.Add(gre);
            }
        }

        public void StartGame(int gameID)
        {
            lock (this)
            {
                var gre = GameRoomEventView.StartGame(gameID);
                foreach (var p in _responseQueues)
                    p.Add(gre);
            }
        }
        public void RemovePlayer(string playerID)
        {
            throw new NotImplementedException();
            _players.Remove(playerID);
        }
        public string[] Players { get { return _players.ToArray(); } }

        internal GameRoomEventView ConsumeEvent(string playerID)
        {
            int side = _players.IndexOf(playerID);
            if (side < 0) return null;//the player is not present in the room and has no events here
            GameRoomEventView current;
            bool foundOne = _responseQueues[side].TryTake(out current, -1);
            if (foundOne)
            {
                return current;
            }
            else
                return null;
        }
    }
    public class GameRoomManager
    {
        Dictionary<int,GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _nextID = 0;

        public GameRoomManager()
        {
            AddRoom("Room Zero");
        }
        public int AddRoom(string name)
        {
            lock (this)
            {
                int id = _nextID;
                _nextID++;
                var room = new GameRoom(id, name);
                _rooms[id] = room;
                return id;
            }
        }
        public GameRoom GetRoom(int id)
        {
            lock (this)
            {
                return _rooms.GetValueOrDefault(id);
            }
        }

        public GameRoom[] GetRooms()
        {
            lock (this)
            {
                return _rooms.Values.ToArray();
            }
        }
    }
}
