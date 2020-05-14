using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taenkeboks;

namespace PIM.Server.Game
{
    public enum GameType { Taenkeboks };
    public class GameSpec
    {
        public string[] Players { get; set; }
        public GameType Game { get; set; }
        public TbGameSpec Taenkeboks { get; set; }
    }
}
