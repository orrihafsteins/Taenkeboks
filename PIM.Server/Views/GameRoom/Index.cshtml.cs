using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PIM.Server.Pages.GameRoom
{
    public class GameRoomModel : PageModel
    {
        int _id;
        public int GameRoomID
        {
            get
            {
                return _id;
            }
        }

        public void OnGet(int id)
        {
            _id = id;
        }
    }
}