using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Server.Models.Taenkeboks
{
    public class ActionView
    {
        public ActionView(int side, HT9000.Taenkeboks.Action action)
        {
            PlayerSide = side;
            PlayerAction = action;
        }
        public int PlayerSide { get; }
        public HT9000.Taenkeboks.Action PlayerAction { get; }
    }
}
