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
using PIM.Server.DataModel;
using Taenkeboks;

namespace PIM.Server.Controllers
{
    

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

        public GameTask CreateGame()
        {
            var game = GameTask.Example();
            _games[game.ID] = game;
            var gameTask = game.Start();//the task will end when game is completed or game.Stop() is called by cleanup task
            return game;
        }
    }

    [ApiController]
    [Route("game/{gameName}/play/{id}")]
    [Authorize]
    public class PlayApiController : Controller
    {
        private static GameTask _gameThread = GameManager.Instance.CreateGame();
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger _logger;
        public PlayApiController(SignInManager<IdentityUser> signInManager, ILogger<PlayApiController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }
        [HttpGet("current")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404)]
        public string Current(string gameName, string id)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            var v = _gameThread.GetCurrent(playerName);
            var jv = Taenkeboks.Json.serializeIndented(v);
            return jv;
        }

        [HttpGet("exception")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404)]
        public string Exception(string gameName, string id)
        {
            throw new Exception("Exception Exception");
            return "";
        }

        [HttpGet("next")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404)]
        public async Task<string> Next(string gameName, string id)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            var v = await _gameThread.GetNext(playerName);
            var jv = Taenkeboks.Json.serializeIndented(v);
            return jv;
        }

        [HttpPost("action")]
        public async void Action(string gameName, string id, [FromBody]string sAction)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            var action = JsonConvert.DeserializeObject<TbAction>(sAction);
            await _gameThread.PerformAction(playerName,action);
        }
    }
}
