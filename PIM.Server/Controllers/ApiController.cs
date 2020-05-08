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
        private Random _r = new Random();
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
            spec.Players = spec.Players.OrderBy(x => _r.Next()).ToArray();
            var game = GameManager.Instance.CreateGame(spec);
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
