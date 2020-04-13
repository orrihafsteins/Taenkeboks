using PIM.Server.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Server.DataModel
{
    public static class DataInterface
    {
        public static TaenkeboksManager Taenkeboks = new TaenkeboksManager();
        public static GameRoomManager Rooms = new GameRoomManager();
        public static PlayerManager Players = new PlayerManager();
        
    }
}
