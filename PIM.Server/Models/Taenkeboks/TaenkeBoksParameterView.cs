using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Server.Models.Taenkeboks
{
    public class TaenkeboksParameterView
    {
        public TaenkeboksParameterView(string[] players, HT9000.Taenkeboks.TaenkeboksGameSpec specification)
        {
            Players = players;
            Specification = specification;
        }
        public string[] Players { get; }
        public HT9000.Taenkeboks.TaenkeboksGameSpec Specification { get; }
    }
}