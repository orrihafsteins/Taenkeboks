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
using PIM.Server.DataModel;
using PIM.Server.Models.Taenkeboks;
using PIM.Server.Models;

namespace PIM.Server.Controllers
{
    [Route("api/taenkeboks")]
    [Authorize]
    public class TaenkeboksApiController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger _logger;
        public TaenkeboksApiController(SignInManager<IdentityUser> signInManager, ILogger<TaenkeboksApiController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(BoardView<HT9000.Taenkeboks.Action, PublicInformation, HiddenPlayerState, TaenkeboksStatus>))]
        [ProducesResponseType(404)]
        public IActionResult Get(int id)
        {
            string playerID = _signInManager.UserManager.GetUserId(this.User);
            var _gameThread = DataModel.DataInterface.Taenkeboks.GetGame(id);
            if (_gameThread == null)
            {
                return NotFound();
                //_gameThread = DataModel.DataInterface.Taenkeboks.GetGame(id);
            }

            var boardView = _gameThread.GetPlayerPerspective(playerID);
            return Ok(boardView);
        }

        [HttpGet("{id}/events")]
        [ProducesResponseType(200, Type = typeof(GameEventView<HT9000.Taenkeboks.Action, PublicInformation, HiddenPlayerState, TaenkeboksStatus>))]
        [ProducesResponseType(404)]
        public IActionResult GetNextEvent(int id)
        {
            //System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var _gameThread = DataModel.DataInterface.Taenkeboks.GetGame(id);
            if (_gameThread == null)
                return NotFound();
            var playerID = _signInManager.UserManager.GetUserId(this.User);
            var boardView = _gameThread.GetPlayerPerspective(playerID);
            var nextEvent = _gameThread.ConsumeEvent(playerID);
            return Ok(nextEvent);
        }
        //POST api/values/5
        [HttpPost("{id}")]
        public string Post(int id, [FromBody]HT9000.Taenkeboks.Action action)
        {
            var playerID = _signInManager.UserManager.GetUserId(this.User);
            var gameThread = DataModel.DataInterface.Taenkeboks.GetGame(id);
            gameThread.PostAction(playerID, action);
            return "";
        }
        ////DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
