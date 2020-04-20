using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Taenkeboks.AsyncConsole
{
    enum PlayerType { Cpu,Console}
    class PlayerSpec
    {
        public string Name { get; set; }
        public PlayerType Type { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var players = new PlayerSpec[]
            {
                new PlayerSpec{Name="Bob", Type=PlayerType.Cpu},
                new PlayerSpec{Name="Alice", Type=PlayerType.Cpu},
                new PlayerSpec{Name="Orri", Type=PlayerType.Console}
            };

            var playerNames = players.Select(p => p.Name).ToArray();
            var spec = TbGameSpecModule.initClassicRules(players.Length);
            var game = TbGameModule.create(spec);
            var asyncGame = new AsyncGame<TbState, TbAction, TbVisible>(game, playerNames);
            Task[] playerTask = players.Select((p,i) => {
                return p.Type switch
                {
                    PlayerType.Cpu => new AsyncCpuPlayer(asyncGame.Players[i], spec).RunPlayer(),
                    PlayerType.Console => new AsyncConsolePlayer(asyncGame.Players[i]).RunPlayer(),
                    _ => throw new Exception("Death"),
                };
            }).ToArray();
            await asyncGame.RunGame();
        }
    }
}
