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

        #region Game creation methods
        public class DefaultParams { }
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
        #endregion Game creation methods

        #region Game play methods
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
            await gameThread.PerformAction(playerName, action);
            return Ok();
        }
        #endregion Game play methods

        #region LobbyMethods
        [HttpPost("lobby/update")]
        public ActionResult LobbyUpdate([FromBody]object oGameSpec)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            var sGameSpec = oGameSpec.ToString();
            var gameSpec = JsonConvert.DeserializeObject<GameSpec>(sGameSpec);
            Lobby.Instance.UpdateSpec(gameSpec);
            return Ok();
        }

        [HttpPost("lobby/addPlayer")]
        public ActionResult LobbyAddPlayer([FromBody]object o)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            Lobby.Instance.AddPlayer(playerName);
            return Ok();
        }

        [HttpPost("lobby/removePlayer")]
        public ActionResult LobbyRemovePlayer([FromBody]object o)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            Lobby.Instance.RemovePlayer(playerName);
            return Ok();
        }

        [HttpGet("lobby/readyPlayer/{lastVersion}")]
        public async Task<ActionResult<string>> LobbyReadyPlayer(int readyVersion)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            var gameId = await Lobby.Instance.Ready(playerName, readyVersion);
            return Ok(gameId);
        }

        [HttpGet("lobby/next/{lastVersion}")]
        public async Task<ActionResult<string>> LobbyNext(int lastVersion)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            var v = await Lobby.Instance.Next(lastVersion);
            var jv = Taenkeboks.Json.serializeIndented(v);
            return Ok(jv);
        }

        [HttpPost("lobby/startGame")]
        public ActionResult<string> StartGame()
        {
            var gameId = Lobby.Instance.StartGame();
            return Ok(gameId);
        }
        #endregion LobbyMethods
    }
}
