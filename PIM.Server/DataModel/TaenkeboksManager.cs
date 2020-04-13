using HT9000.Taenkeboks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Server.DataModel
{
    public class TaenkeboksManager : GameManager<HT9000.Taenkeboks.Action, PublicInformation, HiddenPlayerState, TaenkeboksStatus>
    {
        public TaenkeboksManager()
            : base()
        {
            //start default game for debugging
            //string[] playerNames = new string[] { "Brjánn", "Þormóður", "Starkaður", "Hjálmar" };
            //string[] playerTypes = new string[] { "local", "prior", "api", "local" };
            //string[] playerIDs = new string[] { "CPU", "CPU", "ce76bde9-cc8d-4ea0-ab06-9ab27f0c629e", "CPU" };
            string[] playerNames = new string[] { "Þormóður", "Orri2000"};
            string[] playerTypes = new string[] { "prior", "api"};
            string[] playerIDs = new string[] { "CPU", "fb242e9f-bdb0-4c2e-ab3f-59722379b2e9" };

            AddGame(TaenkeboksGameSpecModule.initTestRules(playerNames.Length), playerNames, playerTypes, playerIDs);
        }
        public int AddGame(HT9000.Taenkeboks.TaenkeboksGameSpec spec, string[] playerNames, string[] playerTypes,string[] playerIDs)
        {
                var game = TaenkeboksInterfaceModule.createTaenkeboks(spec, playerNames);
                return AddGame(game,playerTypes, playerIDs);
        }
    }
}
