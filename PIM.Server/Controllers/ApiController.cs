using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PIM.Server.Data;
using Microsoft.Extensions.Logging;
using PIM.Server.Controllers;
using System.Threading;
using PIM.Server.Models;
using PIM.Server.Game;
using Taenkeboks;

namespace PIM.Server.Controllers
{
    public enum GameType { Taenkeboks };
    public class GameSpec
    {
        public string[] Players { get; set; }
        public GameType Game { get; set; }
        public TbGameSpec Taenkeboks { get; set; }
    }
    public class GameManager
    {
        public static GameManager Instance { get; } = new GameManager();

        private Dictionary<string, GameTask> _games = new Dictionary<string, GameTask>();
        public GameTask GetGame(string id) => _games[id]; //TODO: Throw custom exception
        public GameManager()
        {
            //TODO: Clean up completed and abandoned games in task
            //when game completed: remove from _games
            //whend some amount of time since last move: remove from _games and call game.Stop()
        }

        public GameTask DuplicateGame(string gameId)
        {
            var oldGame = _games[gameId];
            var game = oldGame.Duplicate();
            _games[game.Id] = game;
            var gameTask = game.Start();//the task will end when game is completed or game.Stop() is called by cleanup task
            return game;
        }

        string[] CpuPlayers = new string[] {"Bob","Alice","Carol"};
        public GameTask CreateGame(GameSpec spec)
        {
            PlayerSpec[] playerSpecs = spec.Players.Select(pn => new PlayerSpec()
            {
                PlayerName = pn,
                PlayerType = CpuPlayers.Contains(pn) ? PlayerType.Cpu : PlayerType.Async
            }).ToArray();
            GameTask game;
            switch (spec.Game)
            {
                case GameType.Taenkeboks:
                    game = new GameTask(spec.Taenkeboks, playerSpecs);
                    break;
                default:
                    throw new Exception("Unknown game type");
            }
            _games[game.Id] = game;
            var gameTask = game.Start();//the task will end when game is completed or game.Stop() is called by cleanup task
            return game;
        }

        internal GameTask CreateDefaultGame(string playerName)
        {
            var game = GameTask.Default(playerName);
            _games[game.Id] = game;
            var gameTask = game.Start();//the task will end when game is completed or game.Stop() is called by cleanup task
            return game;
        }
    }



    [ApiController]
    [Route("api")]
    [Authorize]
    public class ApiController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger _logger;
        public ApiController(SignInManager<IdentityUser> signInManager, ILogger<ApiController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }
        [HttpPost("create")]
        public ActionResult<string> Create([FromBody]object oSpec)
        {
            var sSpec = oSpec.ToString();
            var spec = JsonConvert.DeserializeObject<GameSpec>(sSpec);
            var game = GameManager.Instance.CreateGame(spec);
            return Ok(game.Id);
        }

        public class DefaultParams {  }
        [HttpPost("default")]
        public ActionResult<string> CreateDefault([FromBody] DefaultParams p)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            GameTask game = GameManager.Instance.CreateDefaultGame(playerName);
            return Ok(game.Id);
        }
        public class DuplicateParams { public string GameId { get; set; } }
        [HttpPost("duplicate")]
        public ActionResult<string> Duplicate([FromBody] DuplicateParams p)
        {
            var game = GameManager.Instance.DuplicateGame(p.GameId);
            return Ok(game.Id);
        } 
        [HttpGet("current/{id}")]
        public ActionResult<string> Current(string id)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            var gameThread = GameManager.Instance.GetGame(id);
            var v = gameThread.GetCurrent(playerName);
            var jv = Taenkeboks.Json.serializeIndented(v);
            return Ok(jv);
        }
        [HttpGet("next/{id}")]
        public async Task<ActionResult<string>> Next(string id)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            var gameThread = GameManager.Instance.GetGame(id);
            var v = await gameThread.GetNext(playerName);
            var jv = Taenkeboks.Json.serializeIndented(v);
            return Ok(jv);
        }
        [HttpPost("action/{id}")]
        public async Task<ActionResult> Action(string id, [FromBody]object oAction)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            var sAction = oAction.ToString();
            var action = JsonConvert.DeserializeObject<TbAction>(sAction);
            var gameThread = GameManager.Instance.GetGame(id);
            await gameThread.PerformAction(playerName,action);
            return Ok();
        }
    }
}
