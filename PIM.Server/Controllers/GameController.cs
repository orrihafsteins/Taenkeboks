using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PIM.Server.Models;
using Taenkeboks;

namespace PIM.Server.Controllers
{
    [Route("game")]
    [Authorize]
    public class GameController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public GameController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("play/{gameId}")]
        public IActionResult Play(string gameId)
        {
            ViewData["GameId"] = gameId;
            return View();
        }
    }
}
