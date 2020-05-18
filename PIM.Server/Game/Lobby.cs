using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taenkeboks;

namespace PIM.Server.Game
{
    public class LobbyState
    {
        public List<string> Players { get; set; }
        private HashSet<string> _readyPlayers;
        public int Version { get; set; }
        public GameSpec Spec { get; set; }
        public LobbyState()
        {
            Players = new System.Collections.Generic.List<string>(GameManager.CpuPlayers);
            _readyPlayers = new HashSet<string>();
            Spec = new GameSpec();
            Spec.Players = GameManager.CpuPlayers;
            Spec.Game = GameType.Taenkeboks;
            Spec.Taenkeboks = TbGameSpecModule.initClassicTournament(Spec.Players.Length);
        }

        public async Task<LobbyState> Next(int currentVersion)
        {
            if (Version > currentVersion)
                return this;
            else
                return await _next.Task;
        }

        TaskCompletionSource<LobbyState> _next = new TaskCompletionSource<LobbyState>();
        TaskCompletionSource<string> _ready = new TaskCompletionSource<string>();
        private void Update()
        {
            Version++;
            _readyPlayers.Clear();
            _next.SetResult(this);
            _ready.TrySetResult("");
            _next = new TaskCompletionSource<LobbyState>();
            _ready = new TaskCompletionSource<string>();
        }
        public void AddPlayer(string player)
        {
            if (Players.Contains(player))
                return;
            Players.Add(player);
            Update();
        }
        public void RemovePlayer(string player)
        {
            if (!Players.Contains(player))
                return;
            Players.Remove(player);
            Spec.Players = Spec.Players.Where(n => n != player).ToArray();
            Update();
        }
        public async Task<string> ReadyPlayer(string player,int readyVersion)
        {
            _readyPlayers.Add(player);
            if (Spec.Players.Where(p=> !GameManager.CpuPlayers.Contains(p)).All(p => _readyPlayers.Contains(p)))
                StartGame();
            return await _ready.Task;
        }

        //public void UnReadyPlayer(string player)
        //{
        //    var pix = Players.IndexOf(player);
        //    if (!Ready[pix])
        //        return;
        //    Ready[pix] = false;
        //    Update();
        //}

        public void UpdateSpec(GameSpec spec)
        {
            Spec = spec;
            Update();
        }


        private Random _r = new Random();
        public string StartGame()
        {
            Spec.Players = Spec.Players.OrderBy(x => _r.Next()).ToArray();
            var gameId = GameManager.Instance.CreateGame(Spec).Id;
            _readyPlayers.Clear();
            _ready.SetResult(gameId);
            return gameId;
        }
    }
    public class Lobby
    {
        public static Lobby Instance { get; } = new Lobby();
        private LobbyState _state = new LobbyState();
        public Lobby()
        {
            //add support for multiple lobbies
        }

        public void AddPlayer(string name) => _state.AddPlayer(name);
        public void RemovePlayer(string name) => _state.RemovePlayer(name);
        public async Task<LobbyState> Next(int currentVersion) => await _state.Next(currentVersion);
        public async Task<string> Ready(string playerName,int readyVersion) => await _state.ReadyPlayer(playerName, readyVersion);
        //public void ReadyPlayer(string name) => _state.ReadyPlayer(name);
        //public void UnreadyPlayer(string name) => _state.UnReadyPlayer(name);
        public void UpdateSpec(GameSpec spec) => _state.UpdateSpec(spec);
        public string StartGame() => _state.StartGame();
    }
}
