using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PIM.Server.Data;
using PIM.Server.DataModel;
using PIM.Server.Models;
using PIM.Server.Models.Taenkeboks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PIM.Server.Controllers
{
    [Route("api/gameroom")]
    [Authorize]
    public class GameRoomApiController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger _logger;
        public GameRoomApiController(SignInManager<IdentityUser> signInManager, ILogger<GameRoomApiController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(GameRoomView))]
        [ProducesResponseType(404)]
        public IActionResult Get(int id)
        {
            var room = DataModel.DataInterface.Rooms.GetRoom(id);
            if (room == null)
            {
                return NotFound();
            }
            return Ok(new GameRoomView(_signInManager.UserManager, room));
        }

        [HttpGet()]
        public GameRoomView[] Get()
        {
            return DataInterface.Rooms.GetRooms().Select(r => new GameRoomView(_signInManager.UserManager, r)).ToArray();
        }

        [HttpPost()]
        public int Post([FromBody]string name)
        {
            return DataModel.DataInterface.Rooms.AddRoom(name);
        }

        [HttpPost("{id}/players")]
        public void Post(int id)
        {
            string playerID = _signInManager.UserManager.GetUserId(this.User);
            DataInterface.Rooms.GetRoom(id).AddPlayer(playerID);
        }


        [HttpPost("{id}")]
        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        public IActionResult Post(int id, [FromBody]TaenkeboksParameterView parameters)
        {
            var gameRoom = DataInterface.Rooms.GetRoom(id);
            if (gameRoom == null)
                return NotFound();

            CpuPlayerPool pool = new CpuPlayerPool();
            int pCount = parameters.Players.Length;
            string[] pNames = new string[pCount];
            string[] pTypes = new string[pCount];
            string[] pIDs = new string[pCount];
            for (int i = 0; i < pCount; i++)
            {
                string pID = parameters.Players[i];
                pIDs[i] = pID;
                if (pool.IsCpuPlayer(pID))
                {
                    var cpuPlayer = pool.Take();
                    pNames[i] = cpuPlayer.Name;
                    pTypes[i] = cpuPlayer.Type;
                    pIDs[i] = cpuPlayer.ID;
                }
                else
                {
                    var p = DataInterface.Players.GetPlayer(_signInManager.UserManager, pID);
                    pNames[i] = p.UserName;
                    pTypes[i] = "api";
                }
            }

            int gameID = DataInterface.Taenkeboks.AddGame(parameters.Specification, pNames, pTypes, pIDs);
            gameRoom.StartGame(gameID);
            return Ok(gameID);
        }
        [HttpGet("{id}/events")]
        [ProducesResponseType(200, Type = typeof(GameRoomEventView))]
        [ProducesResponseType(404)]
        public IActionResult GetNextEvent(int id)
        {
            //System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var gameRoom = DataInterface.Rooms.GetRoom(id);
            if (gameRoom == null)
                return NotFound();
            var playerID = _signInManager.UserManager.GetUserId(this.User);
            var nextEvent = gameRoom.ConsumeEvent(playerID);
            return Ok(nextEvent);
        }
    }
    public class CpuPlayer
    {
        public string Name;
        public string Type;
        public string ID;
    }
    public class CpuPlayerPool
    {
        private CpuPlayer[] _playerPool;
        private bool[] _taken;
        private HashSet<string> _cpuPlayerIDs;
        public bool IsCpuPlayer(string playerID)
        {
            return (playerID == "CPU") || _cpuPlayerIDs.Contains(playerID);
        }
        public CpuPlayerPool()
        {
            _playerPool = new CpuPlayer[]
            {
                new CpuPlayer
                {
                    Name = "Calculon",
                    Type = "prior",
                    ID = "prior",
                },
                new CpuPlayer
                {
                    Name = "Hair Robot",
                    Type = "min",
                    ID = "min",
                },
                new CpuPlayer
                {
                    Name = "Robot Devil",
                    Type = "aggro",
                    ID = "aggro",
                },
                new CpuPlayer
                {
                    Name = "Humorbot 5.0",
                    Type = "prior",
                    ID = "prior",
                },
                new CpuPlayer
                {
                    Name = "Roberto",
                    Type = "aggro",
                    ID = "aggro",
                }
            };
            _taken = new bool[_playerPool.Length];
            _cpuPlayerIDs = _playerPool.Select(p => p.ID).ToHashSet();
        }
        private Random _r = new Random();
        public CpuPlayer Take()
        {
            var playersLeft =
                _taken
                .Select((t, i) => new { t, i })
                .Where(x => !x.t)
                .Select(x => x.i)
                .ToArray();
            var pCount = playersLeft.Length;
            if (pCount < 1)
                throw new Exception("Player pool depleted");
            var selected = playersLeft[_r.Next() % pCount];
            var player = _playerPool[selected];
            _taken[selected] = true;
            return player;
        }
    }
}