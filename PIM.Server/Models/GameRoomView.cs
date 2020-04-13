using PIM.Server.Data;
using PIM.Server.DataModel;
using PIM.Server.Models.Taenkeboks;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Server.Models
{
    public class GameRoomView
    {
        //public GameRoomView(int gameRoomID, PlayerView[] players)
        //{
        //    GameRoomID = gameRoomID;
        //    Players = players;
        //}
        public GameRoomView(UserManager<IdentityUser> userManager,GameRoom room)
        {
            GameRoomID = room.GameRoomID;
            Name = room.Name;
            Players = room.Players.Select(p => new PlayerView(DataInterface.Players.GetPlayer(userManager, p))).ToArray();
        }
        public int GameRoomID { get; }
        public string Name { get; }
        public PlayerView[] Players { get; }
    }
}
