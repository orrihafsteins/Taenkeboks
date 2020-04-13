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

namespace PIM.Server.Controllers
{

    [Route("game/{gameName}/play/{id}")]
    [Authorize]
    public class PlayApiController : Controller
    {
        private static GameThread _gameThread = GameThread.Example();
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger _logger;
        public PlayApiController(SignInManager<IdentityUser> signInManager, ILogger<PlayApiController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
            _gameThread.StartGame();
        }
        [HttpGet("current")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404)]
        public IActionResult Get(string gameName, string id)
        {
            string playerName = _signInManager.UserManager.GetUserName(this.User);
            return Ok($"CurrentState of {gameName} {id}");
        }

        //[HttpPost("{id}")]
        //public string Post(int id, [FromBody]string action)
        //{
        //    return "Action ";
        //}
    }
}
