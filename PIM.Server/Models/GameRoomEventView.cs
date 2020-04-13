using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Server.Models
{
    public enum GameRoomEventCode { StartGame, PlayerAdded, ParamChanged}
    public class GameRoomEventView
    {
        public GameRoomEventView(string playerID, GameRoomEventCode code, string key, string value)
        {
            PlayerID = playerID;    
            EventCode = code.ToString();
            Key = key;
            Value = value;
        }
        public string PlayerID { get; }
        public string EventCode { get; }
        public string Key { get; }
        public string Value { get; }
        public static GameRoomEventView PlayerAdded(string player)
        {
            return new GameRoomEventView(player, GameRoomEventCode.PlayerAdded, null, null);
        }
        public static GameRoomEventView StartGame(int gameID)
        {
            return new GameRoomEventView(null, GameRoomEventCode.StartGame, gameID.ToString(), null);
        }
    }
}
